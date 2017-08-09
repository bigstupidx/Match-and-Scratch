using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;
using UnityEngine.UI;

public class QuitScreen : UIScreen {

	public static QuitScreen Instance { get; private set;}

	public override void Awake ()
	{
		if(Instance != null && Instance != this) {
			Destroy(gameObject);
		}
		Instance = this;
		IsOpen = false;
		base.Awake ();
	}

	public void ButtonNo() {
		GameManager.instance.DisableInput ();
		CloseWindow (GameManager.instance.EnableInput);
	}

	public void ButtonYes() {
		Application.Quit();
	}
}
