using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements; 
//using UnityEditor.Advertisements;

public class UnityAds : MonoBehaviour {

	public enum videoType {
		REWARDED_VIDEO,
		SKIPPABLES_VIDEO
	}
	string[] videoTypeStrings = new string[] {"rewardedVideo", "video"};

	videoType currentVideoType;
	videoType lastVideoType;

	public delegate void callback(int result);
	callback resultCallback;


	public bool IsReady {
		get { 
			#if UNITY_ANDROID
			bool videoRewards = Advertisement.IsReady (videoTypeStrings [(int)videoType.REWARDED_VIDEO]);
			bool videoSkkipables = Advertisement.IsReady (videoTypeStrings [(int)videoType.SKIPPABLES_VIDEO]);

			if (videoRewards) 
				currentVideoType = videoType.REWARDED_VIDEO;
			else if (videoSkkipables)
				currentVideoType = videoType.SKIPPABLES_VIDEO;
			else {
				// Si no estan disponibles por que no está el servicio iniciado, intentamos iniciarlos para que la siguiente vez estén disponibles.
				if (!Advertisement.isInitialized) {
					StartCoroutine (AssertInitialization ());
				}
				return false;
			}
			if (lastVideoType != currentVideoType) {
				Debug.Log("videos disponibles unityAds: " + currentVideoType.ToString());
				lastVideoType = currentVideoType;
			}
			return true;
			#endif

			return false;
		}
	}

	void Start() {
		StartCoroutine (AssertInitialization ());
	}

	IEnumerator AssertInitialization() {
		while (!Advertisement.isInitialized) {
			Debug.Log ("Servicio  UnityAds  Iniciando manualmente...");
			Advertisement.Initialize (Advertisement.gameId);
			yield return new WaitForSeconds (2f);
		}
		Debug.Log ("Servicio  UnityAds inicializados.");
	}

	public void ShowAds() {
		ShowAds (null);
	}

	public void ShowAds(callback _callback = null) {

		resultCallback = _callback;

		if (Advertisement.isInitialized) {
			if (Advertisement.isSupported) {
				if (Advertisement.IsReady (videoTypeStrings[(int)currentVideoType])) {
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

		if (resultCallback != null) {
			
			resultCallback ((int)result);
		} 
	}
}
