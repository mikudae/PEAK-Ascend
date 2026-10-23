using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using BepInEx.Logging;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using PhotonPlayer = Photon.Realtime.Player;
using Object = UnityEngine.Object;

namespace Summit;

/// <summary>
/// Embedded HTTP/JSON Live Debug Bridge for Ascend (PEAK mod).
/// Exposes real-time game state inspection (GET /status, /scouts, /inventory, /logs)
/// and interactive main-thread administrative control (POST /action).
/// </summary>
public static class DebugBridge
{
	public class RingBuffer
	{
		private readonly string[] buffer;
		private int start = 0;
		private int count = 0;
		private readonly int capacity;
		private readonly object syncRoot = new object();

		public RingBuffer(int capacity = 1000)
		{
			this.capacity = Math.Max(1, capacity);
			buffer = new string[this.capacity];
		}

		public void Clear()
		{
			lock (syncRoot)
			{
				start = 0;
				count = 0;
				Array.Clear(buffer, 0, buffer.Length);
			}
		}

		public void Add(string entry)
		{
			if (entry == null) return;
			lock (syncRoot)
			{
				if (count < capacity)
				{
					buffer[(start + count) % capacity] = entry;
					count++;
				}
				else
				{
					buffer[start] = entry;
					start = (start + 1) % capacity;
				}
			}
		}

		public List<string> GetLast(int n)
		{
			lock (syncRoot)
			{
				if (n > count) n = count;
				if (n <= 0) return new List<string>();

				List<string> result = new List<string>(n);
				int startIndex = (start + count - n) % capacity;
				for (int i = 0; i < n; i++)
				{
					result.Add(buffer[(startIndex + i) % capacity]);
				}
				return result;
			}
		}

		public int Count
		{
			get
			{
				lock (syncRoot)
				{
					return count;
				}
			}
		}
	}

	internal class ActionItem
	{
		public string Action;
		public Dictionary<string, string> Params;
		public ManualResetEventSlim Done = new ManualResetEventSlim(false);
		public string ResponseJson;
		public int StatusCode = 200;
		public volatile bool Cancelled = false;
	}

	internal class BepInLogListener : ILogListener
	{
		public void LogEvent(object sender, LogEventArgs eventArgs)
		{
			if (eventArgs != null)
			{
				string level = eventArgs.Level.ToString();
				string source = eventArgs.Source?.SourceName ?? "BepInEx";
				string data = eventArgs.Data?.ToString() ?? "";
				Log($"[{level}:{source}] {data}");
			}
		}

		public void Dispose()
		{
		}
	}

	public static int ActivePort { get; private set; } = 0;
	public static string ActiveUrl { get; private set; } = "";
	public static bool IsRunning { get; private set; } = false;
	public static readonly RingBuffer Logs = new RingBuffer(1000);

	private static HttpListener _listener;
	private static Thread _listenerThread;
	private static BepInLogListener _logListener;
	private static readonly ConcurrentQueue<ActionItem> _actionQueue = new ConcurrentQueue<ActionItem>();
	private static readonly object _initLock = new object();
	private static string _tempFilePath = null;

	private static volatile string _cachedStatusJson = null;
	private static volatile string _cachedScoutsJson = null;
	private static volatile string _cachedInventoryJson = null;
	private static long _lastSnapshotTick = 0;

	public static void Start()
	{
		lock (_initLock)
		{
			if (IsRunning) return;

			bool isMaster = (ModNet.Role == ModNet.ModRole.Master);
			int basePort = isMaster ? 18080 : 18081;
			int allocatedPort = 0;
			HttpListener listener = null;

			for (int port = basePort; port < basePort + 100; port++)
			{
				try
				{
					listener = new HttpListener();
					listener.Prefixes.Add($"http://127.0.0.1:{port}/");
					listener.Start();
					allocatedPort = port;
					break;
				}
				catch (Exception)
				{
					try { listener?.Close(); } catch {}
					listener = null;
				}
			}

			if (listener == null || allocatedPort == 0)
			{
				Plugin.L?.LogError($"[DebugBridge] Failed to bind HttpListener to any port starting from {basePort}.");
				return;
			}

			_listener = listener;
			ActivePort = allocatedPort;
			ActiveUrl = $"http://127.0.0.1:{allocatedPort}";
			IsRunning = true;

			// Record active URL to %TEMP%\peak_debug_master.txt or peak_debug_slave.txt
			try
			{
				string fileName = isMaster ? "peak_debug_master.txt" : "peak_debug_slave.txt";
				_tempFilePath = Path.Combine(Path.GetTempPath(), fileName);
				File.WriteAllText(_tempFilePath, ActiveUrl);
				Plugin.L?.LogInfo($"[DebugBridge] Recorded active URL to: {_tempFilePath}");

				AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
				AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
				AppDomain.CurrentDomain.DomainUnload -= OnProcessExit;
				AppDomain.CurrentDomain.DomainUnload += OnProcessExit;
			}
			catch (Exception ex)
			{
				Plugin.L?.LogWarning($"[DebugBridge] Failed to write temp file: {ex.Message}");
			}

			// Pre-initialize snapshots so early GET requests don't hit null cache
			try
			{
				RefreshSnapshots();
			}
			catch {}

			// Hook BepInEx logging
			try
			{
				_logListener = new BepInLogListener();
				BepInEx.Logging.Logger.Listeners.Add(_logListener);
			}
			catch (Exception ex)
			{
				Plugin.L?.LogWarning($"[DebugBridge] Failed to hook BepInEx logger: {ex.Message}");
			}

			// Start HTTP listener background worker thread
			_listenerThread = new Thread(ListenLoop)
			{
				IsBackground = true,
				Name = "AscendDebugBridgeListener"
			};
			_listenerThread.Start();

			Log($"[DebugBridge] Server started on {ActiveUrl} (Role: {(isMaster ? "Master" : "Slave")})");
			Plugin.L?.LogInfo($"[DebugBridge] Live Debug Bridge listening on {ActiveUrl}");
		}
	}

	private static void OnProcessExit(object sender, EventArgs e)
	{
		Stop();
	}

	public static void Stop()
	{
		lock (_initLock)
		{
			if (!IsRunning) return;
			IsRunning = false;

			string stoppedUrl = ActiveUrl;
			ActivePort = 0;
			ActiveUrl = "";

			_cachedStatusJson = null;
			_cachedScoutsJson = null;
			_cachedInventoryJson = null;

			try
			{
				AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
				AppDomain.CurrentDomain.DomainUnload -= OnProcessExit;
			}
			catch {}

			// Delete temp URL file on shutdown if it still records our URL
			try
			{
				if (!string.IsNullOrEmpty(_tempFilePath) && File.Exists(_tempFilePath))
				{
					try
					{
						string existing = File.ReadAllText(_tempFilePath).Trim();
						if (string.IsNullOrEmpty(stoppedUrl) || string.Equals(existing, stoppedUrl, StringComparison.OrdinalIgnoreCase))
						{
							File.Delete(_tempFilePath);
						}
					}
					catch
					{
						try { File.Delete(_tempFilePath); } catch {}
					}
				}
				_tempFilePath = null;
			}
			catch {}

			try
			{
				if (_logListener != null)
				{
					BepInEx.Logging.Logger.Listeners.Remove(_logListener);
					_logListener.Dispose();
					_logListener = null;
				}
			}
			catch {}

			try
			{
				if (_listener != null)
				{
					_listener.Stop();
					_listener.Close();
					_listener = null;
				}
			}
			catch {}

			try
			{
				if (_listenerThread != null && _listenerThread.IsAlive)
				{
					_listenerThread.Join(500);
					_listenerThread = null;
				}
			}
			catch {}

			// Drain any pending items with cancellation
			while (_actionQueue.TryDequeue(out var item))
			{
				item.Cancelled = true;
				item.StatusCode = 503;
				item.ResponseJson = ToJson(new Dictionary<string, object>
				{
					["success"] = false,
					["error"] = "DebugBridge stopped"
				});
				item.Done.Set();
			}

			Log($"[DebugBridge] Server stopped on {stoppedUrl}");
		}
	}

	public static void Log(string message)
	{
		if (string.IsNullOrEmpty(message)) return;
		string timestamp = DateTime.UtcNow.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
		Logs.Add($"[{timestamp}] {message}");
	}

