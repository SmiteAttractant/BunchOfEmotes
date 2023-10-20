using HarmonyLib;
using Reptile;

namespace BunchOfEmotes.Patches
{
    // TODO Review this file and update to your own requirements, or remove it altogether if not required

    /// <summary>
    /// Sample Harmony Patch class. Suggestion is to use one file per patched class
    /// though you can include multiple patch classes in one file.
    /// Below is included as an example, and should be replaced by classes and methods
    /// for your mod.
    /// </summary>
    [HarmonyPatch(typeof(Player))]
    internal class PlayerPatches
    {
        [HarmonyPatch(nameof(Player.Init))]
        [HarmonyPrefix]
        public static bool Awake_Prefix(Player __instance)
        {
            if (BunchOfEmotesPlugin.player == null)
            {
                BunchOfEmotesPlugin.player = __instance;
            }
            return true;
        }
    }
}