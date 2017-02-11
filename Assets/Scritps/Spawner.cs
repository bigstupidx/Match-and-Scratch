using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

	public GameObject pinPrefab;

	void Update() {
		if (Input.GetButtonDown("Fire1")) {
			SpawnPin();
		}
	}

	void SpawnPin () {
		GameObject pin = Instantiate(pinPrefab, transform.position, transform.rotation);
		pin.name = "Pin " + GameManager.instance.score.ToString();
		Debug.Log ("Instanciado " + pin.name);
	}
}
