using HarmonyLib;
using Reptile;
using BunchOfEmotes;

namespace BunchOfEmotes.Patches
{

    [HarmonyPatch(typeof(DieAbility))]
    internal class DieAbilityPatches
    {
        [HarmonyPatch(nameof(DieAbility.Init))]
        [HarmonyPostfix]
        public static void Init_Postfix(DieAbility __instance)
        {
            BunchOfEmotesPlugin.dieAbility = __instance;
        }
    }
}