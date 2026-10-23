using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;
using Object = UnityEngine.Object;
using STATUSTYPE = CharacterAfflictions.STATUSTYPE;

namespace Summit;

internal static class Features
{
	public static Action<string> Say = delegate
	{
	};

	private static readonly MethodInfo UpdateWeight = AccessTools.Method(typeof(CharacterAfflictions), "UpdateWeight", (Type[])null, (Type[])null);

	private static readonly FieldInfo AllCampfires = AccessTools.Field(typeof(Campfire), "ALL_CAMPFIRES");

	private static CharacterMovement moveOwner;

	private static CharacterClimbing climbOwner;

	private static float jumpBase;

	private static float climbBase;

	private static float clipAsserted;

	private static bool stamTouched;

	private static bool jumpTouched;

	private static bool clipOff;

	private static bool lastNoWeight;

	private static bool hudTouched;

	internal static readonly List<Campfire> Fires = new List<Campfire>();

	internal static readonly List<RespawnChest> Statues = new List<RespawnChest>();

	internal static readonly List<KeyValuePair<Transform, string>> Hazards = new List<KeyValuePair<Transform, string>>();

	private static float nextLook;

	private static readonly string[] MobNames = new string[3]
	{
		Lang.K("Beetle"),
		Lang.K("Frog tongue"),
		Lang.K("Scorpion")
	};

	public static Character Me => Character.localCharacter;

	public static float JumpBase => jumpBase;

	public static bool InGame
	{
		get
		{
			try
			{
				return (Object)(object)Me != (Object)null && Me.refs != null && (Object)(object)Me.refs.movement != (Object)null;
			}
			catch
			{
				return false;
			}
		}
	}

	public static void Heal()
	{
		Me.refs.afflictions.ClearAllStatus(false, false);
		Character.GainFullStamina();
		Say(Lang.T("Statuses cleared, stamina full"));
	}

	public static void BonusStamina()
	{
		Me.SetExtraStamina(1f);
		Say(Lang.T("Bonus stamina bar filled"));
	}

	public static void Revive()
	{
		if (!Me.data.dead && !Me.data.fullyPassedOut)
		{
			Say(Lang.T("You are not down"));
			return;
		}
		Character.Revive();
		Say(Lang.T("Revived where you went down"));
	}

	public static void KillSelf()
	{
		if ((UnityEngine.Object)(object)Me == (UnityEngine.Object)null)
		{
			return;
		}
		if (Me.data.dead)
		{
			Say(Lang.T("You are already dead"));
			return;
		}
		Character.Die();
		Say(Lang.T("Killed self"));
	}

