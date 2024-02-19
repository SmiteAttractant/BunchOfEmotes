using HarmonyLib;
using Reptile;
using BunchOfEmotes;

namespace BunchOfEmotes.Patches
{

    [HarmonyPatch(typeof(StageManager))]
    internal class StageManagerPatches
    {
        [HarmonyPatch(nameof(StageManager.SetupWorldHandler))]
        [HarmonyPostfix]
        public static void SetupWorldHandler_Postfix(StageManager __instance)
        {
            BunchOfEmotesPlugin.stageManager = __instance;
        }
    }
}