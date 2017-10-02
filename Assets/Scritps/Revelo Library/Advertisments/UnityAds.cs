using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections.Generic;

public class UnityAds : MonoBehaviour
{
    public enum videoType
    {
        REWARDED_VIDEO,
        SKIPPABLES_VIDEO
    }

    public static UnityAds Instance = null;

    public delegate void callback(int result);

    private readonly string[] videoTypeStrings = new string[] { "rewardedVideo", "video" };

    private videoType currentVideoType;
    private videoType lastVideoType;
    private callback resultCallback;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public bool IsReady
    {
        get
        { 
            #if UNITY_ANDROID
                bool videoRewards = Advertisement.IsReady(videoTypeStrings[(int)videoType.REWARDED_VIDEO]);
                bool videoSkkipables = Advertisement.IsReady(videoTypeStrings[(int)videoType.SKIPPABLES_VIDEO]);

                if (videoRewards)
                    currentVideoType = videoType.REWARDED_VIDEO;
                else if (videoSkkipables)
                    currentVideoType = videoType.SKIPPABLES_VIDEO;
                else
                {
                    // If the service is not initialized, start the connection coroutine
                    if (!Advertisement.isInitialized)
                    {
                        StartCoroutine(AssertInitialization());
                    }
                    return false;
                }
                if (lastVideoType != currentVideoType)
                {
                    lastVideoType = currentVideoType;
                }
                return true;
            #else
			    return false;
            #endif
        }
    }

    void Start()
    {
        StartCoroutine(AssertInitialization());
    }

    IEnumerator AssertInitialization()
    {
        while (!Advertisement.isInitialized)
        {
            Advertisement.Initialize(Advertisement.gameId);
            yield return new WaitForSeconds(2f);
        }
    }

    public void ShowAds(bool rewarded)
    {
        ShowAds(rewarded, null);
    }

    public void ShowAds(bool rewarded, callback _callback = null)
    {

        resultCallback = _callback;

        if (Advertisement.isInitialized)
        {
            if (Advertisement.isSupported)
            {
				

                if (Advertisement.IsReady())
                {
                    var options = new ShowOptions { resultCallback = HandleShowResult };
                    string videotypeString = videoTypeStrings[rewarded ? (int)videoType.REWARDED_VIDEO : (int)videoType.SKIPPABLES_VIDEO];
                    AnalyticsSender.SendCustomAnalitycs("supportVideo", new Dictionary<string, object>()
                        {
                            { "type", rewarded ? "rewarded" : "skippable" }
                        });
                    Advertisement.Show(videotypeString, options);
                }
                else
                {
                    HandleShowResult(ShowResult.Failed);
                }
            }
            else
            {
                HandleShowResult(ShowResult.Failed);
            }
        }
        else
        {
            HandleShowResult(ShowResult.Failed);
        }
    }

    private void HandleShowResult(ShowResult result)
    {
        if (resultCallback != null)
        {
			
            resultCallback((int)result);
        } 
    }
}
