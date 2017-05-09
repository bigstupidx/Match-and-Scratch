using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ReveloLibrary {
	public class ScreenManager : MonoBehaviour {

		public static ScreenManager Instance { get; private set;}

		public List<UIScreen> screens;
		public UIScreen currentGUIScreen;

		public delegate void Callback();
		Callback resultCallback;

		private UIScreen _newScreen;

		private GameObject ProfilePlayerInstance;

		public UIScreen lastGUIScreen { get; set; }

		public Action<ScreenDefinitions> OnScreenChange;

		void Awake() {
			
			if(Instance != null && Instance != this) {
				Destroy(gameObject);
			}

			Instance = this;

			DontDestroyOnLoad(gameObject);

			screens = new List<UIScreen> ();
			FindScreens ();
		}

		void Start() {			
		}

		void FindScreens() {
			GameObject[] screensFound = GameObject.FindGameObjectsWithTag ("GameScreen");
			foreach (GameObject s in screensFound) {
				screens.Add (s.GetComponent<UIScreen> ());
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
		public void ShowScreen(ScreenDefinitions definition, Callback theCallback= null) {

			resultCallback = theCallback;
			UIScreen uiScreen = screens.Find (s => s.screenDefinition == definition);

			if (currentGUIScreen != null && uiScreen != currentGUIScreen) {
	            currentGUIScreen.CloseWindow();
			}

	        lastGUIScreen = currentGUIScreen;
			currentGUIScreen = uiScreen;
			
			if (currentGUIScreen != null) {
				currentGUIScreen.OpenWindow();
			}
			else {
				Debug.LogError("[CanvasManager in " + name +"]: La guiScreen es null. Quizás no has establecido la primera desde el inspector.");
			}

			StartCoroutine (AnimationEndProcess ());
		}
	
		public void HideScreen() {
			if (currentGUIScreen != null) {
				currentGUIScreen.CloseWindow();
			}
		}

		IEnumerator AnimationEndProcess() {
			while (currentGUIScreen.Animator.IsInTransition(0))
				yield return null;

			if (OnScreenChange != null)
				OnScreenChange (currentGUIScreen.screenDefinition);
			
			if (resultCallback != null) 
				resultCallback();
		}
	}
}
