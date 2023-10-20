using BunchOfEmotes.Patches;
using HarmonyLib;
using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;


namespace BunchOfEmotes.Patches
{
    [HarmonyPatch(typeof(GameplayUI))]
    public class GameplayUIPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Init")]
        public static void Init(GameplayUI __instance)
        {
            BunchOfEmotesPlugin.ui = __instance;
            __instance.gameplayScreen.gameObject.AddComponent<UI>();
        }
    }
}
