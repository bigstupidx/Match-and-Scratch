using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReveloLibrary;
using UnityEngine.Analytics;

public class InputNameScreen : UIScreen {

	public static InputNameScreen Instance { get; private set;}
	InputField nameField;
	string lastName;
	public Button sendButton;

	public override void Awake() {
		if(Instance != null && Instance != this) {
			Destroy(gameObject);
		}
		Instance = this;
		lastName = PlayerPrefs.GetString ("name", "");
		nameField = GetComponentInChildren<InputField> ();
		IsOpen = false;

		base.Awake ();
	}

	public override void OpenWindow() {
		if (!string.IsNullOrEmpty (lastName))
			nameField.text = lastName;
		EvaluateNick ();
		base.OpenWindow();
	}

	public void CancelSendScore() {
		Analytics.CustomEvent("scoreNotSend", new Dictionary<string, object> {
			{ "score", GameManager.instance.Score },
			{ "nameUSedLastTime", lastName}
		});
		CloseWindow();
	}

	public override void CloseWindow() {
		base.CloseWindow ();
	}

	public void EvaluateNick() {
		sendButton.interactable = nameField.text.Length >= 3;
	}

	public void SendScore() {
		UnityAds.Instance.ShowAds (true, SendScoreToBBDD);
	}
	
	void SendScoreToBBDD(int result) {
		lastName = nameField.text;
		PlayerPrefs.SetString ("name", lastName);

		if (GameManager.instance.currentSource == HighScoresSource.DREAMLO) {			
			Dreamlo_HighScores.instance.AddNewHighscore ( new ScoreEntry ( lastName, GameManager.instance.Score ) );
		} 
		else if (GameManager.instance.currentSource == HighScoresSource.FIREBASE) {
			Firebase_HighScores.instance.AddNewHighscore( new ScoreEntry ( lastName, GameManager.instance.Score ) );
		}

		Analytics.CustomEvent("scoreSended", new Dictionary<string, object> {
			{ "score", GameManager.instance.Score },
			{ "name", lastName}
		});
		//TODO 
		/*
		else {
			Ventana modal avisando de que viendo videos colaboras a que mas juegos gratuitos como este sean creados.
		} 
		*/
		CloseWindow ();
	}
	

	// Update is called once per frame
	public override void Update () {
		base.Update ();
	}
}
