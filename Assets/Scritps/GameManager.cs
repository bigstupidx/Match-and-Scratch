using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using ReveloLibrary;
using SmartLocalization;

public enum GameState {
	None,
	MainMenu,
	GoToPlay,
	Playing,
	GameOver,
	Highscores
}

public enum DifficultType {
	NONE,
	MORE_COLORS,
	REVERSE_ENABLED,
	SPEEDUP
}

public class GameManager : MonoBehaviour {


	public static GameManager instance = null;

	public GameState currentState;
	public Color[] posibleColors;
	public Rotator rotator;
	public Spawner spawner;
	public Animator animator;
	//public UnityAds unityAds;

	public LevelUp levelUp;
	public Text newScore;
	public Text scoreLabel;
	public Text levelLabel;
	public Text GameOverPoints;

	public int initialLevel;
	[SerializeField]
	private int currentLevel;
	public int CurrentLevel {
		get{return currentLevel;}
		set { 
			currentLevel = value;
			levelLabel.text = LanguageManager.Instance.GetTextValue("ui.label.level") + " " + currentLevel.ToString();
		}
	}
	public bool canInverseDir;
	public bool gameHasEnded;

	private int lastScore;
	private int score;
	public int Score {
		get { return score;}
		private set {
			score = value;
			scoreLabel.text = LanguageManager.Instance.GetTextValue("ui.label.score") + " " + score.ToString();
		}
	}

	public void AddScore(int pts) {
		score += pts;
		scoreLabel.text = LanguageManager.Instance.GetTextValue("ui.label.score") + " " + score.ToString();

		newScore.text = "+" + pts.ToString();
		if ( !newScore.GetComponent<Animator>().GetCurrentAnimatorStateInfo (0).IsName ("start") )
			newScore.GetComponent<Animator> ().SetTrigger ("start");
	}

	public int MAX_COLORS_IN_GAME = 5;

	private List<int> pointsRequiredToLevelUp = new List<int>() { 1, 4, 8, 12, 16, 20, 25, 30, 34, 36, 40, 45, 50, 55, 60 };
	private List<DifficultType> difficulty = new List<DifficultType>() { 
		DifficultType.NONE,

		DifficultType.MORE_COLORS,
		DifficultType.MORE_COLORS,
		DifficultType.SPEEDUP,
		DifficultType.MORE_COLORS, 		
		DifficultType.REVERSE_ENABLED,	// 5
		DifficultType.MORE_COLORS,
		DifficultType.MORE_COLORS,
		DifficultType.SPEEDUP,	
		DifficultType.MORE_COLORS,
		DifficultType.REVERSE_ENABLED,	// 10
		DifficultType.SPEEDUP,
		DifficultType.MORE_COLORS,
		DifficultType.REVERSE_ENABLED,
		DifficultType.MORE_COLORS,	
		DifficultType.SPEEDUP			// 15
	};

	void Awake() {
		if (instance == null) {
			instance = this;
		}
		else if (instance != this) {
			Destroy(gameObject);
		}

		//TODO: Seleccionar lenguaje del sistema
		if (Application.systemLanguage == SystemLanguage.Spanish)
			LanguageManager.Instance.ChangeLanguage("es");
		else
			LanguageManager.Instance.ChangeLanguage("en");
	}

	public void changeLanguage(string lang) {
		LanguageManager.Instance.ChangeLanguage (lang);
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
					if (ScreenManager.Instance.currentGUIScreen.screenDefinition  == ScreenDefinitions.HIGHSCORES)
						animator.SetBool("highscores", false);
					ShowScreen (ScreenDefinitions.MAIN_MENU);
					AudioMaster.instance.StopAll (false);
					AudioMaster.instance.PlayLoop (SoundDefinitions.THEME_MAINMENU);
					rotator.EraseAllPins ();
				if(currentState != GameState.Highscores && currentState != GameState.None)
						animator.SetTrigger ("menu");
				break;
				case GameState.GoToPlay:
					animator.SetTrigger ("start");
					SetGameState (GameState.Playing);
				break;
			case GameState.Playing:
				ScreenManager.Instance.HideScreen ();
					StartCoroutine (WaitUntilPlayingState ());			
				break;
				case GameState.GameOver:
					AudioMaster.instance.Play (SoundDefinitions.END_FX);
					animator.SetTrigger ("exit");
					GameOverPoints.text = Score.ToString ();
					if (Score > 0)
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

	IEnumerator WaitUntilPlayingState() {
		while ( !animator.GetCurrentAnimatorStateInfo (0).IsName("PlayingGame") ){
			yield return null;
		}

		ShowScreen (ScreenDefinitions.GAME, BeginGame);		
	}

	void BeginGame() {
		ResetGame ();
		spawner.SpawnPin();
	}

	public void ShowGameOverScreen() {
		ShowScreen (ScreenDefinitions.GAME_OVER);
	}

	public void ShowScreen(ScreenDefinitions screenDef, ScreenManager.Callback TheCallback = null) {
		ScreenManager.Instance.ShowScreen(screenDef, TheCallback);
	}

	public void StartGame() {		
		SetGameState(GameState.GoToPlay);
	}

	public void GameOver() {
		if (gameHasEnded)
			return;
		
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
		canInverseDir = false;
		CurrentLevel = initialLevel;
		Score = 0;
		gameHasEnded = false;		
		spawner.enabled = true;
		rotator.enabled = true;
		AudioMaster.instance.StopAll(true);
		AudioMaster.instance.PlayLoop(SoundDefinitions.LOOP_1);
		//ShowScreen (ScreenDefinitions.GAME);
	}

	public void ShowHighscores() {		
		SetGameState(GameState.Highscores);
	}

	IEnumerator RefreshHighscores() {
		while (true) {
			Dreamlo_HighScores.instance.DownloadHighscores ();
			yield return new WaitForSeconds (20);
		}
	}

	public void CheckDifficulty() {
		if (canLevelUp(score)) {
			LevelUp ();
		}
	}

	bool canLevelUp(int score) {

		if (pointsRequiredToLevelUp.Contains (score)) {
			return true;
		} 
		else if (score > 0 && score % 5 == 0) {
			return true;
		}
		
		return false;
	}
		
	void LevelUp() {
		CurrentLevel++;

		DifficultType difficult;

		if (CurrentLevel < difficulty.Count)
			difficult = difficulty [CurrentLevel];
		else if (CurrentLevel % 2 == 0)
			difficult = DifficultType.SPEEDUP;
		else if (CurrentLevel % 4 == 0)
			difficult = DifficultType.REVERSE_ENABLED;
		else
			difficult = DifficultType.NONE;

		switch (difficult) {
		case DifficultType.MORE_COLORS:
				int newMaxColors = spawner.colorsInGame + 1;
				spawner.colorsInGame = Mathf.Min (newMaxColors, MAX_COLORS_IN_GAME);
				AudioMaster.instance.Play (SoundDefinitions.SFX_SPEED);
			break;
			case DifficultType.SPEEDUP:
				rotator.speed += 20;
				AudioMaster.instance.Play (SoundDefinitions.SFX_SPEED);
			break;
			case DifficultType.REVERSE_ENABLED:
				canInverseDir = !canInverseDir;
				AudioMaster.instance.Play (SoundDefinitions.SFX_REVERSE);
			break;
		}

		levelUp.Show (difficult);
	}
}
