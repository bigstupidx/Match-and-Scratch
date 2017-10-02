using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;
using UnityEngine.UI;

public class QuitScreen : UIScreen
{

    public static QuitScreen Instance = null;

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

        Instance = this;
        IsOpen = false;
        base.Awake();
    }

    public void ButtonNo()
    {
        GameManager.Instance.DisableInput();
        CloseWindow(GameManager.Instance.EnableInput);
    }

    public void ButtonYes()
    {
        Application.Quit();
    }
}
