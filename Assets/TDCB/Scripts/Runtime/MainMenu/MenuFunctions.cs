using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TDCB
{
    public class MenuFunctions : MonoBehaviour
    {
        [SerializeField] private Button loadGameButton;

        private void Update()
        {
            loadGameButton.interactable = !SceneLoader.LoadInProgress;
        }

        private void OnEnable()
        {
            Time.timeScale = 1;
        }

        public void Play()
        {
            SceneLoader.LoadMainScene();
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
