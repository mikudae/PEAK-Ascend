using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using BepInEx.Configuration;
using UnityEngine;

namespace Summit;

public static class Updater
{
	public class ReleaseAsset
	{
		public string Name { get; set; }
		public string DownloadUrl { get; set; }

		public ReleaseAsset(string name, string downloadUrl)
		{
			Name = name;
			DownloadUrl = downloadUrl;
		}
	}

	public static ConfigEntry<string> GitHubRepo { get; set; }

	public static string OverrideTargetDllPath { get; set; } = null;

	public static string OverrideCurrentVersion { get; set; } = null;

	private static int _isUpdating = 0;

	public static Action TerminateProcess { get; set; } = DefaultTerminateProcess;

	public static void DefaultTerminateProcess()
	{
		try
		{
			Application.Quit();
		}
		catch
		{
		}
		try
		{
			Process.GetCurrentProcess().Kill();
		}
		catch
		{
		}
		try
		{
			Environment.Exit(0);
		}
		catch
		{
		}
	}

	public static bool IsSlaveRole()
	{
#if SLAVE_BUILD
		return true;
#else
		return ModNet.Role == ModNet.ModRole.Slave;
#endif
	}

	public static string CurrentVersion
	{
		get
		{
			if (!string.IsNullOrEmpty(OverrideCurrentVersion))
			{
				return OverrideCurrentVersion;
			}
			if (!string.IsNullOrEmpty(Kit.Version))
			{
				return Kit.Version;
			}
			try
			{
				var asmVer = typeof(Updater).Assembly.GetName().Version;
				if (asmVer != null && (asmVer.Major > 0 || asmVer.Minor > 0))
				{
					return asmVer.ToString();
				}
			}
			catch
			{
			}
			return "2.0";
		}
	}

	public static string GetActiveDllPath()
	{
		if (!string.IsNullOrEmpty(OverrideTargetDllPath))
		{
			return Path.GetFullPath(OverrideTargetDllPath);
		}
		try
		{
			// Check CodeBase first (handles shadow-copying where Assembly.Location points to temp cache)
			string codeBase = typeof(Updater).Assembly.CodeBase;
			if (!string.IsNullOrEmpty(codeBase))
			{
				try
				{
					if (Uri.TryCreate(codeBase, UriKind.Absolute, out Uri cbUri) && cbUri.IsFile)
					{
						string cbPath = Path.GetFullPath(cbUri.LocalPath);
						if (File.Exists(cbPath))
						{
							return cbPath;
						}
					}
				}
				catch { }
			}

			string loc = typeof(Updater).Assembly.Location;
			if (!string.IsNullOrEmpty(loc))
			{
				string fullLoc = Path.GetFullPath(loc);
				if (File.Exists(fullLoc))
				{
					// If fullLoc is in a temp / shadow copy directory and BepInEx PluginPath has the real DLL, prefer PluginPath
					bool isTempOrCache = fullLoc.IndexOf("Temp", StringComparison.OrdinalIgnoreCase) >= 0 ||
					                     fullLoc.IndexOf("ShadowCopy", StringComparison.OrdinalIgnoreCase) >= 0;
					if (!isTempOrCache)
					{
						return fullLoc;
					}
				}
			}
		}
		catch
		{
		}
		try
		{
			string pluginDir = BepInEx.Paths.PluginPath;
			if (!string.IsNullOrEmpty(pluginDir) && Directory.Exists(pluginDir))
			{
				string c1 = Path.GetFullPath(Path.Combine(pluginDir, "Summit.dll"));
				if (File.Exists(c1)) return c1;
				string c2 = Path.GetFullPath(Path.Combine(pluginDir, "Summit_Slave.dll"));
				if (File.Exists(c2)) return c2;
				foreach (var file in Directory.GetFiles(pluginDir, "Summit*.dll", SearchOption.AllDirectories))
				{
					return Path.GetFullPath(file);
				}
				return c1;
			}
		}
		catch
		{
		}
		return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins", "Summit.dll"));
	}

