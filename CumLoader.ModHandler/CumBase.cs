using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CumLoader
{
    public abstract class CumBase
    {
        /// <summary>
        /// Gets the Assembly of the Mod or Plugin.
        /// </summary>
        public Assembly Assembly { get; internal set; }

        /// <summary>
        /// Gets the File Location of the Mod or Plugin.
        /// </summary>
        public string Location { get; internal set; }

        /// <summary>
        /// Enum for Cum Compatibility
        /// </summary>
        public enum CumCompatibility
        {
            UNIVERSAL = 0,
            COMPATIBLE = 1,
            NOATTRIBUTE = 2,
            INCOMPATIBLE = 3,
        }

        /// <summary>
        /// Gets the Compatibility of the Mod or Plugin.
        /// </summary>
        public CumCompatibility Compatibility { get; internal set; }

        /// <summary>
        /// Gets the Info Attribute of the Mod or Plugin.
        /// </summary>
        public CumInfoAttribute Info { get; internal set; }

        [Obsolete()]
        internal CumModInfoAttribute LegacyModInfo { get; set; }
        [Obsolete()]
        internal CumPluginInfoAttribute LegacyPluginInfo { get; set; }

        /// <summary>
        /// Gets the Game Attributes of the Mod or Plugin.
        /// </summary>
        public CumOptionalDependenciesAttribute OptionalDependenciesAttribute { get; internal set; }

        /// <summary>
        /// Gets the Game Attributes of the Mod or Plugin.
        /// </summary>
        public CumGameAttribute[] Games { get; internal set; }

        /// <summary>
        /// Gets the Auto-Created Harmony Instance of the Mod or Plugin.
        /// </summary>
        public Harmony.HarmonyInstance harmonyInstance { get; internal set; }

        [Obsolete()]
        internal CumModGameAttribute[] LegacyModGames { get; set; }
        [Obsolete()]
        internal CumPluginGameAttribute[] LegacyPluginGames { get; set; }

        public virtual void OnApplicationStart() { }
        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnGUI() { }
        public virtual void OnApplicationQuit() { }
        public virtual void OnModSettingsApplied() { }
        public virtual void VRChat_OnUiManagerInit() { }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class CumOptionalDependenciesAttribute : Attribute
    {
        /// <summary>
        /// The (simple) assembly names of the dependencies that should be regarded as optional.
        /// </summary>
        public string[] AssemblyNames { get; internal set; }

        public CumOptionalDependenciesAttribute(params string[] assemblyNames)
        {
            AssemblyNames = assemblyNames;
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class CumInfoAttribute : Attribute
    {
        /// <summary>
        /// Gets the System.Type of the Mod.
        /// </summary>
        public Type SystemType { get; internal set; }

        /// <summary>
        /// Gets the Name of the Mod.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the Version of the Mod.
        /// </summary>
        public string Version { get; internal set; }

        /// <summary>
        /// Gets the Author of the Mod.
        /// </summary>
        public string Author { get; internal set; }

        /// <summary>
        /// Gets the Download Link of the Mod.
        /// </summary>
        public string DownloadLink { get; internal set; }

        public CumInfoAttribute(Type type, string name, string version, string author, string downloadLink = null)
        {
            SystemType = type;
            Name = name;
            Version = version;
            Author = author;
            DownloadLink = downloadLink;
        }

        [Obsolete()]
        internal CumModInfoAttribute ConvertLegacy_Mod() => new CumModInfoAttribute(SystemType, Name, Version, Author, DownloadLink);
        [Obsolete()]
        internal CumPluginInfoAttribute ConvertLegacy_Plugin() => new CumPluginInfoAttribute(SystemType, Name, Version, Author, DownloadLink);
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class CumGameAttribute : Attribute
    {
        /// <summary>
        /// Gets the target Developer
        /// </summary>
        public string Developer { get; internal set; }

        /// <summary>
        /// Gets target Game Name
        /// </summary>
        public string GameName { get; internal set; }

        /// <summary>
        /// Gets whether this Mod can target any Game.
        /// </summary>
        public bool Universal { get => string.IsNullOrEmpty(Developer) || string.IsNullOrEmpty(GameName); }

        /// <summary>
        /// Mark this Mod as Universal or Compatible with specific Games.
        /// </summary>
        public CumGameAttribute(string developer = null, string gameName = null)
        {
            Developer = developer;
            GameName = gameName;
        }

        [Obsolete()]
        internal CumModGameAttribute ConvertLegacy_Mod() => new CumModGameAttribute(Developer, GameName);
        [Obsolete()]
        internal CumPluginGameAttribute ConvertLegacy_Plugin() => new CumPluginGameAttribute(Developer, GameName);

        public bool IsGame(string developer, string gameName) => (Universal || ((developer != null) && (gameName != null) && Developer.Equals(developer) && GameName.Equals(gameName)));
        public bool IsCompatible(CumGameAttribute att) => ((att == null) || IsCompatibleBecauseUniversal(att) || (att.Developer.Equals(Developer) && att.GameName.Equals(GameName)));
        public bool IsCompatibleBecauseUniversal(CumGameAttribute att) => ((att == null) || Universal || att.Universal);
    }

    [Obsolete()]
    internal class CumLegacyAttributeSupport
    {
        internal class Response_Info
        {
            internal CumInfoAttribute Default;
            [Obsolete()]
            internal CumModInfoAttribute Legacy_Mod;
            [Obsolete()]
            internal CumPluginInfoAttribute Legacy_Plugin;
            internal Response_Info(CumInfoAttribute def, CumModInfoAttribute legacy_mod, CumPluginInfoAttribute legacy_plugin)
            {
                Default = def;
                Legacy_Mod = legacy_mod;
                Legacy_Plugin = legacy_plugin;
            }
            internal void SetupCum(CumBase baseInstance)
            {
                if (Default != null)
                    baseInstance.Info = Default;
                if (Legacy_Mod != null)
                    baseInstance.LegacyModInfo = Legacy_Mod;
                if (Legacy_Plugin != null)
                    baseInstance.LegacyPluginInfo = Legacy_Plugin;
            }
        }

        internal class Response_Game
        {
            internal CumGameAttribute[] Default;
            internal CumModGameAttribute[] Legacy_Mod;
            internal CumPluginGameAttribute[] Legacy_Plugin;
            internal Response_Game(CumGameAttribute[] def, CumModGameAttribute[] legacy_mod, CumPluginGameAttribute[] legacy_plugin)
            {
                Default = def;
                Legacy_Mod = legacy_mod;
                Legacy_Plugin = legacy_plugin;
            }
            internal void SetupCum(CumBase baseInstance)
            {
                if (Default.Length > 0)
                    baseInstance.Games = Default;
                if (Legacy_Mod.Length > 0)
                    baseInstance.LegacyModGames = Legacy_Mod;
                if (Legacy_Plugin.Length > 0)
                    baseInstance.LegacyPluginGames = Legacy_Plugin;
            }
        }

        internal static Response_Info GetCumInfoAttribute(Assembly asm, bool isPlugin = false)
        {
            CumInfoAttribute def = asm.GetCustomAttributes(false).FirstOrDefault(x => (x.GetType() == typeof(CumInfoAttribute))) as CumInfoAttribute;
            CumModInfoAttribute legacy_mod = null;
            CumPluginInfoAttribute legacy_plugin = null;
            if (def == null)
            {
                if (isPlugin)
                {
                    legacy_plugin = asm.GetCustomAttributes(false).FirstOrDefault(x => (x.GetType() == typeof(CumPluginInfoAttribute))) as CumPluginInfoAttribute;
                    if ((legacy_plugin != null) && (def == null))
                        def = legacy_plugin.Convert();
                }
                else
                {
                    legacy_mod = asm.GetCustomAttributes(false).FirstOrDefault(x => (x.GetType() == typeof(CumModInfoAttribute))) as CumModInfoAttribute;
                    if ((legacy_mod != null) && (def == null))
                        def = legacy_mod.Convert();
                }
            }
            else
            {
                if (isPlugin)
                    legacy_plugin = def.ConvertLegacy_Plugin();
                else
                    legacy_mod = def.ConvertLegacy_Mod();
            }
            return new Response_Info(def, legacy_mod, legacy_plugin);
        }

        internal static Response_Game GetCumGameAttributes(Assembly asm, bool isPlugin = false)
        {
            CumGameAttribute[] def = asm.GetCustomAttributes(typeof(CumGameAttribute), true) as CumGameAttribute[];
            CumModGameAttribute[] legacy_mod = new CumModGameAttribute[0];
            CumPluginGameAttribute[] legacy_plugin = new CumPluginGameAttribute[0];
            if (def.Length <= 0)
            {
                if (isPlugin)
                {
                    legacy_plugin = asm.GetCustomAttributes(typeof(CumPluginGameAttribute), true) as CumPluginGameAttribute[];
                    if (legacy_plugin.Length > 0)
                    {
                        List<CumGameAttribute> deflist = new List<CumGameAttribute>();
                        foreach (CumPluginGameAttribute att in legacy_plugin)
                            deflist.Add(att.Convert());
                        def = deflist.ToArray();
                    }
                }
                else
                {
                    legacy_mod = asm.GetCustomAttributes(typeof(CumModGameAttribute), true) as CumModGameAttribute[];
                    if (legacy_mod.Length > 0)
                    {
                        List<CumGameAttribute> deflist = new List<CumGameAttribute>();
                        foreach (CumModGameAttribute att in legacy_mod)
                            deflist.Add(att.Convert());
                        def = deflist.ToArray();
                    }
                }
            }
            else
            {
                if (isPlugin)
                {
                    List<CumPluginGameAttribute> legacy_pluginlist = new List<CumPluginGameAttribute>();
                    foreach (CumGameAttribute att in def)
                        legacy_pluginlist.Add(att.ConvertLegacy_Plugin());
                    legacy_plugin = legacy_pluginlist.ToArray();
                }
                else
                {
                    List<CumModGameAttribute> legacy_modlist = new List<CumModGameAttribute>();
                    foreach (CumGameAttribute att in def)
                        legacy_modlist.Add(att.ConvertLegacy_Mod());
                    legacy_mod = legacy_modlist.ToArray();
                }
            }
            return new Response_Game(def, legacy_mod, legacy_plugin);
        }
    }
}
