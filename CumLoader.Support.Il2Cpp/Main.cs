using System;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CumLoader.Support
{
    internal static class Main
    {
        internal static bool IsDestroying = false;
        internal static GameObject obj = null;
        internal static CumLoaderComponent comp = null;
        private static Camera OnPostRenderCam = null;

        private static ISupportModule Initialize()
        {
            LogSupport.RemoveAllHandlers();
            if (CumConsole.Enabled || Imports.IsDebugMode())
                LogSupport.InfoHandler += CumLogger.Log;
            LogSupport.WarningHandler += CumLogger.LogWarning;
            LogSupport.ErrorHandler += CumLogger.LogError;
            if (Imports.IsDebugMode())
                LogSupport.TraceHandler += CumLogger.Log;

            ClassInjector.DoHook += Imports.Hook;
            GetUnityVersionNumbers(out var major, out var minor, out var patch);
            UnityVersionHandler.Initialize(major, minor, patch);

            // Il2CppSystem.Console.SetOut(new Il2CppSystem.IO.StreamWriter(Il2CppSystem.IO.Stream.Null));
            try
            {
                var il2CppSystemAssembly = Assembly.Load("Il2Cppmscorlib");

                var consoleType = il2CppSystemAssembly.GetType("Il2CppSystem.Console");
                var streamWriterType = il2CppSystemAssembly.GetType("Il2CppSystem.IO.StreamWriter");
                var streamType = il2CppSystemAssembly.GetType("Il2CppSystem.IO.Stream");

                var setOutMethod = consoleType.GetMethod("SetOut", BindingFlags.Static | BindingFlags.Public);
                var nullStreamField = streamType.GetProperty("Null", BindingFlags.Static | BindingFlags.Public).GetGetMethod();
                var streamWriterCtor = streamWriterType.GetConstructor(new[] {streamType});

                var nullStream = nullStreamField.Invoke(null, new object[0]);
                var steamWriter = streamWriterCtor.Invoke(new[] {nullStream});
                setOutMethod.Invoke(null, new[] {steamWriter});
            }
            catch (Exception ex)
            {
                CumLogger.LogError($"Console cleaning failed: {ex}");
            }

            SetAsLastSiblingDelegateField = IL2CPP.ResolveICall<SetAsLastSiblingDelegate>("UnityEngine.Transform::SetAsLastSibling");

            ClassInjector.RegisterTypeInIl2Cpp<CumLoaderComponent>();
            CumLoaderComponent.Create();

            SceneManager.sceneLoaded = (
                (SceneManager.sceneLoaded == null)
                ? new Action<Scene, LoadSceneMode>(OnSceneLoad)
                : Il2CppSystem.Delegate.Combine(SceneManager.sceneLoaded, (UnityAction<Scene, LoadSceneMode>)new Action<Scene, LoadSceneMode>(OnSceneLoad)).Cast<UnityAction<Scene, LoadSceneMode>>()
                );
            Camera.onPostRender = (
                (Camera.onPostRender == null)
                ? new Action<Camera>(OnPostRender)
                : Il2CppSystem.Delegate.Combine(Camera.onPostRender, (Camera.CameraCallback)new Action<Camera>(OnPostRender)).Cast<Camera.CameraCallback>()
                );

            return new Module();
        }

        private static void GetUnityVersionNumbers(out int major, out int minor, out int patch)
        {
            var unityVersionSplit = CumLoaderBase.UnityVersion.Split('.');
            major = int.Parse(unityVersionSplit[0]);
            minor = int.Parse(unityVersionSplit[1]);
            var patchString = unityVersionSplit[2];
            var firstBadChar = patchString.FirstOrDefault(it => it < '0' || it > '9');
            patch = int.Parse(firstBadChar == 0 ? patchString : patchString.Substring(0, patchString.IndexOf(firstBadChar)));
        }

        private static void OnSceneLoad(Scene scene, LoadSceneMode mode) { if (!scene.Equals(null)) SceneHandler.OnSceneLoad(scene.buildIndex); }
        private static void OnPostRender(Camera cam) { if (OnPostRenderCam == null) OnPostRenderCam = cam; if (OnPostRenderCam == cam) CumCoroutines.ProcessWaitForEndOfFrame(); }

        internal delegate bool SetAsLastSiblingDelegate(IntPtr u0040this);
        internal static SetAsLastSiblingDelegate SetAsLastSiblingDelegateField;
    }

    public class CumLoaderComponent : MonoBehaviour
    {
        internal static void Create()
        {
            Main.obj = new GameObject();
            DontDestroyOnLoad(Main.obj);
            Main.comp = new CumLoaderComponent(Main.obj.AddComponent(UnhollowerRuntimeLib.Il2CppType.Of<CumLoaderComponent>()).Pointer);
            Main.SetAsLastSiblingDelegateField(IL2CPP.Il2CppObjectBaseToPtrNotNull(Main.obj.transform));
            Main.SetAsLastSiblingDelegateField(IL2CPP.Il2CppObjectBaseToPtrNotNull(Main.comp.transform));
        }
        internal static void Destroy() { Main.IsDestroying = true; if (Main.obj != null) GameObject.Destroy(Main.obj); }
        public CumLoaderComponent(IntPtr intPtr) : base(intPtr) { }
        void Start() => transform.SetAsLastSibling();
        void Update()
        {
            transform.SetAsLastSibling();
            CumHandler.OnUpdate();
            CumCoroutines.Process();
        }
        void FixedUpdate()
        {
            CumHandler.OnFixedUpdate();
            CumCoroutines.ProcessWaitForFixedUpdate();
        }
        void LateUpdate() => CumHandler.OnLateUpdate();
        void OnGUI() => CumHandler.OnGUI();
        void OnDestroy() { if (!Main.IsDestroying) Create(); }
        void OnApplicationQuit() { Destroy(); CumHandler.OnApplicationQuit(); }
    }
}