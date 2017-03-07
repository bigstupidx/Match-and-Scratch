using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState {
	None,
	MainMenu,
	Game,
	GameOver
}

public class GameManager : MonoBehaviour {
	public static GameManager instance = null;
	public GameState currentState = GameState.None;
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
		GameOverScreen.SetActive(false);
		int highscore = PlayerPrefs.GetInt("MaxScore");
		maxScoreLabel.text =  highscore == 0 ? "" : "Max: " + highscore.ToString();

		SetGameState(GameState.MainMenu);
	}

	void SetGameState(GameState newState) {
		if (currentState != newState) {
			HideAllScreens();
			switch(newState) {
				case GameState.MainMenu:
					MainMenuScreen.SetActive(true);
					animator.SetTrigger("MainMenu");
				break;
				case GameState.Game:
					ResetGame();
					GameScreen.SetActive(true);
					if (currentState == GameState.MainMenu)
						animator.SetTrigger("StartGame");
					else if (currentState == GameState.GameOver)
						animator.SetTrigger("StartGame");

					spawner.SpawnPin(1f);
				break;
				case GameState.GameOver:
					GameOverScreen.SetActive(true);
					animator.SetTrigger ("EndGame");					
					GameOverPoints.text = score.ToString();
				break;
			}
			currentState = newState;
		}	
	}

	void HideAllScreens() {
		GameOverScreen.SetActive(false);
		MainMenuScreen.SetActive(false);
		GameScreen.SetActive(false);
	}

	public void StartGame() {		
		SetGameState(GameState.Game);
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
