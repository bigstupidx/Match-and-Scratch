using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using ReveloLibrary;

public enum GameState {
	None,
	MainMenu,
	GoToPlay,
	Playing,
	GameOver,
	Highscores
}

public class GameManager : MonoBehaviour {
	enum DifficultType {
		NONE,
		ADD_COLOR,
		INVERSE_ENABLED,
		INCREASE_SPEED
	}

	public static GameManager instance = null;

	public GameState currentState;
	public Color[] posibleColors;
	public Rotator rotator;
	public Spawner spawner;
	public Animator animator;

	public Text levelUpText;
	public Text scoreLabel;
	public Text maxScoreLabel;
	public Text GameOverPoints;

	public int initialLevel;
	public int currentLevel;
	public bool canInverseDir;
	public bool gameHasEnded;

	private int lastScore;
	private int score;
	public int Score {
		get{return score;}
		set {score = value;}
	}

	public int MAX_COLORS_IN_GAME = 5;

	private List<int> pointsRequiredToLevelUp = new List<int>() { 2, 5, 7, 10, 12, 15, 17, 20, 22, 25 };
	private List<DifficultType> difficulty = new List<DifficultType>() { 
		DifficultType.NONE,
		DifficultType.ADD_COLOR,
		DifficultType.NONE,
		DifficultType.ADD_COLOR,
		DifficultType.INCREASE_SPEED,
		DifficultType.ADD_COLOR, 		//5
		DifficultType.INVERSE_ENABLED,
		DifficultType.ADD_COLOR,
		DifficultType.NONE,
		DifficultType.ADD_COLOR,
		DifficultType.INCREASE_SPEED,	//10
		DifficultType.ADD_COLOR,
		DifficultType.INVERSE_ENABLED,
		DifficultType.INCREASE_SPEED,
		DifficultType.ADD_COLOR,
		DifficultType.NONE,				//15
		DifficultType.INVERSE_ENABLED,
		DifficultType.ADD_COLOR,	
		DifficultType.INCREASE_SPEED,
		DifficultType.INVERSE_ENABLED,
		DifficultType.ADD_COLOR,		//20
		DifficultType.INVERSE_ENABLED
	};

	void Awake() {
		if (instance == null) {
			instance = this;
		}
		else if (instance != this) {
			Destroy(gameObject);
		}
		int highscore = PlayerPrefs.GetInt("MaxScore");
		maxScoreLabel.text =  highscore == 0 ? "" : "Max: " + highscore.ToString();
	}

	public void Start() {
		ShowScreen (ScreenDefinitions.MAIN_MENU);
		StartCoroutine(RefreshHighscores());
		SetGameState(GameState.MainMenu);
	}

	public void SetGameState(GameState newState) {
		if (currentState != newState) {
			switch(newState) {
				case GameState.MainMenu:
					animator.SetBool("highscores", false);
					ShowScreen (ScreenDefinitions.MAIN_MENU);
					AudioMaster.instance.StopAll (false);
					AudioMaster.instance.PlayLoop (SoundDefinitions.THEME_MAINMENU);
					rotator.EraseAllPins ();
				if(currentState != GameState.Highscores && currentState != GameState.None)
						animator.SetTrigger ("menu");
				break;
				case GameState.GoToPlay:
					ShowScreen (ScreenDefinitions.GAME);
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
					if (score > 0)
						InputNameScreen.Instance.OpenWindow ();

				break;
				case GameState.Highscores:
					animator.SetBool("highscores", true);
					ShowScreen (ScreenDefinitions.HIGHSCORES);
				break;
			}
			currentState = newState;
		}
	}

	public void EnableSpawner() {
		spawner.SpawnPin(0.2f);
	}

	public void ShowGameOverScreen() {
		ShowScreen (ScreenDefinitions.GAME_OVER);
	}

	public void ShowScreen(ScreenDefinitions screenDef) {
		ScreenManager.Instance.ShowScreen(screenDef);
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
		currentLevel = initialLevel;
		score = 0;
		gameHasEnded = false;		
		spawner.enabled = true;
		rotator.enabled = true;
		AudioMaster.instance.StopAll(true);
		AudioMaster.instance.PlayLoop(SoundDefinitions.LOOP_1);
		ShowScreen (ScreenDefinitions.GAME);
	}

	public void ShowHighscores() {		
		SetGameState(GameState.Highscores);
	}

	IEnumerator RefreshHighscores() {
		while (true) {
			HighScores.instance.DownloadHighscores ();
			yield return new WaitForSeconds (30);
		}
	}

	void Update() {
		scoreLabel.text = score.ToString();
	}

	public void CheckDifficulty() {
		if (lastScore != score) {
			if (score > 0 ) {
				if (canLevelUp(score)) {
					LevelUp ();
				}
			}
			lastScore = score;
		}
	}

	bool canLevelUp(int score) {

		if (pointsRequiredToLevelUp.Contains (score)) {
			return true;
		} 
		else if (score % 5 == 0) {
			return true;
		}
		
		return false;
	}
		
	void LevelUp() {
		currentLevel++;

		DifficultType difficult;

		if (currentLevel < difficulty.Count)
			difficult = difficulty [currentLevel];
		else if (currentLevel % 2 == 0)
			difficult = DifficultType.INCREASE_SPEED;
		else if (currentLevel % 4 == 0)
			difficult = DifficultType.INVERSE_ENABLED;
		else
			difficult = DifficultType.NONE;

		switch (difficult) {
		case DifficultType.ADD_COLOR:
				int newMaxColors = spawner.colorsInGame + 1;
				spawner.colorsInGame = Mathf.Min (newMaxColors, MAX_COLORS_IN_GAME);
				AudioMaster.instance.Play (SoundDefinitions.SFX_SPEED);
			break;
			case DifficultType.INCREASE_SPEED:
				rotator.speed += 30;
				AudioMaster.instance.Play (SoundDefinitions.SFX_SPEED);
			break;
			case DifficultType.INVERSE_ENABLED:
				canInverseDir = !canInverseDir;
				AudioMaster.instance.Play (SoundDefinitions.SFX_REVERSE);
			break;
		}

		levelUpText.GetComponent<Animator>().SetTrigger("levelup");
	}
}
