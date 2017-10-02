using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;

public class TermsOfUseScreen : UIScreen
{
	
    public static TermsOfUseScreen Instance = null;

    public string PrivacyPolicyUrl;

    Callback continueCallback;

    public override void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        IsOpen = false;
		
        base.Awake();
    }

    public void Show(Callback theCallBack = null)
    {
        continueCallback = theCallBack;
        OpenWindow();
    }

    public void OpenPrivacyPolicy()
    {
        Application.OpenURL(PrivacyPolicyUrl);
    }

    public void ButtonAccept()
    {
        GameManager.Instance.DisableInput();
        PlayerPrefs.SetInt("firstRun", 1);

        CloseWindow(continueCallback);
    }

    public void ButtonCancel()
    {
        Application.Quit();
    }
}
