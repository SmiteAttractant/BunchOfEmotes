using HarmonyLib;
using Reptile;
using System.Linq;
using UnityEngine;

namespace BunchOfEmotes.Patches
{
    // TODO Review this file and update to your own requirements, or remove it altogether if not required

    /// <summary>
    /// Sample Harmony Patch class. Suggestion is to use one file per patched class
    /// though you can include multiple patch classes in one file.
    /// Below is included as an example, and should be replaced by classes and methods
    /// for your mod.
    /// </summary>
    [HarmonyPatch(typeof(NPC))]
    internal class NPCPatches
    {
        [HarmonyPatch(nameof(NPC.InitSceneObject))]
        [HarmonyPostfix]
        public static void Awake_Postfix(NPC __instance)
        {
            try
            {
                if (BunchOfEmotesPlugin.myAnim == null)
                { 
                    if (__instance.crew != Crew.NONE && __instance.character != Characters.boarder)
                    {
                        BunchOfEmotesPlugin.Log.LogMessage("npc name : " + __instance.name);
                        if (__instance.animators != null)
                        {
                            if (BunchOfEmotesPlugin.myAnim == null)
                            {
                            
                                BunchOfEmotesPlugin.myAnim = __instance.transform.GetChild(0).GetChild(1).GetComponent<Animator>().runtimeAnimatorController;
                                //BunchOfEmotesPlugin.myAnimRuntime = __instance.transform.GetChild(0).GetChild(1).GetComponent<Animator>().AnimatorOverrideController;
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