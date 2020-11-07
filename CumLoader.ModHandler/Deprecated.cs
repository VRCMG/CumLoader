using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable 0108

namespace CumLoader
{
    [Obsolete("Main is obsolete. Please use CumLoaderBase or CumHandler instead.")]
    public static class Main
    {
        public static List<CumMod> CumMods = null;
        public static List<CumPlugin> CumPlugins = null;
        public static bool IsVRChat = false;
        public static bool IsBoneworks = false;
        public static string GetUnityVersion() => CumLoaderBase.UnityVersion;
        public static string GetCumDataPath() => CumLoaderBase.CumDataPath;
        internal static void LegacySupport(List<CumMod> mods, List<CumPlugin> plugins, bool isVRChat, bool isBoneworks)
        {
            CumMods = mods;
            CumPlugins = plugins;
            IsVRChat = isVRChat;
            IsBoneworks = isBoneworks;
        }
    }
    [Obsolete("CumModGame is obsolete. Please use CumGame instead.")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class CumModGameAttribute : Attribute
    {
        public string Developer { get; }
        public string GameName { get; }
        public CumModGameAttribute(string developer = null, string gameName = null)
        {
            Developer = developer;
            GameName = gameName;
        }
        internal CumGameAttribute Convert() => new CumGameAttribute(Developer, GameName);
    }
    [Obsolete("CumModInfo is obsolete. Please use CumInfo instead.")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class CumModInfoAttribute : Attribute
    {
        public Type SystemType { get; }
        public string Name { get; }
        public string Version { get; }
        public string Author { get; }
        public string DownloadLink { get; }

        public CumModInfoAttribute(Type type, string name, string version, string author, string downloadLink = null)
        {
            SystemType = type;
            Name = name;
            Version = version;
            Author = author;
            DownloadLink = downloadLink;
        }
        internal CumInfoAttribute Convert() => new CumInfoAttribute(SystemType, Name, Version, Author, DownloadLink);
    }
    [Obsolete("CumPluginGame is obsolete. Please use CumGame instead.")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class CumPluginGameAttribute : Attribute
    {
        public string Developer { get; }
        public string GameName { get; }
        public CumPluginGameAttribute(string developer = null, string gameName = null)
        {
            Developer = developer;
            GameName = gameName;
        }
        public CumGameAttribute Convert() => new CumGameAttribute(Developer, GameName);
    }
    [Obsolete("CumPluginInfo is obsolete. Please use CumInfo instead.")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class CumPluginInfoAttribute : Attribute
    {
        public Type SystemType { get; }
        public string Name { get; }
        public string Version { get; }
        public string Author { get; }
        public string DownloadLink { get; }

        public CumPluginInfoAttribute(Type type, string name, string version, string author, string downloadLink = null)
        {
            SystemType = type;
            Name = name;
            Version = version;
            Author = author;
            DownloadLink = downloadLink;
        }
        public CumInfoAttribute Convert() => new CumInfoAttribute(SystemType, Name, Version, Author, DownloadLink);
    }
    [Obsolete("CumModLogger is obsolete. Please use CumLogger instead.")]
    public class CumModLogger : CumLogger {}
    [Obsolete("ModPrefs is obsolete. Please use CumPrefs instead.")]
    public class ModPrefs : CumPrefs
    {
        public static Dictionary<string, Dictionary<string, PrefDesc>> GetPrefs()
        {
            Dictionary<string, Dictionary<string, PrefDesc>> output = new Dictionary<string, Dictionary<string, PrefDesc>>();
            Dictionary<string, Dictionary<string, CumPreference>> prefs = GetPreferences();
            for (int i = 0; i < prefs.Values.Count; i++)
            {
                Dictionary<string, CumPreference> prefsdict = prefs.Values.ElementAt(i);
                Dictionary<string, PrefDesc> newprefsdict = new Dictionary<string, PrefDesc>();
                for (int j = 0; j < prefsdict.Values.Count; j++)
                {
                    CumPreference pref = prefsdict.Values.ElementAt(j);
                    PrefDesc newpref = new PrefDesc(pref.Value, (PrefType)pref.Type, pref.Hidden, pref.DisplayText);
                    newpref.ValueEdited = pref.ValueEdited;
                    newprefsdict.Add(prefsdict.Keys.ElementAt(j), newpref);
                }
                output.Add(prefs.Keys.ElementAt(i), newprefsdict);
            }
            return output;
        }
        public static void RegisterPrefString(string section, string name, string defaultValue, string displayText = null, bool hideFromList = false) => RegisterString(section, name, defaultValue, displayText, hideFromList);
        public static void RegisterPrefBool(string section, string name, bool defaultValue, string displayText = null, bool hideFromList = false) => RegisterBool(section, name, defaultValue, displayText, hideFromList);
        public static void RegisterPrefInt(string section, string name, int defaultValue, string displayText = null, bool hideFromList = false) => RegisterInt(section, name, defaultValue, displayText, hideFromList);
        public static void RegisterPrefFloat(string section, string name, float defaultValue, string displayText = null, bool hideFromList = false) => RegisterFloat(section, name, defaultValue, displayText, hideFromList);
        public enum PrefType
        {
            STRING,
            BOOL,
            INT,
            FLOAT
        }
        public class PrefDesc : CumPreference
        {
            public PrefType Type { get => (PrefType)base.Type; }
            public PrefDesc(string value, PrefType type, bool hidden, string displayText) : base(value, type, hidden, displayText)
            {
                Value = value;
                ValueEdited = value;
                base.Type = (CumPreferenceType)type;
                Hidden = hidden;
                DisplayText = displayText;
            }
        }
    }
 }