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
	GameOver,
	Highscores
}

public class GameManager : MonoBehaviour {
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
	public bool gameHasEnded = false;

	private int lastScore = 0;
	private int score = 0;
	public int Score {
		get{return score;}
		set {score = value;}
	}

	private List<int> pointsRequiredToLevelUp = new List<int>(){ 2, 5, 7, 10, 12, 15, 17, 20, 22, 25 };

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
		//AudioMaster.instance.PlayLoop (SoundDefinitions.THEME_MAINMENU);
		StartCoroutine(RefreshHighscores());
		SetGameState(GameState.MainMenu);
	}

	public void SetGameState(GameState newState) {
		if (currentState != newState) {
			switch(newState) {
				case GameState.MainMenu:
					ShowScreen (ScreenDefinitions.MAIN_MENU);
					AudioMaster.instance.StopAll (false);
					AudioMaster.instance.PlayLoop (SoundDefinitions.THEME_MAINMENU);
					rotator.EraseAllPins ();
					if(currentState != GameState.Highscores)
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

		switch (currentLevel) {
			case 2:
			case 4:
			case 5:
			case 7:
			case 13:
				spawner.maxColor++;
				break;		
			case 3:
			case 6:
			case 9:
			case 11:
				canInverseDir = !canInverseDir;
				break;
		}

		levelUpText.GetComponent<Animator>().SetTrigger("levelup");
	}
}
