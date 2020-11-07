using System;
using System.Collections.Generic;
using System.IO;

namespace CumLoader
{
    public class CumPrefs
    {
        private static string ConfigFileName = "modprefs.ini";
        private static IniFile ConfigFile = null;
        private static Dictionary<string, Dictionary<string, CumPreference>> prefs = new Dictionary<string, Dictionary<string, CumPreference>>();
        private static Dictionary<string, string> categoryDisplayNames = new Dictionary<string, string>();

        internal static void Setup() { if (ConfigFile == null) ConfigFile = new IniFile(Path.Combine(CumLoaderBase.CumDataPath, ConfigFileName)); }

        public static void RegisterCategory(string name, string displayText) { categoryDisplayNames[name] = displayText; }
        public static void RegisterString(string section, string name, string defaultValue, string displayText = null, bool hideFromList = false) { Register(section, name, defaultValue, displayText, CumPreferenceType.STRING, hideFromList); }
        public static void RegisterBool(string section, string name, bool defaultValue, string displayText = null, bool hideFromList = false) { Register(section, name, defaultValue ? "true" : "false", displayText, CumPreferenceType.BOOL, hideFromList); }
        public static void RegisterInt(string section, string name, int defaultValue, string displayText = null, bool hideFromList = false) { Register(section, name, "" + defaultValue, displayText, CumPreferenceType.INT, hideFromList); }
        public static void RegisterFloat(string section, string name, float defaultValue, string displayText = null, bool hideFromList = false) { Register(section, name, "" + defaultValue, displayText, CumPreferenceType.FLOAT, hideFromList); }
        private static void Register(string section, string name, string defaultValue, string displayText, CumPreferenceType type, bool hideFromList)
        {
            if (prefs.TryGetValue(section, out Dictionary<string, CumPreference> prefsInSection))
            {
                if (prefsInSection.TryGetValue(name, out CumPreference pref))
                    CumLogger.LogError("Trying to registered Pref " + section + ":" + name + " more than one time");
                else
                {
                    string toStoreValue = defaultValue;
                    if (ConfigFile.HasKey(section, name))
                        toStoreValue = ConfigFile.GetString(section, name, defaultValue);
                    else ConfigFile.SetString(section, name, defaultValue);
                    prefsInSection.Add(name, new CumPreference(toStoreValue, type, hideFromList, (displayText ?? "") == "" ? name : displayText));
                }
            }
            else
            {
                Dictionary<string, CumPreference> dic = new Dictionary<string, CumPreference>();
                string toStoreValue = defaultValue;
                if (ConfigFile.HasKey(section, name))
                    toStoreValue = ConfigFile.GetString(section, name, defaultValue);
                else ConfigFile.SetString(section, name, defaultValue);
                dic.Add(name, new CumPreference(toStoreValue, type, hideFromList, (displayText ?? "") == "" ? name : displayText));
                prefs.Add(section, dic);
            }
        }

        public static bool HasKey(string section, string name) { return prefs.TryGetValue(section, out Dictionary<string, CumPreference> prefsInSection) && prefsInSection.ContainsKey(name); }
        public static Dictionary<string, Dictionary<string, CumPreference>> GetPreferences() { return prefs; }
        public static string GetCategoryDisplayName(string key) { if (categoryDisplayNames.TryGetValue(key, out string name)) return name; return key; }

        public static void SaveConfig()
        {
            foreach (KeyValuePair<string, Dictionary<string, CumPreference>> prefsInSection in prefs)
            {
                foreach (KeyValuePair<string, CumPreference> pref in prefsInSection.Value)
                {
                    pref.Value.Value = pref.Value.ValueEdited;
                    ConfigFile.SetString(prefsInSection.Key, pref.Key, pref.Value.Value);
                }
            }
            CumHandler.OnModSettingsApplied();
            CumLogger.Log("Config Saved!");
        }

        public static string GetString(string section, string name)
        {
            if (prefs.TryGetValue(section, out Dictionary<string, CumPreference> prefsInSection) && prefsInSection.TryGetValue(name, out CumPreference pref))
                return pref.Value;
            CumLogger.LogError("Trying to get unregistered Pref " + section + ":" + name);
            return "";
        }

        public static void SetString(string section, string name, string value)
        {
            if (prefs.TryGetValue(section, out Dictionary<string, CumPreference> prefsInSection) && prefsInSection.TryGetValue(name, out CumPreference pref))
            {
                pref.Value = pref.ValueEdited = value;
                ConfigFile.SetString(section, name, value);
            }
            else
                CumLogger.LogError("Trying to save unknown pref " + section + ":" + name);
        }

        public static bool GetBool(string section, string name)
        {
            if (prefs.TryGetValue(section, out Dictionary<string, CumPreference> prefsInSection) && prefsInSection.TryGetValue(name, out CumPreference pref))
                return (pref.Value.Equals("true") || pref.Value.Equals("1"));
            CumLogger.LogError("Trying to get unregistered Pref " + section + ":" + name);
            return false;
        }
        public static void SetBool(string section, string name, bool value) { SetString(section, name, value ? "true" : "false"); }

        public static int GetInt(string section, string name)
        {
            if (prefs.TryGetValue(section, out Dictionary<string, CumPreference> prefsInSection) && prefsInSection.TryGetValue(name, out CumPreference pref))
                if (int.TryParse(pref.Value, out int valueI))
                    return valueI;
            CumLogger.LogError("Trying to get unregistered Pref " + section + ":" + name);
            return 0;
        }
        public static void SetInt(string section, string name, int value) { SetString(section, name, value.ToString()); }

        public static float GetFloat(string section, string name)
        {
            if (prefs.TryGetValue(section, out Dictionary<string, CumPreference> prefsInSection) && prefsInSection.TryGetValue(name, out CumPreference pref))
                if (float.TryParse(pref.Value, out float valueF))
                    return valueF;
            CumLogger.LogError("Trying to get unregistered Pref " + section + ":" + name);
            return 0.0f;
        }
        public static void SetFloat(string section, string name, float value) { SetString(section, name, value.ToString()); }

        public enum CumPreferenceType
        {
            STRING,
            BOOL,
            INT,
            FLOAT
        }

        public class CumPreference
        {
            public string Value { get; set; }
            public string ValueEdited { get; set; }
            public CumPreferenceType Type { get; internal set; }
            public bool Hidden { get; internal set; }
            public String DisplayText { get; internal set; }

            public CumPreference(string value, CumPreferenceType type, bool hidden, string displayText)
            {
                Value = value;
                ValueEdited = value;
                Type = type;
                Hidden = hidden;
                DisplayText = displayText;
            }

            [Obsolete()]
            public CumPreference(string value, ModPrefs.PrefType type, bool hidden, string displayText)
            {
                Value = value;
                ValueEdited = value;
                Type = (CumPreferenceType)type;
                Hidden = hidden;
                DisplayText = displayText;
            }
        }
    }
}