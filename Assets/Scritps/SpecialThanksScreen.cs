using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;
using UnityEngine.UI;
using SmartLocalization;
using UnityEngine.Analytics;

public class SpecialThanksScreen : UIScreen {

	public static SpecialThanksScreen Instance { get; private set;}
	public Text specialThanksContentText;
	string lastName;

	public override void Awake ()
	{
		if(Instance != null && Instance != this) {
			Destroy(gameObject);
		}
		Instance = this;

		lastName = PlayerPrefs.GetString ("name", "");
		IsOpen = false;
		base.Awake ();
	}

	public override void OpenWindow() {
		specialThanksContentText.text = LanguageManager.Instance.GetTextValue ("ui.label.specialthankscontent").Replace ("#username", lastName);
	
		Analytics.CustomEvent("mentionShowed", new Dictionary<string, object> {
			{ "name", lastName}
		});
		base.OpenWindow();
	}

	public override void CloseWindow() {
		PlayerPrefs.SetInt ("specialMentionShowed", 1);
		base.CloseWindow ();
	}
}
