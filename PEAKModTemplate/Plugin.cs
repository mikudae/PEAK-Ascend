using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace PEAKModTemplate
{
    [BepInPlugin("com.feldring.peakmodtemplate", "PEAKModTemplate", "1.0.0")]
    [BepInProcess("PEAK.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.feldring.peakmodtemplate";
        public const string ModName = "PEAKModTemplate";
        public const string ModVersion = "1.0.0";

        public static Plugin Instance { get; private set; } = null!;
        public static new ManualLogSource Logger { get; private set; } = null!;
        private Harmony? _harmony;

        // Configuration entries
        public static ConfigEntry<bool> EnableMod { get; private set; } = null!;
        public static ConfigEntry<string> CustomGreeting { get; private set; } = null!;

        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            // Bind configuration settings
            EnableMod = Config.Bind(
                "General",
                "EnableMod",
                true,
                "Enable or disable the mod functionality."
            );

            CustomGreeting = Config.Bind(
                "General",
                "CustomGreeting",
                "Welcome to PEAK!",
                "Custom message logged when a character is initialized."
            );

            if (!EnableMod.Value)
            {
                Logger.LogInfo($"{ModName} is disabled in configuration.");
                return;
            }

            // Initialize and apply Harmony patches
            _harmony = Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, ModGUID);

            Logger.LogInfo($"Plugin {ModName} (v{ModVersion}) successfully loaded and patches applied!");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
