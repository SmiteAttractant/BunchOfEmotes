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
using System.Security.Cryptography;
using System.IO;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace BunchOfEmotes
{
    // TODO Review this file and update to your own requirements.

    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class BunchOfEmotesPlugin : BaseUnityPlugin
    {

        private const string MyGUID = "com.Dragsun.BunchOfEmotes";
        private const string PluginName = "Bunch of emotes";
        private const string VersionString = "1.3.0";


        public static string KeyboardPlusKey = "Next emote";
        public static string KeyboardMinusKey = "Previous emote";
        public static string KeyboardConfirmKey = "Confirm / Open menu";
        public static string KeyboardSwapKey = "Swap between custom and normal menu";

        // Configuration entries. Static, so can be accessed directly elsewhere in code via
        // e.g.
        // float myFloat = BunchOfEmotesPlugin.FloatExample.Value;
        // TODO Change this code or remove the code if not required.
        public static ConfigEntry<KeyboardShortcut> KeyboardPlus;
        public static ConfigEntry<KeyboardShortcut> KeyboardMinus;
        public static ConfigEntry<KeyboardShortcut> KeyboardConfirm;
        public static ConfigEntry<KeyboardShortcut> KeyboardSwap;
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

        private readonly string animationFolder = Path.Combine(Application.streamingAssetsPath, "Mods", "BunchOfEmotes", "Anims");

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
            KeyboardSwap = Config.Bind("Custom emotes", KeyboardSwapKey, new KeyboardShortcut(KeyCode.B));
            customList = Config.Bind("Custom list", customListKey, defaultValue: false, "A custom list of emotes is the list that is gonna be replacing the basic one that appears when you trigger the mod in game. A list of all of them can be found on my Github");
            myCustomList = Config.Bind("Custom list", myCustomListKey, "jumpNEW,fallNEW,wallRunLeftNEW,grafSlashUP_RIGHT", "Your custom list of animations they must be without any spaces and separated by ,");

            KeyboardPlus.SettingChanged += ConfigSettingChanged;

            // Apply all of our patches
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
        public static Dictionary<int, string> myCustomAnims2 = new Dictionary<int, string>();

        public DirectoryInfo AssetFolder { get; protected set; }

        public static bool customMenu = false;

        public static RuntimeAnimatorController myAnim;
        public static RuntimeAnimatorController myAnim2;

        private void Update()
        {
            if (player != null)
            {
                if (myCustomAnims.Count == 0 && myAnim != null)
                {
                    string path = Paths.PluginPath + "/BunchofEmotes/Anims/bunchofemotes";

                    AddAnimationClipToController(myAnim, path);

                    initEmotes();
                }

                if (KeyboardMinus.Value.IsDown())
                {
                    if (myAnimationKey != 0)
                    {
                        myAnimationKey --;
                    }
                }
                if (KeyboardPlus.Value.IsDown())
                {
                    if (myAnimationKey != myCustomAnims.Count-1)
                    {
                        myAnimationKey++;          
                    }
                }

                if (KeyboardSwap.Value.IsDown())
                {
                    if (myCustomAnims2 != null)
                    {                    
                        customMenu = !customMenu;
                        initEmotes();
                    }
                }

                if (showMenu)
                {
                    if (myAnimationKey == 0)
                    {
                        if (myCustomAnims.Count != 1)
                        {                        
                            UI.Instance.ShowNotification("",myCustomAnims.ElementAt(myAnimationKey).Value + " <", myCustomAnims.ElementAt(myAnimationKey + 1).Value);
                        }
                        else
                        {
                            UI.Instance.ShowNotification("",myCustomAnims.ElementAt(myAnimationKey).Value + " <","");
                        }
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
                        inAnimation = true;
                        childcount = player.transform.GetChild(0).childCount;
                        if (customMenu)
                        {
                            player.transform.GetChild(0).GetChild(childcount - 1).GetChild(0).GetComponent<Animator>().runtimeAnimatorController = myAnim;
                            player.ActivateAbility(player.sitAbility);
                            //player.PlayAnim(myCustomAnims2.ElementAt(myAnimationKey).Key, false, false, -1f);
                            player.PlayAnim(myCustomAnims2.ElementAt(myAnimationKey).Key, false, false, -1f);
                        }
                        else
                        {
                            player.transform.GetChild(0).GetChild(childcount - 1).GetChild(0).GetComponent<Animator>().runtimeAnimatorController = myAnim;
                            player.ActivateAbility(player.sitAbility);
                            player.PlayAnim(myCustomAnims.ElementAt(myAnimationKey).Key, false, false, -1f);
                        }


                        UI.Instance.HideNotification();
                        timer = 0.5f;
                        showMenu = false;
                        player.ui.TurnOn(true);
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
        }

        public static Animation anim;

        public static void initEmotes()
        {
            childcount = player.transform.GetChild(0).childCount;
            //player.transform.GetChild(0).GetChild(childcount - 1).GetChild(0).GetComponent<Animator>().runtimeAnimatorController = myAnim;

            AnimationClip[] clips = myAnim.animationClips;

            if (customList.Value == true && myCustomList.Value != "")
            {
                myCustomAnims = FillDictionaryFromCommaSeparatedString(myCustomList.Value); 
            }
            else
            {

                string defaultList = "lieDown,sit,SolaceSit,surrender,HandsOnHipsIdle,belStandingPhoneIdle,redCrouchIdle,tryceCrossedArms,headScrewing,leanWall,leanWallStill,rightHandOnHipIdle,presentingIdle,redKnockedOut,redsitGroundIdle,crouchingIdle,berlageStorySitIdle1,berlageStorySitIdle2,berlageStorySitIdle3,copStoryIdle,copStoryIdleCrossedArms,copStorySaluteIdle,squatPhone,squatPhoneHuh,FauxSit,sitLaidback,oldheadAIdle,oldheadASitIdle,oldheadBIdle,oldheadBSitIdle,oldheadCIdle,oldheadCSitIdle,tryceCrouch,tryceCrouchLook,copRadio,belCrouchLook,belCrouch,sniperCaptainSlumpedIdle,Ayoooo,shoutStory,sitLegsCrossed,RedSnipedHandOnEyeIdle,RedSnipedEndIdle,sniperCaptainKO,VinylStrangledIdle,DJStranglesIdle,RedHeadacheIdle,SolaceBetweenBuildingsIdle,sitSadFloor,storyLookAround,belShowsToStandingPhone,IrenePowerPose,sitPhoneHighTyping,sitPhoneHigh,solaceLayingDown,IreneCall,belShowsIdlePhone,sitPhoneTyping,leanWallSlowClappingIdle,onCouchIdle,RedLyingOnCouch,injuredLookUp,protectArmsWideIdle,layingFloor,highFiveIdle,eclipseStand_idle,eclipse_foresight02_idle,eclipse_foresight01_idle,gaspShock,squat";
                
                myCustomAnims = FillDictionaryFromCommaSeparatedString(defaultList); 

            }
            if (customMenu)
            {
                myCustomAnims = myCustomAnims2;
            }

            if (myAnimationKey > myCustomAnims.Count-1)
            {
                myAnimationKey = myCustomAnims.Count-1;
            }

        }

        public static AssetBundle bundle;
        public static AssetBundle bundleController;
        public static int[] customemoteshash;
        public static AnimatorOverrideController[] Controllers;
        public static RuntimeAnimatorController[] AControllers;
        public static Animator playerAn;


        //initialisation of our controller and adding the custom emotes to it.
        public static void AddAnimationClipToController(RuntimeAnimatorController baseController, string clipPath = null)
        {
            if(clipPath == null)
            {
                clipPath = Paths.PluginPath + "/BunchofEmotes/Anims/bunchofemotes";
            }

            string path = Paths.PluginPath + "/BunchofEmotes/RuntimeAnimatorController/bunchofemotescontroller";



            if (bundleController == null)
            {
                if (File.Exists(path))
                {
                    bundleController = AssetBundle.LoadFromFile(path);
                }
                else
                {
                    Log.LogError("No controller files found, mod might not work as intended.");
                    return;
                }
            }

            if (AControllers == null)
            {
                AControllers = bundleController.LoadAllAssets<RuntimeAnimatorController>();            
            }

            if (bundle == null)
            {
                if (File.Exists(clipPath))
                {
                    bundle = AssetBundle.LoadFromFile(clipPath);                
                }
                else
                {
                    Log.LogError("No custom animation files found, mod will work with only the one from the game.");
                    myAnim2 = null;
                    return;
                }
            }

            AnimationClip newClipToAdd = null;
            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();

            foreach (RuntimeAnimatorController controller in AControllers)
            {
                if (true)
                {
                    animatorOverrideController.runtimeAnimatorController = controller;
                }
            }

            int count = 0;
            var animationClips = bundle.LoadAllAssets<AnimationClip>();

            //animatorOverrideController.clips. = baseController.animationClips;

            HashSet<string> seenNames = new HashSet<string>();


            foreach (AnimationClip clips in baseController.animationClips)
            {
                customemoteshash.AddItem(clips.GetHashCode());

                string nameToCheck = clips.name;

                if (seenNames.Contains(nameToCheck))
                {
                    // The name has already been seen, so we do nothing
                }
                else
                {
                    try
                    {
                        string clipname = clips.name;

                        Log.LogMessage(animatorOverrideController[clipname].GetHashCode() + " = " + animatorOverrideController[clipname].name + " > to > " + clips.GetHashCode() + " = " + clips.name);

                        animatorOverrideController[clipname] = clips;

                        //animatorOverrideController[clipname].name = clips.name;

                        count++;

                        seenNames.Add(nameToCheck);
                    }
                    catch (Exception)
                    {
                        Log.LogMessage(clips.name + " is causing problems");
                    }



                }

            }

            foreach (AnimationClip clip in animationClips)
            {
                Log.LogMessage("Look at me, I'm the clip now");
                newClipToAdd = clip as AnimationClip;


                if (newClipToAdd == null)
                {
                    Debug.LogError("Animation clip not found: " + clipPath);
                    break;
                }

                AnimationClipPair clipPair = new AnimationClipPair
                {
                    originalClip = newClipToAdd,
                    overrideClip = newClipToAdd
                };

                string nameofthereplacedanimation = animatorOverrideController.animationClips[count].name;

                myCustomAnims2[Animator.StringToHash(nameofthereplacedanimation)] = newClipToAdd.name;

                animatorOverrideController[nameofthereplacedanimation] = newClipToAdd;

                Log.LogMessage(nameofthereplacedanimation + " > to > " + clipPair.overrideClip.name);

                count++;

            }

            Log.LogMessage("Custom animations succesfully loaded.");
            myAnim = animatorOverrideController;

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
