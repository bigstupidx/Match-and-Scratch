using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class Spawner : MonoBehaviour {
	public GameObject colorNeedlePrefab;

	/*** gameType = match-three ***/

	public int nextColor;
	public int currentColor;
	public Image nextNeedle;
	public int needlesSpawnedCount = 0;

	/*** gameType = match-three ***/

	void Start() {
		nextColor = Random.Range (0, Mathf.Min(GameManager.instance.colorsCountRoof, GameManager.instance.posibleColors.Length));
		nextNeedle.color = GameManager.instance.posibleColors[nextColor];		
	}

	void Update() {
	}

	public void SpawnNeedle () {
		GameObject pin;

		currentColor = nextColor;
		nextColor = Random.Range (0, Mathf.Min(GameManager.instance.currentLevel +3, GameManager.instance.posibleColors.Length));
		nextNeedle.color = GameManager.instance.posibleColors[nextColor];
		pin = Instantiate(colorNeedlePrefab, transform.position, transform.rotation);
		pin.name = needlesSpawnedCount + "-Type_" + currentColor.ToString();
		pin.GetComponent<SpriteRenderer>().color = GameManager.instance.posibleColors[currentColor];

		needlesSpawnedCount++;
	}
}
