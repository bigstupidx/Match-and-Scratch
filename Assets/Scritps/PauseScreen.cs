using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;
using UnityEngine.UI;

public class PauseScreen : UIScreen
{

    public static PauseScreen Instance = null;

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

    public void ResumeGame()
    {
        GameManager.Instance.PauseGame(false);
    }

    public void ExitToMainManu()
    {
        GameManager.Instance.ExitToMainMenu();
    }
}