using HarmonyLib;
using Reptile;
using System.IO;
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

        [HarmonyPatch(nameof(Player.PlayAnim))]
        [HarmonyPostfix]
        public static void PlayAnim(Player __instance, bool instant)
        {
            int childcount = __instance.transform.GetChild(0).childCount;

            //slop crew compatibility
            if (__instance.name != "Player_HUMAN0" && __instance.moveStyle.ToString() == "ON_FOOT" && __instance.transform.GetChild(0).GetChild(childcount - 1).GetChild(0).GetComponent<Animator>().runtimeAnimatorController != BunchOfEmotesPlugin.myAnim)
            {
                __instance.transform.GetChild(0).GetChild(childcount - 1).GetChild(0).GetComponent<Animator>().runtimeAnimatorController = BunchOfEmotesPlugin.myAnim;
            }
        }
    }
}