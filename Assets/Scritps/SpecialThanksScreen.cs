using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;
using UnityEngine.UI;
using SmartLocalization;

public class SpecialThanksScreen : UIScreen
{

    public static SpecialThanksScreen Instance = null;

    public Text titleText;
    public Text specialThanksContentText;

    private string lastName;

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

    public void ShowSpecialMention()
    {
        titleText.text = LanguageManager.Instance.GetTextValue("ui.label.specialthaks");
        lastName = PlayerPrefs.GetString("name", "");
        specialThanksContentText.text = LanguageManager.Instance.GetTextValue("ui.label.specialthankscontent").Replace("#username", lastName);

        AnalyticsSender.SendCustomAnalitycs("mentionShowed", new Dictionary<string, object>
            {
                { "name", lastName }
            }
        );
        OpenWindow(GameManager.Instance.EnableInput);
    }

    public void ShowRememberSendScore()
    {
        titleText.text = LanguageManager.Instance.GetTextValue("ui.label.remember");
        lastName = PlayerPrefs.GetString("name", "");
        specialThanksContentText.text = LanguageManager.Instance.GetTextValue("ui.label.remembersendscore");

        AnalyticsSender.SendCustomAnalitycs("rememberSendScore", new Dictionary<string, object>
            {
                { "name", lastName }
            }
        );
        OpenWindow(GameManager.Instance.EnableInput);
    }

    public override void OpenWindow(Callback openCallback = null)
    {
        base.OpenWindow(openCallback);
    }

    public void ButtonContinue()
    {
        GameManager.Instance.DisableInput();
        CloseWindow(GameManager.Instance.EnableInput);
    }

    public override void CloseWindow(Callback closeCallback = null)
    {
        PlayerPrefs.SetInt("specialMentionShowed", 1);
        GameManager.Instance.DisableInput();
        if (closeCallback == null)
        {
            closeCallback = GameManager.Instance.EnableInput;
        }

        base.CloseWindow(closeCallback);
    }
}
