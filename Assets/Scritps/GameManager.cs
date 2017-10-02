using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using ReveloLibrary;
using SmartLocalization;
using System;

public enum GameState
{
    NONE,
    MAIN_MENU,
    GOTO_PLAY,
    PLAYING,
    GAME_OVER,
    HIGH_SCORES
}

public enum GamePlayState
{
    TUTORIAL,
    NORMAL
}

public enum DifficultType
{
    NONE,
    MORE_COLORS,
    SPEEDUP,
    SWITCH_REVERSE,
    SWITCH_REVERSE_CANCEL,
    SWITCH_CRAZY_SPEED,
    SWITCH_CRAZY_SPEED_CANCEL
}

public class GameManager : MonoBehaviour
{

    public static GameManager Instance = null;

    public bool isGamePaused;
    public bool isGameOver;
    public bool isInputEnabled;

    public GameState currentState;

    public GamePlayState currentGamePlayState = GamePlayState.NORMAL;

    public bool EnableDebugAtStart;
    public GameObject debugMenu;

    public Color[] posibleColors;
    public Rotator rotator;
    public Spawner spawner;
	
    public LevelUp levelUp;
    public Text scoreLabel;
    public Text GameOverPoints;

    public Animator animator;
    public int initialLevel;

    public Action OnBeginGame;

    [SerializeField]
    private int score;

    [SerializeField]
    private int currentLevel;

    private GameState nextGameState = GameState.NONE;

    private Queue<int> pointsRequiredToLevelUpQueue;
    private Queue<DifficultType> difficultyStepsQueue;

    private List<int> pointsRequiredToLevelUp = new List<int>()
    {
        1,
        10,
        20,
        30,
        40,     //5
        50,
        60,
        70,
        80,
        90,     // 10
        100,
        110,    
        120,
        130,
        140     // 15
    };

    private List<DifficultType> difficultySteps = new List<DifficultType>()
    {
        DifficultType.MORE_COLORS,
        DifficultType.SPEEDUP,
        DifficultType.MORE_COLORS,
        DifficultType.SPEEDUP,
        DifficultType.MORE_COLORS,          // 5
        DifficultType.SWITCH_REVERSE,
        DifficultType.MORE_COLORS,
        DifficultType.SPEEDUP,
        DifficultType.MORE_COLORS,
        DifficultType.SPEEDUP,              //10
        DifficultType.SWITCH_CRAZY_SPEED,
        DifficultType.MORE_COLORS,
        DifficultType.MORE_COLORS,
        DifficultType.SPEEDUP,
        DifficultType.SPEEDUP               //15
    };

    private SoundDefinitions[] musics = { SoundDefinitions.LOOP_1, SoundDefinitions.LOOP_2, SoundDefinitions.LOOP_3 };
    private int currentMusic = 0;

    private Animator scoreLabel_Animator;

    public int CurrentLevel
    {
        get
        { 
            return currentLevel; 
        }
        set
        { 
            currentLevel = value;
        }
    }

    public int Score
    {
        get
        { 
            return score; 
        }
        private set
        {
            score = value;
            scoreLabel.text = score.ToString();
        }
    }

    public bool ServicesConfigurationInitialized
    { 
        get; 
        set; 
    }

    public bool isTappxBannerVisible
    { 
        get; 
        set; 
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        // Selection os system languaje "es" for Spanish, "en" the any other
        if (Application.systemLanguage == SystemLanguage.Spanish)
            LanguageManager.Instance.ChangeLanguage("es");
        else
            LanguageManager.Instance.ChangeLanguage("en");

        scoreLabel_Animator = scoreLabel.GetComponent<Animator>();
    }

    public void Start()
    {
        if (EnableDebugAtStart)
            Instantiate(debugMenu, GameObject.FindGameObjectWithTag("UICanvas").transform);

        int firstRun = PlayerPrefs.GetInt("firstRun", 0);
        if (firstRun == 0)
            TermsOfUseScreen.Instance.Show(StartTheGame);
        else
            StartTheGame();
    }

