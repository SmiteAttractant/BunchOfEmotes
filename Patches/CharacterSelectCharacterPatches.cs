using HarmonyLib;
using Reptile;
using UnityEngine;

namespace BunchOfEmotes.Patches
{
    [HarmonyPatch(typeof(CharacterVisual))]
    internal class CharacterVisualAnimationEventRelayPatches
    {
        [HarmonyPatch(nameof(CharacterVisual.Init))]
        [HarmonyPrefix]
        public static void Init_Prefix(CharacterVisual __instance, ref RuntimeAnimatorController animatorController)
        {
            if (BunchOfEmotesPlugin.myNPC.animators.Length != 0)
            {
                animatorController = BunchOfEmotesPlugin.myNPC.animators[0].runtimeAnimatorController;
            }
            else if(BunchOfEmotesPlugin.myAnimUntouched != null)
            {
                animatorController = BunchOfEmotesPlugin.myAnimUntouched;
            }

        }
    }
}