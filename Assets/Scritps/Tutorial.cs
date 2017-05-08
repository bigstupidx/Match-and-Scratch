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


	public void ClearPlayerPrefs() {
		PlayerPrefs.DeleteKey ("tutorialShowed");
		PlayerPrefs.DeleteKey ("undeliveredScores");
	}

	public void AddTestScore() {
		//HighScores.instance.AddNewHighscore ("test_user", -1);
		Leaderboards.instance.AddScore("user_" + Random.Range(100,999), Random.Range(-10, 0));
	}
}
