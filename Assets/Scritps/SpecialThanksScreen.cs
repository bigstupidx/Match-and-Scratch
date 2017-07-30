using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;
using UnityEngine.UI;
using SmartLocalization;
using UnityEngine.Analytics;

public class SpecialThanksScreen : UIScreen {

	public static SpecialThanksScreen Instance { get; private set;}
	public Text titleText;
	public Text specialThanksContentText;
	string lastName;

	public override void Awake ()
	{
		if(Instance != null && Instance != this) {
			Destroy(gameObject);
		}
		Instance = this;


		IsOpen = false;
		base.Awake ();
	}

	public void ShowSpecialMention() {
		titleText.text = LanguageManager.Instance.GetTextValue ("ui.label.specialthaks");
		lastName = PlayerPrefs.GetString ("name", "");
		specialThanksContentText.text = LanguageManager.Instance.GetTextValue ("ui.label.specialthankscontent").Replace ("#username", lastName);

		Analytics.CustomEvent("mentionShowed", new Dictionary<string, object> {
			{ "name", lastName}
		});
		OpenWindow ();
	}

	public void ShowRememberSendScore() {
		titleText.text = LanguageManager.Instance.GetTextValue ("ui.label.remember");
		lastName = PlayerPrefs.GetString ("name", "");
		specialThanksContentText.text = LanguageManager.Instance.GetTextValue ("ui.label.remembersendscore");

		Analytics.CustomEvent("rememberSendScore", new Dictionary<string, object> {
			{ "name", lastName}
		});
		OpenWindow ();
	}

	public override void OpenWindow() {
		
		base.OpenWindow();
	}

	public override void CloseWindow() {
		PlayerPrefs.SetInt ("specialMentionShowed", 1);
		base.CloseWindow ();
	}
}
