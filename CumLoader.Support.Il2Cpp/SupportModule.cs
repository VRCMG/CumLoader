using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CumLoader.Support
{
    public class Module : ISupportModule
    {
        public int GetActiveSceneIndex() => SceneManager.GetActiveScene().buildIndex;
        public object StartCoroutine(IEnumerator coroutine) => CumCoroutines.Start(coroutine);
        public void StopCoroutine(object coroutineToken) => CumCoroutines.Stop((IEnumerator)coroutineToken);
        public void UnityDebugLog(string msg) => Debug.Log(msg);
        public void Destroy() => CumLoaderComponent.Destroy();
    }
}