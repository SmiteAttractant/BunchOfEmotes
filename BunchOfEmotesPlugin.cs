using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Reptile;
using HarmonyLib;
using UnityEngine;

using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using BunchOfEmotes.Patches;

namespace BunchOfEmotes
{
    // TODO Review this file and update to your own requirements.

    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class BunchOfEmotesPlugin : BaseUnityPlugin
    {

        private const string MyGUID = "com.Dragsun.BunchOfEmotes";
        private const string PluginName = "Bunch of emotes";
        private const string VersionString = "1.1.0";


        public static string KeyboardPlusKey = "Next emote";
        public static string KeyboardMinusKey = "Previous emote";
        public static string KeyboardConfirmKey = "Confirm / Open menu";

        // Configuration entries. Static, so can be accessed directly elsewhere in code via
        // e.g.
        // float myFloat = BunchOfEmotesPlugin.FloatExample.Value;
        // TODO Change this code or remove the code if not required.
        public static ConfigEntry<KeyboardShortcut> KeyboardPlus;
        public static ConfigEntry<KeyboardShortcut> KeyboardMinus;
        public static ConfigEntry<KeyboardShortcut> KeyboardConfirm;
        public static ConfigEntry<bool> customList;
        public static ConfigEntry<string> myCustomList;
        public static DieAbility dieAbility;
        public static bool showMenu = false;
        public static bool myVariable = true; // This is the variable you want to toggle
        public static float timer = 0.0f;
        public static bool keyIsPressed = false;
        public static bool inAnimation = false;
        public static string customListKey = "Do you want to use a custom list ?";
        public static string myCustomListKey = "Enter your custom list";

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        public static GameplayUI ui;

        /// <summary>
        /// Initialise the configuration settings and patch methods
        /// </summary>
        private void Awake()
        {

            // Keyboard shortcut setting example
            // TODO Change this code or remove the code if not required.
            KeyboardPlus = Config.Bind("Menu controls",KeyboardPlusKey,new KeyboardShortcut(KeyCode.LeftBracket));
            KeyboardMinus = Config.Bind("Menu controls", KeyboardMinusKey, new KeyboardShortcut(KeyCode.RightBracket));
            KeyboardConfirm = Config.Bind("Open menu / Confirm", KeyboardConfirmKey, new KeyboardShortcut(KeyCode.N));
            KeyboardConfirm = Config.Bind("Custom list", KeyboardConfirmKey, new KeyboardShortcut(KeyCode.N));
            customList = Config.Bind("Custom list", customListKey, defaultValue: false, "if you want a custom list of emotes");
            myCustomList = Config.Bind("Custom list", myCustomListKey, "jumpNEW,fallNEW,wallRunLeftNEW,grafSlashUP_RIGHT", "Your custom list of animations they must be without any spaces and separated by ,");

            KeyboardPlus.SettingChanged += ConfigSettingChanged;

            // Apply all of our patches
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");

            Log = Logger;
        }



        public static Player player { get; set; }
        public static Vector3 playerpos1;
        private float keyPressStartTime = 0f;
        public static int myAnimationKey = 0;
        public static int childcount = 0;
        public static Dictionary<int, string> myCustomAnims = new Dictionary<int, string>();
        

        public static RuntimeAnimatorController myAnim;

        private void Update()
        {
            if (player != null)
            {
                if (myCustomAnims.Count == 0)
                {


                    initEmotes();
                }

                if (KeyboardMinus.Value.IsDown())
                {
                    if (myAnimationKey != 0)
                    {
                        myAnimationKey --;
                        Logger.LogInfo(myCustomAnims);
                    }
                }
                if (KeyboardPlus.Value.IsDown())
                {
                    if (myAnimationKey != myCustomAnims.Count-1)
                    {
                        myAnimationKey++;          
                        Logger.LogInfo(myCustomAnims);
                    }
                }

                if (showMenu)
                {
                    if (myAnimationKey == 0)
                    {
                        UI.Instance.ShowNotification("",myCustomAnims.ElementAt(myAnimationKey).Value + " <", myCustomAnims.ElementAt(myAnimationKey + 1).Value);
                    }
                    else if (myAnimationKey == myCustomAnims.Count-1)
                    {
                        UI.Instance.ShowNotification(myCustomAnims.ElementAt(myAnimationKey - 1).Value, myCustomAnims.ElementAt(myAnimationKey).Value + " <", "");
                    }
                    else
                    {
                        UI.Instance.ShowNotification(myCustomAnims.ElementAt(myAnimationKey - 1).Value, myCustomAnims.ElementAt(myAnimationKey).Value + " <", myCustomAnims.ElementAt(myAnimationKey + 1).Value);
                    }
                    if (KeyboardConfirm.Value.IsDown())
                    {
                        player.ActivateAbility(player.sitAbility);
                        player.PlayAnim(myCustomAnims.ElementAt(myAnimationKey).Key, true, true,-1f);
                        UI.Instance.HideNotification();
                        timer = 0.5f;
                        showMenu = false;
                        inAnimation = true;
                    }
                }

                if (KeyboardConfirm.Value.IsDown() && player.moveStyle.ToString() == "ON_FOOT")
                {
                    initEmotes();
                }

                if (timer >0)
                {
                    timer -= Time.deltaTime; // Increment the timer while the key is pressed
                }

                if (!showMenu && timer <=0)
                {
                    UI.Instance.HideNotification();
                    if (KeyboardConfirm.Value.IsDown())
                    {
                        showMenu = true;
                    }
                }
            }

            //if (UnityEngine.Input.GetKey(KeyCode.JoystickButton9) || UnityEngine.Input.GetKey(KeyCode.JoystickButton11))
            //{
            //    // Check if the key was just pressed or has been held for 2 seconds
            //    if (keyPressStartTime == 0f)
            //    {
            //        // Record the start time when the key is first pressed
            //        keyPressStartTime = Time.time;
            //    }
            //    else if (Time.time - keyPressStartTime >= 2f)
            //    {
            //        // The key has been held for more than 2 seconds
            //        player.ActivateAbility(player.sitAbility);
            //    }
            //}
            //else
            //{
            //    // Reset the start time when the key is released
            //    keyPressStartTime = 0f;
            //}
        }

        public static Animation anim;

        public static void initEmotes()
        {
            //anim = player
            //new Player.AnimInfo(player, "testy", Player.AnimType.NORMAL, "", 0f, 0f, 1f);

            //foreach (AnimationState state in anim)
            //{
            //    if (state.name == "lockPick")
            //    {
            //        Log.LogMessage("lockpick found");
            //    }

            //    if (state.name == "testy")
            //    {
            //        Log.LogMessage("testy found");
            //    }
            //}

            //freestyle1 > 18 
            childcount = player.transform.GetChild(0).childCount;
            player.transform.GetChild(0).GetChild(childcount - 1).GetChild(0).GetComponent<Animator>().runtimeAnimatorController = myAnim;

            AnimationClip[] clips = myAnim.animationClips;

            if (customList.Value == true && myCustomList.Value != "")
            {
                myCustomAnims = FillDictionaryFromCommaSeparatedString(myCustomList.Value); 
            }
            else
            {
                myCustomAnims = new Dictionary<int, string>();

                myCustomAnims[Animator.StringToHash("lieDown")] = "lieDown";
                myCustomAnims[Animator.StringToHash("joinCypher3")] = "test";
                myCustomAnims[Animator.StringToHash("MocapChoregraphy")] = "MocapChoregraphy";
                myCustomAnims[Animator.StringToHash("surrender")] = "surrender";
                myCustomAnims[Animator.StringToHash("getUp")] = "getUp";
                myCustomAnims[Animator.StringToHash("belStandingPhoneIdle")] = "belStandingPhoneIdle";
                myCustomAnims[Animator.StringToHash("leanWall")] = "leanWall";
                myCustomAnims[Animator.StringToHash("sitLegsCrossed")] = "sitLegsCrossed";
                myCustomAnims[Animator.StringToHash("squatPhone")] = "squatPhone";
                myCustomAnims[Animator.StringToHash("squatPhoneHuh")] = "squatPhoneHuh";




                myCustomAnims[int.Parse("-2109934503")] = "idle dance 1";
                myCustomAnims[int.Parse("-386899626")] = "idle dance 2";
                myCustomAnims[int.Parse("1912156396")] = "idle dance 3";
                myCustomAnims[int.Parse("117309562")] = "idle dance 4";
                myCustomAnims[int.Parse("-1734699559")] = "idle dance 5";
                myCustomAnims[int.Parse("-274881201")] = "idle dance 6";
                myCustomAnims[int.Parse("1989473525")] = "idle dance 7";
                myCustomAnims[int.Parse("26461283")] = "idle dance 8";
                myCustomAnims[int.Parse("-1859331598")] = "idle dance 9";
                myCustomAnims[int.Parse("-433329820")] = "idle dance 10";
                myCustomAnims[int.Parse("1983634305")] = "idle dance 11";
                myCustomAnims[int.Parse("20769559")] = "idle dance 12";
                myCustomAnims[int.Parse("-1741309267")] = "idle dance 13";
                myCustomAnims[int.Parse("-281900485")] = "idle dance 14";
                myCustomAnims[int.Parse("1901469592")] = "idle dance 15";
                myCustomAnims[int.Parse("105983758")] = "idle dance 16";
                myCustomAnims[int.Parse("-1621590348")] = "idle dance 17";
                myCustomAnims[int.Parse("-396407262")] = "idle dance 18";
                myCustomAnims[int.Parse("2027967411")] = "idle dance 19";
                myCustomAnims[int.Parse("1977987316")] = "dance 1";
                myCustomAnims[int.Parse("1975745410")] = "dance 2";
                myCustomAnims[int.Parse("-201765946")] = "dance 3";
                myCustomAnims[int.Parse("1591477752")] = "dance 4";
                myCustomAnims[int.Parse("486581086")] = "dance 5";
                myCustomAnims[int.Parse("-1825172808")] = "dance 6";
                myCustomAnims[int.Parse("-408874368")] = "lockpicking";
                myCustomAnims[dieAbility.dieHash] = "die";
                myCustomAnims[int.Parse("942417314")] = "handplant ";
                myCustomAnims[int.Parse("766504765")] = "get up from sit down";
                //myCustomAnims[int.Parse("328190043")] =         "chair sit down";
                //myCustomAnims[int.Parse("2025091455")] =        "chair sit down";
                //myCustomAnims[int.Parse("-866131528")] =        "chair sit down";
                //myCustomAnims[int.Parse("1257289357")] =        "chair sit down";
                //myCustomAnims[int.Parse("-738597065")] =        "chair sit down";
                myCustomAnims[int.Parse("-1182623238")] = "chain T then death";
                myCustomAnims[int.Parse("-1459645111")] = "chain debating";
                //myCustomAnims[int.Parse("828946306")] =         "chain caught";
                myCustomAnims[int.Parse("-1469979080")] = "chain caught";
                myCustomAnims[int.Parse("-1919845803")] = "walking";
                myCustomAnims[int.Parse("317602828")] = "cartwheel";
                myCustomAnims[int.Parse("1709771930")] = "low-spin kick";
                myCustomAnims[int.Parse("-52404960")] = "high-spin kick";
                myCustomAnims[int.Parse("-1947767370")] = "handstand kick";
                //myCustomAnims[int.Parse("241309284")]=          "cartwheel x2";
                myCustomAnims[int.Parse("426974652")] = "high-spin kick into run";
                myCustomAnims[int.Parse("1853107498")] = "cartwheel into run";
                myCustomAnims[int.Parse("-142771056")] = "low-spin kick into run";
                myCustomAnims[int.Parse("-2139468794")] = "handstand kick into run";
                myCustomAnims[int.Parse("-890375202")] = "boost trick (ground, no gear)";
                //myCustomAnims[int.Parse("-1108688056")]=        "boost trick (ground, no gear)*";
                //myCustomAnims[int.Parse("618894066")]=          "boost trick (ground, no gear)**";
                //myCustomAnims[int.Parse("-2060184142")]=        "cartwheel into run";
                //myCustomAnims[int.Parse("392676792")]=          "cartwheel";
                myCustomAnims[int.Parse("-1782641262")] = "remove skateboard on stairs";
                myCustomAnims[int.Parse("-1852872249")] = "remove skateboard";
                myCustomAnims[int.Parse("-848711311")] = "remove bmx on stairs ??";
                myCustomAnims[int.Parse("638283417")] = "remove bmx";
                myCustomAnims[int.Parse("1770817977")] = "spin slide running";
                myCustomAnims[int.Parse("220868516")] = "remove rollers";
                myCustomAnims[int.Parse("-1476340264")] = "landing";
                //myCustomAnims[int.Parse("1349952704")]=         "running";
                myCustomAnims[int.Parse("1816649355")] = "start running";
                myCustomAnims[int.Parse("-78586100")] = "air fall";
                //myCustomAnims[int.Parse("549560670")]=          "air fall";
                myCustomAnims[int.Parse("-2115972706")] = "BMX air trick ";
                myCustomAnims[int.Parse("-496818058")] = "vending machine kick";
                myCustomAnims[int.Parse("-140994052")] = "air trick";
                myCustomAnims[int.Parse("-1591718491")] = "air trick";
                myCustomAnims[int.Parse("-702055117")] = "air trick 1";
                myCustomAnims[int.Parse("1328426121")] = "air trick 2";
                myCustomAnims[int.Parse("942218271")] = "air trick 3";
                myCustomAnims[int.Parse("787922390")] = "air boost trick";
                myCustomAnims[int.Parse("1509002560")] = "air boost trick";
                myCustomAnims[int.Parse("-1057432326")] = "air boost trick";
                myCustomAnims[int.Parse("401520474")] = "landing into run";
                myCustomAnims[int.Parse("288735129")] = "choppy run";
                myCustomAnims[int.Parse("930619693")] = "stop walking";
                myCustomAnims[int.Parse("1993323932")] = "idle animation straight look";
                myCustomAnims[int.Parse("-1092259894")] = "boosting";
                myCustomAnims[int.Parse("-528801622")] = "boost dash end drift";
                myCustomAnims[int.Parse("2074719062")] = "huge knock back";
                myCustomAnims[int.Parse("-1939675132")] = "hit";
                myCustomAnims[int.Parse("968025800")] = "knocked back";
                myCustomAnims[int.Parse("1252092624")] = "sit down state (end position)";
                myCustomAnims[int.Parse("1989152923")] = "sit down actual animation";
                //myCustomAnims[int.Parse("-601574123")]=         "idle animation ? straight look";
                myCustomAnims[int.Parse("637092816")] = "die wake up";
                //myCustomAnims[int.Parse("750572753")]=          "idle animation";
                //myCustomAnims[int.Parse("-1246395029")]=        "idle animation";
                myCustomAnims[int.Parse("106747952")] = "air dash";
                myCustomAnims[int.Parse("783626958")] = "roll into slide";
                myCustomAnims[int.Parse("1928326754")] = "slide";
                myCustomAnims[int.Parse("-1788093250")] = "slide end";
                myCustomAnims[int.Parse("-13612767")] = "Mantle";
                myCustomAnims[int.Parse("517785735")] = "Mantle";
                //myCustomAnims[int.Parse("-798955003")]=         "Running";
            }

        }

        public static Dictionary<int, string> FillDictionaryFromCommaSeparatedString(string input)
        {
            Dictionary<int, string> hashDictionary = new Dictionary<int, string>();

            string[] strings = input.Split(',');

            foreach (string str in strings)
            {
                int hash = Animator.StringToHash(str);
                hashDictionary[hash] = str;
            }

            return hashDictionary;
        }

        public static void LogDictionaryContents<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
            }
        }

        private void ConfigSettingChanged(object sender, System.EventArgs e)
        {
            SettingChangedEventArgs settingChangedEventArgs = e as SettingChangedEventArgs;

            // Check if null and return
            if (settingChangedEventArgs == null)
            {
                return;
            }

            // Example Keyboard Shortcut setting changed handler
            if (settingChangedEventArgs.ChangedSetting.Definition.Key == KeyboardPlusKey)
            {
                KeyboardShortcut newValue = (KeyboardShortcut)settingChangedEventArgs.ChangedSetting.BoxedValue;
            }
        }
    }

}
