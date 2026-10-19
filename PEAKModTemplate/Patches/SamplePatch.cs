using HarmonyLib;
using UnityEngine;

namespace PEAKModTemplate.Patches
{
    /// <summary>
    /// Sample Harmony patch hooking into Character lifecycle.
    /// Demonstrates safe postfix execution on the game's Character entity.
    /// </summary>
    [HarmonyPatch(typeof(Character), nameof(Character.Start))]
    public static class SamplePatch
    {
        [HarmonyPostfix]
        public static void Postfix(Character __instance)
        {
            if (__instance == null)
            {
                return;
            }

            if (Plugin.EnableMod != null && !Plugin.EnableMod.Value)
            {
                return;
            }

            string greeting = Plugin.CustomGreeting?.Value ?? "Welcome to PEAK!";
            Plugin.Logger?.LogInfo($"[SamplePatch] Character.Start hook triggered for '{__instance.name}' - {greeting}");
        }
    }
}
