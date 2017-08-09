using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;
using UnityEngine.UI;
using SmartLocalization;

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

		AnalyticsSender.SendCustomAnalitycs ("mentionShowed", new Dictionary<string, object> {
			{ "name", lastName}
		});
		OpenWindow (GameManager.instance.EnableInput);
	}

	public void ShowRememberSendScore() {
		titleText.text = LanguageManager.Instance.GetTextValue ("ui.label.remember");
		lastName = PlayerPrefs.GetString ("name", "");
		specialThanksContentText.text = LanguageManager.Instance.GetTextValue ("ui.label.remembersendscore");

		AnalyticsSender.SendCustomAnalitycs ("rememberSendScore", new Dictionary<string, object> {
			{ "name", lastName}
		});
		OpenWindow (GameManager.instance.EnableInput);
	}

	public override void OpenWindow(Callback openCallback = null) {
		
		base.OpenWindow(openCallback);
	}

	public override void CloseWindow (Callback closeCallback = null) {
		PlayerPrefs.SetInt ("specialMentionShowed", 1);
		GameManager.instance.DisableInput ();
		if (closeCallback == null)
			closeCallback = GameManager.instance.EnableInput;
		base.CloseWindow (closeCallback);
	}
}
