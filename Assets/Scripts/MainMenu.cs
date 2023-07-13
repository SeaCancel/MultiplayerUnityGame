using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Con.IgorGuriev.FPSMultiplayer
{
    public class MainMenu : MonoBehaviour
    {
        public Launcher launcher;

        public void Start()
        {
            Pause.paused = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void JoinMatch()
        {
            launcher.Join();
        }

        public void CreateMatch()
        {
            launcher.Create();
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}