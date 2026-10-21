using System;
using System.Collections.Generic;
using System.IO;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zorro.Core;
using PhotonPlayer = Photon.Realtime.Player;

namespace Summit;

internal static class ModNet
{
	public enum ModRole
	{
		Master,
		Slave
	}

	public class SlaveInfo
	{
		public int ActorNumber;
		public string NickName = "";
		public string Version = "2.5";
		public float LastSeen;
	}

	public const byte BeaconEventCode = 142;
	public const byte PermEventCode = 143;
	public const byte InjectItemEventCode = 144;

	public const string PropGlobalPerms = "THM_GlobalPerms";
	public const string PropSlavePermPrefix = "THM_Perm_";

	public static readonly string[] ManageableSections = new string[10]
	{
		"Home", "Scout", "Movement", "Items", "Teleport", "World", "Camera", "Help", "Troll", "About"
	};

	// Master permission tracking
	public static readonly HashSet<string> GlobalAllowedSections = new HashSet<string>
	{
		"Home", "Scout", "About"
	};
	public static readonly Dictionary<int, HashSet<string>> SlaveOverrides = new Dictionary<int, HashSet<string>>();

	// Slave permission tracking
	public static readonly HashSet<string> LocalSlaveGlobalSections = new HashSet<string>
	{
		"Home", "Scout", "About"
	};
	public static HashSet<string> LocalSlaveOverrideSections = null;

#if SLAVE_BUILD
	public static ModRole Role = ModRole.Slave;
#else
	public static ModRole Role = ModRole.Master;
#endif

	public static bool IsPhotonConnected
	{
		get
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
	}
	private static bool initialized = false;
	private static float broadcastTimer = 0f;
	private static float cleanupTimer = 0f;

	public static readonly Dictionary<int, SlaveInfo> DetectedSlaves = new Dictionary<int, SlaveInfo>();

	public static bool IsSectionAllowed(string section)
	{
		if (Role == ModRole.Master)
		{
			return true;
		}
		if (LocalSlaveOverrideSections != null)
		{
			return LocalSlaveOverrideSections.Contains(section);
		}
		return LocalSlaveGlobalSections.Contains(section);
	}

	public static bool IsGlobalSectionAllowed(string section)
	{
		return GlobalAllowedSections.Contains(section);
	}

	public static bool HasSlaveOverride(int actorNumber)
	{
		return SlaveOverrides.ContainsKey(actorNumber);
	}

	public static bool IsSlaveSectionAllowed(int actorNumber, string section)
	{
		if (SlaveOverrides.TryGetValue(actorNumber, out var set))
		{
			return set.Contains(section);
		}
		return GlobalAllowedSections.Contains(section);
	}

	public static void ToggleGlobalSection(string section)
	{
		if (GlobalAllowedSections.Contains(section))
		{
			GlobalAllowedSections.Remove(section);
		}
		else
		{
			GlobalAllowedSections.Add(section);
		}
		SyncGlobalPermissions();
		Menu.Instance?.RefreshTabs();
	}

	public static void SetAllGlobalSections(bool allowed)
	{
		GlobalAllowedSections.Clear();
		if (allowed)
		{
			foreach (string s in ManageableSections)
			{
				GlobalAllowedSections.Add(s);
			}
		}
		SyncGlobalPermissions();
		Menu.Instance?.RefreshTabs();
	}

	public static void ToggleSlaveSection(int actorNumber, string section)
	{
		if (!SlaveOverrides.TryGetValue(actorNumber, out var set))
		{
			set = new HashSet<string>(GlobalAllowedSections);
			SlaveOverrides[actorNumber] = set;
		}

		if (set.Contains(section))
		{
			set.Remove(section);
		}
		else
		{
			set.Add(section);
		}
		SyncSlavePermissions(actorNumber);
		Menu.Instance?.RefreshTabs();
	}

	public static void SetAllSlaveSections(int actorNumber, bool allowed)
	{
		HashSet<string> set = new HashSet<string>();
		if (allowed)
		{
			foreach (string s in ManageableSections)
			{
				set.Add(s);
			}
		}
		SlaveOverrides[actorNumber] = set;
		SyncSlavePermissions(actorNumber);
		Menu.Instance?.RefreshTabs();
	}

