using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Reptile;
using HarmonyLib;
using UnityEngine;

using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using BunchOfEmotes.Patches;
using System.Security.Cryptography;
using System.IO;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using System.Text.RegularExpressions;
using static DynamicBoneColliderBase;

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
        public static ConfigEntry<bool> wantACustomListOfInject;
        public static ConfigEntry<string> myCustomList;
        public static ConfigEntry<string> myCustomListOfInject;
        public static DieAbility dieAbility;
        public static StageManager stageManager;
        public static bool showMenu = false;
        public static bool myVariable = true; // This is the variable you want to toggle
        public static float timer = 0.0f;
        public static bool keyIsPressed = false;
        public static bool inAnimation = false;
        public static string customListKey = "Do you want to use a custom list ?";
        public static string wantACustomListOfInjectKey = "Do you want your list of injected animation to be customised ?";
        public static string myCustomListKey = "Enter your custom list";
        public static string myCustomListOfInjectKey = "Custom list for the injected animations (works for the in game one too)";

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
            wantACustomListOfInject = Config.Bind("Injected emotes list", wantACustomListOfInjectKey, defaultValue: false, "A custom list but for the injected animations");
            myCustomListOfInject = Config.Bind("Injected emotes list", myCustomListOfInjectKey, "jumpNEW,fallNEW,wallRunLeftNEW,grafSlashUP_RIGHT", "Same thing as the custom list but this one also accept custom animations. Write their name with the spaces and uppercases.");

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
        public static Dictionary<int, string> myCustomAnimsInject = new Dictionary<int, string>(); 

        public DirectoryInfo AssetFolder { get; protected set; }
        public static string BunchOfEmotesPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static bool customMenu = false;

        public static NPC myNPC;
        public static RuntimeAnimatorController myAnim;
        public static RuntimeAnimatorController myAnimBMX;
        public static RuntimeAnimatorController myAnimInlines;
        public static RuntimeAnimatorController myAnimSkateboard;
        public static RuntimeAnimatorController myAnimUntouched;
        public static RuntimeAnimatorController myAnim2;

        private void Update()
        {
            if (player != null)
            {
                if (myCustomAnims.Count == 0 && myAnim != null)
                {
                    string path = BunchOfEmotesPath + "/bunchofemotes";

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
                        if (customMenu && !wantACustomListOfInject.Value)
                        {
                            //player.anim.SetLayerWeight(5, 1f);
                            //player.transform.GetChild(0).GetChild(childcount - 1).GetChild(0).GetComponent<Animator>().runtimeAnimatorController = myAnim;
                            player.ActivateAbility(player.sitAbility);
                            //player.PlayAnim(myCustomAnims2.ElementAt(myAnimationKey).Key, false, false, -1f);
                            player.PlayAnim(myCustomAnims2.ElementAt(myAnimationKey).Key, false, false, -1f);
                        }
                        else if(customMenu && wantACustomListOfInject.Value)
                        {
                            //player.anim.SetLayerWeight(5, 1f);
                            //player.transform.GetChild(0).GetChild(childcount - 1).GetChild(0).GetComponent<Animator>().runtimeAnimatorController = myAnim;
                            player.ActivateAbility(player.sitAbility);
                            player.PlayAnim(myCustomAnimsInject.ElementAt(myAnimationKey).Key, false, false, -1f);
                        }
                        else
                        {
                            //player.anim.SetLayerWeight(5, 0f);
                            //player.transform.GetChild(0).GetChild(childcount - 1).GetChild(0).GetComponent<Animator>().runtimeAnimatorController = myAnim;
                            player.ActivateAbility(player.sitAbility);
                            player.PlayAnim(myCustomAnims.ElementAt(myAnimationKey).Key, false, false, -1f);
                        }


                        UI.Instance.HideNotification();
                        timer = 0.1f;
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
            if (customMenu && !wantACustomListOfInject.Value)
            {
                myCustomAnims = myCustomAnims2;
            }
            else if (customMenu && wantACustomListOfInject.Value)
            {
                myCustomAnims = BunchOfEmotesPlugin.myCustomAnimsInject;
            }

            if (myAnimationKey > myCustomAnims.Count-1)
            {
                myAnimationKey = myCustomAnims.Count-1;
            }

        }

        public static AssetBundle bundle;
        public static AssetBundle bundleController;
        public static AssetBundle bundleControllerinlines;
        public static AssetBundle bundleControllerSkateboard;
        public static int[] customemoteshash;
        public static int[] customemotesCheck = null;
        public static AnimatorOverrideController[] Controllers;
        public static RuntimeAnimatorController[] AControllers;
        public static Animator playerAn;


        //initialisation of our controller and adding the custom emotes to it.
        public static void AddAnimationClipToController(RuntimeAnimatorController baseController, string clipPath = null)
        {
            if(clipPath == null)
            {
                clipPath = Paths.PluginPath + "/Dragsun-Bunch_Of_Emotes/bunchofemotes";
            }

            string path = BunchOfEmotesPath + "/bunchofemotescontroller";
            string pathbmx = BunchOfEmotesPath + "/bunchofemotescontrollerbmx";
            string pathinlines = BunchOfEmotesPath + "/bunchofemotescontrollerinlines";
            string pathskateboard = BunchOfEmotesPath + "/bunchofemotescontrollerskateboard";

            string replace = BunchOfEmotesPath + "/replace";
            string replaceBMX = BunchOfEmotesPath + "/replacebmx";
            string replaceInline = BunchOfEmotesPath + "/replaceinline";
            string replaceSkateboard = BunchOfEmotesPath + "/replaceskateboard";
            AssetBundle replaceBundle = null;
            AssetBundle replaceBundleBMX = null;
            AssetBundle replaceBundleInline = null;
            AssetBundle replaceBundleSkateboard = null;
            AnimationClip[] replaceAnimations = null;
            AnimationClip[] replaceAnimationsBMX = null;
            AnimationClip[] replaceAnimationsInline = null;
            AnimationClip[] replaceAnimationsSkateboard = null;

            RuntimeAnimatorController[] ControllersInlines = null;
            RuntimeAnimatorController[] ControllersBmx = null;
            RuntimeAnimatorController[] ControllersSkateboard = null;



            //loading the RuntimeAnimatorController

            bundleController = loadFromString(path, bundleController);
            bundleControllerinlines = loadFromString(pathinlines, bundleControllerinlines);
            bundleControllerSkateboard = loadFromString(pathskateboard, bundleControllerSkateboard);


            //loading the replaced animations
            replaceBundle = loadFromString(replace, replaceBundle);
            replaceBundleBMX = loadFromString(replaceBMX, replaceBundleBMX);
            replaceBundleInline = loadFromString(replaceInline, replaceBundleInline);
            replaceBundleSkateboard = loadFromString(replaceSkateboard, replaceBundleSkateboard);

            //loading from bundle
            if (AControllers == null)
            {
                AControllers = bundleController.LoadAllAssets<RuntimeAnimatorController>();            
            }

            if (bundleControllerinlines != null)
            {
                ControllersInlines = bundleControllerinlines.LoadAllAssets<RuntimeAnimatorController>();
            }
            if (bundleControllerSkateboard != null)
            {
                ControllersSkateboard = bundleControllerSkateboard.LoadAllAssets<RuntimeAnimatorController>();
            }

            //loading from replace bundle
            if (replaceAnimations == null && replaceBundle != null)
            {
                replaceAnimations = replaceBundle.LoadAllAssets<AnimationClip>();
            }
            if (replaceAnimationsBMX == null && replaceBundleBMX != null)
            {
                replaceAnimationsBMX = replaceBundleBMX.LoadAllAssets<AnimationClip>();
            }
            if (replaceAnimationsInline == null && replaceBundleInline != null)
            {
                replaceAnimationsInline = replaceBundleInline.LoadAllAssets<AnimationClip>();
            }
            if (replaceAnimationsSkateboard == null && replaceBundleSkateboard != null)
            {
                replaceAnimationsSkateboard = replaceBundleSkateboard.LoadAllAssets<AnimationClip>();
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
            AnimatorOverrideController animatorOverrideControllerbmx = new AnimatorOverrideController();
            AnimatorOverrideController animatorOverrideControllerinlines = new AnimatorOverrideController();
            AnimatorOverrideController animatorOverrideControllerskateboard = new AnimatorOverrideController();

            foreach (RuntimeAnimatorController controller in AControllers)
            {
                if (true)
                {
                    animatorOverrideController.runtimeAnimatorController = controller;
                }
            }

            if (ControllersInlines != null)
            {
                foreach (RuntimeAnimatorController controller in ControllersInlines)
                {
                    if (true)
                    {
                        animatorOverrideControllerinlines.runtimeAnimatorController = controller;
                        animatorOverrideControllerinlines.name = "BunchOfEmotesControllerinlines";
                    }
                }                
            }
            if (ControllersSkateboard != null)
            {
                foreach (RuntimeAnimatorController controller in ControllersSkateboard)
                {
                    if (true)
                    {
                        animatorOverrideControllerskateboard.runtimeAnimatorController = controller;
                        animatorOverrideControllerskateboard.name = "BunchOfEmotesControllerSkateboard";
                    }
                }
            }

            Log.LogDebug(animatorOverrideController.runtimeAnimatorController.name);

            animatorOverrideController.name = "BunchOfEmotesController";
            int count = 0;
            var animationClips = bundle.LoadAllAssets<AnimationClip>();

            //animatorOverrideController.clips. = baseController.animationClips;

            HashSet<string> seenNames = new HashSet<string>();
            List<int> termsList = new List<int>();
            
            foreach (AnimationClip clip in baseController.animationClips)
            {
                if (clip != null)
                {
                    customemoteshash.AddItem(clip.GetHashCode());

                    string nameToCheck = clip.name;

                    if (seenNames.Contains(nameToCheck))
                    {
                        // The name has already been seen, so we do nothing
                    }
                    else
                    {
                        try
                        {
                            string clipname = clip.name;

                            animatorOverrideController[clipname] = clip;

                            //animatorOverrideController[clipname].name = clip.name;


                            seenNames.Add(nameToCheck);
                        }
                        catch (Exception)
                        {
                            Log.LogError(clip.name + " is causing problems");
                        }

                    }
                }

            }

            //aaaaa i love animations for the skates
            foreach (AnimationClip clips in player.animatorControllerSkates.animationClips)
            {
                if (clips != null && ControllersInlines != null)
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

                            animatorOverrideControllerinlines[clipname] = clips;

                            //animatorOverrideController[clipname].name = clips.name;


                            seenNames.Add(nameToCheck);
                        }
                        catch (Exception)
                        {
                            Log.LogError(clips.name + " is causing problems");
                        }

                    }
                }
            }

            if (bundleControllerinlines != null)
            {
                player.animatorControllerSkates = animatorOverrideControllerinlines;             
            }

            //aaaaa i love animations for the skates
            foreach (AnimationClip clips in player.animatorControllerSkateboard.animationClips)
            {
                if (clips != null && ControllersInlines != null)
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

                            animatorOverrideControllerskateboard[clipname] = clips;

                            //animatorOverrideController[clipname].name = clips.name;


                            seenNames.Add(nameToCheck);
                        }
                        catch (Exception)
                        {
                            Log.LogError(clips.name + " is causing problems");
                        }

                    }
                }
            }

            if (bundleControllerSkateboard != null)
            {
                player.animatorControllerSkateboard = animatorOverrideControllerskateboard;
            }

            var trueCustomIndex = 0;
            foreach (AnimationClip clips in animatorOverrideController.animationClips)
            {
                try
                {
                    if (clips.name == "z1")
                    {
                        trueCustomIndex = count;
                        Log.LogDebug("found " + count); 
                        break;
                    }
                }
                catch (Exception e)
                {
                    Log.LogError(e.Message); 
                }
                count++;


            }

            count = AddAnimations(animationClips, animatorOverrideController, count, termsList);

            //AnimatorOverrideController contSkateboard = new AnimatorOverrideController();
            //contSkateboard.runtimeAnimatorController = player.animatorControllerSkateboard;
            //AnimatorOverrideController contBMX = new AnimatorOverrideController();
            //contBMX.runtimeAnimatorController = player.animatorControllerBMX;
            //AnimatorOverrideController contInline = new AnimatorOverrideController();
            //contInline.runtimeAnimatorController = player.animatorControllerSkates;

            injectAnimation(replaceAnimations, animatorOverrideController);
            injectAnimation(replaceAnimationsBMX, player.animatorControllerBMX);
            injectAnimation(replaceAnimationsInline, player.animatorControllerSkates);
            injectAnimation(replaceAnimationsSkateboard, player.animatorControllerSkateboard);

            //player.animatorControllerSkateboard = contSkateboard;
            //player.animatorControllerSkates = contInline;
            //player.animatorControllerBMX = contBMX;

            string pattern = @"bunchofemotes\d";
            Regex regex = new Regex(pattern);
            var info = new DirectoryInfo(BunchOfEmotesPath + "/bulk/");
            var fileInfo = info.GetFiles();
            foreach (var item in fileInfo)
            {
                var info = new DirectoryInfo(Paths.PluginPath + "/Dragsun-Bunch_Of_Emotes/bulk/");
                var fileInfo = info.GetFiles();
                foreach (var item in fileInfo)
                {
                    if (regex.IsMatch(item.Name)) 
                    {
                        bundle.Unload(false);
                        bundle = AssetBundle.LoadFromFile(item.FullName);
                        animationClips = bundle.LoadAllAssets<AnimationClip>();
                        count = AddAnimations(animationClips, animatorOverrideController, count, termsList);
                        Log.LogMessage(item.Name + " loaded");
                    }
                }
            }


            customemotesCheck = termsList.ToArray();

            Log.LogMessage("Custom animations succesfully loaded.");
            myAnim = animatorOverrideController;

            myAnimBMX = player.animatorControllerBMX;
            myAnimInlines = player.animatorControllerSkates;
            myAnimSkateboard = player.animatorControllerSkateboard;

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

        private static AssetBundle loadFromString(string path, AssetBundle bundleTemp)
        {
            if (bundleTemp == null)
            {
                if (File.Exists(path))
                {
                    bundleTemp = AssetBundle.LoadFromFile(path);
                    //Log.LogError(path + " found");
                }
                else
                {
                    //Log.LogError(path + " doesnt exist skipping");
                    return null;
                }
            }

            return bundleTemp;
        }

        private static void injectAnimation(AnimationClip[] replaceAnimations, AnimatorOverrideController animatorOverrideController)
        {
            if (replaceAnimations != null)
            {
                foreach (AnimationClip clips in replaceAnimations)
                {
                    try
                    {

                        animatorOverrideController[clips.name] = clips;

                        Log.LogDebug("successfully replaced " + clips.name);

                    }
                    catch (Exception)
                    {
                        Log.LogMessage(clips.name + " is causing problems in the replace");
                    }
                }
            }
        }

        private static void injectAnimation(AnimationClip[] replaceAnimations, RuntimeAnimatorController animatorOverrideController)
        {
            if (replaceAnimations != null)
            {
                AnimatorOverrideController tempOverride = new AnimatorOverrideController();
                tempOverride.runtimeAnimatorController = animatorOverrideController;


                foreach (AnimationClip clips in replaceAnimations)
                {
                    try
                    {
                        tempOverride[clips.name] = clips;
                        Log.LogMessage(clips.name + " > " + clips);
                    }
                    catch (Exception)
                    {
                        Log.LogError(clips.name + " is causing problems in the replace");
                    }

                }
                animatorOverrideController = tempOverride;
            }

        }

        private static int AddAnimations(AnimationClip[] animationClips, AnimatorOverrideController animatorOverrideController, int count, List<int> termsList)
        {
            if (myCustomAnimsInject.Count == 0)
            {
                myCustomAnimsInject = FillDictionaryFromCommaSeparatedString(myCustomListOfInject.Value);      
            }


            foreach (AnimationClip clip in animationClips)
            {
                var newClipToAdd = clip as AnimationClip;

                if (newClipToAdd == null)
                {
                    Debug.LogError("Animation clip not found: " + animationClips);
                    break;
                }

                termsList.Add(newClipToAdd.GetHashCode());

                string nameofthereplacedanimation = animatorOverrideController.animationClips[count].name;

                BunchOfEmotesPlugin.myCustomAnims2[Animator.StringToHash(nameofthereplacedanimation)] = newClipToAdd.name;

                animatorOverrideController[nameofthereplacedanimation] = newClipToAdd;

                //Log.LogMessage(nameofthereplacedanimation + " > " + newClipToAdd.name); 

                string clipName = newClipToAdd.name;
                int clipHash = Animator.StringToHash(clipName);



                if (myCustomAnimsInject.ContainsKey(clipHash))
                {
                    var tempsid = Animator.StringToHash(newClipToAdd.name);

                    if (myCustomAnimsInject.Remove(tempsid))
                    {
                        myCustomAnimsInject.Add(Animator.StringToHash(nameofthereplacedanimation), newClipToAdd.name);
                    }
                    else
                    {
                        Log.LogError("Failed to remove the key.");
                    }
                }


                count++;

            }
            return count;
        }
    }

}
