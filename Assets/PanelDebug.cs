using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;

public class PanelDebug : MonoBehaviour {

	public GameObject highScoresButtonsWrapper;

	// Use this for initialization
	void Start () {
		ScreenManager.Instance.OnScreenChange += screenChange;
	}

	void screenChange(ScreenDefinitions def) {
		highScoresButtonsWrapper.SetActive (def == ScreenDefinitions.HIGHSCORES);
	}

}
