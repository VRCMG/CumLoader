using System;
using System.Collections;
using System.IO;
using System.Reflection;
#pragma warning disable 0168

namespace CumLoader
{
    public interface ISupportModule
    {
        int GetActiveSceneIndex();
        object StartCoroutine(IEnumerator coroutine);
        void StopCoroutine(object coroutineToken);
        void UnityDebugLog(string msg);
        void Destroy();
    }

    internal static class SupportModule
    {
        private static Assembly assembly = null;
        private static Type type = null;
        internal static ISupportModule supportModule = null;

        internal static void Initialize()
        {
            try
            {
                string basedir = Path.Combine(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CumLoader"), "Dependencies"), "SupportModules");
                string filepath = null;
                if (Imports.IsIl2CppGame())
                    filepath = Path.Combine(basedir, "CumLoader.Support.Il2Cpp.dll");
                else
                {
                    if (File.Exists(Path.Combine(Imports.GetAssemblyDirectory(), "UnityEngine.CoreModule.dll")))
                        filepath = Path.Combine(basedir, "CumLoader.Support.Mono.dll");
                    else
                    {
                        if (IsOldUnity())
                            filepath = Path.Combine(basedir, "CumLoader.Support.Mono.Pre2017.2.dll");
                        else
                            filepath = Path.Combine(basedir, "CumLoader.Support.Mono.Pre2017.dll");
                    }
                }
                if (File.Exists(filepath))
                {
                    byte[] data = File.ReadAllBytes(filepath);
                    if (data.Length > 0)
                    {
                        assembly = Assembly.Load(data);
                        if (!assembly.Equals(null))
                        {
                            type = assembly.GetType("CumLoader.Support.Main");
                            if (!type.Equals(null))
                            {
                                MethodInfo method = type.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static);
                                if (!method.Equals(null))
                                    supportModule = (ISupportModule)method.Invoke(null, new object[0]);
                            }
                        }
                    }
                }
                else
                {
                    CumLogger.LogError("Unable to load Support Module! Support Module is Missing!");
                    CumLogger.Log("------------------------------");
                }
            }
            catch (Exception e)
            {
                CumLogger.LogError("Unable to load Support Module!\n" + e.ToString());
                CumLogger.Log("------------------------------");
            }
        }

        internal static bool IsOldUnity()
        {
            try
            {
                Assembly unityengine = Assembly.Load("UnityEngine");
                if (unityengine != null)
                {
                    Type scenemanager = unityengine.GetType("UnityEngine.SceneManagement.SceneManager");
                    if (scenemanager != null)
                    {
                        EventInfo sceneLoaded = scenemanager.GetEvent("sceneLoaded");
                        if (sceneLoaded != null)
                            return false;
                    }
                }
            }
            catch (Exception e) { }
            return true;
        }

        internal static int GetActiveSceneIndex() => supportModule?.GetActiveSceneIndex() ?? -9;
        internal static void UnityDebugLog(string msg) => supportModule?.UnityDebugLog(msg);
        internal static void Destroy() { supportModule?.Destroy(); supportModule = null; }
    }
}
