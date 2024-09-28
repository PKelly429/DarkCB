using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDCB
{
    public class MenuFunctions : MonoBehaviour
    {
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
