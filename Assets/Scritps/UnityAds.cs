using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class UnityAds : MonoBehaviour {

	public string gameID;

	void Start() {
		Advertisement.Initialize (gameID);
	}

	public void ShowAds() {
		if (Advertisement.isSupported) {
			if (Advertisement.IsReady ("matchscratch")) {
				Advertisement.Show ("matchscratch", new ShowOptions (){ resultCallback = HandleAdResult });
			} else {
				Debug.Log ("Ads not Ready");
			}
		} else {
			Debug.Log ("Ads no soportados");
		}
	}

	private void HandleAdResult(ShowResult result) {
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
