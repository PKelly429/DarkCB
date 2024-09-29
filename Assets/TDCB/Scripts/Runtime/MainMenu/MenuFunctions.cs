using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class MenuFunctions : MonoBehaviour
    {
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
