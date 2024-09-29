using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class PauseMenu : MonoBehaviour
    {
        public bool Paused { get; private set; }
        
        public void TogglePause()
        {
            Paused = !Paused;
            gameObject.SetActive(Paused);
            Time.timeScale = Paused ? 0 : 1;
        }

        public void ExitGame()
        {
            SceneLoader.LoadMainMenu();
        }
    }
}
