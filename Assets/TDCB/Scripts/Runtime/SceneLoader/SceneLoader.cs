using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TDCB
{
    public static class SceneLoader
    {
        private static int MenuScene = 0;
        private static int LoadingScene = 1;
        private static int MainScene = 2;

        public static Action<AsyncOperation> OnSceneLoaded;

        private static Scene _currentScene;
        private static int _nextScene;
        
        public static bool LoadInProgress { get; private set; }
        
        public static void LoadMainScene()
        {
            if (LoadInProgress) return;
            
            LoadInProgress = true;
            _currentScene = SceneManager.GetSceneByBuildIndex(MenuScene);
            _nextScene = MainScene;
            
            SceneManager.LoadSceneAsync(LoadingScene, LoadSceneMode.Additive);
        }
        
        public static void LoadMainMenu()
        {
            if (LoadInProgress) return;
            
            LoadInProgress = true;
            _currentScene = SceneManager.GetSceneByBuildIndex(MainScene);
            _nextScene = MenuScene;
            
            SceneManager.LoadSceneAsync(LoadingScene, LoadSceneMode.Additive);
        }


        public static void LoadingSceneReady()
        {
            try
            {
                SceneManager.UnloadSceneAsync(_currentScene);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            var asyncOperation = SceneManager.LoadSceneAsync(_nextScene, LoadSceneMode.Additive);

            asyncOperation.completed += OnSceneLoaded;
        }

        public static void UnloadLoadingScene()
        {
            var asyncOperation = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(LoadingScene));
            asyncOperation.completed += (x) => { LoadInProgress = false; };
        }

    }
}
