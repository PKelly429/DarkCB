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
        
        public static void LoadMainScene()
        {
            _currentScene = SceneManager.GetActiveScene();
            _nextScene = MainScene;
            
            SceneManager.LoadSceneAsync(LoadingScene, LoadSceneMode.Additive);
        }
        
        public static void LoadMainMenu()
        {
            _currentScene = SceneManager.GetActiveScene();
            _nextScene = MenuScene;
            
            SceneManager.LoadSceneAsync(LoadingScene, LoadSceneMode.Additive);
        }


        public static void LoadingSceneReady()
        {
            SceneManager.UnloadSceneAsync(_currentScene);
            var asyncOperation = SceneManager.LoadSceneAsync(_nextScene, LoadSceneMode.Additive);

            asyncOperation.completed += OnSceneLoaded;
        }

        public static void UnloadLoadingScene()
        {
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(LoadingScene));
        }

    }
}
