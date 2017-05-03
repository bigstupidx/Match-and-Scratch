using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class Spawner : MonoBehaviour {
	[SerializeField]
	public const float MINIMUM_SPAWN_TIME = 0f;
	 
	public GameObject PinPrefab;
	public int nextColor;
	public int currentColor;
	public int colorsInGame;
	public Image nextPin;
	public int pinsCount;

	void Start() {}

	public void SpawnPin(float secondsDelay = 0) {
		StartCoroutine(Spawn(secondsDelay));
	}

	public void Reset() {
		pinsCount = 0;
		colorsInGame = 0;
		nextColor = GetNextColor ();
		nextPin.color = GameManager.instance.posibleColors[nextColor];
	}

	private IEnumerator Spawn (float secondsDelay = 0f) {
		GameObject pin;

		yield return new WaitForSeconds(secondsDelay);

		currentColor = nextColor;
		pin = Instantiate(PinPrefab, transform.position, transform.rotation);
		pin.GetComponent<Circumference>().colorType   = currentColor;
		pin.name = pinsCount + "-Type_" + currentColor.ToString();
		pin.GetComponent<SpriteRenderer>().color = GameManager.instance.posibleColors[currentColor];

		pinsCount++;

		nextColor = GetNextColor ();
		nextPin.color = GameManager.instance.posibleColors[nextColor];
	}

	int GetNextColor() {
		return Random.Range (0, Mathf.Min(Mathf.Max(0, colorsInGame +1), GameManager.instance.posibleColors.Length));
	}
}
