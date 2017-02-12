using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

	public GameObject needlePrefab;

	void Update() {
		if (Input.GetButtonDown("Fire1")) {
			SpawnPin();
		}
	}

	void SpawnPin () {
		GameObject pin = Instantiate(needlePrefab, transform.position, transform.rotation);
		pin.name = "Pin " + (GameManager.instance.score + 1).ToString();
		Debug.Log ("Instanciado " + pin.name);
	}
}
