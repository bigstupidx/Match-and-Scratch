using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;

	public Color[] posibleColors;

	public Rotator rotator;
	public Spawner spawner;

	public Animator animator;

	public GameObject GameOverScreen;
	public Text levelUpText;

	public int currentLevel = 0;
	//public int colorsCountRoof;

	public Text scoreLabel;
	public Text maxScoreLabel;

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

		//colorsCountRoof = posibleColors.Length;
		spawner.SpawnPin(1f);
	}

	public void EndGame() {
		if (gameHasEnded)
			return;

		rotator.enabled = false;
		spawner.enabled = false;

		if (score > PlayerPrefs.GetInt("MaxScore")) {
			PlayerPrefs.SetInt("MaxScore", score);
		}

		gameHasEnded = true;
		animator.SetTrigger ("EndGame");
		spawner.enabled = false;
	}

	public void ShowGameOverScreen(){
		GameOverScreen.SetActive(true);
	}

	public void RestartLevel () {
		//TODO: reiniciar el level sin recargar la escena
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex  );		
		GameOverScreen.SetActive(false);
		score = 0;
		gameHasEnded = false;
		currentLevel = 0;
		spawner.enabled = true;
	}

	void Update() {
		scoreLabel.text = score.ToString();
		//colorsCountRoof = currentLevel + 3;
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
