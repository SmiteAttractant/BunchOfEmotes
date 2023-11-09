using HarmonyLib;
using Reptile;
using System.IO;
using System.Linq;
using UnityEngine;
using static Reptile.Player;

namespace BunchOfEmotes.Patches
{
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

        [HarmonyPatch(nameof(Player.EnablePlayer))]
        [HarmonyPostfix]
        public static void EnablePlayer(Player __instance)
        {
            //int childcount = __instance.transform.GetChild(0).childCount;
            //slop crew compatibility
            bool isAi = __instance.name.Contains("AI");

            if (__instance.animatorController != BunchOfEmotesPlugin.myAnim && !isAi)
            {
                __instance.animatorController = BunchOfEmotesPlugin.myAnim;
            }

        }
        [HarmonyPatch(nameof(Player.PlayAnim))]
        [HarmonyPostfix]
        public static void PlayAnim(Player __instance)
        {
            //int childcount = __instance.transform.GetChild(0).childCount;
            //slop crew compatibility
            bool isAi = __instance.name.Contains("AI");

            if (__instance.animatorController != BunchOfEmotesPlugin.myAnim && !isAi)
            {
                __instance.animatorController = BunchOfEmotesPlugin.myAnim;
                __instance.anim.runtimeAnimatorController = BunchOfEmotesPlugin.myAnim;
            }

        }
    }
}