	public static string NormalizeRepository(string repo)
	{
		if (string.IsNullOrWhiteSpace(repo))
		{
			return "mikudae/PEAK-Ascend";
		}
		string s = repo.Trim().Trim('"', '\'');

		if (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			var match = Regex.Match(s, @"^https?://github\.com/([^/]+)/([^/#\?]+)", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				string owner = match.Groups[1].Value.Trim();
				string name = match.Groups[2].Value.Trim();
				if (name.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
				{
					name = name.Substring(0, name.Length - 4);
				}
				return $"{owner}/{name}";
			}
			return s;
		}

		if (s.StartsWith("git@github.com:", StringComparison.OrdinalIgnoreCase))
		{
			s = s.Substring("git@github.com:".Length);
		}

		if (s.StartsWith("github.com/", StringComparison.OrdinalIgnoreCase))
		{
			s = s.Substring("github.com/".Length);
		}

		int hashOrQuery = s.IndexOfAny(new[] { '#', '?' });
		if (hashOrQuery >= 0)
		{
			s = s.Substring(0, hashOrQuery);
		}

		if (s.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
		{
			s = s.Substring(0, s.Length - 4);
		}

		s = s.Trim('/');
		if (s.Contains("/"))
		{
			var parts = s.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length >= 2)
			{
				string owner = parts[0].Trim();
				string name = parts[1].Trim();
				if (name.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
				{
					name = name.Substring(0, name.Length - 4);
				}
				return $"{owner}/{name}";
			}
		}

		return string.IsNullOrWhiteSpace(s) ? "mikudae/PEAK-Ascend" : s;
	}

	public static string GetRepoTarget()
	{
		if (GitHubRepo != null && !string.IsNullOrWhiteSpace(GitHubRepo.Value))
		{
			return NormalizeRepository(GitHubRepo.Value);
		}
		return "mikudae/PEAK-Ascend";
	}

	public static void Bind(ConfigFile config)
	{
		if (config == null) return;
		GitHubRepo = config.Bind<string>("Updater", "GitHubRepo", "mikudae/PEAK-Ascend", "GitHub repository in 'owner/repo' format for automatic mod updates.");
	}

	public static void Start(ConfigFile config)
	{
		Bind(config);
		CleanupOldFiles(GetActiveDllPath());

		if (!IsSlaveRole())
		{
			return;
		}

		ThreadPool.QueueUserWorkItem(_ =>
		{
			try
			{
				CheckAndApplyUpdate();
			}
			catch (Exception ex)
			{
				Kit.Log?.LogWarning("[Updater] Background update check encountered error: " + ex.Message);
			}
		});
	}

	public static int[] NormalizeVersionComponents(string v)
	{
		if (string.IsNullOrWhiteSpace(v))
		{
			return new int[0];
		}
		string s = v.Trim();

		int firstDigit = -1;
		for (int idx = 0; idx < s.Length; idx++)
		{
			if (char.IsDigit(s[idx]))
			{
				firstDigit = idx;
				break;
			}
		}

		if (firstDigit < 0)
		{
			return new int[0];
		}

		s = s.Substring(firstDigit);

		int endOfVersion = s.Length;
		for (int idx = 0; idx < s.Length; idx++)
		{
			char c = s[idx];
			if (c == '-' || c == '+' || (!char.IsDigit(c) && c != '.'))
			{
				endOfVersion = idx;
				break;
			}
		}

		s = s.Substring(0, endOfVersion).TrimEnd('.');

		string[] parts = s.Split('.');
		List<int> components = new List<int>();
		foreach (string part in parts)
		{
			if (int.TryParse(part, out int val))
			{
				components.Add(val);
			}
			else
			{
				components.Add(0);
			}
		}
		return components.ToArray();
	}

	public static int CompareVersions(string versionA, string versionB)
	{
		int[] compA = NormalizeVersionComponents(versionA);
		int[] compB = NormalizeVersionComponents(versionB);

		int maxLen = Math.Max(compA.Length, compB.Length);
		for (int i = 0; i < maxLen; i++)
		{
			int valA = i < compA.Length ? compA[i] : 0;
			int valB = i < compB.Length ? compB[i] : 0;
			if (valA != valB)
			{
				return valA.CompareTo(valB);
			}
		}
		return 0;
	}

	public static bool IsNewerVersion(string currentVersion, string latestVersion)
	{
		if (string.IsNullOrWhiteSpace(latestVersion))
		{
			return false;
		}
		return CompareVersions(latestVersion, currentVersion) > 0;
	}

	public static string UnescapeJsonString(string s)
	{
		if (string.IsNullOrEmpty(s)) return s;
		return Regex.Replace(s, @"\\([""\\/bfnrt]|u[0-9a-fA-F]{4})", m =>
		{
			string val = m.Groups[1].Value;
			if (val.StartsWith("u") && val.Length == 5)
			{
				int code = Convert.ToInt32(val.Substring(1), 16);
				return ((char)code).ToString();
			}
			switch (val)
			{
				case "\"": return "\"";
				case "\\": return "\\";
				case "/": return "/";
				case "b": return "\b";
				case "f": return "\f";
				case "n": return "\n";
				case "r": return "\r";
				case "t": return "\t";
				default: return m.Value;
			}
		});
	}

	public static string GetRootJsonProperty(string json, string propertyName)
	{
		if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(propertyName)) return null;

		int i = 0;
		while (i < json.Length && json[i] != '{') i++;
		if (i >= json.Length) return null;
		i++;

		while (i < json.Length)
		{
			while (i < json.Length && (char.IsWhiteSpace(json[i]) || json[i] == ',')) i++;
			if (i >= json.Length || json[i] == '}') break;

			if (json[i] != '"')
			{
				i++;
				continue;
			}

			int keyStart = i + 1;
			int keyEnd = keyStart;
			while (keyEnd < json.Length)
			{
				if (json[keyEnd] == '\\') { keyEnd += 2; continue; }
				if (json[keyEnd] == '"') break;
				keyEnd++;
			}
			if (keyEnd >= json.Length) break;
			string key = UnescapeJsonString(json.Substring(keyStart, keyEnd - keyStart));
			i = keyEnd + 1;

			while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
			if (i >= json.Length || json[i] != ':') break;
			i++;
			while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
			if (i >= json.Length) break;

			bool isTarget = string.Equals(key, propertyName, StringComparison.OrdinalIgnoreCase);

			if (json[i] == '"')
			{
				int valStart = i + 1;
				int valEnd = valStart;
				while (valEnd < json.Length)
				{
					if (json[valEnd] == '\\') { valEnd += 2; continue; }
					if (json[valEnd] == '"') break;
					valEnd++;
				}
				if (valEnd >= json.Length) break;
				if (isTarget)
				{
					return UnescapeJsonString(json.Substring(valStart, valEnd - valStart));
				}
				i = valEnd + 1;
			}
			else if (json[i] == '{' || json[i] == '[')
			{
				char openChar = json[i];
				char closeChar = openChar == '{' ? '}' : ']';
				int nestDepth = 1;
				i++;
				bool inValQuote = false;
				while (i < json.Length && nestDepth > 0)
				{
					char ch = json[i];
					if (ch == '\\' && inValQuote)
					{
						i += 2;
						continue;
					}
					if (ch == '"')
					{
						inValQuote = !inValQuote;
					}
					else if (!inValQuote)
					{
						if (ch == openChar) nestDepth++;
						else if (ch == closeChar) nestDepth--;
					}
					i++;
				}
			}
			else
			{
				int valStart = i;
				while (i < json.Length && json[i] != ',' && json[i] != '}' && !char.IsWhiteSpace(json[i]))
				{
					i++;
				}
				if (isTarget)
				{
					return json.Substring(valStart, i - valStart).Trim();
				}
			}
		}

		return null;
	}

	public static string ParseReleaseTag(string json)
	{
		if (string.IsNullOrEmpty(json)) return null;
		return GetRootJsonProperty(json, "tag_name");
	}

	public static List<ReleaseAsset> ParseAssets(string json)
	{
		var list = new List<ReleaseAsset>();
		if (string.IsNullOrEmpty(json)) return list;

		int assetsIndex = -1;
		int i = 0;
		while (i < json.Length && json[i] != '{') i++;
		if (i >= json.Length) return list;
		i++;

		int depth = 1;
		bool inQuote = false;
		while (i < json.Length)
		{
			char c = json[i];
			if (c == '\\' && inQuote)
			{
				i += 2;
				continue;
			}
			if (c == '"')
			{
				inQuote = !inQuote;
				if (inQuote && depth == 1)
				{
					int keyStart = i + 1;
					int keyEnd = keyStart;
					while (keyEnd < json.Length)
					{
						if (json[keyEnd] == '\\') { keyEnd += 2; continue; }
						if (json[keyEnd] == '"') break;
						keyEnd++;
					}
					if (keyEnd < json.Length)
					{
						string key = json.Substring(keyStart, keyEnd - keyStart);
						i = keyEnd + 1;
						inQuote = false;

						while (i < json.Length && (char.IsWhiteSpace(json[i]) || json[i] == ':')) i++;

						if (string.Equals(key, "assets", StringComparison.OrdinalIgnoreCase))
						{
							while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
							if (i < json.Length && json[i] == '[')
							{
								assetsIndex = i;
								break;
							}
						}
						continue;
					}
				}
			}
			else if (!inQuote)
			{
				if (c == '{' || c == '[') depth++;
				else if (c == '}' || c == ']')
				{
					depth--;
					if (depth == 0) break;
				}
			}
			i++;
		}

		if (assetsIndex < 0)
		{
			var fallback = Regex.Match(json, @"""assets""\s*:\s*\[");
			if (!fallback.Success) return list;
			assetsIndex = fallback.Index + fallback.Length - 1;
		}

		int openBracket = assetsIndex;
		i = openBracket + 1;
		inQuote = false;
		while (i < json.Length)
		{
			char c = json[i];
			if (c == ']' && !inQuote) break;
			if (c == '{' && !inQuote)
			{
				int objStart = i;
				int objBraces = 1;
				i++;
				while (i < json.Length && objBraces > 0)
				{
					char ch = json[i];
					if (ch == '\\' && inQuote)
					{
						i += 2;
						continue;
					}
					if (ch == '"')
					{
						inQuote = !inQuote;
					}
					else if (!inQuote)
					{
						if (ch == '{') objBraces++;
						else if (ch == '}') objBraces--;
					}
					i++;
				}
				int safeEnd = Math.Min(i, json.Length);
				string objStr = json.Substring(objStart, safeEnd - objStart);
				var nameMatch = Regex.Match(objStr, @"""name""\s*:\s*""((?:\\.|[^""\\])*)""");
				var urlMatch = Regex.Match(objStr, @"""browser_download_url""\s*:\s*""((?:\\.|[^""\\])*)""");

				if (nameMatch.Success && urlMatch.Success)
				{
					string rawName = UnescapeJsonString(nameMatch.Groups[1].Value);
					string url = UnescapeJsonString(urlMatch.Groups[1].Value);

					// Path traversal protection: ensure asset name does not contain directory separators or parent paths
					if (!string.IsNullOrEmpty(rawName) && !rawName.Contains("..") && !rawName.Contains("/") && !rawName.Contains("\\"))
					{
						// Ensure URL is a valid absolute HTTP or HTTPS URL
						if (Uri.TryCreate(url, UriKind.Absolute, out Uri parsedUri) &&
						    (parsedUri.Scheme == Uri.UriSchemeHttp || parsedUri.Scheme == Uri.UriSchemeHttps))
						{
							list.Add(new ReleaseAsset(rawName, url));
						}
					}
				}
			}
			else
			{
				i++;
			}
		}

		return list;
	}

	public static string ResolveDownloadUrl(string json, params string[] candidateNames)
	{
		if (candidateNames == null || candidateNames.Length == 0)
		{
			candidateNames = new string[] { "Summit_Slave.dll", "Summit-Slave.dll", "Summit.dll", "Ascend_Slave.dll", "Ascend-Slave.dll", "Ascend.dll" };
		}

		var assets = ParseAssets(json);
		foreach (var target in candidateNames)
		{
			var found = assets.Find(a => string.Equals(a.Name, target, StringComparison.OrdinalIgnoreCase));
			if (found != null && !string.IsNullOrEmpty(found.DownloadUrl))
			{
				if (Uri.TryCreate(found.DownloadUrl, UriKind.Absolute, out Uri parsedUri) &&
				    (parsedUri.Scheme == Uri.UriSchemeHttp || parsedUri.Scheme == Uri.UriSchemeHttps))
				{
					return found.DownloadUrl;
				}
			}
		}

		return null;
	}

	public static void CleanupOldFiles(string activeDllPath)
	{
		try
		{
			if (string.IsNullOrEmpty(activeDllPath)) return;
			string dir = Path.GetDirectoryName(activeDllPath);
			if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

			string oldFile = activeDllPath + ".old";
			if (File.Exists(oldFile))
			{
				try { File.Delete(oldFile); } catch { }
			}

			string downloadFile = activeDllPath + ".download";
			if (File.Exists(downloadFile))
			{
				try { File.Delete(downloadFile); } catch { }
			}

			string searchPrefix = Path.GetFileNameWithoutExtension(activeDllPath);
			var patterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				$"{searchPrefix}*.old",
				$"{searchPrefix}*.download",
				"Summit*.old",
				"Summit*.download",
				"Ascend*.old",
				"Ascend*.download"
			};

			foreach (string pattern in patterns)
			{
				foreach (var f in Directory.GetFiles(dir, pattern))
				{
					try { File.Delete(f); } catch { }
				}
			}
		}
		catch (Exception ex)
		{
			Kit.Log?.LogWarning($"[Updater] CleanupOldFiles encountered non-fatal error: {ex.Message}");
		}
	}

	public static bool IsValidDownloadedBinary(string filePath)
	{
		if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
		{
			return false;
		}

		FileInfo info = new FileInfo(filePath);
		if (info.Length == 0)
		{
			return false;
		}

		byte[] header = new byte[Math.Min(256, (int)info.Length)];
		using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			int read = fs.Read(header, 0, header.Length);
			if (read == 0) return false;
		}

		// Windows PE / .NET DLL binaries always start with DOS header 'MZ' (0x4D, 0x5A)
		if (header.Length >= 2 && header[0] == 0x4D && header[1] == 0x5A)
		{
			return true;
		}

		// Reject archive formats (ZIP: PK, GZIP: 0x1F 0x8B)
		if (header.Length >= 2)
		{
			if (header[0] == 0x50 && header[1] == 0x4B) return false; // PK zip
			if (header[0] == 0x1F && header[1] == 0x8B) return false; // gzip
		}

		string headerText = System.Text.Encoding.ASCII.GetString(header).TrimStart();

		// Detect XML error responses (e.g. AWS S3 <Error><Code>NoSuchKey</Code></Error>, <?xml)
		if (headerText.StartsWith("<", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		// Detect JSON error payloads (e.g. {"message":..., {"error":...})
		if (headerText.StartsWith("{", StringComparison.OrdinalIgnoreCase) || headerText.StartsWith("[", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		// Detect common plain-text HTTP status error phrases and proxy error messages
		if (headerText.StartsWith("40", StringComparison.OrdinalIgnoreCase) ||
		    headerText.StartsWith("50", StringComparison.OrdinalIgnoreCase) ||
		    headerText.StartsWith("Error", StringComparison.OrdinalIgnoreCase) ||
		    headerText.StartsWith("Exception", StringComparison.OrdinalIgnoreCase) ||
		    headerText.StartsWith("Access Denied", StringComparison.OrdinalIgnoreCase) ||
		    headerText.StartsWith("Not Found", StringComparison.OrdinalIgnoreCase) ||
		    headerText.StartsWith("Bad Gateway", StringComparison.OrdinalIgnoreCase) ||
		    headerText.StartsWith("Unauthorized", StringComparison.OrdinalIgnoreCase) ||
		    headerText.StartsWith("Forbidden", StringComparison.OrdinalIgnoreCase) ||
		    headerText.StartsWith("Service Unavailable", StringComparison.OrdinalIgnoreCase) ||
		    headerText.StartsWith("upstream", StringComparison.OrdinalIgnoreCase) ||
		    headerText.StartsWith("Timeout", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		return true;
	}

	public static bool PerformFileSwap(string activeDllPath, string downloadTempPath)
	{
		if (string.IsNullOrEmpty(activeDllPath) || string.IsNullOrEmpty(downloadTempPath))
		{
			throw new ArgumentException("activeDllPath and downloadTempPath must not be null or empty.");
		}

		if (!File.Exists(downloadTempPath))
		{
			throw new FileNotFoundException("Download file does not exist", downloadTempPath);
		}

		if (!IsValidDownloadedBinary(downloadTempPath))
		{
			throw new InvalidDataException("Downloaded file is empty or corrupted (not a valid assembly binary).");
		}

		string targetDir = Path.GetDirectoryName(activeDllPath);
		if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
		{
			Directory.CreateDirectory(targetDir);
		}

		string oldPath = activeDllPath + ".old";

		if (File.Exists(oldPath))
		{
			try
			{
				File.Delete(oldPath);
			}
			catch (Exception ex)
			{
				Kit.Log?.LogWarning($"[Updater] Could not delete existing {oldPath}: {ex.Message}");
				int counter = 0;
				do
				{
					string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
					string suffix = counter == 0 ? Guid.NewGuid().ToString("N").Substring(0, 4) : counter.ToString();
					oldPath = $"{activeDllPath}.{timestamp}_{suffix}.old";
					counter++;
				} while (File.Exists(oldPath));
			}
		}

		if (File.Exists(activeDllPath))
		{
			File.Move(activeDllPath, oldPath);
		}

		try
		{
			File.Move(downloadTempPath, activeDllPath);
			return true;
		}
		catch (Exception ex)
		{
			Kit.Log?.LogError($"[Updater] Failed to move download to target DLL: {ex.Message}. Rolling back...");
			try
			{
				if (!File.Exists(activeDllPath) && File.Exists(oldPath))
				{
					File.Move(oldPath, activeDllPath);
				}
			}
			catch
			{
			}
			throw;
		}
	}

	public static bool CheckAndApplyUpdate(string repoOverride = null, string targetDllOverride = null, HttpClient customHttpClient = null)
	{
		if (Interlocked.CompareExchange(ref _isUpdating, 1, 0) != 0)
		{
			Kit.Log?.LogWarning("[Updater] Update check is already in progress. Skipping duplicate request.");
			return false;
		}

		HttpClient client = customHttpClient;
		bool disposeClient = false;
		string downloadTemp = null;

		try
		{
			string repo = NormalizeRepository(repoOverride ?? GetRepoTarget());
			if (string.IsNullOrWhiteSpace(repo) || (!repo.Contains("/") && !repo.StartsWith("http", StringComparison.OrdinalIgnoreCase)))
			{
				return false;
			}

			string activeDll = targetDllOverride ?? GetActiveDllPath();
			if (string.IsNullOrEmpty(activeDll))
			{
				return false;
			}

			string apiUrl;
			if (repo.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || repo.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
			{
				apiUrl = repo;
			}
			else
			{
				var parts = repo.Split(new[] { '/' }, 2);
				string escapedOwner = Uri.EscapeDataString(parts[0]);
				string escapedRepo = Uri.EscapeDataString(parts[1]);
				apiUrl = $"https://api.github.com/repos/{escapedOwner}/{escapedRepo}/releases/latest";
			}

			string currentVer = CurrentVersion;

			if (client == null)
			{
				try
				{
					System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12 | (System.Net.SecurityProtocolType)12288 /* Tls13 */;
				}
				catch { }

				var handler = new HttpClientHandler
				{
					AllowAutoRedirect = true,
					AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
				};
				client = new HttpClient(handler);
				client.Timeout = TimeSpan.FromSeconds(30);
				try
				{
					string safeVer = Regex.Replace(currentVer ?? "2.0", @"[^a-zA-Z0-9\.\-_]", "");
					if (string.IsNullOrEmpty(safeVer)) safeVer = "2.0";
					client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "PEAK-Ascend-Updater/" + safeVer);
					client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/vnd.github.v3+json, application/octet-stream");
				}
				catch { }
				disposeClient = true;
			}

			downloadTemp = activeDll + ".download";

			string json = client.GetStringAsync(apiUrl).GetAwaiter().GetResult();
			string latestTag = ParseReleaseTag(json);
			if (string.IsNullOrEmpty(latestTag))
			{
				return false;
			}

			if (!IsNewerVersion(currentVer, latestTag))
			{
				return false;
			}

			string downloadUrl = ResolveDownloadUrl(json);
			if (string.IsNullOrEmpty(downloadUrl))
			{
				return false;
			}

			if (!Uri.TryCreate(downloadUrl, UriKind.Absolute, out Uri initialUri) ||
			    (initialUri.Scheme != Uri.UriSchemeHttp && initialUri.Scheme != Uri.UriSchemeHttps))
			{
				Kit.Log?.LogWarning($"[Updater] Invalid download URL scheme: {downloadUrl}");
				return false;
			}

			string dir = Path.GetDirectoryName(activeDll);
			if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			if (File.Exists(downloadTemp))
			{
				try { File.Delete(downloadTemp); } catch { }
			}

			string currentDownloadUrl = downloadUrl;
			using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
			{
				HttpResponseMessage response = null;
				int redirects = 0;
				while (redirects < 10)
				{
					response = client.GetAsync(currentDownloadUrl, HttpCompletionOption.ResponseHeadersRead, cts.Token).GetAwaiter().GetResult();
					int statusCode = (int)response.StatusCode;
					if ((statusCode == 301 || statusCode == 302 || statusCode == 303 || statusCode == 307 || statusCode == 308) && response.Headers.Location != null)
					{
						Uri redirectUri = response.Headers.Location;
						if (!redirectUri.IsAbsoluteUri)
						{
							redirectUri = new Uri(new Uri(currentDownloadUrl), redirectUri);
						}
						if (redirectUri.Scheme != Uri.UriSchemeHttp && redirectUri.Scheme != Uri.UriSchemeHttps)
						{
							Kit.Log?.LogWarning($"[Updater] Refusing redirect to non-HTTP(S) target: {redirectUri}");
							response.Dispose();
							return false;
						}
						currentDownloadUrl = redirectUri.ToString();
						response.Dispose();
						redirects++;
						continue;
					}
					break;
				}

				using (response)
				{
					response.EnsureSuccessStatusCode();
					using (var responseStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
					using (var fileStream = new FileStream(downloadTemp, FileMode.Create, FileAccess.Write, FileShare.None))
					{
						byte[] buffer = new byte[81920];
						int bytesRead;
						while ((bytesRead = responseStream.ReadAsync(buffer, 0, buffer.Length, cts.Token).GetAwaiter().GetResult()) > 0)
						{
							fileStream.Write(buffer, 0, bytesRead);
						}
						fileStream.Flush();
					}
				}
			}

			if (!IsValidDownloadedBinary(downloadTemp))
			{
				Kit.Log?.LogWarning("[Updater] Downloaded file is empty or invalid. Update aborted.");
				if (File.Exists(downloadTemp))
				{
					try { File.Delete(downloadTemp); } catch { }
				}
				return false;
			}

			bool swapped = PerformFileSwap(activeDll, downloadTemp);
			if (!swapped)
			{
				return false;
			}

			Kit.Log?.LogInfo($"[Updater] Successfully updated mod to {latestTag}. Terminating game process.");

			TerminateProcess?.Invoke();
			return true;
		}
		catch (Exception ex)
		{
			Kit.Log?.LogWarning($"[Updater] Update check failed: {ex.Message}");
			if (!string.IsNullOrEmpty(downloadTemp) && File.Exists(downloadTemp))
			{
				try { File.Delete(downloadTemp); } catch { }
			}
			return false;
		}
		finally
		{
			if (disposeClient && client != null)
			{
				try { client.Dispose(); } catch { }
			}
			Interlocked.Exchange(ref _isUpdating, 0);
		}
	}
}
