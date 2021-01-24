using UnityEngine;

namespace DB.SimpleFramework.SimpleGameSettings {

    public static class SimplePersistence {

        public static bool HasInt(string key, out int value) {
            if (PlayerPrefs.HasKey(key)) {
                value = GetInt(key);
                return true;
            } else {
                value = 0;
                return false;
            }
        }

        public static int GetInt(string key) {
            return PlayerPrefs.GetInt(key);
        }

        public static void SetInt(string key, int value) {
            PlayerPrefs.SetInt(key, value);
        }

        public static void DeleteKey(string key) {
            PlayerPrefs.DeleteKey(key);
        }
    }
}
