using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Harmony;
using CumLoader.ICSharpCode.SharpZipLib.Zip;
#pragma warning disable 0612
#pragma warning disable 0618

namespace CumLoader
{
    public static class CumHandler
    {
        internal static bool HasCums = false;
        internal static Assembly Assembly_CSharp = null;
        private static List<CumPlugin> _TempCumPlugins = null;
        internal static List<CumPlugin> _CumPlugins = new List<CumPlugin>();
        public static List<CumPlugin> CumPlugins { get => _CumPlugins; }
        internal static List<CumMod> _CumMods = new List<CumMod>();
        public static List<CumMod> CumMods { get => _CumMods; }

        internal static void LoadAll(bool plugins = false)
        {
            string searchdir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, (plugins ? "CumPlugins" : "CumMods"));
            if (!Directory.Exists(searchdir))
            {
                Directory.CreateDirectory(searchdir);
                return;
            }
            LoadMode loadmode = (plugins ? GetLoadMode_CumPlugins() : GetLoadMode_CumMods());

            // DLL
            string[] files = Directory.GetFiles(searchdir, "*.dll");
            if (files.Length > 0)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];
                    if (string.IsNullOrEmpty(file))
                        continue;

                    bool file_extension_check = Path.GetFileNameWithoutExtension(file).EndsWith("-dev");
                    if ((loadmode != LoadMode.BOTH) && ((loadmode == LoadMode.DEV) ? !file_extension_check : file_extension_check))
                        continue;

