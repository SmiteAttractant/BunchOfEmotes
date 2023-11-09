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
        if (BunchOfEmotesPlugin.customMenu)
        {
            if (setState != SitAbility.State.END_SIT)
                return true;
            if (setState == SitAbility.State.END_SIT)
            {
                var childcount = BunchOfEmotesPlugin.player.transform.GetChild(0).childCount;
                __instance.p.StopCurrentAbility();
                BunchOfEmotesPlugin.player.transform.GetChild(0).GetChild(childcount - 1).GetChild(0).GetComponent<Animator>().runtimeAnimatorController = BunchOfEmotesPlugin.myAnim;
                BunchOfEmotesPlugin.player.anim.SetLayerWeight(5, 0f);
            }
            if (__instance.p.curAnim != __instance.startSitHash && __instance.p.curAnim != __instance.stopSitHash)
            {
                BunchOfEmotesPlugin.player.anim.SetLayerWeight(5, 0f);
                __instance.p.StopCurrentAbility();
                return false;
            }
            return true;
        }
        else
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
                int childcount = BunchOfEmotesPlugin.player.transform.GetChild(0).childCount;
                BunchOfEmotesPlugin.player.anim.SetLayerWeight(5, 0f);
                BunchOfEmotesPlugin.player.transform.GetChild(0).GetChild(childcount - 1).GetChild(0).GetComponent<Animator>().runtimeAnimatorController = BunchOfEmotesPlugin.myAnim;
                __instance.p.StopCurrentAbility();
                return false;
            }
            return true;
        }
    }
}


