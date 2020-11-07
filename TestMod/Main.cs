using CumLoader;

namespace TestMod
{
    public static class BuildInfo
    {
        public const string Name = "TestMod"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "Mod for Testing"; // Description for the Mod.  (Set as null if none)
        public const string Author = null; // Author of the Mod.  (Set as null if none)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class TestMod : CumMod
    {
        public override void OnApplicationStart() // Runs after Game Initialization.
        {
            CumLogger.Log("OnApplicationStart");
        }

        public override void OnLevelIsLoading() // Runs when a Scene is Loading or when a Loading Screen is Shown. Currently only runs if the Mod is used in BONEWORKS.
        {
            CumLogger.Log("OnLevelIsLoading");
        }

        public override void OnLevelWasLoaded(int level) // Runs when a Scene has Loaded.
        {
            CumLogger.Log("OnLevelWasLoaded: " + level.ToString());
        }

        public override void OnLevelWasInitialized(int level) // Runs when a Scene has Initialized.
        {
            CumLogger.Log("OnLevelWasInitialized: " + level.ToString());
        }

        public override void OnUpdate() // Runs once per frame.
        {
            CumLogger.Log("OnUpdate");
        }

        public override void OnFixedUpdate() // Can run multiple times per frame. Mostly used for Physics.
        {
            CumLogger.Log("OnFixedUpdate");
        }

        public override void OnLateUpdate() // Runs once per frame after OnUpdate and OnFixedUpdate have finished.
        {
            CumLogger.Log("OnLateUpdate");
        }

        public override void OnGUI() // Can run multiple times per frame. Mostly used for Unity's IMGUI.
        {
            CumLogger.Log("OnGUI");
        }

        public override void OnApplicationQuit() // Runs when the Game is told to Close.
        {
            CumLogger.Log("OnApplicationQuit");
        }

        public override void OnModSettingsApplied() // Runs when Mod Preferences get saved to CumData/modprefs.ini.
        {
            CumLogger.Log("OnModSettingsApplied");
        }

        public override void VRChat_OnUiManagerInit() // Runs upon VRChat's UiManager Initialization. Only runs if the Mod is used in VRChat.
        {
            CumLogger.Log("VRChat_OnUiManagerInit");
        }
    }
}