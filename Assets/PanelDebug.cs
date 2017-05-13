using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;
using System;

public class PanelDebug : MonoBehaviour {

	public GameObject highScoresButtonsWrapper;

	// Use this for initialization
	void Start () {
		//ScreenManager.Instance.OnScreenChange += screenChange;
	}

	/*void screenChange(ScreenDefinitions def) {
		highScoresButtonsWrapper.SetActive (def == ScreenDefinitions.HIGHSCORES);
	}*/

	public void ClearPlayerPrefs() {
		PlayerPrefs.DeleteKey ("tutorialShowed");
		PlayerPrefs.DeleteKey ("undeliveredScores");
		PlayerPrefs.DeleteKey ("name");
	}

	public void AddTestScore() {
		Dreamlo_HighScores.instance.AddNewHighscore ( new ScoreEntry ( "user_" + UnityEngine.Random.Range(100,999), UnityEngine.Random.Range(-10, 0), DateTime.UtcNow.AddDays(UnityEngine.Random.Range(-10,10)).ToOADate() ) );
		Firebase_HighScores.instance.AddNewHighscore( new ScoreEntry ( "user_" + UnityEngine.Random.Range(100,999), UnityEngine.Random.Range(-10, 0), DateTime.UtcNow.AddDays(UnityEngine.Random.Range(-10,10)).ToOADate() ) );
	}

	public void SetHighscoresSource (int source) {
		GameManager.instance.SetNewHighScoresSource((HighScoresSource)source);
	}

}
