using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ReveloLibrary;

public enum GameState {
	MainMenu,
	GoToPlay,
	Playing,
	GameOver
}

public class GameManager : MonoBehaviour {
	public static GameManager instance = null;
	public GameState currentState;
	public Color[] posibleColors;
	public Rotator rotator;
	public Spawner spawner;
	public Animator animator;
	public GameObject MainMenuScreen;
	public GameObject GameScreen;
	public GameObject GameOverScreen;
	public Text levelUpText;
	public int currentLevel = 0;
	public Text scoreLabel;
	public Text maxScoreLabel;
	public Text GameOverPoints;
	public bool gameHasEnded = false;

	private int score = 0;
	public int Score {
		get{return score;}
		set {score = value;}
	}
	private int lastScore = 0;

	void Awake() {
		if (instance == null) {
			instance = this;
		}
		else if (instance != this) {
			Destroy(gameObject);
		}
		//GameOverScreen.SetActive(false);
		int highscore = PlayerPrefs.GetInt("MaxScore");
		maxScoreLabel.text =  highscore == 0 ? "" : "Max: " + highscore.ToString();
	}

	public void Start() {
		HideAllScreens ();
		ShowScreen (MainMenuScreen);
		AudioMaster.instance.PlayLoop (SoundDefinitions.THEME_MAINMENU);
		SetGameState(GameState.MainMenu);
	}

	public void SetGameState(GameState newState) {
		if (currentState != newState) {
			switch(newState) {
				case GameState.MainMenu:
					ShowScreen (MainMenuScreen);
					AudioMaster.instance.StopAll (false);
					AudioMaster.instance.PlayLoop (SoundDefinitions.THEME_MAINMENU);
					rotator.EraseAllPins ();
					animator.SetTrigger ("menu");
				break;
				case GameState.GoToPlay:
					ShowScreen(GameScreen);
					animator.SetTrigger ("start");
					SetGameState (GameState.Playing);
				break;
				case GameState.Playing:
					ResetGame();
				break;
			case GameState.GameOver:
				AudioMaster.instance.Play (SoundDefinitions.END_FX);
				animator.SetTrigger ("exit");
				GameOverPoints.text = score.ToString ();

				break;
			}
			currentState = newState;
		}
	}

	public void EnableSpawner() {
		spawner.SpawnPin(0.2f);
	}

	public void ShowGameOverScreen() {
		ShowScreen (GameOverScreen);
	}

	public void ShowScreen(GameObject screen) {
		HideAllScreens();
		screen.SetActive(true);
	}

	void HideAllScreens() {
		GameOverScreen.SetActive(false);
		MainMenuScreen.SetActive(false);
		GameScreen.SetActive(false);
	}

	public void StartGame() {		
		SetGameState(GameState.GoToPlay);
	}

	public void EndGame() {
		if (gameHasEnded)
			return;

		if (score > PlayerPrefs.GetInt("MaxScore")) {
			PlayerPrefs.SetInt("MaxScore", score);
		}
		
		rotator.enabled = false;
		spawner.enabled = false;
		gameHasEnded = true;
		
		SetGameState(GameState.GameOver);
	}

	public void BackToMainMenu() {
		SetGameState(GameState.MainMenu);
	}

	void ResetGame() {
		spawner.Reset();
		rotator.Reset();
		currentLevel = 0;
		score = 0;
		gameHasEnded = false;		
		spawner.enabled = true;
		rotator.enabled = true;
		AudioMaster.instance.StopAll(true);
		AudioMaster.instance.PlayLoop(SoundDefinitions.LOOP_1);
		GameScreen.SetActive (true);
	}

	void Update() {
		scoreLabel.text = score.ToString();
	}

	public void CheckDifficulty() {
		if (lastScore != score) {
			if (score > 0 ) {
				if (score % 5 == 0) {
					LevelUp ();
				}
			}
			lastScore = score;
		}
	}

	void LevelUp() {
		currentLevel++;
		levelUpText.GetComponent<Animator>().SetTrigger("levelup");
	}
}