	public static void ClearSlaveOverride(int actorNumber)
	{
		if (SlaveOverrides.Remove(actorNumber))
		{
			SyncSlaveClearOverride(actorNumber);
			Menu.Instance?.RefreshTabs();
		}
	}

	public static List<string> GetLocalSlaveAllowedSections()
	{
		List<string> list = new List<string>();
		foreach (string s in ManageableSections)
		{
			if (IsSectionAllowed(s))
			{
				list.Add(s);
			}
		}
		return list;
	}

	public static bool HasLocalSlaveOverride()
	{
		return LocalSlaveOverrideSections != null;
	}

	public static bool DeliverItemToScout(int targetActor, string destination, string itemPrefabName)
	{
		try
		{
			if (!IsPhotonConnected) return false;

			string resolvedPrefab = itemPrefabName;
			try
			{
				var list = Features.ItemList();
				if (list != null)
				{
					Item match = list.Find(x => x != null && (
						((UnityEngine.Object)x).name.Equals(itemPrefabName, StringComparison.OrdinalIgnoreCase) ||
						(x.GetName() != null && x.GetName().Equals(itemPrefabName, StringComparison.OrdinalIgnoreCase)) ||
						((UnityEngine.Object)x).name.Replace(" ", "").Equals(itemPrefabName.Replace(" ", "").Replace("-", ""), StringComparison.OrdinalIgnoreCase) ||
						(x.GetName() != null && x.GetName().Replace(" ", "").Equals(itemPrefabName.Replace(" ", "").Replace("-", ""), StringComparison.OrdinalIgnoreCase))
					));
					if (match != null)
					{
						resolvedPrefab = ((UnityEngine.Object)match).name;
					}
				}
			}
			catch {}

			try
			{
				RaiseEventOptions options = new RaiseEventOptions
				{
					TargetActors = new int[] { targetActor }
				};
				string payload = "INJECT|" + targetActor + "|" + destination + "|" + resolvedPrefab;
				PhotonNetwork.RaiseEvent(InjectItemEventCode, payload, options, SendOptions.SendReliable);
				DebugBridge.LogPhoton($"Sent INJECT event to actor {targetActor} for {resolvedPrefab} ({destination})");
			}
			catch {}

			Character targetChar = null;
			if (!PlayerHandler.TryGetCharacter(targetActor, out targetChar) || targetChar == null)
			{
				foreach (var other in Features.Others())
				{
					if (other != null)
					{
						PhotonView opv = other.GetComponent<PhotonView>() ?? (other.refs != null ? other.refs.view : null);
						if (opv != null && opv.Owner != null && opv.Owner.ActorNumber == targetActor)
						{
							targetChar = other;
							break;
						}
					}
				}
				if (targetChar == null && Character.localCharacter != null)
				{
					PhotonView lpv = Character.localCharacter.GetComponent<PhotonView>() ?? (Character.localCharacter.refs != null ? Character.localCharacter.refs.view : null);
					if (lpv != null && lpv.Owner != null && lpv.Owner.ActorNumber == targetActor)
					{
						targetChar = Character.localCharacter;
					}
				}
			}

			if (targetChar != null)
			{
				PhotonView targetPv = targetChar.GetComponent<PhotonView>() ?? (targetChar.refs != null ? targetChar.refs.view : null);
				Vector3 forward = (targetChar.data != null && targetChar.data.lookDirection_Flat.sqrMagnitude > 0.01f)
					? targetChar.data.lookDirection_Flat
					: targetChar.transform.forward;
				Vector3 spawnPoint = targetChar.Center + forward * 0.85f + Vector3.up * 0.25f;

				GameObject spawnedObj = PhotonNetwork.Instantiate("0_Items/" + resolvedPrefab, spawnPoint, Quaternion.identity, 0);
				if (spawnedObj != null && targetPv != null)
				{
					if (PhotonNetwork.IsMasterClient)
					{
						Item spawnedItem = spawnedObj.GetComponent<Item>();
						if (spawnedItem != null)
						{
							spawnedItem.RequestPickup(targetPv);
						}
					}
					else
					{
						PhotonView itemPv = spawnedObj.GetComponent<PhotonView>();
						if (itemPv != null)
						{
							itemPv.RPC("RequestPickup", RpcTarget.MasterClient, targetPv);
						}
						else
						{
							Item spawnedItem = spawnedObj.GetComponent<Item>();
							if (spawnedItem != null)
							{
								spawnedItem.RequestPickup(targetPv);
							}
						}
					}
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[ModNet] DeliverItemToScout failed: " + ex.Message);
			return false;
		}
	}

	public static void HandleInjectedItem(string destination, string itemName)
	{
		try
		{
			string destText = destination == "Backpack" ? "backpack/gear" : "hands";
			Menu.Instance?.Say(Lang.F("Master delivered {0} to your {1}", itemName, destText));
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[ModNet] HandleInjectedItem error: " + ex);
		}
	}

	public static void GetPlayerInventory(Character c, out string currentItem, out List<string> pockets, out List<string> backpack)
	{
		currentItem = "None";
		pockets = new List<string>();
		backpack = new List<string>();

		if (c == null) return;
		try
		{
			if (c.data != null && c.data.currentItem != null)
			{
				currentItem = c.data.currentItem.name;
			}
			if (c.player != null)
			{
				if (c.player.itemSlots != null)
				{
					for (int i = 0; i < c.player.itemSlots.Length; i++)
					{
						var slot = c.player.itemSlots[i];
						if (slot != null && !slot.IsEmpty() && slot.prefab != null)
						{
							pockets.Add(slot.prefab.name);
						}
						else
						{
							pockets.Add("Empty");
						}
					}
				}
				if (c.player.backpackSlot != null && !c.player.backpackSlot.IsEmpty() && c.player.backpackSlot.prefab != null)
				{
					string bpName = c.player.backpackSlot.prefab.name;
					backpack.Add(bpName);
					if (c.player.backpackSlot.prefab is Backpack bp)
					{
						var bpData = bp.GetData<BackpackData>(DataEntryKey.BackpackData);
						if (bpData != null && bpData.itemSlots != null)
						{
							foreach (var bs in bpData.itemSlots)
							{
								if (bs != null && !bs.IsEmpty() && bs.prefab != null)
								{
									backpack.Add(bs.prefab.name);
								}
							}
						}
					}
				}
			}
		}
		catch
		{
		}
	}

	public static void SyncGlobalPermissions()
	{
		try
		{
			if (!IsPhotonConnected || PhotonNetwork.CurrentRoom == null) return;
			string globalCsv = string.Join(",", GlobalAllowedSections);

			Hashtable props = new Hashtable();
			props[PropGlobalPerms] = globalCsv;
			PhotonNetwork.CurrentRoom.SetCustomProperties(props);

			RaiseEventOptions options = new RaiseEventOptions
			{
				Receivers = ReceiverGroup.Others
			};
			PhotonNetwork.RaiseEvent(PermEventCode, "PERM_G|" + globalCsv, options, SendOptions.SendReliable);
			DebugBridge.LogPhoton($"Synced global permissions: {globalCsv}");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[ModNet] SyncGlobalPermissions failed: " + ex.Message);
		}
	}

	public static void SyncSlavePermissions(int actorNumber)
	{
		try
		{
			if (!IsPhotonConnected || PhotonNetwork.CurrentRoom == null) return;
			if (!SlaveOverrides.TryGetValue(actorNumber, out var set)) return;
			string slaveCsv = string.Join(",", set);

			Hashtable props = new Hashtable();
			props[PropSlavePermPrefix + actorNumber] = slaveCsv;
			PhotonNetwork.CurrentRoom.SetCustomProperties(props);

			RaiseEventOptions options = new RaiseEventOptions
			{
				Receivers = ReceiverGroup.Others
			};
			PhotonNetwork.RaiseEvent(PermEventCode, "PERM_S|" + actorNumber + "|" + slaveCsv, options, SendOptions.SendReliable);
			DebugBridge.LogPhoton($"Synced slave permissions for actor {actorNumber}: {slaveCsv}");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[ModNet] SyncSlavePermissions failed: " + ex.Message);
		}
	}

	public static void SyncSlaveClearOverride(int actorNumber)
	{
		try
		{
			if (!IsPhotonConnected || PhotonNetwork.CurrentRoom == null) return;

			Hashtable props = new Hashtable();
			props[PropSlavePermPrefix + actorNumber] = null;
			PhotonNetwork.CurrentRoom.SetCustomProperties(props);

			RaiseEventOptions options = new RaiseEventOptions
			{
				Receivers = ReceiverGroup.Others
			};
			PhotonNetwork.RaiseEvent(PermEventCode, "PERM_C|" + actorNumber, options, SendOptions.SendReliable);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[ModNet] SyncSlaveClearOverride failed: " + ex.Message);
		}
	}

	public static void SyncAllPermissionsToSlave(int actorNumber)
	{
		try
		{
			if (!IsPhotonConnected) return;

			RaiseEventOptions options = new RaiseEventOptions
			{
				TargetActors = new int[] { actorNumber }
			};

			string globalCsv = string.Join(",", GlobalAllowedSections);
			PhotonNetwork.RaiseEvent(PermEventCode, "PERM_G|" + globalCsv, options, SendOptions.SendReliable);

			if (SlaveOverrides.TryGetValue(actorNumber, out var set))
			{
				string slaveCsv = string.Join(",", set);
				PhotonNetwork.RaiseEvent(PermEventCode, "PERM_S|" + actorNumber + "|" + slaveCsv, options, SendOptions.SendReliable);
			}
			else
			{
				PhotonNetwork.RaiseEvent(PermEventCode, "PERM_C|" + actorNumber, options, SendOptions.SendReliable);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[ModNet] SyncAllPermissionsToSlave failed: " + ex.Message);
		}
	}

	public static void UpdateLocalGlobalPermissions(string csv)
	{
		LocalSlaveGlobalSections.Clear();
		if (!string.IsNullOrEmpty(csv))
		{
			string[] items = csv.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string item in items)
			{
				LocalSlaveGlobalSections.Add(item.Trim());
			}
		}
		Menu.Instance?.RefreshTabs();
	}

	public static void UpdateLocalSlaveOverride(string csv)
	{
		HashSet<string> set = new HashSet<string>();
		if (!string.IsNullOrEmpty(csv))
		{
			string[] items = csv.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string item in items)
			{
				set.Add(item.Trim());
			}
		}
		LocalSlaveOverrideSections = set;
		Menu.Instance?.RefreshTabs();
	}

	public static void ClearLocalSlaveOverride()
	{
		LocalSlaveOverrideSections = null;
		Menu.Instance?.RefreshTabs();
	}

	private static void HandlePermPayload(string payload)
	{
		if (string.IsNullOrEmpty(payload)) return;

		if (payload.StartsWith("PERM_G|"))
		{
			string csv = payload.Substring(7);
			UpdateLocalGlobalPermissions(csv);
		}
		else if (payload.StartsWith("PERM_S|"))
		{
			string[] parts = payload.Split('|');
			if (parts.Length >= 3 && int.TryParse(parts[1], out int targetActor))
			{
				if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.ActorNumber == targetActor)
				{
					UpdateLocalSlaveOverride(parts[2]);
				}
			}
		}
		else if (payload.StartsWith("PERM_C|"))
		{
			string[] parts = payload.Split('|');
			if (parts.Length >= 2 && int.TryParse(parts[1], out int targetActor))
			{
				if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.ActorNumber == targetActor)
				{
					ClearLocalSlaveOverride();
				}
			}
		}
	}

	private static void ScanSlaveRoomProperties()
	{
		try
		{
			if (!IsPhotonConnected || PhotonNetwork.CurrentRoom == null) return;

			var cp = PhotonNetwork.CurrentRoom.CustomProperties;
			if (cp == null) return;

			if (cp.ContainsKey(PropGlobalPerms))
			{
				string g = cp[PropGlobalPerms]?.ToString() ?? "";
				string currentGlobalCsv = string.Join(",", LocalSlaveGlobalSections);
				if (g != currentGlobalCsv)
				{
					UpdateLocalGlobalPermissions(g);
				}
			}

			if (PhotonNetwork.LocalPlayer != null)
			{
				string slaveKey = PropSlavePermPrefix + PhotonNetwork.LocalPlayer.ActorNumber;
				if (cp.ContainsKey(slaveKey))
				{
					string s = cp[slaveKey]?.ToString() ?? "";
					string currentSlaveCsv = LocalSlaveOverrideSections != null ? string.Join(",", LocalSlaveOverrideSections) : null;
					if (s != currentSlaveCsv)
					{
						UpdateLocalSlaveOverride(s);
					}
				}
				else if (LocalSlaveOverrideSections != null)
				{
					ClearLocalSlaveOverride();
				}
			}
		}
		catch
		{
		}
	}

	public static void Init()
	{
		if (initialized) return;
		try
		{
			if (PhotonNetwork.NetworkingClient != null)
			{
				PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
				initialized = true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[ModNet] Failed to register EventReceived: " + ex.Message);
		}
	}

	public static void Tick(float dt)
	{
		if (!initialized && PhotonNetwork.NetworkingClient != null)
		{
			Init();
		}

		if (!IsPhotonConnected)
		{
			if (DetectedSlaves.Count > 0)
			{
				DetectedSlaves.Clear();
			}
			bool needsReset = LocalSlaveOverrideSections != null ||
				LocalSlaveGlobalSections.Count != 3 ||
				!LocalSlaveGlobalSections.Contains("Home") ||
				!LocalSlaveGlobalSections.Contains("Scout") ||
				!LocalSlaveGlobalSections.Contains("About");
			if (needsReset)
			{
				LocalSlaveGlobalSections.Clear();
				LocalSlaveGlobalSections.Add("Home");
				LocalSlaveGlobalSections.Add("Scout");
				LocalSlaveGlobalSections.Add("About");
				LocalSlaveOverrideSections = null;
				Menu.Instance?.RefreshTabs();
			}
			return;
		}

		broadcastTimer += dt;
		cleanupTimer += dt;

		// 1. Slave broadcast & perm check logic
		if (Role == ModRole.Slave)
		{
			if (broadcastTimer >= 3f)
			{
				broadcastTimer = 0f;
				BroadcastSlavePresence();
				SaveActiveSlaveRoom();
			}

			if (cleanupTimer >= 2f)
			{
				cleanupTimer = 0f;
				ScanSlaveRoomProperties();
			}
		}

		// 2. Master discovery & cleanup logic
		if (Role == ModRole.Master)
		{
			// Scan Room player list CustomProperties every 2 seconds
			if (broadcastTimer >= 2f)
			{
				broadcastTimer = 0f;
				ScanRoomProperties();
				SaveActiveRoom();
			}

			// Clean up stale or disconnected players
			if (cleanupTimer >= 5f)
			{
				cleanupTimer = 0f;
				PruneDisconnectedPlayers();
			}
		}
	}

	private static string lastSavedRoom = "";
	public static readonly string SharedRoomFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "peak_active_room.txt");

	private static string lastSavedSlaveRoom = "";
	public static readonly string SharedSlaveRoomFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "peak_active_slave_room.txt");

	public static void SaveActiveRoom()
	{
		try
		{
			if (IsPhotonConnected && PhotonNetwork.CurrentRoom != null && !string.IsNullOrEmpty(PhotonNetwork.CurrentRoom.Name))
			{
				string roomName = PhotonNetwork.CurrentRoom.Name;
				string region = PhotonNetwork.CloudRegion ?? "";
				string scene = SceneManager.GetActiveScene().name;
				string content = roomName + "\n" + region + "\n" + scene;
				if (content != lastSavedRoom)
				{
					System.IO.File.WriteAllText(SharedRoomFile, content);
					lastSavedRoom = content;
				}
			}
		}
		catch
		{
		}
	}

	public static void SaveActiveSlaveRoom()
	{
		try
		{
			if (IsPhotonConnected && PhotonNetwork.CurrentRoom != null && !string.IsNullOrEmpty(PhotonNetwork.CurrentRoom.Name))
			{
				string roomName = PhotonNetwork.CurrentRoom.Name;
				string region = PhotonNetwork.CloudRegion ?? "";
				string scene = SceneManager.GetActiveScene().name;
				string scout = (PhotonNetwork.LocalPlayer != null && !string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.NickName))
					? PhotonNetwork.LocalPlayer.NickName
					: "Slave Scout";
				string content = roomName + "\n" + region + "\n" + scene + "\n" + scout;
				if (content != lastSavedSlaveRoom)
				{
					System.IO.File.WriteAllText(SharedSlaveRoomFile, content);
					lastSavedSlaveRoom = content;
				}
			}
		}
		catch
		{
		}
	}

	public static string GetActiveMasterRoomInfo()
	{
		try
		{
			if (System.IO.File.Exists(SharedRoomFile))
			{
				if (DateTime.UtcNow - System.IO.File.GetLastWriteTimeUtc(SharedRoomFile) > TimeSpan.FromMinutes(5))
				{
					return null;
				}
				string[] lines = System.IO.File.ReadAllLines(SharedRoomFile);
				if (lines.Length >= 2 && !string.IsNullOrEmpty(lines[0]))
				{
					return lines[0].Trim();
				}
			}
		}
		catch
		{
		}
		return null;
	}

	public static string GetActiveSlaveRoomInfo()
	{
		try
		{
			if (System.IO.File.Exists(SharedSlaveRoomFile))
			{
				if (DateTime.UtcNow - System.IO.File.GetLastWriteTimeUtc(SharedSlaveRoomFile) > TimeSpan.FromMinutes(5))
				{
					return null;
				}
				string[] lines = System.IO.File.ReadAllLines(SharedSlaveRoomFile);
				if (lines.Length >= 2 && !string.IsNullOrEmpty(lines[0]))
				{
					string room = lines[0].Trim();
					string scout = lines.Length > 3 ? lines[3].Trim() : "";
					return string.IsNullOrEmpty(scout) ? room : (room + " (" + scout + ")");
				}
			}
		}
		catch
		{
		}
		return null;
	}

	public static bool JoinRoomByDetails(string roomName, string region, string scene = "Airport")
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
			Debug.LogError("[ModNet] Failed to join room '" + roomName + "': " + ex);
			return false;
		}
	}

	public static bool JoinMasterRoom()
	{
		try
		{
			if (!System.IO.File.Exists(SharedRoomFile))
			{
				return false;
			}
			string[] lines = System.IO.File.ReadAllLines(SharedRoomFile);
			if (lines.Length < 2) return false;
			string roomName = lines[0].Trim();
			string region = lines[1].Trim();
			string scene = lines.Length > 2 ? lines[2].Trim() : "Airport";

			return JoinRoomByDetails(roomName, region, scene);
		}
		catch (Exception ex)
		{
			Debug.LogError("[ModNet] Failed to join master room: " + ex);
			return false;
		}
	}

	public static bool JoinSlaveRoom()
	{
		try
		{
			if (!System.IO.File.Exists(SharedSlaveRoomFile))
			{
				return false;
			}
			string[] lines = System.IO.File.ReadAllLines(SharedSlaveRoomFile);
			if (lines.Length < 2) return false;
			string roomName = lines[0].Trim();
			string region = lines[1].Trim();
			string scene = lines.Length > 2 ? lines[2].Trim() : "Airport";

			return JoinRoomByDetails(roomName, region, scene);
		}
		catch (Exception ex)
		{
			Debug.LogError("[ModNet] Failed to join slave room: " + ex);
			return false;
		}
	}

	private static void BroadcastSlavePresence()
	{
		try
		{
			if (!IsPhotonConnected || PhotonNetwork.LocalPlayer == null) return;

			// Sync via CustomProperties
			Hashtable table = new Hashtable();
			table["THM_Role"] = "Slave";
			table["THM_Ver"] = Kit.Version;
			PhotonNetwork.LocalPlayer.SetCustomProperties(table);

			// Also send via reliable custom PUN event code 142
			RaiseEventOptions options = new RaiseEventOptions
			{
				Receivers = ReceiverGroup.Others
			};
			PhotonNetwork.RaiseEvent(BeaconEventCode, "THM_SLAVE|" + Kit.Version + "|" + PhotonNetwork.LocalPlayer.NickName, options, SendOptions.SendReliable);
			DebugBridge.LogPhoton($"Broadcasted presence beacon (Role: Slave, Ver: {Kit.Version})");
		}
		catch
		{
		}
	}

	private static void ScanRoomProperties()
	{
		try
		{
			if (!IsPhotonConnected || PhotonNetwork.CurrentRoom == null) return;

			foreach (PhotonPlayer p in PhotonNetwork.PlayerListOthers)
			{
				if (p == null) continue;
				if (p.CustomProperties != null && p.CustomProperties.ContainsKey("THM_Role"))
				{
					if (p.CustomProperties["THM_Role"]?.ToString() == "Slave")
					{
						string ver = p.CustomProperties.ContainsKey("THM_Ver") ? p.CustomProperties["THM_Ver"]?.ToString() ?? "1.1.1" : "1.1.1";
						bool isNew = false;
						lock (DetectedSlaves)
						{
							isNew = !DetectedSlaves.ContainsKey(p.ActorNumber);
						}
						RecordSlave(p.ActorNumber, p.NickName, ver);
						if (isNew)
						{
							SyncAllPermissionsToSlave(p.ActorNumber);
						}
					}
				}
			}
		}
		catch
		{
		}
	}

	private static void OnEventReceived(EventData photonEvent)
	{
		DebugBridge.LogPhoton($"Event {photonEvent.Code} received from actor {photonEvent.Sender}");
		if (photonEvent.Code == BeaconEventCode)
		{
			if (Role == ModRole.Master)
			{
				int sender = photonEvent.Sender;
				string payload = photonEvent.CustomData as string ?? "";
				string ver = "2.5";
				string name = "";
				string[] parts = payload.Split('|');
				if (parts.Length > 1) ver = parts[1];
				if (parts.Length > 2) name = parts[2];

				if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
				{
					PhotonPlayer p = PhotonNetwork.CurrentRoom.GetPlayer(sender);
					if (p != null && !string.IsNullOrEmpty(p.NickName))
					{
						name = p.NickName;
					}
				}

				RecordSlave(sender, name, ver);
				SyncAllPermissionsToSlave(sender);
			}
		}
		else if (photonEvent.Code == PermEventCode)
		{
			if (Role == ModRole.Slave)
			{
				string payload = photonEvent.CustomData as string ?? "";
				HandlePermPayload(payload);
			}
		}
		else if (photonEvent.Code == InjectItemEventCode)
		{
			string payload = photonEvent.CustomData as string ?? "";
			if (payload.StartsWith("INJECT|"))
			{
				string[] parts = payload.Split('|');
				if (parts.Length >= 4 && int.TryParse(parts[1], out int targetActor))
				{
					if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.ActorNumber == targetActor)
					{
						string dest = parts[2];
						string itemName = parts[3];
						HandleInjectedItem(dest, itemName);
					}
				}
			}
		}
	}

	private static void RecordSlave(int actorNumber, string name, string version)
	{
		if (actorNumber <= 0) return;
		lock (DetectedSlaves)
		{
			if (!DetectedSlaves.TryGetValue(actorNumber, out var info))
			{
				info = new SlaveInfo
				{
					ActorNumber = actorNumber
				};
				DetectedSlaves[actorNumber] = info;
			}
			info.NickName = string.IsNullOrEmpty(name) ? ("Scout #" + actorNumber) : name;
			info.Version = version;
			info.LastSeen = Time.unscaledTime;
		}
	}

	private static void PruneDisconnectedPlayers()
	{
		if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
		{
			DetectedSlaves.Clear();
			SlaveOverrides.Clear();
			return;
		}

		lock (DetectedSlaves)
		{
			List<int> toRemove = new List<int>();
			foreach (var kvp in DetectedSlaves)
			{
				PhotonPlayer p = PhotonNetwork.CurrentRoom.GetPlayer(kvp.Key);
				if (p == null)
				{
					toRemove.Add(kvp.Key);
				}
			}
			foreach (int key in toRemove)
			{
				DetectedSlaves.Remove(key);
				SlaveOverrides.Remove(key);
			}
		}
	}

	public static bool IsSlave(Character c)
	{
		if (c == null) return false;
		try
		{
			PhotonView pv = c.GetComponent<PhotonView>();
			if (pv == null && c.refs != null)
			{
				pv = c.refs.view;
			}
			if (pv != null && pv.Owner != null)
			{
				return IsSlave(pv.Owner.ActorNumber);
			}
		}
		catch
		{
		}
		return false;
	}

	public static bool IsSlave(int actorNumber)
	{
		if (Role != ModRole.Master) return false;
		lock (DetectedSlaves)
		{
			return DetectedSlaves.ContainsKey(actorNumber);
		}
	}
}
