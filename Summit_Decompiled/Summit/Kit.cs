using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Summit;

internal static class Kit
{
	public const string Author = "T.H.M.";

	public const string Discord = "https://discord.gg/Y6PWGsPADV";

	internal static ManualLogSource Log;

	internal static ConfigEntry<KeyCode> MenuKey;

	internal static ConfigEntry<string> Pins;

	internal static ConfigEntry<string> Language;

	internal static ConfigEntry<bool> BlockAchievements;

	internal static ConfigEntry<bool> ItemsPoppedOut;

	internal static ConfigEntry<float> PrimaryR;

	internal static ConfigEntry<float> PrimaryG;

	internal static ConfigEntry<float> PrimaryB;

	internal static ConfigEntry<float> SecondaryR;

	internal static ConfigEntry<float> SecondaryG;

	internal static ConfigEntry<float> SecondaryB;

	internal static ConfigEntry<string> ThemePreset;

	internal static string Title = "Menu";

	internal static string Version = "2.0";

	internal static string Blurb = "";

	internal static int WindowId = 1263096832;

	internal static readonly List<string> FailedPatches = new List<string>();

	public static void Start(ConfigFile config, ManualLogSource log, string title, string version, string blurb, bool testing, KeyCode key = (KeyCode)282)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Log = log;
		Title = title;
		Version = version;
		Blurb = blurb;
		WindowId = 0x4B495400 ^ title.GetHashCode();
		MenuKey = config.Bind<KeyCode>("General", "MenuKey", key, "Key that opens and closes the menu. Can also be changed on the menu's About page.");
		Pins = config.Bind<string>("General", "PinnedToHome", "", "What is pinned to the menu's Home page. Kept by the menu.");
		Language = config.Bind<string>("General", "Language", "auto", "The menu's language: auto (the language the game is in), en, es or zh. Can also be changed on the About page.");
		ItemsPoppedOut = config.Bind<bool>("General", "ItemsPoppedOut", false, "Whether the Item Spawner is detached into a separate floating window.");
		PrimaryR = config.Bind<float>("Theme", "PrimaryR", 47f / 255f, "Primary accent red (0-1).");
		PrimaryG = config.Bind<float>("Theme", "PrimaryG", 158f / 255f, "Primary accent green (0-1).");
		PrimaryB = config.Bind<float>("Theme", "PrimaryB", 143f / 255f, "Primary accent blue (0-1).");
		SecondaryR = config.Bind<float>("Theme", "SecondaryR", 69f / 255f, "Secondary highlight red (0-1).");
		SecondaryG = config.Bind<float>("Theme", "SecondaryG", 191f / 255f, "Secondary highlight green (0-1).");
		SecondaryB = config.Bind<float>("Theme", "SecondaryB", 173f / 255f, "Secondary highlight blue (0-1).");
		ThemePreset = config.Bind<string>("Theme", "Preset", "Emerald Teal", "Active theme preset name.");
		Settings.Bind(config);
		Keys.Bind(config);
		Settings.Paused = testing;
		Settings.Load();
	}
}