	public static void LogPhoton(string message)
	{
		if (string.IsNullOrEmpty(message)) return;
		Log($"[Photon] {message}");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static float GetUnityUnscaledTime() => Time.unscaledTime;

	private static float SafeUnscaledTime()
	{
		try { return GetUnityUnscaledTime(); } catch { return 0f; }
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static string GetActiveSceneName() => SceneManager.GetActiveScene().name;

	private static string SafeSceneName()
	{
		try { string s = GetActiveSceneName(); return string.IsNullOrEmpty(s) ? "None" : s; } catch { return "None"; }
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool CheckInGame() => Features.InGame;

	private static bool SafeInGame()
	{
		try { return CheckInGame(); } catch { return false; }
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Character GetLocalCharacter() => Features.Me;

	private static Character SafeLocalCharacter()
	{
		try { return GetLocalCharacter(); } catch { return null; }
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static List<Item> GetItemList() => Features.ItemList();

	private static List<Item> SafeItemList()
	{
		try { return GetItemList(); } catch { return null; }
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void DoWarp(Vector3 pos, string label) => Features.Warp(pos, label);

	private static bool SafeWarp(Vector3 pos, string label)
	{
		try { DoWarp(pos, label); return true; } catch { return false; }
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void DoItemAdd(Item item) => ItemDatabase.Add(item);

	private static bool SafeItemAdd(Item item)
	{
		try { DoItemAdd(item); return true; } catch { return false; }
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static string GetItemNameInternal(Item item) => item?.GetName();

	private static string SafeGetItemName(Item item)
	{
		if ((Object)(object)item == (Object)null) return "";
		try
		{
			string name = GetItemNameInternal(item);
			if (!string.IsNullOrEmpty(name)) return name;
		}
		catch {}
		try
		{
			return ((Object)item).name ?? "";
		}
		catch {}
		return "";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void MenuSay(string msg) => Menu.Instance?.Say(msg);

	private static void SafeMenuSay(string msg)
	{
		try { MenuSay(msg); } catch {}
		try { Features.Say?.Invoke(msg); } catch {}
	}

	public static void Update()
	{
		if (!IsRunning) return;

		// Refresh state snapshots approximately every 100ms or when forced
		try
		{
			long nowTick = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
			if (nowTick - _lastSnapshotTick >= 100 || _cachedStatusJson == null)
			{
				_lastSnapshotTick = nowTick;
				RefreshSnapshots();
			}
		}
		catch {}

		// Drain inbound main-thread action queue
		while (_actionQueue.TryDequeue(out var item))
		{
			if (item.Cancelled) continue;
			try
			{
				item.ResponseJson = ExecuteActionOnMainThread(item.Action, item.Params);
				item.StatusCode = 200;
			}
			catch (ArgumentException ex)
			{
				item.StatusCode = 400;
				item.ResponseJson = ToJson(new Dictionary<string, object>
				{
					["success"] = false,
					["error"] = ex.Message
				});
			}
			catch (FormatException ex)
			{
				item.StatusCode = 400;
				item.ResponseJson = ToJson(new Dictionary<string, object>
				{
					["success"] = false,
					["error"] = ex.Message
				});
			}
			catch (Exception ex)
			{
				item.StatusCode = 500;
				item.ResponseJson = ToJson(new Dictionary<string, object>
				{
					["success"] = false,
					["error"] = ex.Message
				});
			}
			finally
			{
				// Immediately refresh snapshots so next GET sees the change
				try
				{
					RefreshSnapshots();
				}
				catch {}
				try { item.Done.Set(); } catch {}
			}
		}
	}

	private static void ListenLoop()
	{
		while (IsRunning && _listener != null && _listener.IsListening)
		{
			try
			{
				HttpListenerContext context = _listener.GetContext();
				ThreadPool.QueueUserWorkItem(ProcessRequest, context);
			}
			catch (HttpListenerException)
			{
				break;
			}
			catch (ObjectDisposedException)
			{
				break;
			}
			catch (Exception ex)
			{
				if (!IsRunning) break;
				Log($"[DebugBridge] Listener exception: {ex.Message}");
			}
		}
	}

	private static void ProcessRequest(object state)
	{
		HttpListenerContext context = (HttpListenerContext)state;
		try
		{
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;

			// Add CORS headers for local browser/tool access
			response.Headers["Access-Control-Allow-Origin"] = "*";
			response.Headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS, HEAD";
			response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Accept";

			if (request.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
			{
				response.StatusCode = 204;
				response.Close();
				return;
			}

			string method = request.HttpMethod.ToUpperInvariant();
			string path = request.Url.AbsolutePath.TrimEnd('/').ToLowerInvariant();
			if (string.IsNullOrEmpty(path)) path = "/";

			switch (path)
			{
				case "/":
					if (method == "GET" || method == "HEAD")
					{
						HandleRoot(request, response);
					}
					else
					{
						SendResponse(request, response, 405, ToJson(new Dictionary<string, object>
						{
							["error"] = "Method Not Allowed. Use GET for /."
						}));
					}
					break;

				case "/status":
					if (method == "GET" || method == "HEAD")
					{
						HandleStatus(request, response);
					}
					else
					{
						SendResponse(request, response, 405, ToJson(new Dictionary<string, object>
						{
							["error"] = "Method Not Allowed. Use GET for /status."
						}));
					}
					break;

				case "/scouts":
					if (method == "GET" || method == "HEAD")
					{
						HandleScouts(request, response);
					}
					else
					{
						SendResponse(request, response, 405, ToJson(new Dictionary<string, object>
						{
							["error"] = "Method Not Allowed. Use GET for /scouts."
						}));
					}
					break;

				case "/inventory":
					if (method == "GET" || method == "HEAD")
					{
						HandleInventory(request, response);
					}
					else
					{
						SendResponse(request, response, 405, ToJson(new Dictionary<string, object>
						{
							["error"] = "Method Not Allowed. Use GET for /inventory."
						}));
					}
					break;

				case "/logs":
					if (method == "GET" || method == "HEAD")
					{
						HandleLogs(request, response);
					}
					else
					{
						SendResponse(request, response, 405, ToJson(new Dictionary<string, object>
						{
							["error"] = "Method Not Allowed. Use GET for /logs."
						}));
					}
					break;

				case "/action":
					if (method == "POST")
					{
						HandleAction(request, response);
					}
					else
					{
						SendResponse(request, response, 405, ToJson(new Dictionary<string, object>
						{
							["error"] = "Method Not Allowed. Use POST for /action."
						}));
					}
					break;

				case "/screenshot":
				case "/screenshot.png":
					if (method == "GET" || method == "HEAD")
					{
						HandleScreenshot(request, response);
					}
					else
					{
						SendResponse(request, response, 405, ToJson(new Dictionary<string, object>
						{
							["error"] = "Method Not Allowed. Use GET for /screenshot."
						}));
					}
					break;

				default:
					SendResponse(request, response, 404, ToJson(new Dictionary<string, object>
					{
						["error"] = "Endpoint not found: " + path
					}));
					break;
			}
		}
		catch (Exception ex)
		{
			try
			{
				SendResponse(context.Request, context.Response, 500, ToJson(new Dictionary<string, object>
				{
					["error"] = ex.Message
				}));
			}
			catch {}
		}
	}

	private static void HandleRoot(HttpListenerRequest request, HttpListenerResponse response)
	{
		var info = new Dictionary<string, object>
		{
			["name"] = "Ascend Live Debug Bridge",
			["version"] = Kit.Version,
			["role"] = ModNet.Role.ToString(),
			["port"] = ActivePort,
			["endpoints"] = new string[] { "/status", "/scouts", "/inventory", "/logs", "/screenshot", "/action" }
		};
		SendResponse(request, response, 200, ToJson(info));
	}

	private static void HandleStatus(HttpListenerRequest request, HttpListenerResponse response)
	{
		string json = _cachedStatusJson;
		if (string.IsNullOrEmpty(json))
		{
			json = BuildStatusJson();
		}
		SendResponse(request, response, 200, json);
	}

	private static void HandleScouts(HttpListenerRequest request, HttpListenerResponse response)
	{
		string json = _cachedScoutsJson;
		if (string.IsNullOrEmpty(json))
		{
			json = BuildScoutsJson();
		}
		SendResponse(request, response, 200, json);
	}

	private static void HandleInventory(HttpListenerRequest request, HttpListenerResponse response)
	{
		string json = _cachedInventoryJson;
		if (string.IsNullOrEmpty(json))
		{
			json = BuildInventoryJson();
		}
		SendResponse(request, response, 200, json);
	}

	private static void HandleLogs(HttpListenerRequest request, HttpListenerResponse response)
	{
		int count = 50;
		string countParam = request.QueryString["count"];
		if (!string.IsNullOrEmpty(countParam) && int.TryParse(countParam, out int parsedCount))
		{
			count = Math.Max(0, Math.Min(parsedCount, 1000));
		}

		List<string> lines = Logs.GetLast(count);
		SendResponse(request, response, 200, ToJson(lines));
	}

	private static void HandleScreenshot(HttpListenerRequest request, HttpListenerResponse response)
	{
		if (!IsRunning)
		{
			SendResponse(request, response, 503, ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["error"] = "DebugBridge server is stopped or shutting down."
			}));
			return;
		}

		var parameters = ParseRequestParams(request);
		ActionItem item = new ActionItem
		{
			Action = "screenshot",
			Params = parameters
		};

		_actionQueue.Enqueue(item);

		if (!IsRunning)
		{
			item.Cancelled = true;
			SendResponse(request, response, 503, ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["error"] = "DebugBridge server is stopped or shutting down."
			}));
			return;
		}

		if (!item.Done.Wait(TimeSpan.FromSeconds(5)))
		{
			item.Cancelled = true;
			SendResponse(request, response, 504, ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["error"] = "Screenshot capture timed out waiting for Unity main thread."
			}));
			return;
		}

		try { item.Done.Dispose(); } catch {}

		// Check if caller requested direct binary PNG output
		bool wantsBinary = request.Url.AbsolutePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
		                   (request.AcceptTypes != null && Array.Exists(request.AcceptTypes, a => a.IndexOf("image/", StringComparison.OrdinalIgnoreCase) >= 0)) ||
		                   (parameters.TryGetValue("format", out string fmt) && fmt.Equals("raw", StringComparison.OrdinalIgnoreCase));

		if (wantsBinary && parameters.TryGetValue("path", out string filePath) && File.Exists(filePath))
		{
			try
			{
				byte[] imgBytes = File.ReadAllBytes(filePath);
				SendBinaryResponse(request, response, 200, "image/png", imgBytes);
				return;
			}
			catch {}
		}

		SendResponse(request, response, item.StatusCode, item.ResponseJson);
	}

	private static void HandleAction(HttpListenerRequest request, HttpListenerResponse response)
	{
		if (!IsRunning)
		{
			SendResponse(request, response, 503, ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["error"] = "DebugBridge server is stopped or shutting down."
			}));
			return;
		}

		var parameters = ParseRequestParams(request);

		if (!parameters.TryGetValue("action", out string actionName) || string.IsNullOrEmpty(actionName))
		{
			SendResponse(request, response, 400, ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["error"] = "Missing required 'action' parameter."
			}));
			return;
		}

		ActionItem item = new ActionItem
		{
			Action = actionName,
			Params = parameters
		};

		_actionQueue.Enqueue(item);

		// If server stopped right after enqueuing
		if (!IsRunning)
		{
			item.Cancelled = true;
			SendResponse(request, response, 503, ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["error"] = "DebugBridge server is stopped or shutting down."
			}));
			return;
		}

		// Wait synchronously on background thread for main thread execution
		if (!item.Done.Wait(TimeSpan.FromSeconds(5)))
		{
			item.Cancelled = true;
			SendResponse(request, response, 504, ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["error"] = "Execution timed out waiting for Unity main thread."
			}));
			return;
		}

		try { item.Done.Dispose(); } catch {}
		SendResponse(request, response, item.StatusCode, item.ResponseJson);
	}

	private static void SendResponse(HttpListenerRequest request, HttpListenerResponse response, int statusCode, string json)
	{
		try
		{
			byte[] buffer = Encoding.UTF8.GetBytes(json ?? "{}");
			response.StatusCode = statusCode;
			response.ContentType = "application/json; charset=utf-8";
			response.ContentLength64 = buffer.Length;
			if (request == null || !request.HttpMethod.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
			{
				using (var stream = response.OutputStream)
				{
					stream.Write(buffer, 0, buffer.Length);
					stream.Flush();
				}
			}
		}
		catch {}
		finally
		{
			try { response.Close(); } catch {}
		}
	}

	private static void SendBinaryResponse(HttpListenerRequest request, HttpListenerResponse response, int statusCode, string contentType, byte[] buffer)
	{
		try
		{
			response.StatusCode = statusCode;
			response.ContentType = contentType;
			response.ContentLength64 = buffer != null ? buffer.Length : 0;
			if (request == null || !request.HttpMethod.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
			{
				if (buffer != null && buffer.Length > 0)
				{
					using (var stream = response.OutputStream)
					{
						stream.Write(buffer, 0, buffer.Length);
						stream.Flush();
					}
				}
			}
		}
		catch {}
		finally
		{
			try { response.Close(); } catch {}
		}
	}

	private static string SafeUnescapeDataString(string s)
	{
		if (string.IsNullOrEmpty(s)) return "";
		try
		{
			return Uri.UnescapeDataString(s);
		}
		catch
		{
			try
			{
				return Regex.Replace(s, "%([0-9A-Fa-f]{2})", m =>
				{
					try
					{
						int code = int.Parse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
						return ((char)code).ToString();
					}
					catch
					{
						return m.Value;
					}
				});
			}
			catch
			{
				return s;
			}
		}
	}

	private static Dictionary<string, string> ParseRequestParams(HttpListenerRequest request)
	{
		var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		// 1. URL Query string parameters
		if (request.QueryString != null)
		{
			foreach (string key in request.QueryString.AllKeys)
			{
				if (!string.IsNullOrEmpty(key))
				{
					dict[key] = request.QueryString[key];
				}
			}
		}

		// 2. Body parameters if POST
		if (request.HasEntityBody)
		{
			try
			{
				using (var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8))
				{
					string body = reader.ReadToEnd().Trim().Trim('\uFEFF', '\u200B');
					if (!string.IsNullOrEmpty(body))
					{
						if (body.StartsWith("{") && body.EndsWith("}"))
						{
							ParseJsonBody(body, dict);
						}
						else
						{
							ParseUrlEncodedBody(body, dict);
						}
					}
				}
			}
			catch {}
		}

		return dict;
	}

	private static void ParseUrlEncodedBody(string body, Dictionary<string, string> dict)
	{
		string[] pairs = body.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string pair in pairs)
		{
			int eq = pair.IndexOf('=');
			if (eq >= 0)
			{
				string key = SafeUnescapeDataString(pair.Substring(0, eq).Replace('+', ' ')).Trim();
				string val = SafeUnescapeDataString(pair.Substring(eq + 1).Replace('+', ' '));
				if (!string.IsNullOrEmpty(key))
				{
					dict[key] = val;
				}
			}
			else
			{
				string key = SafeUnescapeDataString(pair.Replace('+', ' ')).Trim();
				if (!string.IsNullOrEmpty(key))
				{
					dict[key] = "";
				}
			}
		}
	}

	private static void ParseJsonBody(string json, Dictionary<string, string> dict)
	{
		if (string.IsNullOrEmpty(json)) return;

		int i = 0;
		int len = json.Length;

		while (i < len)
		{
			while (i < len && (char.IsWhiteSpace(json[i]) || json[i] == '{' || json[i] == ',' || json[i] == '}'))
			{
				i++;
			}
			if (i >= len) break;

			if (json[i] != '"')
			{
				i++;
				continue;
			}

			string key = ReadJsonString(json, ref i);
			if (key == null) break;

			while (i < len && (char.IsWhiteSpace(json[i]) || json[i] == ':'))
			{
				i++;
			}
			if (i >= len) break;

			if (json[i] == '{')
			{
				int objStart = i;
				int braceDepth = 0;
				while (i < len)
				{
					if (json[i] == '{') braceDepth++;
					else if (json[i] == '}')
					{
						braceDepth--;
						if (braceDepth == 0)
						{
							i++;
							break;
						}
					}
					else if (json[i] == '"')
					{
						ReadJsonString(json, ref i);
						continue;
					}
					i++;
				}
				string subJson = json.Substring(objStart, i - objStart);
				ParseJsonBody(subJson, dict);
				continue;
			}
			else if (json[i] == '[')
			{
				int arrStart = i;
				int bracketDepth = 0;
				while (i < len)
				{
					if (json[i] == '[') bracketDepth++;
					else if (json[i] == ']')
					{
						bracketDepth--;
						if (bracketDepth == 0)
						{
							i++;
							break;
						}
					}
					else if (json[i] == '"')
					{
						ReadJsonString(json, ref i);
						continue;
					}
					i++;
				}
				dict[key] = json.Substring(arrStart, i - arrStart);
				continue;
			}

			string val = null;
			if (json[i] == '"')
			{
				val = ReadJsonString(json, ref i);
			}
			else
			{
				int start = i;
				while (i < len && json[i] != ',' && json[i] != '}' && !char.IsWhiteSpace(json[i]))
				{
					i++;
				}
				val = json.Substring(start, i - start).Trim();
			}

			if (key != null && val != null)
			{
				dict[key] = val;
			}
		}
	}

	private static string ReadJsonString(string s, ref int i)
	{
		if (i >= s.Length || s[i] != '"') return null;
		i++;
		var sb = new StringBuilder();
		while (i < s.Length)
		{
			char c = s[i++];
			if (c == '"')
			{
				return sb.ToString();
			}
			if (c == '\\' && i < s.Length)
			{
				char esc = s[i++];
				switch (esc)
				{
					case '"': sb.Append('"'); break;
					case '\\': sb.Append('\\'); break;
					case '/': sb.Append('/'); break;
					case 'b': sb.Append('\b'); break;
					case 'f': sb.Append('\f'); break;
					case 'n': sb.Append('\n'); break;
					case 'r': sb.Append('\r'); break;
					case 't': sb.Append('\t'); break;
					case 'u':
						if (i + 4 <= s.Length && int.TryParse(s.Substring(i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int codePoint))
						{
							sb.Append((char)codePoint);
							i += 4;
						}
						else
						{
							sb.Append("\\u");
						}
						break;
					default:
						sb.Append(esc);
						break;
				}
			}
			else
			{
				sb.Append(c);
			}
		}
		return sb.ToString();
	}

	public static void RefreshSnapshots()
	{
		_cachedStatusJson = BuildStatusJson();
		_cachedScoutsJson = BuildScoutsJson();
		_cachedInventoryJson = BuildInventoryJson();
	}

	private static bool SafeInRoom()
	{
		try
		{
			return PhotonNetwork.NetworkingClient != null && PhotonNetwork.InRoom;
		}
		catch
		{
			return false;
		}
	}

	private static int SafeLocalActorNumber()
	{
		try
		{
			if (PhotonNetwork.NetworkingClient != null && PhotonNetwork.LocalPlayer != null)
			{
				return PhotonNetwork.LocalPlayer.ActorNumber;
			}
		}
		catch {}
		return 1;
	}

	private static string SafeLocalNickName()
	{
		try
		{
			if (PhotonNetwork.NetworkingClient != null && PhotonNetwork.LocalPlayer != null && !string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.NickName))
			{
				return PhotonNetwork.LocalPlayer.NickName;
			}
		}
		catch {}
		return "LocalScout";
	}

	private static int SafePing()
	{
		try
		{
			if (PhotonNetwork.NetworkingClient != null)
			{
				return PhotonNetwork.GetPing();
			}
		}
		catch {}
		return 0;
	}

	private static string GetCharacterName(Character c, string fallback = "Scout")
	{
		if ((Object)(object)c == (Object)null) return fallback;
		try
		{
			PhotonView pv = c.GetComponent<PhotonView>();
			if (pv != null && pv.Owner != null && !string.IsNullOrEmpty(pv.Owner.NickName))
			{
				return pv.Owner.NickName;
			}
		}
		catch {}

		try
		{
			if (c.IsLocal)
			{
				return SafeLocalNickName();
			}
		}
		catch {}

		try
		{
			if (c.gameObject != null && !string.IsNullOrEmpty(c.gameObject.name))
			{
				return c.gameObject.name;
			}
		}
		catch {}

		return fallback;
	}

	public static string BuildStatusJson()
	{
		try
		{
			string sceneName = SafeSceneName();
			bool inGame = SafeInGame();
			bool inRoom = SafeInRoom();

			string role = "Master";
			try { role = ModNet.Role.ToString(); } catch {}

			float posX = 0f, posY = 0f, posZ = 0f;
			float currentStamina = 0f;
			float extraStamina = 0f;
			bool isDead = false;
			bool isPassedOut = false;
			string scoutName = "Scout";

			try
			{
				Character me = SafeLocalCharacter();
				if (inGame && (Object)(object)me != (Object)null)
				{
					Vector3 pos = me.transform.position;
					posX = pos.x; posY = pos.y; posZ = pos.z;

					if (me.data != null)
					{
						currentStamina = me.data.currentStamina;
						extraStamina = me.data.extraStamina;
						isDead = me.data.dead;
						isPassedOut = me.data.fullyPassedOut;
					}

					scoutName = GetCharacterName(me, "LocalScout");
				}
			}
			catch {}

			var cheats = new Dictionary<string, object>
			{
				["god"] = State.God,
				["infStamina"] = State.InfStamina,
				["fly"] = State.Fly,
				["noFall"] = State.NoFall,
				["noClip"] = State.NoClip,
				["noWeight"] = State.NoWeight,
				["airJump"] = State.AirJump,
				["speed"] = State.Speed,
				["jump"] = State.Jump,
				["climb"] = State.Climb,
				["flySpeed"] = State.FlySpeed
			};

			var coords = new Dictionary<string, object>
			{
				["x"] = posX,
				["y"] = posY,
				["z"] = posZ
			};

			double healthVal = isDead ? 0.0 : (isPassedOut ? 20.0 : 100.0);

			var status = new Dictionary<string, object>
			{
				["scene"] = sceneName,
				["Scene"] = sceneName,
				["inGame"] = inGame,
				["InGame"] = inGame,
				["inRoom"] = inRoom,
				["InRoom"] = inRoom,
				["role"] = role,
				["Role"] = role,
				["coords"] = coords,
				["position"] = coords,
				["x"] = posX,
				["y"] = posY,
				["z"] = posZ,
				["health"] = healthVal,
				["Health"] = healthVal,
				["stamina"] = currentStamina,
				["Stamina"] = currentStamina,
				["extraStamina"] = extraStamina,
				["player"] = new Dictionary<string, object>
				{
					["name"] = scoutName,
					["actorNumber"] = SafeLocalActorNumber(),
					["health"] = healthVal,
					["stamina"] = currentStamina,
					["extraStamina"] = extraStamina,
					["isDead"] = isDead,
					["isPassedOut"] = isPassedOut,
					["position"] = coords
				},
				["cheats"] = cheats,
				["god"] = State.God,
				["infStamina"] = State.InfStamina,
				["fly"] = State.Fly,
				["noFall"] = State.NoFall
			};

			return ToJson(status);
		}
		catch (Exception ex)
		{
			Log($"[DebugBridge] BuildStatusJson fallback: {ex.Message}");
			var fallback = new Dictionary<string, object>
			{
				["scene"] = "None",
				["Scene"] = "None",
				["inGame"] = false,
				["InGame"] = false,
				["inRoom"] = false,
				["InRoom"] = false,
				["role"] = "Master",
				["Role"] = "Master",
				["coords"] = new Dictionary<string, object> { ["x"] = 0f, ["y"] = 0f, ["z"] = 0f },
				["position"] = new Dictionary<string, object> { ["x"] = 0f, ["y"] = 0f, ["z"] = 0f },
				["x"] = 0f,
				["y"] = 0f,
				["z"] = 0f,
				["health"] = 100.0,
				["Health"] = 100.0,
				["stamina"] = 0f,
				["Stamina"] = 0f,
				["extraStamina"] = 0f,
				["player"] = new Dictionary<string, object>
				{
					["name"] = "Scout",
					["actorNumber"] = 1,
					["health"] = 100.0,
					["stamina"] = 0f,
					["extraStamina"] = 0f,
					["isDead"] = false,
					["isPassedOut"] = false,
					["position"] = new Dictionary<string, object> { ["x"] = 0f, ["y"] = 0f, ["z"] = 0f }
				},
				["cheats"] = new Dictionary<string, object>
				{
					["god"] = State.God,
					["infStamina"] = State.InfStamina,
					["fly"] = State.Fly,
					["noFall"] = State.NoFall,
					["noClip"] = State.NoClip,
					["noWeight"] = State.NoWeight,
					["airJump"] = State.AirJump,
					["speed"] = State.Speed,
					["jump"] = State.Jump,
					["climb"] = State.Climb,
					["flySpeed"] = State.FlySpeed
				},
				["god"] = State.God,
				["infStamina"] = State.InfStamina,
				["fly"] = State.Fly,
				["noFall"] = State.NoFall
			};
			return ToJson(fallback);
		}
	}

	public static string BuildScoutsJson()
	{
		var list = new List<object>();

		try
		{
			if (SafeInRoom() && PhotonNetwork.PlayerList != null)
			{
				foreach (PhotonPlayer p in PhotonNetwork.PlayerList)
				{
					if (p == null) continue;

					float x = 0f, y = 0f, z = 0f;
					Character targetChar = null;
					try
					{
						if (p.IsLocal)
						{
							targetChar = SafeLocalCharacter();
						}
						else
						{
							PlayerHandler.TryGetCharacter(p.ActorNumber, out targetChar);
						}

						if ((Object)(object)targetChar != (Object)null)
						{
							Vector3 pos = targetChar.transform.position;
							x = pos.x; y = pos.y; z = pos.z;
						}
					}
					catch {}

					bool isMaster = p.IsMasterClient || (p.IsLocal && ModNet.Role == ModNet.ModRole.Master);
					bool isSlave = (p.IsLocal && ModNet.Role == ModNet.ModRole.Slave) || ModNet.IsSlave(p.ActorNumber);

					var posDict = new Dictionary<string, object> { ["x"] = x, ["y"] = y, ["z"] = z };
					var scout = new Dictionary<string, object>
					{
						["ActorNumber"] = p.ActorNumber,
						["actorNumber"] = p.ActorNumber,
						["NickName"] = p.NickName ?? "",
						["nickName"] = p.NickName ?? "",
						["ping"] = SafePing(),
						["isMaster"] = isMaster,
						["isSlave"] = isSlave,
						["position"] = posDict,
						["coords"] = posDict,
						["x"] = x,
						["y"] = y,
						["z"] = z
					};
					list.Add(scout);
				}
			}
			else if (SafeInGame())
			{
				Character me = SafeLocalCharacter();
				if ((Object)(object)me != (Object)null)
				{
					Vector3 pos = me.transform.position;
					var posDict = new Dictionary<string, object> { ["x"] = pos.x, ["y"] = pos.y, ["z"] = pos.z };
					string scoutName = GetCharacterName(me, "LocalScout");
					bool isMaster = (ModNet.Role == ModNet.ModRole.Master);
					bool isSlave = (ModNet.Role == ModNet.ModRole.Slave);

					var scout = new Dictionary<string, object>
					{
						["ActorNumber"] = 1,
						["actorNumber"] = 1,
						["NickName"] = scoutName,
						["nickName"] = scoutName,
						["ping"] = 0,
						["isMaster"] = isMaster,
						["isSlave"] = isSlave,
						["position"] = posDict,
						["coords"] = posDict,
						["x"] = pos.x,
						["y"] = pos.y,
						["z"] = pos.z
					};
					list.Add(scout);
				}
			}
		}
		catch {}

		return ToJson(list);
	}

	public static string BuildInventoryJson()
	{
		var list = new List<object>();

		try
		{
			if (SafeInRoom() && PhotonNetwork.PlayerList != null)
			{
				foreach (PhotonPlayer p in PhotonNetwork.PlayerList)
				{
					if (p == null) continue;

					Character c = null;
					if (p.IsLocal)
					{
						c = SafeLocalCharacter();
					}
					else
					{
						PlayerHandler.TryGetCharacter(p.ActorNumber, out c);
					}

					string heldItem = "None";
					List<string> pockets = new List<string>();
					List<string> backpack = new List<string>();

					if ((Object)(object)c != (Object)null)
					{
						ModNet.GetPlayerInventory(c, out heldItem, out pockets, out backpack);
					}

					// Ensure exactly 3 pocket slots (0-2)
					while (pockets.Count < 3)
					{
						pockets.Add("Empty");
					}

					var itemDict = new Dictionary<string, object>
					{
						["ActorNumber"] = p.ActorNumber,
						["actorNumber"] = p.ActorNumber,
						["NickName"] = p.NickName ?? "",
						["nickName"] = p.NickName ?? "",
						["heldItem"] = heldItem,
						["currentItem"] = heldItem,
						["pockets"] = pockets,
						["pocket_0"] = pockets.Count > 0 ? pockets[0] : "Empty",
						["pocket_1"] = pockets.Count > 1 ? pockets[1] : "Empty",
						["pocket_2"] = pockets.Count > 2 ? pockets[2] : "Empty",
						["backpack"] = backpack
					};
					list.Add(itemDict);
				}
			}
			else if (SafeInGame())
			{
				Character me = SafeLocalCharacter();
				if ((Object)(object)me != (Object)null)
				{
					string heldItem = "None";
					List<string> pockets = new List<string>();
					List<string> backpack = new List<string>();

					ModNet.GetPlayerInventory(me, out heldItem, out pockets, out backpack);
					while (pockets.Count < 3)
					{
						pockets.Add("Empty");
					}

					string scoutName = GetCharacterName(me, "LocalScout");
					var itemDict = new Dictionary<string, object>
					{
						["ActorNumber"] = 1,
						["actorNumber"] = 1,
						["NickName"] = scoutName,
						["nickName"] = scoutName,
						["heldItem"] = heldItem,
						["currentItem"] = heldItem,
						["pockets"] = pockets,
						["pocket_0"] = pockets.Count > 0 ? pockets[0] : "Empty",
						["pocket_1"] = pockets.Count > 1 ? pockets[1] : "Empty",
						["pocket_2"] = pockets.Count > 2 ? pockets[2] : "Empty",
						["backpack"] = backpack
					};
					list.Add(itemDict);
				}
			}
		}
		catch {}

		return ToJson(list);
	}

	public static string ExecuteActionOnMainThread(string action, Dictionary<string, string> p)
	{
		if (string.IsNullOrEmpty(action))
		{
			throw new ArgumentException("Action name cannot be empty.");
		}

		Log($"[Action] Executing action '{action}'");

		switch (action.ToLowerInvariant())
		{
			case "say":
				return ActionSay(p);

			case "spawn":
				return ActionSpawn(p);

			case "warp":
				return ActionWarp(p);

			case "warp_to":
				return ActionWarpTo(p);

			case "toggle":
				return ActionToggle(p);

			case "perm":
				return ActionPerm(p);

			case "deliver":
				return ActionDeliver(p);

			case "screenshot":
				return ActionScreenshot(p);

			case "join_slave":
				return ActionJoinSlave(p);

			case "join_master":
				return ActionJoinMaster(p);

			case "kill":
			case "kill_self":
				return ActionKillSelf(p);

			case "heal":
				return ActionHeal(p);

			case "bonus_stamina":
				return ActionBonusStamina(p);

			case "revive":
				return ActionRevive(p);

			case "warp_campfire":
				return ActionWarpCampfire(p);

			case "warp_statue":
				return ActionWarpStatue(p);

			case "warp_aim":
				return ActionWarpAim(p);

			case "save_spot":
				return ActionSaveSpot(p);

			case "go_spot":
				return ActionGoSpot(p);

			case "freecam":
				return ActionFreeCam(p);

			case "set_time":
				return ActionSetTime(p);

			case "next_section":
				return ActionNextSection(p);

			case "trip":
				return ActionTrip(p);

			case "launch":
				return ActionLaunch(p);

			case "make_jump":
				return ActionMakeJump(p);

			case "bring":
				return ActionBring(p);

			case "swap":
				return ActionSwap(p);

			case "drop_on":
				return ActionDropOn(p);

			case "heal_other":
				return ActionHealOther(p);

			case "revive_other":
				return ActionReviveOther(p);

			case "hand_item":
				return ActionHandItem(p);

			default:
				throw new ArgumentException($"Unknown action '{action}'. Valid actions: say, spawn, warp, warp_to, toggle, perm, deliver, screenshot, join_slave, join_master, kill_self, heal, bonus_stamina, revive, warp_campfire, warp_statue, warp_aim, save_spot, go_spot, freecam, set_time, next_section, trip, launch, make_jump, bring, swap, drop_on, heal_other, revive_other, hand_item.");
		}
	}

	private static string ActionJoinSlave(Dictionary<string, string> p)
	{
		bool ok = ModNet.JoinSlaveRoom();
		return ToJson(new Dictionary<string, object>
		{
			{ "action", "join_slave" },
			{ "success", ok }
		});
	}

	private static string ActionJoinMaster(Dictionary<string, string> p)
	{
		bool ok = ModNet.JoinMasterRoom();
		return ToJson(new Dictionary<string, object>
		{
			{ "action", "join_master" },
			{ "success", ok }
		});
	}

	private static string ActionSay(Dictionary<string, string> p)
	{
		if (!p.TryGetValue("msg", out string msg) && !p.TryGetValue("message", out msg) && !p.TryGetValue("text", out msg))
		{
			throw new ArgumentException("Missing required 'msg' parameter.");
		}
		if (string.IsNullOrWhiteSpace(msg))
		{
			throw new ArgumentException("Missing required 'msg' parameter.");
		}

		SafeMenuSay(msg);

		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "say",
			["message"] = msg,
			["msg"] = msg
		});
	}

	private static bool? ParseFlexibleBool(string s)
	{
		if (string.IsNullOrEmpty(s)) return null;
		string v = s.Trim().ToLowerInvariant();
		if (v == "true" || v == "1" || v == "yes" || v == "on") return true;
		if (v == "false" || v == "0" || v == "no" || v == "off") return false;
		return null;
	}

	private static string ActionSpawn(Dictionary<string, string> p)
	{
		if (!p.TryGetValue("item", out string itemName) && !p.TryGetValue("name", out itemName) && !p.TryGetValue("itemName", out itemName))
		{
			throw new ArgumentException("Missing required 'item' parameter.");
		}
		if (string.IsNullOrEmpty(itemName))
		{
			throw new ArgumentException("Missing required 'item' parameter.");
		}

		if (!SafeInGame() || (Object)(object)SafeLocalCharacter() == (Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "spawn",
				["error"] = "Cannot spawn item: local player is not in-game or character reference is null."
			});
		}

		var items = SafeItemList();
		if (items == null || items.Count == 0)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "spawn",
				["error"] = "ItemDatabase is empty or not loaded yet."
			});
		}

		Item target = null;
		// 1. Exact match on prefab name
		target = items.Find(x => (Object)(object)x != (Object)null && string.Equals(((Object)x).name, itemName, StringComparison.OrdinalIgnoreCase));
		// 2. Exact match on SafeGetItemName
		if ((Object)(object)target == (Object)null)
		{
			target = items.Find(x => (Object)(object)x != (Object)null && string.Equals(SafeGetItemName(x), itemName, StringComparison.OrdinalIgnoreCase));
		}
		// 3. Substring match on prefab name
		if ((Object)(object)target == (Object)null)
		{
			target = items.Find(x => (Object)(object)x != (Object)null && ((Object)x).name.IndexOf(itemName, StringComparison.OrdinalIgnoreCase) >= 0);
		}
		// 4. Substring match on SafeGetItemName
		if ((Object)(object)target == (Object)null)
		{
			target = items.Find(x => (Object)(object)x != (Object)null && SafeGetItemName(x).IndexOf(itemName, StringComparison.OrdinalIgnoreCase) >= 0);
		}

		if ((Object)(object)target == (Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "spawn",
				["error"] = $"Item '{itemName}' not found in ItemDatabase."
			});
		}

		int spawnCount = 1;
		if ((p.TryGetValue("count", out string countStr) || p.TryGetValue("amount", out countStr) || p.TryGetValue("qty", out countStr)) &&
		    int.TryParse(countStr, out int cVal) && cVal > 1)
		{
			spawnCount = Math.Min(cVal, 50);
		}

		bool added = false;
		for (int i = 0; i < spawnCount; i++)
		{
			if (SafeItemAdd(target))
			{
				added = true;
			}
		}

		if (!added)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "spawn",
				["error"] = $"Failed to add item '{itemName}' to player inventory."
			});
		}

		string displayName = ((Object)target).name;
		try { displayName = SafeGetItemName(target); } catch {}
		SafeMenuSay(Lang.F("Spawned {0}", displayName));

		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "spawn",
			["item"] = displayName,
			["prefab"] = ((Object)target).name,
			["count"] = spawnCount
		});
	}

	private static string ActionWarp(Dictionary<string, string> p)
	{
		float x = 0f, y = 0f, z = 0f;
		bool coordsFound = false;

		if (p.TryGetValue("x", out string xs) && float.TryParse(xs, NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
		    p.TryGetValue("y", out string ys) && float.TryParse(ys, NumberStyles.Float, CultureInfo.InvariantCulture, out y) &&
		    p.TryGetValue("z", out string zs) && float.TryParse(zs, NumberStyles.Float, CultureInfo.InvariantCulture, out z))
		{
			coordsFound = true;
		}
		else if (p.TryGetValue("coords", out string coordsStr) || p.TryGetValue("pos", out coordsStr) || p.TryGetValue("position", out coordsStr))
		{
			string[] parts = coordsStr.Split(new char[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length >= 3 &&
			    float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
			    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y) &&
			    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
			{
				coordsFound = true;
			}
		}

		if (!coordsFound)
		{
			throw new ArgumentException("Warp requires valid float 'x', 'y', and 'z' coordinates.");
		}

		if (float.IsNaN(x) || float.IsInfinity(x) || float.IsNaN(y) || float.IsInfinity(y) || float.IsNaN(z) || float.IsInfinity(z))
		{
			throw new ArgumentException("Warp coordinates cannot be NaN or Infinity.");
		}

		if (!SafeInGame() || (Object)(object)SafeLocalCharacter() == (Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "warp",
				["error"] = "Cannot warp: local player is not in-game or character reference is null."
			});
		}

		Vector3 destination = new Vector3(x, y, z);
		bool warped = SafeWarp(destination, string.Format(CultureInfo.InvariantCulture, "{0:F1}, {1:F1}, {2:F1}", x, y, z));
		if (!warped)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "warp",
				["error"] = "Warp RPC execution failed."
			});
		}

		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "warp",
			["position"] = new Dictionary<string, object> { ["x"] = x, ["y"] = y, ["z"] = z }
		});
	}

	private static string ActionWarpTo(Dictionary<string, string> p)
	{
		if (!p.TryGetValue("actor", out string actorStr) && !p.TryGetValue("target", out actorStr) && !p.TryGetValue("scout", out actorStr))
		{
			throw new ArgumentException("Warp_to requires valid 'actor' parameter.");
		}
		if (string.IsNullOrEmpty(actorStr))
		{
			throw new ArgumentException("Warp_to requires valid 'actor' parameter.");
		}

		int targetActor = 0;
		if (int.TryParse(actorStr, out int actNum))
		{
			targetActor = actNum;
		}
		else
		{
			bool found = false;
			try
			{
				if (SafeInRoom() && PhotonNetwork.PlayerList != null)
				{
					foreach (var pl in PhotonNetwork.PlayerList)
					{
						if (pl != null && string.Equals(pl.NickName, actorStr, StringComparison.OrdinalIgnoreCase))
						{
							targetActor = pl.ActorNumber;
							found = true;
							break;
						}
					}
				}
			}
			catch {}

			if (!found)
			{
				throw new ArgumentException($"Invalid actor '{actorStr}'. Expected integer actor number or player nickname.");
			}
		}

		if (!SafeInGame() || (Object)(object)SafeLocalCharacter() == (Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "warp_to",
				["error"] = "Cannot warp: local player is not in-game or character reference is null."
			});
		}

		Character targetChar = null;
		if (targetActor == SafeLocalActorNumber())
		{
			targetChar = SafeLocalCharacter();
		}
		else if (PlayerHandler.TryGetCharacter(targetActor, out targetChar) && (Object)(object)targetChar != (Object)null)
		{
		}
		else
		{
			try
			{
				foreach (Character c in Character.AllCharacters)
				{
					if ((Object)(object)c != (Object)null && c.photonView != null && c.photonView.OwnerActorNr == targetActor)
					{
						targetChar = c;
						break;
					}
				}
			}
			catch {}
		}

		if ((Object)(object)targetChar == (Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "warp_to",
				["error"] = $"Scout with actor number {targetActor} not found in room."
			});
		}

		Vector3 targetPos = targetChar.Center + targetChar.transform.forward * 1.0f + Vector3.up * 0.5f;
		string scoutName = GetCharacterName(targetChar, $"Actor {targetActor}");
		bool warped = SafeWarp(targetPos, scoutName);
		if (!warped)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "warp_to",
				["error"] = "Warp RPC execution failed."
			});
		}

		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "warp_to",
			["actor"] = targetActor,
			["scout"] = scoutName,
			["targetPosition"] = new Dictionary<string, object>
			{
				["x"] = targetPos.x,
				["y"] = targetPos.y,
				["z"] = targetPos.z
			}
		});
	}

	private static string ActionToggle(Dictionary<string, string> p)
	{
		if (!p.TryGetValue("cheat", out string cheat) || string.IsNullOrEmpty(cheat))
		{
			throw new ArgumentException("Missing required 'cheat' parameter. Options: God, InfStamina, Fly, NoFall, NoClip, NoWeight.");
		}

		string normCheat = cheat.Replace("_", "").Replace("-", "").Replace(" ", "").Trim().ToLowerInvariant();
		string valStr = null;
		p.TryGetValue("value", out valStr);
		if (string.IsNullOrEmpty(valStr)) p.TryGetValue("allow", out valStr);
		if (string.IsNullOrEmpty(valStr)) p.TryGetValue("enable", out valStr);
		if (string.IsNullOrEmpty(valStr)) p.TryGetValue("enabled", out valStr);
		if (string.IsNullOrEmpty(valStr)) p.TryGetValue("state", out valStr);

		// Check float multiplier fields first (Speed, Jump, Climb, FlySpeed, Reach, RopeLength, CamDistance, Fov)
		FieldInfo floatField = null;
		foreach (FieldInfo f in typeof(State).GetFields(BindingFlags.Public | BindingFlags.Static))
		{
			if (f.FieldType == typeof(float) &&
				(string.Equals(f.Name, cheat, StringComparison.OrdinalIgnoreCase) ||
				 string.Equals(f.Name.Replace("_", ""), normCheat, StringComparison.OrdinalIgnoreCase)))
			{
				floatField = f;
				break;
			}
		}
		if (floatField != null)
		{
			if (!string.IsNullOrEmpty(valStr) && float.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float fVal))
			{
				if (string.Equals(floatField.Name, "FogHeightOffset", StringComparison.OrdinalIgnoreCase))
				{
					Features.AdjustFogHeight(fVal);
				}
				else if (string.Equals(floatField.Name, "LavaHeightOffset", StringComparison.OrdinalIgnoreCase))
				{
					Features.AdjustLavaHeight(fVal);
				}
				else
				{
					floatField.SetValue(null, fVal);
				}
				SafeMenuSay(Lang.F("{0}: {1:0.0}", cheat, fVal));
				return ToJson(new Dictionary<string, object>
				{
					["success"] = true,
					["action"] = "toggle",
					["cheat"] = cheat,
					["value"] = fVal
				});
			}
			else
			{
				float curVal = (float)floatField.GetValue(null);
				return ToJson(new Dictionary<string, object>
				{
					["success"] = true,
					["action"] = "toggle",
					["cheat"] = cheat,
					["value"] = curVal
				});
			}
		}

		bool? explicitValue = null;
		if (!string.IsNullOrEmpty(valStr))
		{
			explicitValue = ParseFlexibleBool(valStr);
			if (!explicitValue.HasValue)
			{
				throw new ArgumentException($"Invalid boolean value '{valStr}'. Expected true/false, 1/0, yes/no, on/off.");
			}
		}

		bool newState = false;
		switch (normCheat)
		{
			case "god":
				State.God = explicitValue ?? !State.God;
				newState = State.God;
				SafeMenuSay(Lang.F("God mode: {0}", newState ? "on" : "off"));
				break;

			case "infstamina":
			case "infinitestamina":
			case "stamina":
				State.InfStamina = explicitValue ?? !State.InfStamina;
				newState = State.InfStamina;
				SafeMenuSay(Lang.F("Infinite stamina: {0}", newState ? "on" : "off"));
				break;

			case "fly":
				State.Fly = explicitValue ?? !State.Fly;
				newState = State.Fly;
				try
				{
					Character me = SafeLocalCharacter();
					if (newState && SafeInGame() && (Object)(object)me != (Object)null && me.refs != null && (Object)(object)me.refs.climbing != (Object)null)
					{
						me.refs.climbing.StopAnyClimbing();
					}
				}
				catch {}
				SafeMenuSay(Lang.F("Fly: {0}", newState ? "on" : "off"));
				break;

			case "nofall":
				State.NoFall = explicitValue ?? !State.NoFall;
				newState = State.NoFall;
				SafeMenuSay(Lang.F("No fall damage: {0}", newState ? "on" : "off"));
				break;

			case "noclip":
				State.NoClip = explicitValue ?? !State.NoClip;
				newState = State.NoClip;
				SafeMenuSay(Lang.F("NoClip: {0}", newState ? "on" : "off"));
				break;

			case "noweight":
				State.NoWeight = explicitValue ?? !State.NoWeight;
				newState = State.NoWeight;
				SafeMenuSay(Lang.F("No weight: {0}", newState ? "on" : "off"));
				break;

			default:
				// Fallback to reflection on State class fields matching name with or without underscores
				FieldInfo field = null;
				foreach (FieldInfo f in typeof(State).GetFields(BindingFlags.Public | BindingFlags.Static))
				{
					if (f.FieldType == typeof(bool) &&
						(string.Equals(f.Name, cheat, StringComparison.OrdinalIgnoreCase) ||
						 string.Equals(f.Name.Replace("_", ""), normCheat, StringComparison.OrdinalIgnoreCase)))
					{
						field = f;
						break;
					}
				}
				if (field != null)
				{
					bool cur = (bool)field.GetValue(null);
					field.SetValue(null, explicitValue ?? !cur);
					newState = (bool)field.GetValue(null);
					SafeMenuSay(Lang.F("{0}: {1}", cheat, newState ? "on" : "off"));
				}
				else
				{
					throw new ArgumentException($"Unknown cheat '{cheat}'. Supported: God, InfStamina, Fly, NoFall, NoClip, NoWeight, AirJump, Speed, Jump, Climb, FlySpeed, etc.");
				}
				break;
		}

		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "toggle",
			["cheat"] = cheat,
			["enabled"] = newState,
			["value"] = newState,
			["state"] = newState
		});
	}

	private static string ActionPerm(Dictionary<string, string> p)
	{
		bool clearOverride = false;
		if (p.TryGetValue("allow", out string allowStr) || p.TryGetValue("value", out allowStr) || p.TryGetValue("grant", out allowStr))
		{
			if (allowStr.Equals("clear", StringComparison.OrdinalIgnoreCase) || allowStr.Equals("reset", StringComparison.OrdinalIgnoreCase))
			{
				clearOverride = true;
			}
		}

		if (!p.TryGetValue("section", out string section) && !p.TryGetValue("sec", out section))
		{
			if (!clearOverride)
			{
				throw new ArgumentException("Missing required 'section' parameter.");
			}
			section = "All";
		}
		if (string.IsNullOrEmpty(section) && !clearOverride)
		{
			throw new ArgumentException("Missing required 'section' parameter.");
		}

		if (ModNet.Role != ModNet.ModRole.Master)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "perm",
				["error"] = "Only Master instance can manage scout permissions."
			});
		}

		// Normalize section names against ManageableSections (supports comma-separated list)
		List<string> targetSections = new List<string>();
		string[] requestedParts = (section ?? "").Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string part in requestedParts)
		{
			foreach (string s in ModNet.ManageableSections)
			{
				if (string.Equals(s, part, StringComparison.OrdinalIgnoreCase))
				{
					targetSections.Add(s);
					break;
				}
			}
		}

		if (targetSections.Count == 0 && !clearOverride)
		{
			throw new ArgumentException($"Invalid section '{section}'. Valid sections: {string.Join(", ", ModNet.ManageableSections)}.");
		}
		string matchedSection = targetSections.Count > 0 ? string.Join(",", targetSections) : "All";

		int targetActor = 0;
		if (p.TryGetValue("actor", out string actorStr) || p.TryGetValue("target", out actorStr))
		{
			if (!string.IsNullOrEmpty(actorStr))
			{
				if (actorStr.Equals("all", StringComparison.OrdinalIgnoreCase) || actorStr.Equals("global", StringComparison.OrdinalIgnoreCase))
				{
					targetActor = 0;
				}
				else if (int.TryParse(actorStr, out int actNum))
				{
					targetActor = actNum;
				}
				else
				{
					bool found = false;
					try
					{
						if (SafeInRoom() && PhotonNetwork.PlayerList != null)
						{
							foreach (var pl in PhotonNetwork.PlayerList)
							{
								if (pl != null && string.Equals(pl.NickName, actorStr, StringComparison.OrdinalIgnoreCase))
								{
									targetActor = pl.ActorNumber;
									found = true;
									break;
								}
							}
						}
					}
					catch {}

					if (!found)
					{
						throw new ArgumentException($"Invalid actor '{actorStr}'. Expected integer actor number, player nickname, or 'global'.");
					}
				}
			}
		}

		bool? allowVal = null;
		if (p.TryGetValue("allow", out allowStr) || p.TryGetValue("value", out allowStr) || p.TryGetValue("grant", out allowStr))
		{
			if (allowStr.Equals("clear", StringComparison.OrdinalIgnoreCase) || allowStr.Equals("reset", StringComparison.OrdinalIgnoreCase))
			{
				clearOverride = true;
			}
			else
			{
				allowVal = ParseFlexibleBool(allowStr);
				if (!allowVal.HasValue)
				{
					throw new ArgumentException($"Invalid boolean value '{allowStr}'. Expected true/false, 1/0, yes/no, on/off, or clear/reset.");
				}
			}
		}

		bool finalAllow = false;

		if (targetActor > 0)
		{
			if (clearOverride)
			{
				ModNet.ClearSlaveOverride(targetActor);
				finalAllow = targetSections.Count > 0 ? ModNet.IsSlaveSectionAllowed(targetActor, targetSections[0]) : true;
				LogPhoton($"Cleared override for slave {targetActor}");
			}
			else if (allowVal.HasValue)
			{
				if (!ModNet.SlaveOverrides.TryGetValue(targetActor, out var set))
				{
					set = new HashSet<string>(ModNet.GlobalAllowedSections);
					ModNet.SlaveOverrides[targetActor] = set;
				}
				foreach (var s in targetSections)
				{
					if (allowVal.Value) set.Add(s);
					else set.Remove(s);
				}
				ModNet.SyncSlavePermissions(targetActor);
				finalAllow = allowVal.Value;
				LogPhoton($"Perm updated for slave {targetActor}: {matchedSection} = {finalAllow}");
			}
			else
			{
				foreach (var s in targetSections)
				{
					ModNet.ToggleSlaveSection(targetActor, s);
				}
				finalAllow = targetSections.Count > 0 ? ModNet.IsSlaveSectionAllowed(targetActor, targetSections[0]) : false;
				LogPhoton($"Perm toggled for slave {targetActor}: {matchedSection} = {finalAllow}");
			}
		}
		else
		{
			if (allowVal.HasValue)
			{
				foreach (var s in targetSections)
				{
					if (allowVal.Value) ModNet.GlobalAllowedSections.Add(s);
					else ModNet.GlobalAllowedSections.Remove(s);
				}
				ModNet.SyncGlobalPermissions();
				finalAllow = allowVal.Value;
			}
			else
			{
				foreach (var s in targetSections)
				{
					ModNet.ToggleGlobalSection(s);
				}
				finalAllow = targetSections.Count > 0 ? ModNet.IsGlobalSectionAllowed(targetSections[0]) : false;
			}
			LogPhoton($"Global perm updated: {matchedSection} = {finalAllow}");
		}

		Menu.Instance?.RefreshTabs();

		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "perm",
			["actor"] = targetActor > 0 ? (object)targetActor : "global",
			["section"] = matchedSection,
			["allow"] = finalAllow
		});
	}

	private static string ActionDeliver(Dictionary<string, string> p)
	{
		if (!p.TryGetValue("item", out string itemName) && !p.TryGetValue("name", out itemName) && !p.TryGetValue("itemName", out itemName))
		{
			throw new ArgumentException("Missing required 'item' parameter.");
		}
		if (string.IsNullOrEmpty(itemName))
		{
			throw new ArgumentException("Missing required 'item' parameter.");
		}

		int targetActor = 0;
		if (p.TryGetValue("actor", out string actorStr) || p.TryGetValue("target", out actorStr))
		{
			if (!string.IsNullOrEmpty(actorStr))
			{
				if (int.TryParse(actorStr, out int actNum))
				{
					targetActor = actNum;
				}
				else
				{
					bool found = false;
					try
					{
						if (SafeInRoom() && PhotonNetwork.PlayerList != null)
						{
							foreach (var pl in PhotonNetwork.PlayerList)
							{
								if (pl != null && string.Equals(pl.NickName, actorStr, StringComparison.OrdinalIgnoreCase))
								{
									targetActor = pl.ActorNumber;
									found = true;
									break;
								}
							}
						}
					}
					catch {}

					if (!found)
					{
						throw new ArgumentException($"Invalid actor '{actorStr}'. Expected integer actor number or player nickname.");
					}
				}
			}
		}

		if (targetActor <= 0)
		{
			targetActor = SafeLocalActorNumber();
		}

		string destination = "Hands";
		if ((p.TryGetValue("dest", out string dest) || p.TryGetValue("destination", out dest) || p.TryGetValue("slot", out dest)) && !string.IsNullOrEmpty(dest))
		{
			if (dest.IndexOf("back", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				destination = "Backpack";
			}
			else
			{
				destination = "Hands";
			}
		}

		int deliverCount = 1;
		if ((p.TryGetValue("count", out string countStr) || p.TryGetValue("amount", out countStr) || p.TryGetValue("qty", out countStr)) &&
		    int.TryParse(countStr, out int cVal) && cVal > 1)
		{
			deliverCount = Math.Min(cVal, 50);
		}

		var items = SafeItemList();
		Item matchedItem = null;
		if (items != null)
		{
			// Prioritized 4-tier match: exact prefab -> exact name -> substring prefab -> substring name
			matchedItem = items.Find(x => (Object)(object)x != (Object)null && string.Equals(((Object)x).name, itemName, StringComparison.OrdinalIgnoreCase));
			if ((Object)(object)matchedItem == (Object)null)
			{
				matchedItem = items.Find(x => (Object)(object)x != (Object)null && string.Equals(SafeGetItemName(x), itemName, StringComparison.OrdinalIgnoreCase));
			}
			if ((Object)(object)matchedItem == (Object)null)
			{
				matchedItem = items.Find(x => (Object)(object)x != (Object)null && ((Object)x).name.IndexOf(itemName, StringComparison.OrdinalIgnoreCase) >= 0);
			}
			if ((Object)(object)matchedItem == (Object)null)
			{
				matchedItem = items.Find(x => (Object)(object)x != (Object)null && SafeGetItemName(x).IndexOf(itemName, StringComparison.OrdinalIgnoreCase) >= 0);
			}
		}

		string prefabName = (Object)(object)matchedItem != (Object)null ? ((Object)matchedItem).name : itemName;

		bool delivered = false;
		string errorMsg = null;

		if (targetActor != SafeLocalActorNumber())
		{
			if (ModNet.Role != ModNet.ModRole.Master)
			{
				return ToJson(new Dictionary<string, object>
				{
					["success"] = false,
					["action"] = "deliver",
					["actor"] = targetActor,
					["dest"] = destination,
					["item"] = prefabName,
					["error"] = "Only Master instance can deliver items to remote scouts."
				});
			}

			if (!SafeInRoom())
			{
				return ToJson(new Dictionary<string, object>
				{
					["success"] = false,
					["action"] = "deliver",
					["actor"] = targetActor,
					["dest"] = destination,
					["item"] = prefabName,
					["error"] = $"Cannot deliver to actor {targetActor}: not connected to a multiplayer room."
				});
			}

			// Verify target actor exists in the room
			bool actorFound = false;
			try
			{
				if (PhotonNetwork.PlayerList != null)
				{
					foreach (var pl in PhotonNetwork.PlayerList)
					{
						if (pl != null && pl.ActorNumber == targetActor)
						{
							actorFound = true;
							break;
						}
					}
				}
			}
			catch {}

			if (!actorFound)
			{
				return ToJson(new Dictionary<string, object>
				{
					["success"] = false,
					["action"] = "deliver",
					["actor"] = targetActor,
					["dest"] = destination,
					["item"] = prefabName,
					["error"] = $"Scout with actor number {targetActor} not found in room."
				});
			}

			if (items != null && items.Count > 0 && (Object)(object)matchedItem == (Object)null)
			{
				return ToJson(new Dictionary<string, object>
				{
					["success"] = false,
					["action"] = "deliver",
					["actor"] = targetActor,
					["dest"] = destination,
					["item"] = prefabName,
					["error"] = $"Item '{itemName}' not found in ItemDatabase."
				});
			}

			for (int i = 0; i < deliverCount; i++)
			{
				if (ModNet.DeliverItemToScout(targetActor, destination, prefabName))
				{
					delivered = true;
				}
			}

			if (!delivered)
			{
				errorMsg = $"PUN item delivery to actor {targetActor} failed.";
			}
		}
		else
		{
			// Target is local scout
			if (!SafeInGame() || (Object)(object)SafeLocalCharacter() == (Object)null)
			{
				return ToJson(new Dictionary<string, object>
				{
					["success"] = false,
					["action"] = "deliver",
					["actor"] = targetActor,
					["dest"] = destination,
					["item"] = prefabName,
					["error"] = "Cannot deliver to local scout: player is not in-game or character reference is null."
				});
			}

			if ((Object)(object)matchedItem == (Object)null)
			{
				return ToJson(new Dictionary<string, object>
				{
					["success"] = false,
					["action"] = "deliver",
					["actor"] = targetActor,
					["dest"] = destination,
					["item"] = prefabName,
					["error"] = $"Item '{itemName}' not found in ItemDatabase."
				});
			}

			for (int i = 0; i < deliverCount; i++)
			{
				if (SafeItemAdd(matchedItem))
				{
					delivered = true;
				}
			}

			if (delivered)
			{
				SafeMenuSay(Lang.F("Delivered {0} to {1}", prefabName, destination));
			}
			else
			{
				errorMsg = $"Failed to add item '{prefabName}' to local inventory.";
			}
		}

		LogPhoton($"Deliver item '{prefabName}' to actor {targetActor} ({destination}) x{deliverCount}: {(delivered ? "Success" : "Failed")}");

		return ToJson(new Dictionary<string, object>
		{
			["success"] = delivered,
			["action"] = "deliver",
			["actor"] = targetActor,
			["dest"] = destination,
			["item"] = prefabName,
			["count"] = deliverCount,
			["error"] = delivered ? null : (errorMsg ?? $"Delivery of '{prefabName}' to actor {targetActor} failed.")
		});
	}

	private static Character ResolveTargetCharacter(Dictionary<string, string> p)
	{
		if (p != null && p.TryGetValue("actor", out string actorStr) && int.TryParse(actorStr, out int actorNum) && actorNum > 0)
		{
			if (PlayerHandler.TryGetCharacter(actorNum, out var c) && c != null) return c;
			foreach (var other in Features.Others())
			{
				if (other != null)
				{
					var pv = other.GetComponent<PhotonView>() ?? (other.refs != null ? other.refs.view : null);
					if (pv != null && pv.Owner != null && pv.Owner.ActorNumber == actorNum) return other;
				}
			}
		}

		var others = Features.Others();
		if (others != null && others.Count > 0)
		{
			foreach (var c in others)
			{
				if (c != null) return c;
			}
		}
		return null;
	}

	private static Item ResolveItemByName(string name)
	{
		if (string.IsNullOrWhiteSpace(name)) return null;
		var items = Features.ItemList();
		if (items == null) return null;
		return items.Find(x => x != null && (
			((UnityEngine.Object)x).name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
			(x.GetName() != null && x.GetName().Equals(name, StringComparison.OrdinalIgnoreCase)) ||
			((UnityEngine.Object)x).name.Replace(" ", "").Equals(name.Replace(" ", "").Replace("-", ""), StringComparison.OrdinalIgnoreCase) ||
			(x.GetName() != null && x.GetName().Replace(" ", "").Equals(name.Replace(" ", "").Replace("-", ""), StringComparison.OrdinalIgnoreCase))
		));
	}

	private static string ActionKillSelf(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "kill_self",
				["error"] = "Cannot kill self: local player is not in-game."
			});
		}
		Features.KillSelf();
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "kill_self",
			["dead"] = (Features.Me != null && Features.Me.data != null) ? Features.Me.data.dead : false
		});
	}

	private static string ActionHeal(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "heal",
				["error"] = "Cannot heal: local player is not in-game."
			});
		}
		Features.Heal();
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "heal"
		});
	}

	private static string ActionBonusStamina(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "bonus_stamina",
				["error"] = "Cannot give bonus stamina: local player is not in-game."
			});
		}
		Features.BonusStamina();
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "bonus_stamina"
		});
	}

	private static string ActionRevive(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "revive",
				["error"] = "Cannot revive: local player is not in-game."
			});
		}
		Features.Revive();
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "revive"
		});
	}

	private static string ActionWarpCampfire(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "warp_campfire",
				["error"] = "Cannot warp to campfire: local player is not in-game."
			});
		}
		Features.WarpToCampfire();
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "warp_campfire"
		});
	}

	private static string ActionWarpStatue(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "warp_statue",
				["error"] = "Cannot warp to statue: local player is not in-game."
			});
		}
		Features.WarpToStatue();
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "warp_statue"
		});
	}

	private static string ActionWarpAim(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "warp_aim",
				["error"] = "Cannot warp to aim: local player is not in-game."
			});
		}
		Features.WarpToAim();
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "warp_aim"
		});
	}

	private static string ActionSaveSpot(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "save_spot",
				["error"] = "Cannot save spot: local player is not in-game."
			});
		}
		int spot = 1;
		if (p != null && p.TryGetValue("spot", out string s) && int.TryParse(s, out int parsed)) spot = parsed;
		Features.SaveSpot(spot);
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "save_spot",
			["spot"] = spot
		});
	}

	private static string ActionGoSpot(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "go_spot",
				["error"] = "Cannot go to spot: local player is not in-game."
			});
		}
		int spot = 1;
		if (p != null && p.TryGetValue("spot", out string s) && int.TryParse(s, out int parsed)) spot = parsed;
		Features.GoSpot(spot);
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "go_spot",
			["spot"] = spot
		});
	}

	private static string ActionFreeCam(Dictionary<string, string> p)
	{
		if (!SafeInGame())
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "freecam",
				["error"] = "Cannot toggle freecam: local player is not in-game."
			});
		}
		bool on = !Live.FreeCam;
		if (p != null && p.TryGetValue("enabled", out string enStr) && bool.TryParse(enStr, out bool parsed)) on = parsed;
		Features.SetFreeCam(on);
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "freecam",
			["enabled"] = on
		});
	}

	private static string ActionSetTime(Dictionary<string, string> p)
	{
		if (!SafeInGame())
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "set_time",
				["error"] = "Cannot set time: local player is not in-game."
			});
		}
		float hour = 12f;
		string what = "noon";
		if (p != null)
		{
			if (p.TryGetValue("preset", out string preset))
			{
				switch (preset.ToLowerInvariant())
				{
					case "dawn": hour = 6f; what = "dawn"; break;
					case "noon": hour = 12f; what = "noon"; break;
					case "dusk": hour = 18f; what = "dusk"; break;
					case "midnight": hour = 0f; what = "midnight"; break;
				}
			}
			else if (p.TryGetValue("hour", out string hStr) && float.TryParse(hStr, out float h))
			{
				hour = h;
				what = $"{hour:0.0}h";
			}
		}
		Features.SetTime(hour, what);
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "set_time",
			["hour"] = hour
		});
	}

	private static string ActionNextSection(Dictionary<string, string> p)
	{
		if (!SafeInGame())
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "next_section",
				["error"] = "Cannot advance section: local player is not in-game."
			});
		}
		Features.NextSection();
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "next_section"
		});
	}

	private static string ActionTrip(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "trip",
				["error"] = "Cannot trip: local player is not in-game."
			});
		}
		Character target = ResolveTargetCharacter(p);
		if (target == null) throw new InvalidOperationException("No target scout found to trip.");
		float sec = 3f;
		if (p != null && p.TryGetValue("seconds", out string sStr) && float.TryParse(sStr, out float parsed)) sec = parsed;
		Features.Trip(target, sec);
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "trip",
			["seconds"] = sec
		});
	}

	private static string ActionLaunch(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "launch",
				["error"] = "Cannot launch: local player is not in-game."
			});
		}
		Character target = ResolveTargetCharacter(p);
		if (target == null) throw new InvalidOperationException("No target scout found to launch.");
		Vector3 dir = Vector3.up;
		float speed = 15f;
		if (p != null)
		{
			if (p.TryGetValue("speed", out string spStr) && float.TryParse(spStr, out float sp)) speed = sp;
			if (p.TryGetValue("x", out string xStr) && float.TryParse(xStr, out float x) &&
			    p.TryGetValue("y", out string yStr) && float.TryParse(yStr, out float y) &&
			    p.TryGetValue("z", out string zStr) && float.TryParse(zStr, out float z))
			{
				dir = new Vector3(x, y, z);
			}
		}
		Features.Launch(target, dir, speed);
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "launch",
			["speed"] = speed
		});
	}

	private static string ActionMakeJump(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "make_jump",
				["error"] = "Cannot make jump: local player is not in-game."
			});
		}
		Character target = ResolveTargetCharacter(p);
		if (target == null) throw new InvalidOperationException("No target scout found to jump.");
		Features.MakeJump(target);
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "make_jump"
		});
	}

	private static string ActionBring(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "bring",
				["error"] = "Cannot bring: local player is not in-game."
			});
		}
		Character target = ResolveTargetCharacter(p);
		if (target == null) throw new InvalidOperationException("No target scout found to bring.");
		Features.Bring(target);
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "bring"
		});
	}

	private static string ActionSwap(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "swap",
				["error"] = "Cannot swap: local player is not in-game."
			});
		}
		Character target = ResolveTargetCharacter(p);
		if (target == null) throw new InvalidOperationException("No target scout found to swap.");
		Features.Swap(target);
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "swap"
		});
	}

	private static string ActionDropOn(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "drop_on",
				["error"] = "Cannot drop item: local player is not in-game."
			});
		}
		Character target = ResolveTargetCharacter(p);
		if (target == null) throw new InvalidOperationException("No target scout found to drop item on.");
		string itemName = (p != null && p.TryGetValue("item", out string it)) ? it : "Compass";
		Item item = ResolveItemByName(itemName);
		if (item == null) throw new ArgumentException($"Item '{itemName}' not found in item registry.");
		Features.DropOn(target, item);
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "drop_on",
			["item"] = itemName
		});
	}

	private static string ActionHealOther(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "heal_other",
				["error"] = "Cannot heal other: local player is not in-game."
			});
		}
		Character target = ResolveTargetCharacter(p);
		if (target == null) throw new InvalidOperationException("No target scout found to heal.");
		bool ok = Features.HealOther(target);
		return ToJson(new Dictionary<string, object>
		{
			["success"] = ok,
			["action"] = "heal_other"
		});
	}

	private static string ActionReviveOther(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "revive_other",
				["error"] = "Cannot revive other: local player is not in-game."
			});
		}
		Character target = ResolveTargetCharacter(p);
		if (target == null) throw new InvalidOperationException("No target scout found to revive.");
		bool byMe = false;
		if (p != null && p.TryGetValue("by_me", out string bm) && bool.TryParse(bm, out bool parsed)) byMe = parsed;
		Features.ReviveOther(target, byMe);
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "revive_other",
			["by_me"] = byMe
		});
	}

	private static string ActionHandItem(Dictionary<string, string> p)
	{
		if (!SafeInGame() || (UnityEngine.Object)(object)SafeLocalCharacter() == (UnityEngine.Object)null)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "hand_item",
				["error"] = "Cannot hand item: local player is not in-game."
			});
		}
		Character target = ResolveTargetCharacter(p);
		if (target == null) throw new InvalidOperationException("No target scout found to hand item.");
		string itemName = (p != null && p.TryGetValue("item", out string it)) ? it : "Compass";
		Item item = ResolveItemByName(itemName);
		if (item == null) throw new ArgumentException($"Item '{itemName}' not found in item registry.");
		Features.HandItem(target, item);
		return ToJson(new Dictionary<string, object>
		{
			["success"] = true,
			["action"] = "hand_item",
			["item"] = itemName
		});
	}

	private static string ActionScreenshot(Dictionary<string, string> p)
	{
		string defaultPath = Path.Combine(Path.GetTempPath(), (ModNet.Role == ModNet.ModRole.Master ? "peak_screenshot_master.png" : "peak_screenshot_slave.png"));
		string targetPath = (p != null && p.TryGetValue("path", out string customPath) && !string.IsNullOrWhiteSpace(customPath)) ? customPath : defaultPath;

		try
		{
			string dir = Path.GetDirectoryName(targetPath);
			if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			// ScreenCapture writes image to disk at end of frame
			// Capture ONLY to targetPath (calling ScreenCapture twice in 1 frame overrides/cancels the previous capture)
			ScreenCapture.CaptureScreenshot(targetPath);

			if (p != null)
			{
				p["path"] = targetPath;
			}

			int width = 0;
			int height = 0;
			try
			{
				width = Screen.width;
				height = Screen.height;
			}
			catch {}

			return ToJson(new Dictionary<string, object>
			{
				["success"] = true,
				["action"] = "screenshot",
				["path"] = targetPath,
				["width"] = width,
				["height"] = height,
				["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
			});
		}
		catch (Exception ex)
		{
			return ToJson(new Dictionary<string, object>
			{
				["success"] = false,
				["action"] = "screenshot",
				["error"] = ex.Message
			});
		}
	}

	public static string ToJson(object obj)
	{
		if (obj == null) return "null";

		if (obj is string s)
		{
			return EscapeJsonString(s);
		}

		if (obj is bool b)
		{
			return b ? "true" : "false";
		}

		if (obj is int || obj is long || obj is short || obj is byte || obj is uint || obj is ulong)
		{
			return obj.ToString();
		}

		if (obj is float f)
		{
			if (float.IsNaN(f) || float.IsInfinity(f)) return "0.0";
			return f.ToString("R", CultureInfo.InvariantCulture);
		}

		if (obj is double d)
		{
			if (double.IsNaN(d) || double.IsInfinity(d)) return "0.0";
			return d.ToString("R", CultureInfo.InvariantCulture);
		}

		if (obj is decimal dec)
		{
			return dec.ToString(CultureInfo.InvariantCulture);
		}

		if (obj is Vector3 v)
		{
			return $"{{\"x\":{ToJson(v.x)},\"y\":{ToJson(v.y)},\"z\":{ToJson(v.z)}}}";
		}

		if (obj is IDictionary dict)
		{
			var sb = new StringBuilder("{");
			bool first = true;
			foreach (DictionaryEntry kv in dict)
			{
				if (!first) sb.Append(",");
				first = false;
				sb.Append(EscapeJsonString(kv.Key?.ToString() ?? ""));
				sb.Append(":");
				sb.Append(ToJson(kv.Value));
			}
			sb.Append("}");
			return sb.ToString();
		}

		if (obj is IEnumerable list)
		{
			var sb = new StringBuilder("[");
			bool first = true;
			foreach (object item in list)
			{
				if (!first) sb.Append(",");
				first = false;
				sb.Append(ToJson(item));
			}
			sb.Append("]");
			return sb.ToString();
		}

		return EscapeJsonString(obj.ToString());
	}

	private static string EscapeJsonString(string s)
	{
		if (s == null) return "null";
		var sb = new StringBuilder(s.Length + 10);
		sb.Append('"');
		foreach (char c in s)
		{
			switch (c)
			{
				case '"': sb.Append("\\\""); break;
				case '\\': sb.Append("\\\\"); break;
				case '\b': sb.Append("\\b"); break;
				case '\f': sb.Append("\\f"); break;
				case '\n': sb.Append("\\n"); break;
				case '\r': sb.Append("\\r"); break;
				case '\t': sb.Append("\\t"); break;
				default:
					if (c < ' ')
					{
						sb.AppendFormat("\\u{0:x4}", (int)c);
					}
					else
					{
						sb.Append(c);
					}
					break;
			}
		}
		sb.Append('"');
		return sb.ToString();
	}
}
