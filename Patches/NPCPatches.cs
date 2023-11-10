using HarmonyLib;
using Reptile;
using System.Linq;
using UnityEngine;

namespace BunchOfEmotes.Patches
{
    [HarmonyPatch(typeof(NPC))]
    internal class NPCPatches
    {
        [HarmonyPatch(nameof(NPC.InitSceneObject))]
        [HarmonyPostfix]
        public static void Awake_Postfix(NPC __instance)
        {
            try
            {
                if (BunchOfEmotesPlugin.myNPC == null)
                {
                    BunchOfEmotesPlugin.myNPC = __instance;
                }

                if (BunchOfEmotesPlugin.myAnim == null)
                {
                    if (__instance.crew != Crew.NONE && __instance.character != Characters.boarder)
                    {
                        BunchOfEmotesPlugin.Log.LogMessage("npc name : " + __instance.name);
                        if (__instance.animators != null)
                        {
                            if (BunchOfEmotesPlugin.myAnim == null)
                            {
                                //all the animations are loaded on the NPCS we are taking them from here
                                BunchOfEmotesPlugin.myAnim = __instance.transform.GetChild(0).GetChild(1).GetComponent<Animator>().runtimeAnimatorController;
                                if (BunchOfEmotesPlugin.myAnimUntouched == null)
                                {
                                    BunchOfEmotesPlugin.myAnimUntouched = __instance.transform.GetChild(0).GetChild(1).GetComponent<Animator>().runtimeAnimatorController;                                
                                }
                                BunchOfEmotesPlugin.Log.LogMessage("clips amount : " + BunchOfEmotesPlugin.myAnim.animationClips.Count());

                            }
                        }
                    }
                }
            }
            catch
            {
            }

        }
    }
}