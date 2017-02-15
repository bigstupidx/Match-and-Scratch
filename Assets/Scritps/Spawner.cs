using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class Spawner : MonoBehaviour {

	public GameObject needlePrefab;
	public GameObject colorNeedlePrefab;

	/*** gameType = match-three ***/
	public Color[] posibleColors;
	public int nextColor;
	public int currentColor;
	public Image nextNeedle;

	private GameType currentSpawnerType;

	/*** gameType = match-three ***/

	void Start() {
		switch(currentSpawnerType) {
			case GameType.MatchThree:
				nextColor = Random.Range (0, Mathf.Min(GameManager.instance.currentLevel +3, posibleColors.Length));
				nextNeedle.color = posibleColors[nextColor];				
			break;
		}
		SpawnNeedle();
	}

	void Update() {
	}

	public void SetSpawnerType(GameType value) {
		currentSpawnerType = value;
	}

	public void SpawnNeedle () {
		GameObject pin;

		switch(currentSpawnerType) {
		case GameType.Free:
			pin = Instantiate(needlePrefab, transform.position, transform.rotation);
			pin.name = "Needle_" + (GameManager.instance.score + 1).ToString();
			break;
		case GameType.MatchThree:
			currentColor = nextColor;
			nextColor = Random.Range (0, Mathf.Min(GameManager.instance.currentLevel +3, posibleColors.Length));
			nextNeedle.color = posibleColors[nextColor];
			pin = Instantiate(colorNeedlePrefab, transform.position, transform.rotation);
			pin.name = "ColorNeedle_Type_" + currentColor.ToString();
			pin.GetComponent<SpriteRenderer>().color = posibleColors[currentColor];
			break;
		}
	}
}
