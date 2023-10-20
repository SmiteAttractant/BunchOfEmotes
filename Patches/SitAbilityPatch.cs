using HarmonyLib;
using Reptile;
using BunchOfEmotes;
using UnityEngine;
using System.Linq;
// Replace with the actual namespace of SitAbility

[HarmonyPatch(typeof(SitAbility))]
internal class SitAbilityPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SitAbility.SetState))]
    private static bool SetState_Prefix(SitAbility __instance, SitAbility.State setState)
    {
        if (setState == SitAbility.State.END_SIT && !BunchOfEmotesPlugin.inAnimation)
        {
            __instance.p.PlayAnim(__instance.stopSitHash, false, false, -1f);
            return true;
        }
        if (setState != SitAbility.State.END_SIT)
            return true;
        if (__instance.p.curAnim != __instance.startSitHash && __instance.p.curAnim != __instance.stopSitHash)
        {
            BunchOfEmotesPlugin.inAnimation = false;
            __instance.p.StopCurrentAbility();
            return false;
        }
        return true;
    }
}


