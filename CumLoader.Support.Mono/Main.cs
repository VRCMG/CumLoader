using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CumLoader.Support
{
    internal static class Main
    {
        internal static bool IsDestroying = false;
        internal static GameObject obj = null;
        internal static CumLoaderComponent comp = null;
        private static ISupportModule Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
            return new Module();
        }
        private static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (obj == null) CumLoaderComponent.Create();
            if (!scene.Equals(null)) SceneHandler.OnSceneLoad(scene.buildIndex);
        }
    }

    public class CumLoaderComponent : MonoBehaviour
    {
        internal static readonly List<IEnumerator> QueuedCoroutines = new List<IEnumerator>();
        internal static void Create()
        {
            Main.obj = new GameObject();
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
        void Update() { transform.SetAsLastSibling(); CumHandler.OnUpdate(); }
        void FixedUpdate() => CumHandler.OnFixedUpdate();
        void LateUpdate() => CumHandler.OnLateUpdate();
        void OnGUI() => CumHandler.OnGUI();
        void OnDestroy() { if (!Main.IsDestroying) Create(); }
        void OnApplicationQuit() { Destroy(); CumHandler.OnApplicationQuit(); }
    }
}