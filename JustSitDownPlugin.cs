using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Reptile;
using HarmonyLib;
using UnityEngine;

namespace JustSitDown
{
    // TODO Review this file and update to your own requirements.

    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class JustSitDownPlugin : BaseUnityPlugin
    {
        // Mod specific details. MyGUID should be unique, and follow the reverse domain pattern
        // e.g.
        // com.mynameororg.pluginname
        // Version should be a valid version string.
        // e.g.
        // 1.0.0
        private const string MyGUID = "com.Dragsun.MyUnityGameMod1";
        private const string PluginName = "Just sit";
        private const string VersionString = "1.1.5";

        // Config entry key strings
        // These will appear in the config file created by BepInEx and can also be used
        // by the OnSettingsChange event to determine which setting has changed.
        public static string KeyboardShortcutExampleKey = "Recall Keyboard Shortcut";

        // Configuration entries. Static, so can be accessed directly elsewhere in code via
        // e.g.
        // float myFloat = JustSitDownPlugin.FloatExample.Value;
        // TODO Change this code or remove the code if not required.
        public static ConfigEntry<KeyboardShortcut> KeyboardShortcutExample;

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        /// <summary>
        /// Initialise the configuration settings and patch methods
        /// </summary>
        private void Awake()
        {
            // Keyboard shortcut setting example
            // TODO Change this code or remove the code if not required.
            KeyboardShortcutExample = Config.Bind("General",
                KeyboardShortcutExampleKey,
                new KeyboardShortcut(KeyCode.M));

            KeyboardShortcutExample.SettingChanged += ConfigSettingChanged;

            // Apply all of our patches
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");

            // Sets up our static Log, so it can be used elsewhere in code.
            // .e.g.
            // JustSitDownPlugin.Log.LogDebug("Debug Message to BepInEx log file");
            Log = Logger;
        }

        /// <summary>
        /// Code executed every frame. See below for an example use case
        /// to detect keypress via custom configuration.
        /// </summary>
        // TODO - Add your code here or remove this section if not required.

        public static Player player { get; set; }
        public static Vector3 playerpos1;
        private float keyPressStartTime = 0f;

        private void Update()
        {
            if (JustSitDownPlugin.KeyboardShortcutExample.Value.IsDown())
            {

                if (player != null)
                {
                    player.ActivateAbility(player.sitAbility);
                    Logger.LogInfo(player.gameObject.name);
                }
            }

            if (UnityEngine.Input.GetKey(KeyCode.JoystickButton9) || UnityEngine.Input.GetKey(KeyCode.JoystickButton11))
            {
                // Check if the key was just pressed or has been held for 2 seconds
                if (keyPressStartTime == 0f)
                {
                    // Record the start time when the key is first pressed
                    keyPressStartTime = Time.time;
                }
                else if (Time.time - keyPressStartTime >= 2f)
                {
                    // The key has been held for more than 2 seconds
                    player.ActivateAbility(player.sitAbility);
                }
            }
            else
            {
                // Reset the start time when the key is released
                keyPressStartTime = 0f;
            }
        }

        /// <summary>
        /// Method to handle changes to configuration made by the player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigSettingChanged(object sender, System.EventArgs e)
        {
            SettingChangedEventArgs settingChangedEventArgs = e as SettingChangedEventArgs;

            // Check if null and return
            if (settingChangedEventArgs == null)
            {
                return;
            }

            // Example Keyboard Shortcut setting changed handler
            if (settingChangedEventArgs.ChangedSetting.Definition.Key == KeyboardShortcutExampleKey)
            {
                KeyboardShortcut newValue = (KeyboardShortcut)settingChangedEventArgs.ChangedSetting.BoxedValue;
            }
        }
    }
}
