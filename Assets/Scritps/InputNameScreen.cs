using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReveloLibrary;

public class InputNameScreen : UIScreen {

	public static InputNameScreen Instance { get; private set;}
	public Button sendButton;

	InputField nameField;
	string lastName;

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

	public override void OpenWindow(Callback openCallback = null) {
		if (!string.IsNullOrEmpty (lastName))
			nameField.text = lastName;
		EvaluateNick ();
		base.OpenWindow(openCallback);
	}

	public void CancelSendScore() {
		AnalyticsSender.SendCustomAnalitycs("scoreNotSend", new Dictionary<string, object> {
			{ "score", GameManager.instance.Score },
			{ "nameUsedLastTime", lastName}
		});
		if (UnityEngine.Random.Range (0, 2) == 0) {
			CloseWindow (SpecialThanksScreen.Instance.ShowRememberSendScore);
		} else
			CloseWindow ();
	}

	public override void CloseWindow(Callback closeCallback = null) {
		GameManager.instance.DisableInput ();
		if (closeCallback == null)
			closeCallback = GameManager.instance.EnableInput;
		base.CloseWindow (closeCallback);
	}

	public void EvaluateNick() {
		sendButton.interactable = nameField.text.Length >= 3;
	}

	public void SendScore() {
		UnityAds.Instance.ShowAds (ServicesConfiguration.sendscore_video_is_rewarded, SendScoreToBBDD);
	}
	
	void SendScoreToBBDD(int result) {
		lastName = nameField.text;
		PlayerPrefs.SetString ("name", lastName);

		FirebaseDBManager.instance.AddNewHighscore( new ScoreEntry ( lastName, GameManager.instance.Score ) );

		AnalyticsSender.SendCustomAnalitycs("scoreSended", new Dictionary<string, object> {
			{ "score", GameManager.instance.Score },
			{ "name", lastName}
		});
		//TODO 
		/*
		else {
			Ventana modal avisando de que viendo videos colaboras a que mas juegos gratuitos como este sean creados.
		} 
		*/

		// Si no hemos mostrado la mención especial
		bool specialMentionShowed = PlayerPrefs.GetInt ("specialMentionShowed", 0) == 1;
		bool showMention;

		if (!specialMentionShowed) {
			//if (lastName == "Pako")
			//	showMention = true;
			//else
			showMention = UnityEngine.Random.Range (0, 10) == 0;
		} 
		else {
			showMention = UnityEngine.Random.Range (0, 10) == 0;
		}

		if (showMention)
			CloseWindow (SpecialThanksScreen.Instance.ShowSpecialMention);
		else
			CloseWindow ();
	}
	

}
