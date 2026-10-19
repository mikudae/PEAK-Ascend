using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Summit;

[BepInPlugin("duskidle.peak.summit", "Ascend", "2.5")]
public class Plugin : BaseUnityPlugin
{
	public const string Version = "2.5";

	internal static ManualLogSource L;

	internal static ConfigEntry<string>[] Outfits;

	internal static List<string> FailedPatches => Kit.FailedPatches;

	private void Awake()
	{
		L = Logger;
		Kit.BlockAchievements = Config.Bind<bool>("General", "BlockAchievements", true, "While true, no achievement or badge is granted. Can also be switched on the menu's About page.");
		Outfits = new ConfigEntry<string>[4];
		for (int i = 0; i < Outfits.Length; i++)
		{
			Outfits[i] = Config.Bind<string>("Outfits", "Outfit" + (i + 1), "", "A saved outfit: skin, accessory, eyes, mouth, outfit, hat, sash, medal. Kept by the menu.");
		}
		Keys.Job("WarpAim", "Teleport to where I am looking", delegate
		{
			if (Features.InGame)
			{
				Features.WarpToAim();
			}
		});
		Keys.Job("WarpFire", "Teleport to the campfire", delegate
		{
			if (Features.InGame)
			{
				Features.WarpToCampfire();
			}
		});
		Keys.Job("Heal", "Clear statuses and fill stamina", delegate
		{
			if (Features.InGame)
			{
				Features.Heal();
			}
		});
		Keys.Job("FreeCam", "Free camera on and off", delegate
		{
			if (Features.InGame)
			{
				Features.SetFreeCam(!Live.FreeCam);
			}
		});
		Keys.Job("SaveSpot", "Remember this spot (spot 1)", delegate
		{
			if (Features.InGame)
			{
				Features.SaveSpot(0);
			}
		});
		Keys.Job("GoSpot", "Go back to spot 1", delegate
		{
			if (Features.InGame)
			{
				Features.GoSpot(0);
			}
		});
		Keys.Quiet.UnionWith(new string[2] { "CamDistance", "FlySpeed" });
		Kit.Start(Config, Logger, "Ascend", "2.5", "A cheat menu for PEAK.", SelfTest.Asked, (KeyCode)282);
		Theme.Paint(new Color(Kit.PrimaryR.Value, Kit.PrimaryG.Value, Kit.PrimaryB.Value), new Color(Kit.SecondaryR.Value, Kit.SecondaryG.Value, Kit.SecondaryB.Value));
		Menu.Ready = () => Features.InGame;
		Menu.NotReady = "Get into the airport or a run first: nothing here works from the title screen.";
		Menu.OwnCursor = false;
		Lang.OfTheGame = () => LocalizedText.CURRENT_LANGUAGE.ToString();
		Harmony h = new Harmony("duskidle.peak.summit");
		Patch(h, typeof(CharacterAfflictions), "AddStatus", "AddStatusPrefix");
		Patch(h, typeof(CharacterMovement), "CheckFallDamage", "FallDamagePrefix");
		Patch(h, typeof(Character), "Fall", "FallPrefix");
		Patch(h, typeof(CharacterMovement), "GetMovementForce", null, "MoveForcePostfix");
		Patch(h, typeof(CharacterMovement), "FixedUpdate", null, "FlyPostfix");
		Patch(h, typeof(Item), "get_CarryWeight", null, "CarryWeightPostfix");
		Patch(h, typeof(OrbFogHandler), "Move", "FogPrefix");
		Patch(h, typeof(OrbFogHandler), "WaitToMove", "FogPrefix");
		Patch(h, typeof(CustomizationOption), "get_IsLocked", null, "CosmeticLockPostfix");
		Patch(h, typeof(GUIManager), "UpdateWindowStatus", null, "WindowStatusPostfix");
		Patch(h, typeof(AchievementManager), "ThrowAchievement", "AchievementPrefix");
		Patch(h, typeof(Item), "ConsumeDelayed", "ConsumePrefix");
		Patch(h, typeof(Action_ReduceUses), "RunAction", "ReduceUsesPrefix");
		Patch(h, typeof(RopeSpool), "set_RopeFuel", "RopeFuelPrefix");
		Patch(h, typeof(Rope), "get_MaxSegments", null, "MaxSegmentsPostfix");
		Patch(h, typeof(CharacterClimbing), "AcceptableGrabAngle", null, "GrabAnglePostfix");
		Patch(h, typeof(CharacterClimbing), "set_playerSlide", "SlidePrefix");
		Patch(h, typeof(CharacterClimbing), "TryToStartWallClimb", "ReachPrefix");
		Patch(h, typeof(RunSettings), "GetValue", null, "RunValuePostfix", new Type[2]
		{
			typeof(string),
			typeof(bool)
		});
		Patch(h, typeof(LavaRising), "Update", "LavaPrefix");
		Patch(h, typeof(MainCameraMovement), "GetFov", null, "FovPostfix");
		Patch(h, typeof(MainCameraMovement), "CharacterCam", null, "ThirdPersonPostfix");
		Patch(h, typeof(MainCameraMovement), "LateUpdate", null, "FreeCamPostfix");
		Patch(h, typeof(HideTheBody), "Update", "ShowBodyPrefix");
		Patch(h, typeof(CharacterInput), "Sample", "SamplePrefix");
		Type netUtils = AccessTools.TypeByName("Peak.Network.NetworkingUtilities");
		if (netUtils != null)
		{
			Patch(h, netUtils, "GetBestUserID", "GetBestUserIDPrefix");
			Patch(h, netUtils, "GetUsername", "GetUsernamePrefix");
		}
		Type steamApi = AccessTools.TypeByName("Steamworks.SteamAPI");
		if (steamApi != null)
		{
			Patch(h, steamApi, "RestartAppIfNecessary", "RestartAppIfNecessaryPrefix");
		}
		GameObject val = new GameObject("Ascend");
		Object.DontDestroyOnLoad((Object)val);
		((Object)val).hideFlags = (HideFlags)61;
		val.AddComponent<Menu>();
		DebugBridge.Start();
		Updater.Start(Config);
		L.LogInfo((object)string.Format("Ascend {0} loaded - {1} opens the menu.", "2.5", Kit.MenuKey.Value));
	}

	private void Update()
	{
		DebugBridge.Update();
	}

	private void OnDestroy()
	{
		DebugBridge.Stop();
	}

	private void OnDisable()
	{
		DebugBridge.Stop();
	}

	private static void Patch(Harmony h, Type type, string method, string prefix, string postfix = null, Type[] args = null)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			MethodInfo methodInfo = ((args == null) ? AccessTools.Method(type, method, (Type[])null, (Type[])null) : AccessTools.Method(type, method, args, (Type[])null));
			if (methodInfo == null)
			{
				throw new MissingMethodException(type.Name, method);
			}
			h.Patch((MethodBase)methodInfo, (prefix == null) ? ((HarmonyMethod)null) : new HarmonyMethod(typeof(Patches), prefix, (Type[])null), (postfix == null) ? ((HarmonyMethod)null) : new HarmonyMethod(typeof(Patches), postfix, (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		}
		catch (Exception ex)
		{
			Kit.FailedPatches.Add(type.Name + "." + method);
			L.LogError((object)("patch " + type.Name + "." + method + " failed: " + ex.Message));
		}
	}
}
