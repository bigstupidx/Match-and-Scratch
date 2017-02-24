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
	public int colorsCountRoof;

	public Text scoreLabel;
	public Text maxScoreLabel;

	public bool gameHasEnded = false;

	private int score = 0;
	public int Score {get; set;}
	private int lastScore = 0;

	/*** gameType = match-three ***/
	List<List<GameObject>> colorGroups;
	/*** 		*	*	* 		***/

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

		if (colorGroups == null) {
			colorGroups = new List<List<GameObject>>();
		}
		colorGroups.Clear();

		colorsCountRoof = posibleColors.Length;
		spawner.SpawnNeedle();
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
		colorsCountRoof = currentLevel + 3;
	}

	public void EvaluatePinnedNeedle(GameObject needleToPin, List<GameObject> touchedPins) {
		/*
		if (needleDestiny == null) { // Si la colisión es con el rotator
			CreateColorGroup(needleToPin);
		}
		else if ( needleToPin.name.Split('-')[1] != needleDestiny.name.Split('-')[1] ){// Si la colisión es con una burbuja de distinto color
			CreateColorGroup(needleToPin);
		}
		else {
			// Buscamos el grupo en el que ya esté el ultimo objeto que hemos tocado...
			int colorGroupId = -1;
			for (int i = 0; i < colorGroups.Count && colorGroupId == -1; i++){
				if (colorGroups[i].Find(c => c.name == needleDestiny.name) ) {
					colorGroupId = i;
				}
			}

			if (colorGroupId >= 0) {// ... Si hemos localizado un grupo en el que ya existe el último tocado, metemos el nuevo es ese grupo.
				colorGroups[colorGroupId].Add(needleToPin);
			}
			else{// ... Si no hemos encontrado el ultimo objeto colisionado en ningún grupo... creamos unos nuevo con el objeto a pinear
				CreateColorGroup(needleToPin);
				Debug.Log ("<color=red>Error WTF(1): Esto no debería suceder</color>");
			}
		}
		*/
		EvaluateColorGroups();
		CheckDifficulty();

	}

	void CheckDifficulty() {
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

	void CreateColorGroup(GameObject go) {
		List<GameObject> goList = new List<GameObject>();
		goList.Add(go);
		colorGroups.Add(goList);
	}

	void CreateColorGroup(List<GameObject> gos) {
		List<GameObject> goList = new List<GameObject>(gos);
		colorGroups.Add(goList);
	}

	void EvaluateColorGroups() {
		List<int> groupsToDestroy = new List<int>();

		// Si encontramos un grupo de mas de dos miembreo del mismo color...
		for(int i = 0; i < colorGroups.Count; i++) {
			if (colorGroups[i].Count > 2) {
				// ...eliminamos el grupo.
				groupsToDestroy.Add(i);
			}			
		}

		if (groupsToDestroy.Count <= 0){
			spawner.SpawnNeedle();
		}
		else {
			foreach(int i in groupsToDestroy) {
				// ...los objetos que contiene...
				StartCoroutine(DestroyElementsFromGroup(colorGroups[i]));
				colorGroups.RemoveAt(i);
				score++;
			}
			StartCoroutine(SpawnNeedleWithDelay(groupsToDestroy.Count * ColorNeedle.TIME_TO_DESTROY));
		}
	}

	IEnumerator DestroyElementsFromGroup(List<GameObject> listGo) {
		for( int i = 0; i < listGo.Count; i++) {
			listGo[i].GetComponent<ColorNeedle>().Autodestroy();
			yield return new WaitForSeconds(ColorNeedle.TIME_TO_DESTROY/listGo.Count);
		}
	}

	IEnumerator SpawnNeedleWithDelay(float delay) {
		yield return new WaitForSeconds(delay);
		spawner.SpawnNeedle();
	}



	void PrintColorGroupsLog() {
		string log = "";
		for(int i = 0; i < colorGroups.Count; i++) {
			for( int j = 0; j < colorGroups[i].Count; j++) {
				log += (colorGroups[i][j].name + " ");
			}
			log += "\n";
		}
		Debug.Log (log);
	}
}
