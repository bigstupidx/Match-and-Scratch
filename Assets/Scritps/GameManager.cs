using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using ReveloLibrary;
using SmartLocalization;
using System;

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
	SWITCH_REVERSE,
	SPEEDUP,
	VARIABLE_SPEED
}

public enum HighScoresSource {
	DREAMLO,
	FIREBASE
}

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;

	public GameState currentState;
	public Color[] posibleColors;
	public Rotator rotator;
	public Spawner spawner;
	public Animator animator;

	public LevelUp levelUp;
	public Text newScore;
	public Text scoreLabel;
	public Text levelLabel;
	public Text speedLabel;
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
	public bool canUseVariableSpeed;

	public bool gameHasEnded;


	//private int lastScore;
	[SerializeField]
	private int score;
	public int Score {
		get { return score;}
		private set {
			score = value;
			scoreLabel.text = LanguageManager.Instance.GetTextValue("ui.label.score") + " " + score.ToString();
		}
	}

	public HighScoresSource currentSource;
	public Action<HighScoresSource> OnChangeHighScoresSource;

	public void SetNewHighScoresSource(HighScoresSource newSource) {
		currentSource = newSource;
		if(OnChangeHighScoresSource != null)
			OnChangeHighScoresSource (currentSource);
	}

	public void AddScore(int pts) {
		for (int i = 1; i <= pts; i++) {
			score++;
			CheckDifficulty();
		}

		scoreLabel.text = LanguageManager.Instance.GetTextValue("ui.label.score") + " " + score.ToString();

		newScore.text = "+" + pts.ToString();
		if ( !newScore.GetComponent<Animator>().GetCurrentAnimatorStateInfo (0).IsName ("start") )
			newScore.GetComponent<Animator> ().SetTrigger ("start");
	}

	public int MAX_COLORS_IN_GAME = 5;

	private Queue<int> pointsRequiredToLevelUpQueue;
	private List<int> pointsRequiredToLevelUp = new List<int>() {
		1,
		4,
		8,
		12,
		16, 	// 5
		20,
		25,
		30,
		34,
		36,		// 10
		40,
		45,
		50,
		55,
		60 		// 15
	};
	private Queue<DifficultType> difficultyStepsQueue;
	private List<DifficultType> difficultySteps = new List<DifficultType>() {
		DifficultType.MORE_COLORS,
		DifficultType.SPEEDUP,
		DifficultType.MORE_COLORS, 		
		DifficultType.SWITCH_REVERSE,
		DifficultType.MORE_COLORS,		// 5
		DifficultType.VARIABLE_SPEED,
		DifficultType.SPEEDUP,
		DifficultType.SWITCH_REVERSE,
		DifficultType.MORE_COLORS,
		DifficultType.VARIABLE_SPEED,
		DifficultType.SPEEDUP,			// 10
		DifficultType.VARIABLE_SPEED,	
		DifficultType.SWITCH_REVERSE,
		DifficultType.MORE_COLORS,	
		DifficultType.VARIABLE_SPEED,
		DifficultType.SPEEDUP,			// 15
		DifficultType.VARIABLE_SPEED
	};

	SoundDefinitions[] musics = { SoundDefinitions.LOOP_1, SoundDefinitions.LOOP_2, SoundDefinitions.LOOP_3 };
	int currentMusic = 0;

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
		pointsRequiredToLevelUpQueue = new Queue<int> (pointsRequiredToLevelUp);
		difficultyStepsQueue = new Queue<DifficultType> (difficultySteps);
		spawner.Reset();
		rotator.Reset();
		canInverseDir = false;
		CurrentLevel = initialLevel;
		Score = 0;
		gameHasEnded = false;		
		spawner.enabled = true;
		rotator.enabled = true;
		currentMusic = 0;
		AudioMaster.instance.StopAll(true);
		AudioMaster.instance.PlayLoop(musics[currentMusic]);
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
		if (CanLevelUp(score)) {
			LevelUp ();
		}
	}

	bool CanLevelUp(int score) {

		if (pointsRequiredToLevelUpQueue.Count == 0 && currentLevel % 5 == 0) {
			return true;
		} 
		else if (score >= pointsRequiredToLevelUpQueue.Peek()) {
			return true;
		} 
		
		return false;
	}
		
	void LevelUp() {
		DifficultType difficult = DifficultType.NONE;

		CurrentLevel++;

		// hacemos cambios de musica
		if (currentLevel % 30 == 0) {
			AudioMaster.instance.StopSound (musics[currentMusic], true);
			currentMusic = ++currentMusic > musics.Length ? 0 : currentMusic++;
			AudioMaster.instance.PlayLoop (musics[currentMusic]);
		}

		pointsRequiredToLevelUpQueue.Dequeue ();

		if (difficultyStepsQueue.Count == 0) {
			if (CurrentLevel % 10 == 0)
				difficult = DifficultType.SPEEDUP;
			else if (CurrentLevel % 5 == 0)
				difficult = DifficultType.SWITCH_REVERSE;
		} else {
			difficult = difficultyStepsQueue.Dequeue ();
		}
			
		switch (difficult) {
		case DifficultType.MORE_COLORS:
				int newMaxColors = spawner.colorsInGame + 1;
				spawner.colorsInGame = Mathf.Min (newMaxColors, MAX_COLORS_IN_GAME);
				AudioMaster.instance.Play (SoundDefinitions.SFX_SPEED);
			break;
			case DifficultType.SPEEDUP:
				rotator.RotationSpeed += 20;
				AudioMaster.instance.Play (SoundDefinitions.SFX_SPEED);
			break;
			case DifficultType.SWITCH_REVERSE:
				canInverseDir = !canInverseDir;
				AudioMaster.instance.Play (SoundDefinitions.SFX_REVERSE);
			break;
			case DifficultType.VARIABLE_SPEED:
				canUseVariableSpeed = !canUseVariableSpeed;
				StartCoroutine(rotator.VariableSpeedDifficult());
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_9);
				break;
		}
		speedLabel.text = LanguageManager.Instance.GetTextValue("ui.label.speed") + " " + rotator.RotationSpeed.ToString();
		levelUp.Show (difficult);
	}
}
