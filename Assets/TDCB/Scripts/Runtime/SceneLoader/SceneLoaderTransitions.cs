using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace TDCB
{
    public class SceneLoaderTransitions : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvas;

        public void Start()
        {
            SceneLoader.OnSceneLoaded += SceneLoaded;
            Tween.Alpha(canvas, 0 ,1, 1, Ease.Default, 1, CycleMode.Restart, 0, 0, true).OnComplete(SceneLoader.LoadingSceneReady);
        }

        private void SceneLoaded(AsyncOperation obj)
        {
            SceneLoader.OnSceneLoaded -= SceneLoaded;
            Tween.Alpha(canvas, 0, 1, Ease.Default, 1, CycleMode.Restart, 0, 0, true).OnComplete(SceneLoader.UnloadLoadingScene);
        }
    }
}
