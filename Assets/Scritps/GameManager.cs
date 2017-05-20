using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using ReveloLibrary;
using SmartLocalization;
using System;
using UnityEngine.Analytics;

public enum GameState {
	None,
	MainMenu,
	GoToPlay,
	Playing,
	GameOver,
	Highscores
}

public enum GamePlayState {
	Tutorial, 
	Normal
}

public enum DifficultType {
	NONE,
	MORE_COLORS,
	SWITCH_REVERSE,
	SPEEDUP,
	SWITCH_VARIABLE_SPEED
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
	public Text GameOverPoints;

	public int initialLevel;

	[SerializeField]
	private int currentLevel;
	public int CurrentLevel {
		get{return currentLevel;}
		set { 
			currentLevel = value;
		}
	}
	public bool canInverseDir;
	public bool canUseVariableSpeed;

	public bool isGameOver;

	public GamePlayState currentGamePlayState = GamePlayState.Normal;

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

	public Action OnBeginGame;

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
		31,
		35,		// 10
		40,
		45,
		50,
		55,
		60,		// 15
		65,		
		69,
		70		// 18

	};
	private Queue<DifficultType> difficultyStepsQueue;
	private List<DifficultType> difficultySteps = new List<DifficultType>() {
		DifficultType.MORE_COLORS,
		DifficultType.SPEEDUP,
		DifficultType.SWITCH_REVERSE,
		DifficultType.MORE_COLORS, 
		DifficultType.SWITCH_REVERSE,	// 5
		DifficultType.SPEEDUP,	
		DifficultType.SWITCH_REVERSE,
		DifficultType.SPEEDUP,
		DifficultType.SWITCH_VARIABLE_SPEED,
		DifficultType.MORE_COLORS,		// 10
		DifficultType.SWITCH_VARIABLE_SPEED,
		DifficultType.SPEEDUP,		
		DifficultType.SWITCH_REVERSE,
		DifficultType.MORE_COLORS,
		DifficultType.SPEEDUP,			// 15
		DifficultType.MORE_COLORS,		
		DifficultType.SPEEDUP,			// 18
		DifficultType.SWITCH_VARIABLE_SPEED
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
		// StartCoroutine(RefreshHighscores());
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
		Analytics.CustomEvent("gameStart", new Dictionary<string, object>());
		ResetGame ();
		spawner.SpawnPin();
		if (OnBeginGame != null)
			OnBeginGame ();
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
		if (isGameOver)
			return;
		
		rotator.enabled = false;
		spawner.enabled = false;
		isGameOver = true;
		
		SetGameState(GameState.GameOver);
		Analytics.CustomEvent("gameOver", new Dictionary<string, object> 
		{
				{ "score", Score },
				{ "pinsCount", spawner.pinsCount}

		});
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
		isGameOver = false;		
		spawner.enabled = true;
		rotator.enabled = true;
		currentMusic = 0;
		levelUp.GetComponent<Animator> ().ResetTrigger ("start");
		levelUp.GetComponent<Animator> ().Play ("Idle");
		AudioMaster.instance.StopAll(false);
		AudioMaster.instance.PlayLoop(musics[currentMusic]);
	}

	public void ShowHighscores() {		
		SetGameState(GameState.Highscores);
	}
	/*
	IEnumerator RefreshHighscores() {
		while (true) {
			Dreamlo_HighScores.instance.DownloadHighscores ();
			yield return new WaitForSeconds (20);
		}
	}
	*/
	public void ThrowCurrentPin() {
		spawner.ThrowCurrentPin();
	}

	public void CheckDifficulty() {
		if (CanLevelUp(score)) {
			LevelUp ();
		}
		// hacemos cambios de musica
		if (score % 30 == 0) {
			AudioMaster.instance.StopSound (musics[currentMusic], true);
			currentMusic++;
			if (currentMusic > musics.Length -1) {
				currentMusic = 0;
			}
			AudioMaster.instance.PlayLoop (musics[currentMusic]);
		}
	}

	bool CanLevelUp(int score) {
		if (pointsRequiredToLevelUpQueue.Count == 0) {
			return score % 5 == 0;
		}
		else {
			return score == pointsRequiredToLevelUpQueue.Peek ();
		}
	}
		
	void LevelUp() {
		DifficultType difficult = DifficultType.NONE;

		CurrentLevel++;

		if (difficultyStepsQueue.Count == 0) {
			if (score % 15 == 0)
				difficult = DifficultType.SWITCH_VARIABLE_SPEED;
			else if (score % 10 == 0)
				difficult = DifficultType.SPEEDUP;
			else if (score % 5 == 0)
				difficult = DifficultType.SWITCH_REVERSE;
		} else {
			pointsRequiredToLevelUpQueue.Dequeue ();
			difficult = difficultyStepsQueue.Dequeue ();
		}
			
		switch (difficult) {
		case DifficultType.MORE_COLORS:
				spawner.AddColorsInGame(1);
				AudioMaster.instance.Play (SoundDefinitions.SFX_SPEED);
			break;
			case DifficultType.SPEEDUP:
				rotator.RotationSpeed += 15;
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_7);
			break;
			case DifficultType.SWITCH_REVERSE:
				canInverseDir = !canInverseDir;
				AudioMaster.instance.Play (SoundDefinitions.SFX_REVERSE);
			break;
			case DifficultType.SWITCH_VARIABLE_SPEED:
				canUseVariableSpeed = !canUseVariableSpeed;
				StartCoroutine(rotator.VariableSpeedDifficult());
				AudioMaster.instance.Play (SoundDefinitions.SCRATCH_10);
				break;
		}
		levelUp.Show (difficult);

		Debug.LogFormat ("<color=green>Level {0} a los {1} puntos -> Dificultad añadida: {2}</color>", currentLevel, score, difficult.ToString ());
	}
}
