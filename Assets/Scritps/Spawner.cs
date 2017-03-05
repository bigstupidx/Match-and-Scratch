using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class Spawner : MonoBehaviour {
	public GameObject PinPrefab;

	/*** gameType = match-three ***/

	public int nextColor;
	public int currentColor;
	public Image nextPin;
	public int pinsCount = 0;

	/*** gameType = match-three ***/

	void Start() {
		nextColor = Random.Range (0, Mathf.Min(GameManager.instance.currentLevel +1, GameManager.instance.posibleColors.Length));
		nextPin.color = GameManager.instance.posibleColors[nextColor];		
	}

	public void SpawnPin(float secondsDelay = 0) {
		StartCoroutine(Spawn(secondsDelay));
	}

	private IEnumerator Spawn (float secondsDelay = 0f) {
		GameObject pin;

		yield return new WaitForSeconds(secondsDelay);

		currentColor = nextColor;
		nextColor = Random.Range (0, Mathf.Min(GameManager.instance.currentLevel +2, GameManager.instance.posibleColors.Length));
		nextPin.color = GameManager.instance.posibleColors[nextColor];
		pin = Instantiate(PinPrefab, transform.position, transform.rotation);
		pin.GetComponent<Circumference>().colorType   = currentColor;
		pin.name = pinsCount + "-Type_" + currentColor.ToString();
		pin.GetComponent<SpriteRenderer>().color = GameManager.instance.posibleColors[currentColor];

		pinsCount++;
	}
}
