using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CumLoader.Support
{
    internal static class Main
    {
        internal static bool IsDestroying = false;
        internal static GameObject obj = null;
        internal static CumLoaderComponent comp = null;
        internal static int CurrentScene = -9;
        private static ISupportModule Initialize()
        {
            CumLoaderComponent.Create();
            return new Module();
        }
    }

    public class CumLoaderComponent : MonoBehaviour
    {
        internal static readonly List<IEnumerator> QueuedCoroutines = new List<IEnumerator>();
        internal static void Create()
        {
            Main.obj = new GameObject("CumLoader");
            DontDestroyOnLoad(Main.obj);
            Main.comp = (CumLoaderComponent)Main.obj.AddComponent(typeof(CumLoaderComponent));
            Main.obj.transform.SetAsLastSibling();
            Main.comp.transform.SetAsLastSibling();
        }
        internal static void Destroy() { Main.IsDestroying = true; if (Main.obj != null) GameObject.Destroy(Main.obj); }
        void Awake()
        {
            foreach (var queuedCoroutine in QueuedCoroutines) StartCoroutine(queuedCoroutine);
            QueuedCoroutines.Clear();
        }
        void Start() => transform.SetAsLastSibling();
        void Update()
        {
            transform.SetAsLastSibling();
            if (Main.CurrentScene != Application.loadedLevel)
            {
                SceneHandler.OnSceneLoad(Application.loadedLevel);
                Main.CurrentScene = Application.loadedLevel;
            }
            CumHandler.OnUpdate();
        }
        void FixedUpdate() => CumHandler.OnFixedUpdate();
        void LateUpdate() => CumHandler.OnLateUpdate();
        void OnGUI() => CumHandler.OnGUI();
        void OnDestroy() { if (!Main.IsDestroying) Create(); }
        void OnApplicationQuit() { Destroy(); CumHandler.OnApplicationQuit(); }
    }
}