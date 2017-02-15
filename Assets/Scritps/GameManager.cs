using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameType {
	Free,
	MatchThree
}

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;
	public GameType gameType;

	public Rotator rotator;
	public Spawner spawner;

	public Animator animator;

	public int score = 0;

	public int currentLevel = 0;

	public float distanceOfPins = 8f;

	public Text scoreLabel;
	public Text maxScoreLabel;

	public bool gameHasEnded = false;

	/*** gameType = match-three ***/
	List<GameObject[]> colorGroups;
	/*** 		*	*	* 		***/

	void Awake() {
		if (instance == null) {
			instance = this;
		}
		else if (instance != this) {
			Destroy(gameObject);
		}

		int highscore = PlayerPrefs.GetInt("MaxScore");
		maxScoreLabel.text =  highscore == 0 ? "" : "Max: " + highscore.ToString();
		spawner.SetSpawnerType(gameType);
		switch (gameType) {
			case GameType.MatchThree:

				if (colorGroups == null)
					colorGroups = new List<GameObject[]>();

				colorGroups.Clear();

			break;
		}
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
		currentLevel = 0;
	}

	public void PinNeedle(GameObject needleToPin, GameObject lastTouchedNeedle = null) {
		if (lastTouchedNeedle == null) {
			GameObject[] arr = new GameObject[]{needleToPin};
			colorGroups.Add(arr);
		}
		else {
			// Buscamos el grupo en el que ya esté el ultimo objeto qeu hemos tocado
			int id = -1;
			colorGroups.ForEach(g => {
				for(int i = 0; i < g.Length - 1; i++) {
					if (lastTouchedNeedle.name == g[i].name)
						id = i;
				}
			});
			// Si hemos localizado un grupo en el que ya existe el último tocado, metemos el nuevo es ese grupo.
			if (id >= 0) {
				colorGroups[id][colorGroups[id].Length] = needleToPin;
			}
			else{// Si no hemos encontrado el ultimo objeto colisionado en ningún grupo... creamos unos nuevo con el objeto a pinear
				colorGroups.Add(new GameObject[]{needleToPin});
			}
		}

		EvaluateColorGroups();
	}

	void EvaluateColorGroups() {
		//TODO: si encontramos un grupo de mas de dos miembreo del mismo color, eliminamos el grupo y los objetos que contiene.
		string l = "";
		for(int i = 0; i < colorGroups.Count - 1; i++) {
			for( int j = 0; j < colorGroups[i].Length; j++) {
				l += (colorGroups[i][j].name);
			}
			l += "\n";
		}
		Debug.Log (l);

	}

}
