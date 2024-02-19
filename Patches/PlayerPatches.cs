using HarmonyLib;
using Reptile;
using System.IO;
using System.Linq;
using UnityEngine;
using static Reptile.Player;
using MapStation;
using UnityEngine.SceneManagement;

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
            bool isAi = __instance.name.Contains("AI") || __instance.name.Contains("(Clone)");

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
            bool isAi = __instance.name.Contains("AI") || __instance.name.Contains("(Clone)");

            var api = MapStation.API.APIManager.API;
            if (api != null && BunchOfEmotesPlugin.myAnimUntouched == null)
            {

                var sceneId = Utility.SceneNameToStage(SceneManager.GetActiveScene().name);
                var customStage = api.GetCustomStageByID((int)sceneId);
                var iAmOnACustomStage = customStage != null;
                BunchOfEmotesPlugin.Log.LogMessage(iAmOnACustomStage + " is my stage");

                if (iAmOnACustomStage && BunchOfEmotesPlugin.myAnimUntouched == null)
                {
                    if (GameObject.Find("NPC_BOE") && BunchOfEmotesPlugin.myAnim == null)
                    {
                        BunchOfEmotesPlugin.Log.LogMessage("la ca fonctionnes");

                        BunchOfEmotesPlugin.myAnim = GameObject.Find("NPC_BOE").GetComponent<Animator>().runtimeAnimatorController;
                        BunchOfEmotesPlugin.myAnimUntouched = GameObject.Find("NPC_BOE").GetComponent<Animator>().runtimeAnimatorController;
                    }
                }
            }
            //if (BunchOfEmotesPlugin.myAnimUntouched != null)
            //{
            if (!isAi)
                {

                    if (BunchOfEmotesPlugin.myAnimUntouched == null)
                    {
                        BunchOfEmotesPlugin.Log.LogMessage(__instance.animatorController.name);
                        BunchOfEmotesPlugin.myAnimUntouched = __instance.animatorController;
                    }

                    if (__instance.animatorController != BunchOfEmotesPlugin.myAnim)
                    {
                        __instance.animatorController = BunchOfEmotesPlugin.myAnim;
                        __instance.anim.runtimeAnimatorController = BunchOfEmotesPlugin.myAnim;
                    }
                    if (BunchOfEmotesPlugin.myAnimBMX != null)
                    {
                        if (__instance.animatorControllerBMX != BunchOfEmotesPlugin.myAnimBMX)
                        {
                            __instance.animatorControllerBMX = BunchOfEmotesPlugin.myAnimBMX;
                        }
                    }
                    if (BunchOfEmotesPlugin.myAnimInlines != null)
                    {
                        if (__instance.animatorControllerSkates != BunchOfEmotesPlugin.myAnimInlines)
                        {
                            __instance.animatorControllerSkates = BunchOfEmotesPlugin.myAnimInlines;
                        }
                    }
                    if (BunchOfEmotesPlugin.myAnimSkateboard != null)
                    {
                        if (__instance.animatorControllerSkateboard != BunchOfEmotesPlugin.myAnimSkateboard)
                        {
                            __instance.animatorControllerSkateboard = BunchOfEmotesPlugin.myAnimSkateboard;
                        }
                    }

                }
                else if (__instance.name.Contains("(Clone)") && __instance.anim.runtimeAnimatorController != BunchOfEmotesPlugin.myNPC.animators[0].runtimeAnimatorController)
                {
                    __instance.animatorController = BunchOfEmotesPlugin.myNPC.animators[0].runtimeAnimatorController;
                    __instance.anim.runtimeAnimatorController = BunchOfEmotesPlugin.myNPC.animators[0].runtimeAnimatorController;
                }
            //}

        }
    }
}