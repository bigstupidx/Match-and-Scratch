using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;

	public Rotator rotator;
	public Spawner spawner;

	public Animator animator;

	//private int maxScore = 0;
	public int score = 0;

	public float distanceOfPins = 8f;

	public Text scoreLabel;
	public Text maxScoreLabel;

	public bool gameHasEnded = false;

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
	}

	void Update() {
		scoreLabel.text = score.ToString();
	}

	public void RestartLevel () {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex  );
		score = 0;
		gameHasEnded = false;
	}


}
