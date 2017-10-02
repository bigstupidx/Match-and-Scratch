using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;

public class Tutorial : MonoBehaviour
{
    public enum TutorialStep
    {
        FIRST_THROW,
        SECOND_THROW,
        THIRD_THROW,
        END
    }

    TutorialStep currentTutorialStep;
    public GameObject tutorialWrapper;
    public GameObject hand;

    UIScreen screen;
    bool tutorialShowed;
    bool clickAvailable;


    void Awake()
    {
        screen = transform.parent.GetComponent<UIScreen>();
        tutorialWrapper.SetActive(false);
    }

    // Use this for initialization
    public void OnEnable()
    {
        if (GameManager.Instance.OnBeginGame == null)
            GameManager.Instance.OnBeginGame += HandleOnBeginGame;
    }

    void OnDisable()
    {
        GameManager.Instance.OnBeginGame -= HandleOnBeginGame;
    }

    bool completedRotation;

    IEnumerator ShowTutorial()
    {
        if (!tutorialShowed)
        {
            GameManager.Instance.currentGamePlayState = GamePlayState.TUTORIAL;

            GameManager.Instance.rotator.OnPinPinned += HandleOnPinPinned;
            GameManager.Instance.rotator.OnCompleteRotation += HandleCompleteRotation;

            while (!screen.InOpenState)
            {
                yield return null;
            }

            tutorialWrapper.SetActive(true);

            EnableClick(true);
            // · First Throw
            GameManager.Instance.rotator.transform.rotation = new Quaternion(0, 0, 0, 0);
            GameManager.Instance.rotator.RotationSpeed = 0;
			
            clickAvailable = true;
			
            while (currentTutorialStep == TutorialStep.FIRST_THROW)
            {
                if (Input.GetButtonDown("Fire1"))
                    ContinueTurorial();
                yield return null;
            }
            // · Second Throw
            GameManager.Instance.rotator.RotationSpeed = Rotator.INITIAL_SPEED;
            // · --> wait for 360º rotation.
            while (!completedRotation)
            {
                yield return null;
            }
            // . --> when 360º rotation is complete, wait for the player click
            completedRotation = false;
            GameManager.Instance.rotator.transform.rotation = new Quaternion(0, 0, 0, 0);
            GameManager.Instance.rotator.RotationSpeed = 0;
			
            EnableClick(true);
			
            while (currentTutorialStep == TutorialStep.SECOND_THROW)
            {
                if (Input.GetButtonDown("Fire1"))
                    ContinueTurorial();
                yield return null;
            }

            // · Third Throw: Iniciamos la rotación del rotor
            GameManager.Instance.rotator.RotationSpeed = Rotator.INITIAL_SPEED;
            EnableClick(false);
            // · --> wait for 360º rotation.
            while (!completedRotation)
            {
                yield return null;
            }
            // . --> when 360º rotation is complete, wait for the player click
            completedRotation = false;
            GameManager.Instance.rotator.transform.rotation = new Quaternion(0, 0, 0, 0);
            GameManager.Instance.rotator.RotationSpeed = 0;
			
            EnableClick(true);
			
            while (currentTutorialStep == TutorialStep.THIRD_THROW)
            {
                if (Input.GetButtonDown("Fire1"))
                    ContinueTurorial();
                yield return null;
            }

            EndTutorial();
        }
    }

    void EnableClick(bool value)
    {
        hand.SetActive(value);
        clickAvailable = value;
    }

    public void ContinueTurorial()
    {
        if (clickAvailable)
        {
            GameManager.Instance.spawner.lastSpawnedPin.GetComponent<Pin>().isShooted = true;
        }
        EnableClick(false);
    }

    void HandleOnBeginGame()
    {
        tutorialShowed = PlayerPrefs.GetInt("tutorialVisto", 0) == 1;
        if (!tutorialShowed)
        {
            StartCoroutine(ShowTutorial());
        }
    }

    void HandleOnPinPinned()
    {
        currentTutorialStep++;
    }

    void HandleCompleteRotation()
    {
        completedRotation = true;
    }

    public void EndTutorial()
    {
        currentTutorialStep = TutorialStep.END;
        PlayerPrefs.SetInt("tutorialVisto", 1);
        GameManager.Instance.currentGamePlayState = GamePlayState.NORMAL;
        GameManager.Instance.rotator.OnPinPinned -= HandleOnPinPinned;
        GameManager.Instance.rotator.OnCompleteRotation -= HandleCompleteRotation;
        GameManager.Instance.rotator.RotationSpeed = Rotator.INITIAL_SPEED;

        tutorialWrapper.SetActive(false);
        AnalyticsSender.SendCustomAnalitycs("tutorialEnd", new Dictionary<string, object>());
    }
}