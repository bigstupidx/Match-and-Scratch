using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class Spawner : MonoBehaviour {
	[SerializeField]
	public const float MINIMUM_SPAWN_TIME = 0f;
	public int MAX_COLORS_IN_GAME = 7;
	 
	public GameObject PinPrefab;
	public int nextColor;
	public int currentColor;
	public int colorsInGame;
	public Image nextPin;
	public int pinsCount;

	public GameObject lastSpawnedPin;

	void Start() {}

	public void SpawnPin(float secondsDelay = 0) {
		StartCoroutine(Spawn(secondsDelay));
	}

	public void AddColorsInGame(int inc) {
		colorsInGame = Mathf.Min (colorsInGame + inc, MAX_COLORS_IN_GAME - 1);
	}

	public void Reset() {
		pinsCount = 0;
		colorsInGame = 0;
		nextColor = GetNextColor ();
		nextPin.color = GameManager.instance.posibleColors[nextColor];
	}

	private IEnumerator Spawn (float secondsDelay = 0f) {

		yield return new WaitForSeconds(secondsDelay);

		currentColor = nextColor;
		lastSpawnedPin = Instantiate(PinPrefab, transform.position, transform.rotation);
		lastSpawnedPin.GetComponent<Circumference>().colorType   = currentColor;
		lastSpawnedPin.name = pinsCount + "-Type_" + currentColor.ToString();
		lastSpawnedPin.GetComponent<SpriteRenderer>().color = GameManager.instance.posibleColors[currentColor];

		pinsCount++;

		nextColor = GetNextColor ();
		nextPin.color = GameManager.instance.posibleColors[nextColor];
	}

	int GetNextColor() {
		return Random.Range (0, Mathf.Min(Mathf.Max(0, colorsInGame +1), GameManager.instance.posibleColors.Length));
	}

	public void ThrowCurrentPin() {
		if (GameManager.instance.currentGamePlayState == GamePlayState.Normal && lastSpawnedPin != null) {
			//Debug.Log ("Throw Pin : " + lastSpawnedPin.name);
			lastSpawnedPin.GetComponent<Pin> ().isShooted = true;
		}
	}
}
