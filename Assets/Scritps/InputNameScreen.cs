using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReveloLibrary;

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

	public override void CloseWindow() {
		base.CloseWindow ();
	}

	public void EvaluateNick() {
		sendButton.interactable = nameField.text.Length >= 3;
	}

	public void SendScore() {
		lastName = nameField.text;
		HighScores.instance.AddNewHighscore (lastName, GameManager.instance.Score);
		PlayerPrefs.SetString ("name", lastName);
	}

	// Use this for initialization
	public override void Start () {
		
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update ();
	}
}
