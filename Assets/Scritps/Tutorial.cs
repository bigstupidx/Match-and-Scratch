using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;

public class Tutorial : MonoBehaviour {

	public GameObject tutorialWrapper;

	UIScreen screen;
	bool tutorialShowed;

	void Awake() {
		screen = GetComponent<UIScreen> ();
		tutorialShowed = PlayerPrefs.GetInt ("tutorialShowed", 0) == 1;
		tutorialWrapper.SetActive(!tutorialShowed);
	}

	// Use this for initialization
	public void OnEnable () {
		StartCoroutine (ShowTutorial());
	}

	IEnumerator ShowTutorial() {
		if (!tutorialShowed) {
			while (!screen.InOpenState) {
				yield return null;
			}
			tutorialWrapper.SetActive(true);
		}
		yield return null;
	}

	public void EndTutorial() {
		PlayerPrefs.SetInt ("tutorialShowed", 1);
		tutorialWrapper.SetActive(false);
	}
}
