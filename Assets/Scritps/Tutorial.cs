using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReveloLibrary;
using UnityEngine.Analytics;

public class Tutorial : MonoBehaviour {


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


	void Awake() {
		screen = transform.parent.GetComponent<UIScreen> ();
		tutorialWrapper.SetActive(false);
	}

	// Use this for initialization
	public void OnEnable () {
		if (GameManager.instance.OnBeginGame == null)			
			GameManager.instance.OnBeginGame += HandleOnBeginGame;
	}
	void OnDisable() {
		GameManager.instance.OnBeginGame -= HandleOnBeginGame;
	}

	bool completedRotation;

	IEnumerator ShowTutorial() {
		if (!tutorialShowed) {
			GameManager.instance.currentGamePlayState = GamePlayState.Tutorial;

			GameManager.instance.rotator.OnPinPinned += HandleOnPinPinned;
			GameManager.instance.rotator.OnCompleteRotation += HandleCompleteRotation;

			while (!screen.InOpenState) {
				yield return null;
			}

			tutorialWrapper.SetActive(true);
			//Debug.Log ("Tutoral Start");


			EnableClick (true);
			// · First Throw: paramos el rotor para que el jugador dispare la bola
			GameManager.instance.rotator.transform.rotation = new Quaternion(0, 0, 0, 0);
			GameManager.instance.rotator.RotationSpeed = 0;
			clickAvailable = true;
			while (currentTutorialStep == TutorialStep.FIRST_THROW) {
				if ( Input.GetButtonDown("Fire1") )
					ContinueTurorial();
				yield return null;
			}
			//Debug.Log ("Tutoral First Throw");

			// · Second Throw: Iniciamos la rotación del rotor
			GameManager.instance.rotator.RotationSpeed = Rotator.INITIAL_SPEED;
			//EnableClick (false);
			// · --> Esperamos a que de un giro completo.
			while (!completedRotation) {
				yield return null;
			}
			// . --> Cuando ha dado el giro completo, esperamos al click del usuario
			completedRotation = false;
			GameManager.instance.rotator.transform.rotation = new Quaternion(0, 0, 0, 0);
			GameManager.instance.rotator.RotationSpeed = 0;
			EnableClick (true);
			while (currentTutorialStep == TutorialStep.SECOND_THROW) {
				if ( Input.GetButtonDown("Fire1") )
					ContinueTurorial();
				yield return null;
			}
			//Debug.Log ("Tutoral Second Throw");

			// · Third Throw: Iniciamos la rotación del rotor
			GameManager.instance.rotator.RotationSpeed = Rotator.INITIAL_SPEED;
			EnableClick (false);
			// · --> Esperamos a que de un giro completo.
			while (!completedRotation) {
				yield return null;
			}
			// . --> Cuando ha dado el giro completo, esperamos al click del usuario
			completedRotation = false;
			GameManager.instance.rotator.transform.rotation = new Quaternion(0, 0, 0, 0);
			GameManager.instance.rotator.RotationSpeed = 0;
			EnableClick (true);
			while (currentTutorialStep == TutorialStep.THIRD_THROW) {
				if ( Input.GetButtonDown("Fire1"))
					ContinueTurorial();
				yield return null;
			}
			//Debug.Log ("Tutoral Third Throw");
			// Terminamos el tutorial
			EndTutorial();
		}
		//yield return null;
	}

	void EnableClick(bool value) {
		hand.SetActive (value);
		clickAvailable = value;
	}

	public void ContinueTurorial() {
		//Debug.Log ("<color=yellow>Click enabled: " + clickAvailable + "</color>");
		if (clickAvailable) {
			GameManager.instance.spawner.lastSpawnedPin.GetComponent<Pin> ().isShooted = true;
		}
		EnableClick (false);
	}

	void HandleOnBeginGame() {
		tutorialShowed = PlayerPrefs.GetInt ("tutorialVisto", 0) == 1;
		if (!tutorialShowed) {
			StartCoroutine (ShowTutorial());
		}
	}

	void HandleOnPinPinned() {
		currentTutorialStep++;
	}

	void HandleCompleteRotation() {
		completedRotation = true;
	}

	public void EndTutorial() {
		currentTutorialStep = TutorialStep.END;
		PlayerPrefs.SetInt ("tutorialVisto", 1);
		GameManager.instance.currentGamePlayState = GamePlayState.Normal;
		GameManager.instance.rotator.OnPinPinned -= HandleOnPinPinned;
		GameManager.instance.rotator.OnCompleteRotation -= HandleCompleteRotation;
		GameManager.instance.rotator.RotationSpeed = Rotator.INITIAL_SPEED;

		tutorialWrapper.SetActive(false);
		Analytics.CustomEvent("tutorialEnd", new Dictionary<string, object>());
		//Debug.Log ("Tutoral End");
	}
}
