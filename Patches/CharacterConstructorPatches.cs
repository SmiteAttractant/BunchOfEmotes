using HarmonyLib;
using Reptile;
using System;
using UnityEngine;
using UnityEngine.Playables;


namespace BunchOfEmotes.Patches
{
    [HarmonyPatch(typeof(CharacterConstructor))]
    internal class CharacterConstructorPatches
    {
        [HarmonyPatch(nameof(CharacterConstructor.CreateNewCharacterVisual))]
        [HarmonyPrefix]
        public static bool CreateNewCharacterVisual_Prefix(CharacterConstructor __instance,ref CharacterVisual __result, Characters character, RuntimeAnimatorController controller, bool IK = false, float setGroundAngleLimit = 0f)
        {
            try
            {
                CharacterVisual characterVisual = UnityEngine.Object.Instantiate<GameObject>(__instance.GetCharacterVisual(character)).AddComponent<CharacterVisual>();
                if (BunchOfEmotesPlugin.myAnimUntouched != null)
                {
                    characterVisual.Init(character, BunchOfEmotesPlugin.myAnimUntouched, IK, setGroundAngleLimit);
                }
                else
                {
                    characterVisual.Init(character, BunchOfEmotesPlugin.myNPC.animators[0].runtimeAnimatorController, IK, setGroundAngleLimit);
                }
                characterVisual.gameObject.SetActive(true);
                __result = characterVisual;
                return false;
            }
            catch (Exception e)
            {
                BunchOfEmotesPlugin.Log.LogError(e);
                return true;
            }

        }
    }
}