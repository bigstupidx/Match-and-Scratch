using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;
using UnityEngine.UI;

public class PauseScreen : UIScreen {

	public static PauseScreen Instance { get; private set;}

	public override void Awake ()
	{
		if(Instance != null && Instance != this) {
			Destroy(gameObject);
		}
		Instance = this;
		IsOpen = false;
		base.Awake ();
	}

	public void ResumeGame() {
		GameManager.instance.PauseGame (false);
	}

	public void ExitToMainManu() {
		GameManager.instance.ExitToMainMenu ();
	}
}