    public void StartTheGame()
    {
        ShowScreen(ScreenDefinitions.MAIN_MENU);
        SetGameState(GameState.MAIN_MENU);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isInputEnabled)
        {
            Debug.Log("Escape key down when in gamestate: " + currentState.ToString());
            switch (currentState)
            {
                case GameState.MAIN_MENU:
                    if (QuitScreen.Instance.IsOpen)
                    {
                        QuitScreen.Instance.ButtonNo();
                    }
                    else
                    {
                        DisableInput();
                        QuitScreen.Instance.OpenWindow(EnableInput);
                    }
                    break;
                case GameState.HIGH_SCORES:
                    animator.SetBool("highscores", false);
                    BackToMainMenu();
                    break;
                case GameState.PLAYING:
                    if (isGamePaused)
                    {
                        PauseGame(false);
                    }
                    else
                    {
                        PauseGame(true);
                    }
                    break;
                case GameState.GAME_OVER:
                    if (InputNameScreen.Instance.IsOpen)
                        InputNameScreen.Instance.CancelSendScore();
                    else if (SpecialThanksScreen.Instance.IsOpen)
                        SpecialThanksScreen.Instance.CloseWindow();
                    else
                        BackToMainMenu();
                    break;
            }
        }
    }

    public void AddScore(int pts)
    {
        Score += pts;
        CheckDifficulty();
        scoreLabel.text = score.ToString();
        if (!scoreLabel_Animator.IsInTransition(0))
        {
            scoreLabel_Animator.SetTrigger("Action");
        }
    }

    public void SetServicesConfiguration(bool value)
    {
        ServicesConfigurationInitialized = value;
        if (ServicesConfigurationInitialized && ServicesConfiguration.enable_tappx && currentState == GameState.MAIN_MENU && !isTappxBannerVisible)
        {
            ShowTappxBanner(true);
        }
    }

    private void ShowTappxBanner(bool value)
    {
        if (value)
        {
            TappxManagerUnity.instance.show(TappxSDK.TappxSettings.POSITION_BANNER.BOTTOM, false);
        }
        else
        { 
            TappxManagerUnity.instance.hide();
        }
        isTappxBannerVisible = value;
    }

    public void SetGameState(GameState newState)
    {
        if (currentState != newState)
        {
            DisableInput();
            switch (newState)
            {
                case GameState.MAIN_MENU:
                    if (ServicesConfigurationInitialized && ServicesConfiguration.enable_tappx && !isTappxBannerVisible)
                    {
                        ShowTappxBanner(true);
                    }

                    if (ScreenManager.Instance.currentGUIScreen.screenDefinition == ScreenDefinitions.HIGHSCORES)
                    {
                        animator.SetBool("highscores", false);
                    }

                    ShowScreen(ScreenDefinitions.MAIN_MENU);

                    AudioMaster.instance.StopAll(false);
                    AudioMaster.instance.PlayMusic(SoundDefinitions.THEME_MAINMENU);

                    rotator.EraseAllPins();

                    if (currentState != GameState.HIGH_SCORES && currentState != GameState.NONE)
                    {
                        animator.SetTrigger("menu");
                    }
                    break;
                case GameState.GOTO_PLAY:
                    if (ServicesConfigurationInitialized && ServicesConfiguration.enable_tappx)
                    {
                        ShowTappxBanner(false);
                    }

                    animator.SetTrigger("start");
                    nextGameState = GameState.PLAYING;
                    break;
                case GameState.PLAYING:
                    ScreenManager.Instance.HideScreen();
                    StartCoroutine(WaitUntilPlayingState());			
                    break;
                case GameState.GAME_OVER:
                    AudioMaster.instance.Play(SoundDefinitions.END_FX);
                    animator.SetTrigger("exit");
                    GameOverPoints.text = Score.ToString();

                    if (ServicesConfigurationInitialized && ServicesConfiguration.enable_tappx && !isTappxBannerVisible)
                    {
                        ShowTappxBanner(true);
                    }
                    break;
                case GameState.HIGH_SCORES:
                    animator.SetBool("highscores", true);
                    ShowScreen(ScreenDefinitions.HIGHSCORES);
                    break;
            }

            currentState = newState;

            if (nextGameState != GameState.NONE)
            {
                GameState theNextState = nextGameState;
                nextGameState = GameState.NONE;	
                SetGameState(theNextState);
            }
        }
    }

    IEnumerator WaitUntilPlayingState()
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("PlayingGame"))
        {
            yield return null;
        }
        ResetGame();
        ShowScreen(ScreenDefinitions.GAME, BeginGame);
    }

    void BeginGame()
    {
        AnalyticsSender.SendCustomAnalitycs("gameStart", new Dictionary<string, object>());
        spawner.SpawnPin();
        if (OnBeginGame != null)
            OnBeginGame();
		
        EnableInput();
    }

    public void ShowGameOverScreen()
    {
        ShowScreen(ScreenDefinitions.GAME_OVER, ShowSendScoreForm);
    }

    public void ShowScreen(ScreenDefinitions screenDef, UIScreen.Callback TheCallback = null)
    {
        DisableInput();

        if (TheCallback == null)
        {
            TheCallback = EnableInput;
        }

        ScreenManager.Instance.ShowScreen(screenDef, TheCallback);
    }

    public void ShowSendScoreForm()
    {
        if (Score > 0)
        {
            InputNameScreen.Instance.OpenWindow(EnableInput);
        }
    }

    public void EnableInput()
    {
        isInputEnabled = true;
    }

    public void DisableInput()
    {
        isInputEnabled = false;
    }

    public void StartGame()
    {		
        SetGameState(GameState.GOTO_PLAY);
    }

    public void GameOver()
    {
        if (isGameOver)
            return;
		
        rotator.enabled = false;
        spawner.enabled = false;
        isGameOver = true;
		
        SetGameState(GameState.GAME_OVER);
        AnalyticsSender.SendCustomAnalitycs("gameOver", new Dictionary<string, object>
            {
                { "score", Score },
                { "pinsCount", spawner.PinsCount }

            }
        );
    }

    public void BackToMainMenu()
    {
        SetGameState(GameState.MAIN_MENU);
    }

    void ResetGame()
    {
        CurrentLevel = initialLevel;
        currentMusic = 0;
        isGamePaused = false;
        isGameOver = false;

        scoreLabel.text = "0";
        Score = 0;

        pointsRequiredToLevelUpQueue = new Queue<int>(pointsRequiredToLevelUp);
        difficultyStepsQueue = new Queue<DifficultType>(difficultySteps);
				
        spawner.enabled = true;
        spawner.Reset();
        rotator.enabled = true;
        rotator.Reset();

        AudioMaster.instance.StopAll(false);
        AudioMaster.instance.PlayMusic(musics[currentMusic]);
		
        EnableInput();
    }

    public void ShowHighscores()
    {		
        SetGameState(GameState.HIGH_SCORES);
    }

    public void ShowMainScreenVideoService()
    {
        UnityAds.Instance.ShowAds(ServicesConfiguration.mainscreen_video_is_rewarded);
    }

    public void ThrowCurrentPin()
    {
        if (!isGamePaused)
        {
            spawner.ThrowCurrentPin();
        }
    }

    public void CheckDifficulty()
    {
        if (CanLevelUp(Score))
        {
            LevelUp();
        }
        // Change main theme
        if (Score % 50 == 0)
        {
            AudioMaster.instance.StopSound(musics[currentMusic], true);
            currentMusic++;

            if (currentMusic > musics.Length - 1)
            {
                currentMusic = 0;
            }

            AudioMaster.instance.PlayMusic(musics[currentMusic]);
        }
    }

    bool CanLevelUp(int score)
    {
        if (pointsRequiredToLevelUpQueue.Count == 0)
        {
            return score % 5 == 0;
        }
        else
        {
            return score == pointsRequiredToLevelUpQueue.Peek();
        }
    }

    void LevelUp()
    {
        DifficultType difficult = DifficultType.NONE;

        CurrentLevel++;

        if (difficultyStepsQueue.Count == 0)
        {
            if (score % 15 == 0)
            {
                difficult = !rotator.canUseCrazySpeed ? DifficultType.SWITCH_CRAZY_SPEED : DifficultType.SWITCH_CRAZY_SPEED_CANCEL;
            }
            else if (score % 10 == 0)
            {
                difficult = DifficultType.SPEEDUP;
            }
            else if (score % 5 == 0)
            {
                difficult = !rotator.canInverseDir ? DifficultType.SWITCH_REVERSE : DifficultType.SWITCH_REVERSE_CANCEL;
            }
        }
        else
        {
            pointsRequiredToLevelUpQueue.Dequeue();
            difficult = difficultyStepsQueue.Dequeue();
        }

        switch (difficult)
        {
            case DifficultType.MORE_COLORS:
                spawner.AddColorsInGame(1);
                AudioMaster.instance.Play(SoundDefinitions.SFX_SPEED);
                break;
            case DifficultType.SPEEDUP:
                rotator.RotationSpeed += rotator.RotationSpeed < 200 ? 20 : 10;
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_7);
                break;
            case DifficultType.SWITCH_REVERSE:
                rotator.StartInverseDirection();
                AudioMaster.instance.Play(SoundDefinitions.SFX_REVERSE);
                break;
            case DifficultType.SWITCH_REVERSE_CANCEL:
                rotator.StopInverseDirection();
                AudioMaster.instance.Play(SoundDefinitions.SFX_REVERSE);
                break;			
            case DifficultType.SWITCH_CRAZY_SPEED:
                rotator.StartCrazySpeed();
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_10);
                break;
            case DifficultType.SWITCH_CRAZY_SPEED_CANCEL:
                rotator.StopCrazySpeed();
                AudioMaster.instance.Play(SoundDefinitions.SCRATCH_10);
                break;
        }
        levelUp.Show(difficult);
    }

    public void PauseGame(bool pause)
    {
        DisableInput();
        if (pause)
        {
            PauseScreen.Instance.OpenWindow(EnableInput);
        }
        else
        {
            PauseScreen.Instance.CloseWindow(EnableInput);	
        }
        isGamePaused = pause;
    }

    public void ExitToMainMenu()
    {
        animator.SetTrigger("exit");
        BackToMainMenu();
        PauseScreen.Instance.CloseWindow();
    }
}