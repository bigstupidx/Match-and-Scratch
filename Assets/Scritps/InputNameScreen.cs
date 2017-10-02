using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReveloLibrary;

public class InputNameScreen : UIScreen
{
    public static InputNameScreen Instance = null;

    public Button sendButton;

    private InputField nameField;
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

        lastName = PlayerPrefs.GetString("name", "");
        nameField = GetComponentInChildren<InputField>();
        IsOpen = false;

        base.Awake();
    }

    public override void OpenWindow(Callback openCallback = null)
    {
        if (!string.IsNullOrEmpty(lastName))
        {
            nameField.text = lastName;
        }

        EvaluateNick();

        base.OpenWindow(openCallback);
    }

    public void CancelSendScore()
    {
        AnalyticsSender.SendCustomAnalitycs("scoreNotSend", new Dictionary<string, object>
            {
                { "score", GameManager.Instance.Score },
                { "nameUsedLastTime", lastName }
            }
        );

        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            CloseWindow(SpecialThanksScreen.Instance.ShowRememberSendScore);
        }
        else
        {
            CloseWindow();
        }
    }

    public override void CloseWindow(Callback closeCallback = null)
    {
        GameManager.Instance.DisableInput();

        if (closeCallback == null)
        {
            closeCallback = GameManager.Instance.EnableInput;
        }

        base.CloseWindow(closeCallback);
    }

    public void EvaluateNick()
    {
        sendButton.interactable = nameField.text.Length >= 3;
    }

    public void SendScore()
    {
        UnityAds.Instance.ShowAds(ServicesConfiguration.sendscore_video_is_rewarded, SendScoreToBBDD);
    }

    void SendScoreToBBDD(int result)
    {
        lastName = nameField.text;
        PlayerPrefs.SetString("name", lastName);

        FirebaseDBManager.instance.AddNewHighscore(new ScoreEntry(lastName, GameManager.Instance.Score));

        AnalyticsSender.SendCustomAnalitycs("scoreSended", new Dictionary<string, object>
            {
                { "score", GameManager.Instance.Score },
                { "name", lastName }
            }
        );

        // If SpecialMentio is not already shown
        bool specialMentionShowed = PlayerPrefs.GetInt("specialMentionShowed", 0) == 1;
        bool showMention = false;

        if (!specialMentionShowed)
        {
            showMention = UnityEngine.Random.Range(0, 10) == 0;
        }

        if (showMention)
        {
            CloseWindow(SpecialThanksScreen.Instance.ShowSpecialMention);
        }
        else
        {
            CloseWindow();
        }
    }
}