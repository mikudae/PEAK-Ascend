using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;

namespace JoinMasterLobby;

[BepInPlugin("com.thm.peak.joinmaster", "Join Master Lobby", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
	public const string ModName = "Join Master Lobby";
	public const string Version = "1.0.0";

	internal static ManualLogSource Log;
	internal static ConfigEntry<bool> EnableIdentitySpoofing;
	internal static ConfigEntry<string> CustomNickname;
	internal static ConfigEntry<KeyCode> ToggleKey;

	private static readonly string SharedRoomFile = Path.Combine(Path.GetTempPath(), "peak_active_room.txt");

	private bool _showWindow = true;
	private Rect _windowRect = new Rect(30f, 30f, 320f, 210f);
	private string _manualRoom = "";
	private string _manualRegion = "eu";
	private string _statusMsg = "";
	private float _statusTimer = 0f;
	private float _filePollTimer = 0f;
	private string _detectedRoom = "";
	private string _detectedRegion = "";
	private string _detectedScene = "Airport";

	private void Awake()
	{
		Log = Logger;
		EnableIdentitySpoofing = Config.Bind("Multiplayer", "EnableIdentitySpoofing", true, "Gives a distinct user ID and nickname to avoid collisions when running multiple instances on one machine.");
		CustomNickname = Config.Bind("Multiplayer", "CustomNickname", "", "Optional custom nickname. If empty, a random Scout nickname is generated.");
		ToggleKey = Config.Bind("General", "ToggleKey", KeyCode.F8, "Key to show or hide the Join Master Lobby window.");

		Log.LogInfo($"{ModName} v{Version} loaded.");

		// Apply minimal identity spoofing & anti-restart patches if enabled
		try
		{
			Harmony h = new Harmony("com.thm.peak.joinmaster");

			Type steamApi = AccessTools.TypeByName("Steamworks.SteamAPI");
			if (steamApi != null)
			{
				MethodInfo restartMethod = AccessTools.Method(steamApi, "RestartAppIfNecessary");
				if (restartMethod != null)
				{
					MethodInfo prefix = AccessTools.Method(typeof(Plugin), nameof(RestartAppIfNecessaryPrefix));
					h.Patch(restartMethod, new HarmonyMethod(prefix));
					Log.LogInfo("Patched SteamAPI.RestartAppIfNecessary.");
				}
			}

			if (EnableIdentitySpoofing.Value)
			{
				Type netUtils = AccessTools.TypeByName("Peak.Network.NetworkingUtilities");
				if (netUtils != null)
				{
					MethodInfo idMethod = AccessTools.Method(netUtils, "GetBestUserID");
					if (idMethod != null)
					{
						MethodInfo prefix = AccessTools.Method(typeof(Plugin), nameof(GetBestUserIDPrefix));
						h.Patch(idMethod, new HarmonyMethod(prefix));
					}

					MethodInfo nameMethod = AccessTools.Method(netUtils, "GetUsername");
					if (nameMethod != null)
					{
						MethodInfo prefix = AccessTools.Method(typeof(Plugin), nameof(GetUsernamePrefix));
						h.Patch(nameMethod, new HarmonyMethod(prefix));
					}
					Log.LogInfo("Patched NetworkingUtilities for multi-instance testing.");
				}
			}
		}
		catch (Exception ex)
		{
			Log.LogWarning($"Optional identity patches failed: {ex.Message}");
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(ToggleKey.Value))
		{
			_showWindow = !_showWindow;
		}

		if (_statusTimer > 0f)
		{
			_statusTimer -= Time.deltaTime;
			if (_statusTimer <= 0f)
			{
				_statusMsg = "";
			}
		}

		_filePollTimer += Time.deltaTime;
		if (_filePollTimer >= 1.5f)
		{
			_filePollTimer = 0f;
			PollRoomFile();
		}
	}

	private void PollRoomFile()
	{
		try
		{
			if (File.Exists(SharedRoomFile))
			{
				string[] lines = File.ReadAllLines(SharedRoomFile);
				if (lines.Length >= 2 && !string.IsNullOrWhiteSpace(lines[0]))
				{
					_detectedRoom = lines[0].Trim();
					_detectedRegion = lines[1].Trim();
					_detectedScene = lines.Length > 2 && !string.IsNullOrWhiteSpace(lines[2]) ? lines[2].Trim() : "Airport";
					return;
				}
			}
		}
		catch {}
		_detectedRoom = "";
		_detectedRegion = "";
	}

	public static bool JoinRoom(string roomName, string region, string scene = "Airport")
	{
		try
		{
			if (string.IsNullOrEmpty(roomName)) return false;

			JoinSpecificRoomState joinState = GameHandler.GetService<ConnectionService>().StateMachine.SwitchState<JoinSpecificRoomState>();
			if (joinState == null) return false;

			joinState.RoomName = roomName;
			joinState.RegionToJoin = region;

			RetrievableResourceSingleton<LoadingScreenHandler>.Instance.Load(
				LoadingScreen.LoadingScreenType.Basic,
				null,
				RetrievableResourceSingleton<LoadingScreenHandler>.Instance.LoadSceneProcess(scene, false, true)
			);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError($"[JoinMasterLobby] Failed to join room '{roomName}': {ex}");
			return false;
		}
	}

	private void OnGUI()
	{
		if (!_showWindow)
		{
			// Render a mini restore pill in top-left
			if (GUI.Button(new Rect(10f, 10f, 110f, 24f), "Join Lobby [" + ToggleKey.Value + "]"))
			{
				_showWindow = true;
			}
			return;
		}

		_windowRect = GUI.Window(9942, _windowRect, DrawWindow, "Join Master Lobby");
	}

	private void DrawWindow(int windowID)
	{
		GUI.DragWindow(new Rect(0f, 0f, _windowRect.width - 25f, 22f));

		if (GUI.Button(new Rect(_windowRect.width - 22f, 2f, 20f, 18f), "X"))
		{
			_showWindow = false;
		}

		GUILayout.Space(8f);

		bool inRoom = PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null;
		if (inRoom)
		{
			GUI.color = Color.green;
			GUILayout.Label($"\u25CF Connected: {PhotonNetwork.CurrentRoom.Name}");
			GUI.color = Color.white;
			GUILayout.Label($"Ping: {PhotonNetwork.GetPing()}ms | Players: {PhotonNetwork.CurrentRoom.PlayerCount}");

			GUILayout.Space(8f);
			if (GUILayout.Button("Leave Lobby", GUILayout.Height(28f)))
			{
				PhotonNetwork.Disconnect();
				SetStatus("Disconnected from room.");
			}
		}
		else
		{
			// 1. Detected Local Master Room
			if (!string.IsNullOrEmpty(_detectedRoom))
			{
				GUI.color = Color.cyan;
				GUILayout.Label($"Detected Host: {_detectedRoom} ({_detectedRegion})");
				GUI.color = Color.white;

				if (GUILayout.Button("\u25B6 Direct Join Master Lobby", GUILayout.Height(34f)))
				{
					if (JoinRoom(_detectedRoom, _detectedRegion, _detectedScene))
					{
						SetStatus("Connecting to Master lobby...");
					}
					else
					{
						SetStatus("Connection failed.");
					}
				}
			}
			else
			{
				GUI.color = Color.gray;
				GUILayout.Label("Waiting for Master lobby on local machine...");
				GUI.color = Color.white;
			}

			GUILayout.Space(6f);

			// 2. Manual Join option
			GUILayout.Label("Manual Room Join:", EditorStylesMiniLabel());
			GUILayout.BeginHorizontal();
			_manualRoom = GUILayout.TextField(_manualRoom, GUILayout.Width(180f), GUILayout.Height(24f));
			_manualRegion = GUILayout.TextField(_manualRegion, GUILayout.Width(45f), GUILayout.Height(24f));
			if (GUILayout.Button("Join", GUILayout.Height(24f)))
			{
				if (!string.IsNullOrWhiteSpace(_manualRoom))
				{
					if (JoinRoom(_manualRoom.Trim(), _manualRegion.Trim(), "Airport"))
					{
						SetStatus($"Connecting to {_manualRoom}...");
					}
					else
					{
						SetStatus("Manual join failed.");
					}
				}
			}
			GUILayout.EndHorizontal();
		}

		if (!string.IsNullOrEmpty(_statusMsg))
		{
			GUILayout.Space(4f);
			GUI.color = Color.yellow;
			GUILayout.Label(_statusMsg);
			GUI.color = Color.white;
		}
	}

	private GUIStyle EditorStylesMiniLabel()
	{
		var s = new GUIStyle(GUI.skin.label);
		s.fontSize = 11;
		s.normal.textColor = Color.gray;
		return s;
	}

	private void SetStatus(string msg)
	{
		_statusMsg = msg;
		_statusTimer = 4f;
	}

	// Harmony Prefixes for multi-client testing
	public static bool GetBestUserIDPrefix(ref string __result)
	{
		__result = "Joiner_" + UnityEngine.Random.Range(100000, 999999);
		return false;
	}

	public static bool GetUsernamePrefix(ref string __result)
	{
		if (!string.IsNullOrWhiteSpace(CustomNickname.Value))
		{
			__result = CustomNickname.Value.Trim();
		}
		else
		{
			__result = "Joiner Scout #" + UnityEngine.Random.Range(100, 999);
		}
		return false;
	}

	public static bool RestartAppIfNecessaryPrefix(ref bool __result)
	{
		__result = false;
		return false;
	}
}
