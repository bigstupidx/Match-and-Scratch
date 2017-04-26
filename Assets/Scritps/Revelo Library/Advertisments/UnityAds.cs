using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements; 
//using UnityEditor.Advertisements;

public class UnityAds : MonoBehaviour {


	public delegate void callback(int result);
	callback resultCallback;

	public bool IsReady {
		get { return Advertisement.IsReady ("rewardedVideo");}
	}

	void Start() {
		
		Advertisement.Initialize (Advertisement.gameId);
	}

	public void ShowAds() {
		ShowAds (null);
	}

	public void ShowAds(callback _callback = null) {

		resultCallback = _callback;

		if (Advertisement.isInitialized) {
			if (Advertisement.isSupported) {
				if (Advertisement.IsReady ("rewardedVideo")) {
					var options = new ShowOptions { resultCallback = HandleShowResult };
					Advertisement.Show ("rewardedVideo", options);
				} else {
					HandleShowResult (ShowResult.Failed);
					Debug.Log ("Ads not Ready");
				}
			} else {
				HandleShowResult (ShowResult.Failed);
				Debug.Log ("Ads no soportados");
			}
		} else {
			HandleShowResult (ShowResult.Failed);
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

		if (resultCallback != null)
			resultCallback ((int) result);
	}
}
