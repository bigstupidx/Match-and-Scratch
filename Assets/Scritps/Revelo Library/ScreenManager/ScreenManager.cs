using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ReveloLibrary
{
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance { get; private set; }

        public List<UIScreen> screens;
        public UIScreen currentGUIScreen;

        public Action<ScreenDefinitions> OnScreenChange;

        private UIScreen newScreen;
        private GameObject ProfilePlayerInstance;

        public UIScreen lastGUIScreen
        { 
            get; 
            set; 
        }

        void Awake()
        {
			
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            screens = new List<UIScreen>();
            FindScreens();
        }

        void FindScreens()
        {
            GameObject[] screensFound = GameObject.FindGameObjectsWithTag("GameScreen");
            foreach (GameObject s in screensFound)
            {
                screens.Add(s.GetComponent<UIScreen>());
            }
        }

        public void ShowLastScreen()
        {
            ShowScreen(lastGUIScreen.screenDefinition);
        }

        /// <summary>
        /// Shows the screen.
        /// </summary>
        /// <param name="guiScreen">GUI screen.</param>
        public void ShowScreen(ScreenDefinitions definition, UIScreen.Callback theCallback = null)
        {
            UIScreen uiScreen = screens.Find(s => s.screenDefinition == definition);

            if (currentGUIScreen != null && uiScreen != currentGUIScreen)
            {
                currentGUIScreen.CloseWindow();
            }

            lastGUIScreen = currentGUIScreen;
            currentGUIScreen = uiScreen;
			
            if (currentGUIScreen != null)
            {
                currentGUIScreen.OpenWindow(theCallback);
            }
			#if UNITY_EDITOR
			else
            {
                Debug.LogError("[CanvasManager in " + name + "]: La guiScreen es null. Quizás no has establecido la primera desde el inspector.");
            }
            #endif
        }

        public void HideScreen()
        {
            if (currentGUIScreen != null)
            {
                currentGUIScreen.CloseWindow();
            }
        }
    }
}
