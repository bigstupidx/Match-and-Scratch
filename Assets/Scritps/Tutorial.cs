using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;

public class Tutorial : MonoBehaviour {

	public static Tutorial instance;
	public GameObject tutorialScreen;

	UIScreen screen;
	bool tutorialShowed;

	void Awake() {
		if (instance == null) {
			instance = this;
		}
		else if (instance != this) {
			Destroy(gameObject);
		}

		tutorialScreen.SetActive(false);
		screen = GetComponent<UIScreen> ();
		tutorialShowed = PlayerPrefs.GetInt ("tutorialShowed", 0) == 1;
	}

	// Use this for initialization
	public void StartTutorial () {
		StartCoroutine (ShowTutorial());
	}

	IEnumerator ShowTutorial() {
		if (!tutorialShowed) {
			while (!screen.IsOpen) {
				yield return null;
			}
			yield return new WaitForSeconds (2f);
			tutorialScreen.SetActive(true);
		}
		yield return null;
	}

	public void EndTutorial() {
		PlayerPrefs.SetInt ("tutorialShowed", 1);
		tutorialScreen.SetActive(false);
	}
}