                    try
                    {
                        LoadFromFile(file, plugins);
                    }
                    catch (Exception e)
                    {
                        CumLogger.LogError("Unable to load " + file + ":\n" + e.ToString());
                        CumLogger.Log("------------------------------");
                    }
                }
            }

            // ZIP
            string[] zippedFiles = Directory.GetFiles(searchdir, "*.zip");
            if (zippedFiles.Length > 0)
            {
                for (int i = 0; i < zippedFiles.Length; i++)
                {
                    string file = zippedFiles[i];
                    if (string.IsNullOrEmpty(file))
                        continue;
                    try
                    {
                        using (var fileStream = File.OpenRead(file))
                        {
                            using (var zipInputStream = new ZipInputStream(fileStream))
                            {
                                ZipEntry entry;
                                while ((entry = zipInputStream.GetNextEntry()) != null)
                                {
                                    string filename = Path.GetFileName(entry.Name);
                                    if (string.IsNullOrEmpty(filename) || !filename.EndsWith(".dll"))
                                        continue;

                                    bool file_extension_check = Path.GetFileNameWithoutExtension(file).EndsWith("-dev");
                                    if ((loadmode != LoadMode.BOTH) && ((loadmode == LoadMode.DEV) ? !file_extension_check : file_extension_check))
                                        continue;

                                    using (var unzippedFileStream = new MemoryStream())
                                    {
                                        int size = 0;
                                        byte[] buffer = new byte[4096];
                                        while (true)
                                        {
                                            size = zipInputStream.Read(buffer, 0, buffer.Length);
                                            if (size > 0)
                                                unzippedFileStream.Write(buffer, 0, size);
                                            else
                                                break;
                                        }
                                        LoadFromAssembly(Assembly.Load(unzippedFileStream.ToArray()), plugins, (file + "/" + filename));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        CumLogger.LogError("Unable to load " + file + ":\n" + e.ToString());
                        CumLogger.Log("------------------------------");
                    }
                }
            }
            Main.LegacySupport(_CumMods, _CumPlugins, CumLoaderBase._IsVRChat, CumLoaderBase._IsBoneworks);
        }

        public static void LoadFromFile(string filelocation, bool isPlugin = false) => LoadFromAssembly((Imports.IsDebugMode() ? Assembly.LoadFrom(filelocation) : Assembly.Load(File.ReadAllBytes(filelocation))), isPlugin, filelocation);
        public static void LoadFromAssembly(Assembly asm, bool isPlugin = false, string filelocation = null)
        {
            if (!asm.Equals(null))
            {
                CumLegacyAttributeSupport.Response_Info response_Info = CumLegacyAttributeSupport.GetCumInfoAttribute(asm, isPlugin);
                CumInfoAttribute InfoAttribute = response_Info.Default;
                if ((InfoAttribute != null) && (InfoAttribute.SystemType != null) && InfoAttribute.SystemType.IsSubclassOf((isPlugin ? typeof(CumPlugin) : typeof(CumMod))))
                {
                    bool isCompatible = false;
                    bool isUniversal = false;
                    bool hasAttribute = true;
                    CumLegacyAttributeSupport.Response_Game response_Game = CumLegacyAttributeSupport.GetCumGameAttributes(asm, isPlugin);
                    CumGameAttribute[] GameAttributes = response_Game.Default;
                    int GameAttributes_Count = GameAttributes.Length;
                    if (GameAttributes_Count > 0)
                    {
                        for (int i = 0; i < GameAttributes_Count; i++)
                        {
                            CumGameAttribute GameAttribute = GameAttributes[i];
                            if (CumLoaderBase.CurrentGameAttribute.IsCompatible(GameAttribute))
                            {
                                isCompatible = true;
                                isUniversal = CumLoaderBase.CurrentGameAttribute.IsCompatibleBecauseUniversal(GameAttribute);
                                break;
                            }
                        }
                    }
                    else
                        hasAttribute = false;
                    CumBase baseInstance = Activator.CreateInstance(InfoAttribute.SystemType) as CumBase;
                    if (baseInstance != null)
                    {
                        response_Info.SetupCum(baseInstance);
                        response_Game.SetupCum(baseInstance);
                        baseInstance.OptionalDependenciesAttribute = asm.GetCustomAttributes(false).FirstOrDefault(x => (x.GetType() == typeof(CumOptionalDependenciesAttribute))) as CumOptionalDependenciesAttribute;
                        baseInstance.Location = filelocation;
                        baseInstance.Compatibility = (isUniversal ? CumBase.CumCompatibility.UNIVERSAL :
                            (isCompatible ? CumBase.CumCompatibility.COMPATIBLE :
                                (!hasAttribute ? CumBase.CumCompatibility.NOATTRIBUTE : CumBase.CumCompatibility.INCOMPATIBLE)
                            )
                        );
                        if (baseInstance.Compatibility < CumBase.CumCompatibility.INCOMPATIBLE)
                        {
                            baseInstance.Assembly = asm;
                            baseInstance.harmonyInstance = HarmonyInstance.Create(asm.FullName);
                        }
                        if (isPlugin)
                            _CumPlugins.Add((CumPlugin)baseInstance);
                        else
                            _CumMods.Add((CumMod)baseInstance);
                    }
                    else
                        CumLogger.LogError("Unable to load " + asm.GetName() + "! Failed to Create Instance!");
                }
            }
        }

        internal static void LogAndPrune()
        {
            if (_CumPlugins.Count > 0)
            {
                for (int i = 0; i < _CumPlugins.Count; i++)
                    if (_CumPlugins[i] != null)
                        LogCumInfo(_CumPlugins[i]);
                _CumPlugins = _TempCumPlugins;
            }
            if (_CumPlugins.Count <= 0)
            {
                CumLogger.Log("No CumPlugins Loaded!");
                CumLogger.Log("------------------------------");
            }
            else
                HasCums = true;

            if (_CumMods.Count > 0)
            {
                for (int i = 0; i < _CumMods.Count; i++)
                    if (_CumMods[i] != null)
                        LogCumInfo(_CumMods[i]);
                _CumMods.RemoveAll((CumMod mod) => ((mod == null) || (mod.Compatibility >= CumBase.CumCompatibility.INCOMPATIBLE)));
                DependencyGraph<CumMod>.TopologicalSort(_CumMods, mod => mod.Info.Name);
            }
            if (_CumMods.Count <= 0)
            {
                CumLogger.Log("No CumMods Loaded!");
                CumLogger.Log("------------------------------");
            }
            else
                HasCums = true;
        }

        private static void LogCumInfo(CumBase cum)
        {
            CumLogger.Log(cum.Info.Name
                            + (!string.IsNullOrEmpty(cum.Info.Version)
                            ? (" v" + cum.Info.Version) : "")
                            + (!string.IsNullOrEmpty(cum.Info.Author)
                            ? (" by " + cum.Info.Author) : "")
                            + (!string.IsNullOrEmpty(cum.Info.DownloadLink)
                            ? (" (" + cum.Info.DownloadLink + ")")
                            : "")
                            );
            CumLogger.LogCumCompatibility(cum.Compatibility);
            CumLogger.Log("------------------------------");
        }

        internal static void OnPreInitialization()
        {
            if (_CumPlugins.Count > 0)
            {
                HashSet<CumPlugin> failedCumPlugins = new HashSet<CumPlugin>();
                _TempCumPlugins = _CumPlugins.Where(plugin => (plugin.Compatibility < CumBase.CumCompatibility.INCOMPATIBLE)).ToList();
                DependencyGraph<CumPlugin>.TopologicalSort(_TempCumPlugins, plugin => plugin.Info.Name);
                for (int i = 0; i < _TempCumPlugins.Count; i++)
                    if (_TempCumPlugins[i] != null)
                        try { _TempCumPlugins[i].OnPreInitialization(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _TempCumPlugins[i].Info.Name); failedCumPlugins.Add(_TempCumPlugins[i]); }
                _TempCumPlugins.RemoveAll(plugin => ((plugin == null) || failedCumPlugins.Contains(plugin)));
                Main.LegacySupport(_CumMods, _TempCumPlugins, CumLoaderBase._IsVRChat, CumLoaderBase._IsBoneworks);
            }
        }

        internal static void OnApplicationStart()
        {
            if (_CumPlugins.Count > 0)
            {
                HashSet<CumPlugin> failedCumPlugins = new HashSet<CumPlugin>();
                for (int i = 0; i < _CumPlugins.Count; i++)
                    if (_CumPlugins[i] != null)
                        try { _CumPlugins[i].harmonyInstance.PatchAll(_CumPlugins[i].Assembly); _CumPlugins[i].OnApplicationStart(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumPlugins[i].Info.Name); HarmonyInstance.UnpatchAllCumInstances(_CumPlugins[i]); failedCumPlugins.Add(_CumPlugins[i]); }
                _CumPlugins.RemoveAll(plugin => ((plugin == null) || failedCumPlugins.Contains(plugin)));
                Main.LegacySupport(_CumMods, _CumPlugins, CumLoaderBase._IsVRChat, CumLoaderBase._IsBoneworks);
            }
            if (_CumMods.Count > 0)
            {
                HashSet<CumMod> failedCumMods = new HashSet<CumMod>();
                for (int i = 0; i < _CumMods.Count; i++)
                    if (_CumMods[i] != null)
                        try { _CumMods[i].harmonyInstance.PatchAll(_CumMods[i].Assembly); _CumMods[i].OnApplicationStart(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumMods[i].Info.Name); HarmonyInstance.UnpatchAllCumInstances(_CumMods[i]); failedCumMods.Add(_CumMods[i]); }
                _CumMods.RemoveAll(mod => ((mod == null) || failedCumMods.Contains(mod)));
                Main.LegacySupport(_CumMods, _CumPlugins, CumLoaderBase._IsVRChat, CumLoaderBase._IsBoneworks);
            }
        }

        public static void OnApplicationQuit()
        {
            if (_CumPlugins.Count > 0)
                for (int i = 0; i < _CumPlugins.Count; i++)
                    if (_CumPlugins[i] != null)
                        try { _CumPlugins[i].OnApplicationQuit(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumPlugins[i].Info.Name); }
            if (_CumMods.Count > 0)
                for (int i = 0; i < _CumMods.Count; i++)
                    if (_CumMods[i] != null)
                        try { _CumMods[i].OnApplicationQuit(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumMods[i].Info.Name); }
            CumLoaderBase.Quit();
        }

        public static void OnModSettingsApplied()
        {
            if (_CumPlugins.Count > 0)
                for (int i = 0; i < _CumPlugins.Count; i++)
                    try { _CumPlugins[i].OnModSettingsApplied(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumPlugins[i].Info.Name); }
            if (_CumMods.Count > 0)
                for (int i = 0; i < _CumMods.Count; i++)
                    if (_CumMods[i] != null)
                        try { _CumMods[i].OnModSettingsApplied(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumMods[i].Info.Name); }
        }

        public static void OnUpdate()
        {
            SceneHandler.CheckForSceneChange();
            if (Imports.IsIl2CppGame() && CumLoaderBase._IsVRChat)
                VRChat_CheckUiManager();
            if (_CumPlugins.Count > 0)
                for (int i = 0; i < _CumPlugins.Count; i++)
                    if (_CumPlugins[i] != null)
                        try { _CumPlugins[i].OnUpdate(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumPlugins[i].Info.Name); }
            if (_CumMods.Count > 0)
                for (int i = 0; i < _CumMods.Count; i++)
                    if (_CumMods[i] != null)
                        try { _CumMods[i].OnUpdate(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumMods[i].Info.Name); }
        }

        public static void OnFixedUpdate()
        {
            if (_CumMods.Count > 0)
                for (int i = 0; i < _CumMods.Count; i++)
                    if (_CumMods[i] != null)
                        try { _CumMods[i].OnFixedUpdate(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumMods[i].Info.Name); }
        }

        public static void OnLateUpdate()
        {
            if (_CumPlugins.Count > 0)
                for (int i = 0; i < _CumPlugins.Count; i++)
                    if (_CumPlugins[i] != null)
                        try { _CumPlugins[i].OnLateUpdate(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumPlugins[i].Info.Name); }
            if (_CumMods.Count > 0)
                for (int i = 0; i < _CumMods.Count; i++)
                    if (_CumMods[i] != null)
                        try { _CumMods[i].OnLateUpdate(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumMods[i].Info.Name); }
        }

        public static void OnGUI()
        {
            if (_CumPlugins.Count > 0)
                for (int i = 0; i < _CumPlugins.Count; i++)
                    if (_CumPlugins[i] != null)
                        try { _CumPlugins[i].OnGUI(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumPlugins[i].Info.Name); }
            if (_CumMods.Count > 0)
                for (int i = 0; i < _CumMods.Count; i++)
                    if (_CumMods[i] != null)
                        try { _CumMods[i].OnGUI(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumMods[i].Info.Name); }
        }

        internal static void OnLevelIsLoading()
        {
            if (_CumMods.Count > 0)
                for (int i = 0; i < _CumMods.Count; i++)
                    if (_CumMods[i] != null)
                        try { _CumMods[i].OnLevelIsLoading(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumMods[i].Info.Name); }
        }

        internal static void OnLevelWasLoaded(int level)
        {
            if (_CumMods.Count > 0)
                for (int i = 0; i < _CumMods.Count; i++)
                    if (_CumMods[i] != null)
                        try { _CumMods[i].OnLevelWasLoaded(level); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumMods[i].Info.Name); }
        }

        internal static void OnLevelWasInitialized(int level)
        {
            if (_CumMods.Count > 0)
                for (int i = 0; i < _CumMods.Count; i++)
                    if (_CumMods[i] != null)
                        try { _CumMods[i].OnLevelWasInitialized(level); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumMods[i].Info.Name); }
        }

        private static bool ShouldCheckForUiManager = true;
        private static Type VRCUiManager = null;
        private static MethodInfo VRCUiManager_Instance = null;
        private static void VRChat_CheckUiManager()
        {
            if (!ShouldCheckForUiManager)
                return;
            if (VRCUiManager == null)
                VRCUiManager = Assembly_CSharp.GetType("VRCUiManager");
            if (VRCUiManager == null)
            {
                ShouldCheckForUiManager = false;
                return;
            }
            if (VRCUiManager_Instance == null)
                VRCUiManager_Instance = VRCUiManager.GetMethods().First(x => (x.ReturnType == VRCUiManager));
            if (VRCUiManager_Instance == null)
            {
                ShouldCheckForUiManager = false;
                return;
            }
            object returnval = VRCUiManager_Instance.Invoke(null, new object[0]);
            if (returnval == null)
                return;
            ShouldCheckForUiManager = false;
            if (_CumMods.Count > 0)
                for (int i = 0; i < _CumMods.Count; i++)
                    if (_CumMods[i] != null)
                        try { _CumMods[i].VRChat_OnUiManagerInit(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumMods[i].Info.Name); }
            if (_CumPlugins.Count > 0)
                for (int i = 0; i < _CumPlugins.Count; i++)
                    if (_CumPlugins[i] != null)
                        try { _CumPlugins[i].VRChat_OnUiManagerInit(); } catch (Exception ex) { CumLogger.LogCumError(ex.ToString(), _CumPlugins[i].Info.Name); }
        }

        private enum LoadMode
        {
            NORMAL,
            DEV,
            BOTH
        }
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static LoadMode GetLoadMode_CumPlugins();
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static LoadMode GetLoadMode_CumMods();
    }
}
