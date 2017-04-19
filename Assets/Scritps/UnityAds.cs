﻿using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements; 
//using UnityEditor.Advertisements;

public class UnityAds : MonoBehaviour {

	void Start() {
		
		Advertisement.Initialize (Advertisement.gameId);
	}

	public void ShowAds() {

		if (Advertisement.isInitialized) {
			if (Advertisement.isSupported) {
				if (Advertisement.IsReady ("rewardedVideo")) {
					var options = new ShowOptions { resultCallback = HandleShowResult };
					Advertisement.Show ("rewardedVideo", options);
				} else {
					Debug.Log ("Ads not Ready");
				}
			} else {
				Debug.Log ("Ads no soportados");
			}
		} else {
			Debug.Log ("Ads no iniciados");
		}
	}

	private void HandleShowResult(ShowResult result) {
		switch (result) {
		case ShowResult.Finished:
			Debug.Log ("Video Visto");
			break;
		case ShowResult.Skipped:
			Debug.Log ("Video Saltado");
			break;
		case ShowResult.Failed:
			Debug.Log ("Video Failed");
			break;
		}
	}
}
