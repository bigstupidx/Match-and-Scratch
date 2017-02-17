using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin : MonoBehaviour {

	public float speed = 20f;
	Rigidbody2D rb;


	private bool isPinned = false;


	void Awake() {
		rb = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate() {
		if (!isPinned) {
			rb.MovePosition(rb.position + Vector2.up * speed * Time.fixedDeltaTime);
		}
	}

	void OnTriggerEnter2D (Collider2D col) {

		if (col.tag == "Rotator") {
			transform.SetParent(col.transform);

			if (!GameManager.instance.gameHasEnded)
				GameManager.instance.Score++;

			GetComponent<CircleCollider2D>().isTrigger = false;
			isPinned = true;
		}

		if (col.tag == "Pin") {
			isPinned = true;
			switch (GameManager.instance.gameType){
				case GameType.Free:
					GameManager.instance.EndGame();
				break;
				case GameType.MatchThree:

				break;
			}

			Debug.Log ("<color=red>Colisión: " + name + " " +  col.gameObject.name + "</color>");
		}

	}
}
