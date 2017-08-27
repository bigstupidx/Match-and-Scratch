using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReveloLibrary;
using System;
using SmartLocalization;

public class PanelDebug : MonoBehaviour {

	public GameObject highScoresButtonsWrapper;
	public Text levelLabel;
	public Text rotatorSpeedLabel;
	public Text currentRotatorSpeedLabel;
	public Text colorsLabel;
	// Use this for initialization
	void Start () {
	}

	public void ClearPlayerPrefs() {
		/*
		PlayerPrefs.DeleteKey ("tutorialShowed");
		PlayerPrefs.DeleteKey ("tutorialVisto");
		PlayerPrefs.DeleteKey ("undeliveredScores");
		PlayerPrefs.DeleteKey ("name");
		PlayerPrefs.DeleteKey ("soundState");
		PlayerPrefs.DeleteKey ("specialMentionShowed");
		PlayerPrefs.DeleteKey ("firstRun");
		*/
		PlayerPrefs.DeleteAll ();
	}

	public void changeLanguage(string lang) {
		LanguageManager.Instance.ChangeLanguage (lang);
	}

	public void AddOnePoint() {
		GameManager.instance.AddScore (1);
	}

	public void AddTestScore() {
		FirebaseDBManager.instance.AddNewHighscore( new ScoreEntry ( "user_" + UnityEngine.Random.Range(100,999), UnityEngine.Random.Range(-10, 0), DateTime.UtcNow.AddDays(UnityEngine.Random.Range(-10,10)).ToOADate() ) );
	}

	void Update() {
		levelLabel.text = "Level: " + GameManager.instance.CurrentLevel.ToString();
		rotatorSpeedLabel.text = "Rotator Speed: " + GameManager.instance.rotator.RotationSpeed.ToString();
		currentRotatorSpeedLabel.text = "Current Rotator Speed: " + GameManager.instance.rotator.currentSpeed.ToString();
		colorsLabel.text = "Colors in use: " + GameManager.instance.spawner.colorsInGame.ToString() + " / " + Spawner.MAX_COLORS_IN_GAME.ToString ();
	}

	public void ShowSpecialMention() {
		// Si no hemos mostrado la mención especial
		bool specialMentionShowed = PlayerPrefs.GetInt ("specialMentionShowed", 0) == 1;
		bool showMention;
		string lastName = PlayerPrefs.GetString ("name", "");

		if (!specialMentionShowed) {
			if (lastName == "Pako")
				showMention = true;
			else
				showMention = UnityEngine.Random.Range (0, 10) == 0;
		} 
		else {
			showMention = UnityEngine.Random.Range (0, 10) == 0;
		}

		Debug.Log ("showinMention : " + showMention.ToString ());

		if (showMention)
			SpecialThanksScreen.Instance.ShowSpecialMention ();
	}

	public void ShowRememberSendScore() {
		if (UnityEngine.Random.Range (0, 2) == 0) {
			SpecialThanksScreen.Instance.ShowRememberSendScore ();
		}
	}

	public void ImPako() {
		PlayerPrefs.SetString ("name", "Pako");
		Debug.Log ("Ahora soy Pako");
	}
}