	public static void Warp(Vector3 position, string what)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		((MonoBehaviourPun)Me).photonView.RPC("WarpPlayerRPC", (RpcTarget)0, new object[2] { position, true });
		Say(Lang.F("Teleported to {0}", what));
	}

	public static void WarpToAim()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)MainCamera.instance == (Object)null))
		{
			Transform transform = ((Component)MainCamera.instance).transform;
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(transform.position + transform.forward * 2f, transform.forward, out val, 2000f, -1, (QueryTriggerInteraction)1))
			{
				Warp(val.point + val.normal * 1.5f, Lang.T("where you were looking"));
			}
			else
			{
				Say(Lang.T("Nothing in sight to land on"));
			}
		}
	}

	public static void WarpToCampfire()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Campfire val = null;
		try
		{
			val = MapHandler.CurrentCampfire;
		}
		catch
		{
		}
		if ((Object)(object)val == (Object)null)
		{
			Say(Lang.T("No campfire on this part of the map"));
		}
		else
		{
			Warp(((Component)val).transform.position + new Vector3(2f, 2f, 0f), Lang.T("the campfire"));
		}
	}

	public static void WarpToStatue()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		RespawnChest val = null;
		try
		{
			val = MapHandler.CurrentScoutStatue;
		}
		catch
		{
		}
		if ((Object)(object)val == (Object)null)
		{
			Say(Lang.T("No scout statue on this part of the map"));
		}
		else
		{
			Warp(((Component)val).transform.position + new Vector3(2f, 2f, 0f), Lang.T("the scout statue"));
		}
	}

	public static void SaveSpot(int i)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Live.Spots[i] = Me.Center;
		Say(Lang.F("Spot {0} remembered", i + 1));
	}

	public static void GoSpot(int i)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (!Live.Spots[i].HasValue)
		{
			Say(Lang.F("Spot {0} is empty: remember a place first", i + 1));
		}
		else
		{
			Warp(Live.Spots[i].Value + Vector3.up * 0.5f, Lang.F("spot {0}", i + 1));
		}
	}

	public static List<Character> Others()
	{
		List<Character> list = new List<Character>();
		foreach (Character allCharacter in Character.AllCharacters)
		{
			if ((Object)(object)allCharacter != (Object)null && (Object)(object)allCharacter != (Object)(object)Me && !allCharacter.isBot)
			{
				list.Add(allCharacter);
			}
		}
		return list;
	}

	public static void SetFreeCam(bool on)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (on && (Object)(object)MainCamera.instance != (Object)null)
		{
			Transform transform = ((Component)MainCamera.instance).transform;
			Live.CamPos = transform.position;
			Live.CamYaw = transform.eulerAngles.y;
			float x = transform.eulerAngles.x;
			Live.CamPitch = ((x > 180f) ? (x - 360f) : x);
		}
		Live.FreeCam = on;
		Say(Lang.T(on ? "Free camera: on. Move keys fly the view, jump goes up, crouch goes down, sprint goes faster." : "Free camera: off"));
	}

	public static bool Down(Character c)
	{
		if (!c.data.dead)
		{
			return c.data.fullyPassedOut;
		}
		return true;
	}

	public static void ReviveOther(Character c, bool byMe)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (!Down(c))
		{
			Say(Lang.T("That scout is not down"));
			return;
		}
		Vector3 val = (byMe ? (Me.Center + Me.data.lookDirection_Flat * 2f + Vector3.up) : (c.Center + Vector3.up));
		((MonoBehaviourPun)c).photonView.RPC("RPCA_ReviveAtPosition", (RpcTarget)0, new object[3] { val, false, -1 });
	}

	public static bool HealOther(Character c)
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			return false;
		}
		int num = Enum.GetNames(typeof(STATUSTYPE)).Length;
		float[] array = new float[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = -5f;
		}
		((MonoBehaviourPun)c).photonView.RPC("RPC_ApplyStatusesFromFloatArray", (RpcTarget)0, new object[1] { array });
		return true;
	}

	public static void HandItem(Character c, Item item)
	{
		if ((Object)(object)c == (Object)null || (Object)(object)item == (Object)null) return;
		string prefabName = ((Object)item).name;

		PhotonView pv = c.GetComponent<PhotonView>();
		if (pv == null && c.refs != null)
		{
			pv = c.refs.view;
		}

		int actor = (pv != null && pv.Owner != null) ? pv.Owner.ActorNumber : -1;
		if (actor > 0)
		{
			ModNet.DeliverItemToScout(actor, "Hands", prefabName);
		}
		else
		{
			Vector3 spawnPoint = c.Center + Vector3.down * 500f;
			GameObject obj = PhotonNetwork.Instantiate("0_Items/" + prefabName, spawnPoint, Quaternion.identity, (byte)0, (object[])null);
			if (obj != null)
			{
				try
				{
					Rigidbody rb = obj.GetComponent<Rigidbody>();
					if (rb != null) rb.isKinematic = true;
					foreach (var rend in obj.GetComponentsInChildren<Renderer>(true)) rend.enabled = false;
					foreach (var col in obj.GetComponentsInChildren<Collider>(true)) col.enabled = false;
				}
				catch {}

				if (pv != null)
				{
					if (PhotonNetwork.IsMasterClient)
					{
						Item spawnedItem = obj.GetComponent<Item>();
						if (spawnedItem != null)
						{
							spawnedItem.RequestPickup(pv);
						}
					}
					else
					{
						PhotonView itemPv = obj.GetComponent<PhotonView>();
						if (itemPv != null)
						{
							itemPv.RPC("RequestPickup", RpcTarget.MasterClient, pv);
						}
						else
						{
							Item spawnedItem = obj.GetComponent<Item>();
							if (spawnedItem != null)
							{
								spawnedItem.RequestPickup(pv);
							}
						}
					}
				}

				ModNet.CleanupSpawnedItem(obj, 0.35f);
			}
		}
	}

	public static int[] Wearing()
	{
		CharacterCustomizationData customizationData = GameHandler.GetService<PersistentPlayerDataService>().GetPlayerData(PhotonNetwork.LocalPlayer).customizationData;
		return new int[8] { customizationData.currentSkin, customizationData.currentAccessory, customizationData.currentEyes, customizationData.currentMouth, customizationData.currentOutfit, customizationData.currentHat, customizationData.currentSash, customizationData.currentMedal };
	}

	public static int[] Outfit(int slot)
	{
		string[] array = (Plugin.Outfits[slot].Value ?? "").Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 8)
		{
			return null;
		}
		int[] array2 = new int[8];
		for (int i = 0; i < 8; i++)
		{
			if (!int.TryParse(array[i], out array2[i]) || array2[i] < 0)
			{
				return null;
			}
		}
		return array2;
	}

	public static void SaveOutfit(int slot)
	{
		Plugin.Outfits[slot].Value = string.Join(",", Array.ConvertAll(Wearing(), (int x) => x.ToString()));
		Say(Lang.F("Outfit {0} saved", slot + 1));
	}

	public static void Wear(int[] v)
	{
		CharacterCustomization.SetCharacterSkinColor(v[0]);
		CharacterCustomization.SetCharacterAccessory(v[1]);
		CharacterCustomization.SetCharacterEyes(v[2]);
		CharacterCustomization.SetCharacterMouth(v[3]);
		CharacterCustomization.SetCharacterOutfit(v[4]);
		CharacterCustomization.SetCharacterHat(v[5]);
		CharacterCustomization.SetCharacterSash(v[6]);
		CharacterCustomization.SetCharacterMedal(v[7]);
	}

	public static void WearOutfit(int slot)
	{
		int[] array = Outfit(slot);
		if (array == null)
		{
			Say(Lang.F("Outfit {0} is empty: save what you wear first", slot + 1));
		}
		else
		{
			Wear(array);
			Say(Lang.F("Wearing outfit {0}", slot + 1));
		}
	}

	public static CustomizationOption Part(int kind, int index)
	{
		Customization instance = Singleton<Customization>.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return null;
		}
		CustomizationOption[] array = kind switch
		{
			6 => instance.sashes, 
			5 => instance.hats, 
			4 => instance.fits, 
			3 => instance.mouths, 
			2 => instance.eyes, 
			1 => instance.accessories, 
			0 => instance.skins, 
			_ => instance.medals, 
		};
		if (array == null || index < 0 || index >= array.Length)
		{
			return null;
		}
		return array[index];
	}

	public static List<ACHIEVEMENTTYPE> CosmeticRequirements(out int ascent, out int locked)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Invalid comparison between Unknown and I4
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Invalid comparison between Unknown and I4
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		List<ACHIEVEMENTTYPE> list = new List<ACHIEVEMENTTYPE>();
		ascent = 0;
		locked = 0;
		bool cosmetics = State.Cosmetics;
		State.Cosmetics = false;
		try
		{
			AchievementManager instance = Singleton<AchievementManager>.Instance;
			CustomizationOption[] array = Resources.FindObjectsOfTypeAll<CustomizationOption>();
			foreach (CustomizationOption val in array)
			{
				if ((Object)(object)val == (Object)null || !val.IsLocked)
				{
					continue;
				}
				locked++;
				if (val.requiresAscent)
				{
					ascent = Mathf.Max(ascent, val.requiredAscent);
				}
				else if ((int)val.customRequirement == 1)
				{
					ascent = Mathf.Max(ascent, 8);
				}
				else if ((int)val.customRequirement == 2)
				{
					for (int j = 1; j <= 32; j++)
					{
						Want(list, instance, (ACHIEVEMENTTYPE)j);
					}
				}
				else
				{
					Want(list, instance, val.requiredAchievement);
				}
			}
			return list;
		}
		finally
		{
			State.Cosmetics = cosmetics;
		}
	}

	private static void Want(List<ACHIEVEMENTTYPE> need, AchievementManager manager, ACHIEVEMENTTYPE a)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if ((int)a != 0 && !need.Contains(a) && !manager.IsAchievementUnlocked(a))
		{
			need.Add(a);
		}
	}

	public static void UnlockCosmeticsForever()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		int ascent;
		int locked;
		List<ACHIEVEMENTTYPE> list = CosmeticRequirements(out ascent, out locked);
		if (locked == 0)
		{
			Say(Lang.T("Every cosmetic is already yours"));
			return;
		}
		if (RunSettings.blockingAchievements)
		{
			Say(Lang.T("This run's settings block achievements - do it from the airport"));
			return;
		}
		AchievementManager instance = Singleton<AchievementManager>.Instance;
		Patches.Granting = true;
		try
		{
			foreach (ACHIEVEMENTTYPE item in list)
			{
				AchievementManager.Grant(item);
			}
			if (ascent > instance.GetMaxAscent())
			{
				instance.SetSteamStat((STEAMSTATTYPE)13, ascent);
			}
		}
		finally
		{
			Patches.Granting = false;
		}
		CosmeticRequirements(out var _, out var locked2);
		Say((locked2 == 0) ? Lang.T("Every cosmetic is unlocked for good") : Lang.F("{0} unlocked, {1} would not open", locked - locked2, locked2));
	}

	public static List<Item> ItemList()
	{
		return ((DatabaseAsset<ItemDatabase, Item>)(object)SingletonAsset<ItemDatabase>.Instance).Objects;
	}

	public static void Give(Item item, string name)
	{
		ItemDatabase.Add(item);
		Say(Lang.F("Spawned {0}", name));
	}

	public static void SetTime(float hour, string what)
	{
		if ((Object)(object)DayNightManager.instance == (Object)null)
		{
			Say(Lang.T("No day and night here"));
			return;
		}
		DayNightManager.SetTimeOfDay(hour);
		try
		{
			if (PhotonNetwork.IsMasterClient)
			{
				PhotonView pv = DayNightManager.instance.GetComponent<PhotonView>();
				if ((Object)(object)pv != (Object)null)
				{
					pv.RPC("RPCA_SyncTime", (RpcTarget)0, new object[2] { DayNightManager.instance.dayCount, hour });
				}
			}
		}
		catch {}
		Say(PhotonNetwork.IsMasterClient ? Lang.F("It is now {0}", Lang.T(what)) : Lang.F("It is now {0} (until the host's clock overrides it)", Lang.T(what)));
	}

	public static void NextSection()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			Say(Lang.T("Only the host can move the lobby on"));
			return;
		}
		if (!MapHandler.Exists)
		{
			Say(Lang.T("Start a run first"));
			return;
		}
		MapHandler.JumpToNextSegment();
		Say(Lang.T("Skipped to the next section"));
	}

	public static void LookAround(bool now = false)
	{
		if (!now && Time.unscaledTime < nextLook)
		{
			return;
		}
		nextLook = Time.unscaledTime + 1.5f;
		Fires.Clear();
		Statues.Clear();
		Hazards.Clear();
		if (State.EspCampfires)
		{
			List<Campfire> list = ((AllCampfires != null) ? (AllCampfires.GetValue(null) as List<Campfire>) : null);
			if (list != null)
			{
				foreach (Campfire item2 in list)
				{
					if ((Object)(object)item2 != (Object)null)
					{
						Fires.Add(item2);
					}
				}
			}
		}
		if (State.EspStatues)
		{
			RespawnChest[] array = Object.FindObjectsByType<RespawnChest>((FindObjectsSortMode)0);
			foreach (RespawnChest item in array)
			{
				Statues.Add(item);
			}
		}
		if (State.EspHazards)
		{
			Add<Mob>((Func<Mob, string>)((Mob m) => Nice(((object)m).GetType().Name)));
			Add<Tornado>((Func<Tornado, string>)((Tornado _) => Lang.K("Tornado")));
			Add<BeeSwarm>((Func<BeeSwarm, string>)((BeeSwarm _) => Lang.K("Bees")));
			Add<Antlion>((Func<Antlion, string>)((Antlion _) => Lang.K("Antlion")));
			Add<MushroomZombie>((Func<MushroomZombie, string>)((MushroomZombie _) => Lang.K("Zombie")));
			Add<Spider>((Func<Spider, string>)((Spider _) => Lang.K("Spider")));
		}
	}

	private static void Add<T>(Func<T, string> name) where T : Component
	{
		T[] array = Object.FindObjectsByType<T>((FindObjectsSortMode)0);
		foreach (T val in array)
		{
			if ((Object)(object)val != (Object)null && ((Component)val).gameObject.activeInHierarchy)
			{
				Hazards.Add(new KeyValuePair<Transform, string>(((Component)val).transform, Lang.T(name(val))));
			}
		}
	}

	private static string Nice(string type)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < type.Length; i++)
		{
			if (i > 0 && char.IsUpper(type[i]))
			{
				stringBuilder.Append(' ').Append(char.ToLowerInvariant(type[i]));
			}
			else
			{
				stringBuilder.Append(type[i]);
			}
		}
		return stringBuilder.ToString();
	}

	public static void Trip(Character c, float seconds)
	{
		((MonoBehaviourPun)c).photonView.RPC("RPCA_Fall", (RpcTarget)0, new object[2] { seconds, 0f });
	}

	public static void Launch(Character c, Vector3 direction, float speed)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		int num2 = 0;
		foreach (Bodypart part in c.refs.ragdoll.partList)
		{
			if (!((Object)(object)part.Rig == (Object)null))
			{
				num += part.Rig.mass;
				num2++;
			}
		}
		if (num2 != 0)
		{
			Trip(c, 1.5f);
			((MonoBehaviourPun)c).photonView.RPC("RPCA_AddForceAtPosition", (RpcTarget)0, new object[3]
			{
				direction.normalized * speed * (num / (float)num2),
				c.Center,
				40f
			});
		}
	}

	public static void MakeJump(Character c)
	{
		((MonoBehaviourPun)c).photonView.RPC("JumpRpc", (RpcTarget)0, new object[1] { false });
	}

	public static void Bring(Character c)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Vector3 lookDirection_Flat = Me.data.lookDirection_Flat;
		((MonoBehaviourPun)c).photonView.RPC("WarpPlayerRPC", (RpcTarget)0, new object[2]
		{
			Me.Center + lookDirection_Flat * 2f + Vector3.up,
			true
		});
	}

	public static void Swap(Character c)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Me.Center + Vector3.up;
		Vector3 val2 = c.Center + Vector3.up;
		((MonoBehaviourPun)c).photonView.RPC("WarpPlayerRPC", (RpcTarget)0, new object[2] { val, true });
		((MonoBehaviourPun)Me).photonView.RPC("WarpPlayerRPC", (RpcTarget)0, new object[2] { val2, true });
	}

	public static void DropOn(Character c, Item item)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		PhotonNetwork.Instantiate("0_Items/" + ((Object)item).name, c.Head + Vector3.up * 4f, Quaternion.identity, (byte)0, (object[])null);
	}

	private static float lastFogHeightOffset;
	private static float lastLavaHeightOffset;

	public static void AdjustFogHeight(float newOffset)
	{
		float delta = newOffset - lastFogHeightOffset;
		lastFogHeightOffset = newOffset;
		State.FogHeightOffset = newOffset;
		if (Mathf.Abs(delta) < 0.01f) return;

		Fog[] fogs = Object.FindObjectsOfType<Fog>();
		if (fogs != null)
		{
			for (int i = 0; i < fogs.Length; i++)
			{
				if ((Object)(object)fogs[i] != (Object)null)
				{
					fogs[i].fogHeight += delta;
					try
					{
						if (PhotonNetwork.IsMasterClient)
						{
							PhotonView pv = fogs[i].GetComponent<PhotonView>();
							if ((Object)(object)pv != (Object)null)
							{
								pv.RPC("RPCA_SyncFog", (RpcTarget)0, new object[1] { fogs[i].fogHeight });
							}
						}
					}
					catch {}
				}
			}
		}

		OrbFogHandler orbFog = Singleton<OrbFogHandler>.Instance;
		if ((Object)(object)orbFog != (Object)null)
		{
			orbFog.currentSize += delta;
			if (orbFog.currentSize < 10f) orbFog.currentSize = 10f;
			FogSphere sphere = orbFog.GetComponentInChildren<FogSphere>();
			if ((Object)(object)sphere != (Object)null)
			{
				sphere.currentSize = orbFog.currentSize;
				sphere.fogPoint += Vector3.up * delta;
			}
			try
			{
				if (PhotonNetwork.IsMasterClient)
				{
					PhotonView pv = orbFog.GetComponent<PhotonView>();
					if ((Object)(object)pv != (Object)null)
					{
						pv.RPC("RPCA_SyncFog", (RpcTarget)0, new object[2] { orbFog.currentSize, orbFog.isMoving });
					}
				}
			}
			catch {}
		}
	}

	public static void AdjustLavaHeight(float newOffset)
	{
		float delta = newOffset - lastLavaHeightOffset;
		lastLavaHeightOffset = newOffset;
		State.LavaHeightOffset = newOffset;
		if (Mathf.Abs(delta) < 0.01f) return;

		MovingLava[] mlavas = Object.FindObjectsOfType<MovingLava>();
		if (mlavas != null)
		{
			for (int i = 0; i < mlavas.Length; i++)
			{
				if ((Object)(object)mlavas[i] != (Object)null)
				{
					mlavas[i].transform.position += Vector3.up * delta;
					try
					{
						if (PhotonNetwork.IsMasterClient)
						{
							PhotonView pv = mlavas[i].GetComponent<PhotonView>();
							if ((Object)(object)pv != (Object)null)
							{
								pv.RPC("RPCA_SyncLavaHeight", (RpcTarget)0, new object[1] { mlavas[i].transform.position.y });
							}
						}
					}
					catch {}
				}
			}
		}
		LavaRising[] rlavas = Object.FindObjectsOfType<LavaRising>();
		if (rlavas != null)
		{
			for (int i = 0; i < rlavas.Length; i++)
			{
				if ((Object)(object)rlavas[i] != (Object)null)
				{
					try
					{
						FieldInfo shField = typeof(LavaRising).GetField("startHeight", BindingFlags.Instance | BindingFlags.NonPublic);
						if (shField != null)
						{
							float curSh = (float)shField.GetValue(rlavas[i]);
							shField.SetValue(rlavas[i], curSh + delta);
						}
						if ((Object)(object)rlavas[i].topTransform != (Object)null)
						{
							rlavas[i].topTransform.position += Vector3.up * delta;
						}
						if ((Object)(object)rlavas[i].heightMinimumTransform != (Object)null)
						{
							rlavas[i].heightMinimumTransform.position += Vector3.up * delta;
						}
						if ((Object)(object)rlavas[i].lava != (Object)null)
						{
							rlavas[i].lava.transform.position += Vector3.up * delta;
							rlavas[i].lava.position += Vector3.up * delta;
						}
					}
					catch {}
				}
			}
		}
	}

	public static void Upkeep()
	{
		Character me = Me;
		if (State.InfStamina)
		{
			if (!stamTouched)
			{
				me.data.currentStamina = 1f;
			}
			me.infiniteStam = true;
			stamTouched = true;
		}
		else if (stamTouched)
		{
			me.infiniteStam = false;
			stamTouched = false;
		}
		CharacterMovement movement = me.refs.movement;
		if ((Object)(object)movement != (Object)(object)moveOwner)
		{
			moveOwner = movement;
			jumpBase = movement.jumpImpulse;
			clipOff = false;
		}
		movement.jumpImpulse = jumpBase * State.Jump;
		CharacterClimbing climbing = me.refs.climbing;
		if ((Object)(object)climbing != (Object)null)
		{
			if ((Object)(object)climbing != (Object)(object)climbOwner)
			{
				climbOwner = climbing;
				climbBase = climbing.climbSpeedMod;
			}
			climbing.climbSpeedMod = climbBase * State.Climb;
		}
		if (State.AirJump)
		{
			me.data.jumpsRemaining = 1;
			me.data.extraJumps = 1;
			jumpTouched = true;
		}
		else if (jumpTouched)
		{
			me.data.extraJumps = 0;
			jumpTouched = false;
		}
		if (State.NoWeight != lastNoWeight)
		{
			lastNoWeight = State.NoWeight;
			if (UpdateWeight != null)
			{
				UpdateWeight.Invoke(me.refs.afflictions, null);
			}
		}
		bool flag = State.Fly && State.NoClip;
		if (flag && (!clipOff || Time.unscaledTime > clipAsserted + 0.5f))
		{
			me.refs.ragdoll.ToggleCollision(false);
			clipOff = true;
			clipAsserted = Time.unscaledTime;
		}
		else if (!flag && clipOff)
		{
			me.refs.ragdoll.ToggleCollision(true);
			clipOff = false;
		}
		if (State.RopeLength > 1f && (Object)(object)me.data.currentItem != (Object)null)
		{
			RopeSpool component = ((Component)me.data.currentItem).GetComponent<RopeSpool>();
			float num = (float)Rope.MaxSegments + 2f;
			if ((Object)(object)component != (Object)null && component.RopeFuel < num)
			{
				component.RopeFuel = num;
			}
		}
		GUIManager instance = GUIManager.instance;
		if ((Object)(object)instance != (Object)null && (Object)(object)instance.hudCanvas != (Object)null)
		{
			if (State.HideHud)
			{
				((Behaviour)instance.hudCanvas).enabled = false;
				hudTouched = true;
			}
			else if (hudTouched)
			{
				((Behaviour)instance.hudCanvas).enabled = true;
				hudTouched = false;
			}
		}
		if (State.StopFog)
		{
			Fog[] fogs = Object.FindObjectsOfType<Fog>();
			if (fogs != null)
			{
				for (int i = 0; i < fogs.Length; i++)
				{
					if ((Object)(object)fogs[i] != (Object)null)
					{
						fogs[i].fogHeight = -9999f;
					}
				}
			}
			if ((Object)(object)me != (Object)null && me.data != null)
			{
				me.data.isInFog = false;
			}
		}
		if (State.StopLava)
		{
			LavaRising[] lavas = Object.FindObjectsOfType<LavaRising>();
			if (lavas != null)
			{
				for (int i = 0; i < lavas.Length; i++)
				{
					if ((Object)(object)lavas[i] != (Object)null)
					{
						lavas[i].timeTraveled = 0f;
						lavas[i].secondsWaitedToStart = 0f;
					}
				}
			}
		}
		if (State.NoWeather)
		{
			if ((Object)(object)me != (Object)null && me.refs != null && (Object)(object)me.refs.afflictions != null)
			{
				float heat = me.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hot);
				if (heat > 0f) me.refs.afflictions.SubtractStatus(CharacterAfflictions.STATUSTYPE.Hot, heat, false, false);
			}
		}
		if (State.NoHazards)
		{
			if ((Object)(object)me != (Object)null && me.refs != null && (Object)(object)me.refs.afflictions != null)
			{
				me.refs.afflictions.RemoveAllThorns();
				float thorns = me.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Thorns);
				if (thorns > 0f) me.refs.afflictions.SubtractStatus(CharacterAfflictions.STATUSTYPE.Thorns, thorns, false, false);
				float web = me.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Web);
				if (web > 0f) me.refs.afflictions.SubtractStatus(CharacterAfflictions.STATUSTYPE.Web, web, false, false);
				float spores = me.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Spores);
				if (spores > 0f) me.refs.afflictions.SubtractStatus(CharacterAfflictions.STATUSTYPE.Spores, spores, false, false);
				float poison = me.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Poison);
				if (poison > 0f) me.refs.afflictions.SubtractStatus(CharacterAfflictions.STATUSTYPE.Poison, poison, false, false);
			}
		}
		if (State.EspCampfires || State.EspStatues || State.EspHazards)
		{
			LookAround();
		}
	}
}
