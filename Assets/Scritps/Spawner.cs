using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class Spawner : MonoBehaviour {

	public GameObject needlePrefab;

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
	}

	void Update() {
		if (Input.GetButtonDown("Fire1")) {
			SpawnNeedle();
		}
	}

	public void SetSpawnerType(GameType value) {
		currentSpawnerType = value;
	}

	void SpawnNeedle () {
		GameObject pin = Instantiate(needlePrefab, transform.position, transform.rotation);

		switch(currentSpawnerType) {
		case GameType.Free:
			pin.name = "Pin " + (GameManager.instance.score + 1).ToString();
			break;
		case GameType.MatchThree:
			pin.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			currentColor = nextColor;
			nextColor = Random.Range (0, Mathf.Min(GameManager.instance.currentLevel +3, posibleColors.Length));
			nextNeedle.color = posibleColors[nextColor];
			pin.name = "Needle type" + currentColor.ToString();
			pin.GetComponent<SpriteRenderer>().color = posibleColors[currentColor];
			break;
		}

	}
}
