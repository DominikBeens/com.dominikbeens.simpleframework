using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DB.SimpleFramework.SimpleGameSettings {

    public static class SimpleGameSettings {

        private const string PREFKEY_GLOBALQUALITY = "SimpleGameSettings_GlobalQuality";
        private const string PREFKEY_VSYNC = "SimpleGameSettings_VSync";
        private const string PREFKEY_ANTIALIASING = "SimpleGameSettings_AntiAliasing";
        private const string PREFKEY_RESOLUTION = "SimpleGameSettings_Resolution";
        private const string PREFKEY_SCREENMODE = "SimpleGameSettings_ScreenMode";

        private const int MIN_VIABLE_RESOLUTION_WIDTH = 800;
        private const int MIN_VIABLE_RESOLUTION_HEIGHT = 600;

        private static List<string> globalQualityOptions = new List<string>();
        private static List<Resolution> resolutions = new List<Resolution>();
        private static List<ScreenMode> screenModes = new List<ScreenMode>() {
            new ScreenMode { Name = "Fullscreen", Mode = FullScreenMode.ExclusiveFullScreen },
            new ScreenMode { Name = "Borderless", Mode = FullScreenMode.FullScreenWindow },
            new ScreenMode { Name = "Windowed", Mode = FullScreenMode.Windowed }
        };

        private struct ScreenMode {
            public string Name;
            public FullScreenMode Mode;
        }

        public static void ApplyAll() {
            if (SimplePersistence.HasInt(PREFKEY_GLOBALQUALITY, out int quality)) {
                CacheGlobalQualityOptions();
                SetGlobalQuality(quality);
            }
            if (SimplePersistence.HasInt(PREFKEY_VSYNC, out int vsync)) {
                SetVSync(vsync);
            }
            if (SimplePersistence.HasInt(PREFKEY_ANTIALIASING, out int aa)) {
                SetAntiAliasing(aa);
            }
            if (SimplePersistence.HasInt(PREFKEY_RESOLUTION, out int resolution)) {
                CacheResolutions();
                SetResolution(resolution);
            }
            if (SimplePersistence.HasInt(PREFKEY_SCREENMODE, out int screenmode)) {
                SetScreenMode(screenmode);
            }
        }

        public static List<string> GetGlobalQualityOptions() {
            CacheGlobalQualityOptions();
            return globalQualityOptions;
        }

        public static int GetGlobalQualityIndex() {
            CacheGlobalQualityOptions();
            string currentLevelName = QualitySettings.names[QualitySettings.GetQualityLevel()];
            return globalQualityOptions.FindIndex(x => x == currentLevelName);
        }

        public static void SetGlobalQuality(int level) {
            string levelName = QualitySettings.names[level];
            int index = globalQualityOptions.FindIndex(x => x == levelName);
            QualitySettings.SetQualityLevel(index, true);
            SimplePersistence.SetInt(PREFKEY_GLOBALQUALITY, level);
        }

        private static void CacheGlobalQualityOptions() {
            if (globalQualityOptions.Count > 0) { return; }
            globalQualityOptions = QualitySettings.names.Reverse().ToList();
        }

        public static List<string> GetVSyncOptions() {
            return new List<string>() { "Off", "On" };
        }

        public static int GetVSyncIndex() {
            return QualitySettings.vSyncCount;
        }

        public static void SetVSync(int level) {
            QualitySettings.vSyncCount = level;
            SimplePersistence.SetInt(PREFKEY_VSYNC, level);
        }

        public static List<string> GetAntiAliasingOptions() {
            return new List<string>() { "Off", "2x MSAA", "4x MSAA", "8x MSAA" };
        }

        public static int GetAntiAliasingIndex() {
            return (int)Mathf.Clamp(Mathf.Log(QualitySettings.antiAliasing, 2), 0, 3);
        }

        public static void SetAntiAliasing(int level) {
            QualitySettings.antiAliasing = (int)Mathf.Pow(2, Mathf.Clamp(level, 0, 3));
            SimplePersistence.SetInt(PREFKEY_ANTIALIASING, level);
        }

        public static List<string> GetResolutionOptions() {
            CacheResolutions();
            List<string> formatedResolutions = new List<string>();
            foreach (Resolution resolution in resolutions) {
                formatedResolutions.Add($"{resolution.width}x{resolution.height}");
            }
            return formatedResolutions;
        }

        public static int GetResolutionIndex() {
            CacheResolutions();
            return resolutions.FindIndex(x => x.width == Screen.width && x.height == Screen.height);
        }

        public static void SetResolution(int level) {
            Resolution resolution = resolutions[level];
            if (Screen.width == resolution.width && Screen.height == resolution.height) { return; }
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
            SimplePersistence.SetInt(PREFKEY_RESOLUTION, level);
        }

        private static void CacheResolutions() {
            if (resolutions.Count > 0) { return; }
            resolutions = Screen.resolutions.Select(x => new Resolution { width = x.width, height = x.height }).Distinct().Reverse().ToList();
            for (int i = resolutions.Count - 1; i >= 0; i--) {
                Resolution resolution = resolutions[i];
                if (resolution.width < MIN_VIABLE_RESOLUTION_WIDTH || resolution.height < MIN_VIABLE_RESOLUTION_HEIGHT) {
                    resolutions.Remove(resolution);
                }
            }
        }

        public static List<string> GetScreenModeOptions() {
            List<string> screenModes = new List<string>();
            foreach (ScreenMode screenMode in SimpleGameSettings.screenModes) {
                screenModes.Add(screenMode.Name);
            }
            return screenModes;
        }

        public static int GetScreenModeIndex() {
            return screenModes.FindIndex(x => x.Mode == Screen.fullScreenMode);
        }

        public static void SetScreenMode(int index) {
            ScreenMode screenMode = screenModes[index];
            if (screenMode.Mode == Screen.fullScreenMode) { return; }
            bool fullScreen = screenMode.Mode == FullScreenMode.ExclusiveFullScreen || screenMode.Mode == FullScreenMode.FullScreenWindow;
            Screen.fullScreenMode = screenMode.Mode;
            Screen.fullScreen = fullScreen;
            Screen.SetResolution(Screen.width, Screen.height, fullScreen);
            SimplePersistence.SetInt(PREFKEY_SCREENMODE, index);
        }

        public static void ResetAll() {
            SimplePersistence.DeleteKey(PREFKEY_GLOBALQUALITY);
            SimplePersistence.DeleteKey(PREFKEY_VSYNC);
            SimplePersistence.DeleteKey(PREFKEY_ANTIALIASING);
            SimplePersistence.DeleteKey(PREFKEY_RESOLUTION);
            SimplePersistence.DeleteKey(PREFKEY_SCREENMODE);
        }
    }
}
