using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle : MonoBehaviour {
	public float speed = 20f;

	private Rigidbody2D rb;	
	
	private bool isPinned = false;
	private bool drawSpear = false;

	private Transform rotator;
	private SpriteRenderer sr;
	private LineRenderer lr;

	void Awake() {
		rb = GetComponent<Rigidbody2D>();
		rotator = GameObject.FindGameObjectWithTag("Rotator").transform;
		sr  = GetComponent<SpriteRenderer>();
		SetupLine();
	}

	void SetupLine() {
		lr = gameObject.AddComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.startColor = Color.black;
		lr.endColor = Color.black;
		lr.startWidth = 0.05f;
		lr.endWidth = 0.05f;
		//lr.useWorldSpace = true;
	}
		
	void Update() {

	}

	void LateUpdate() {
		
		DrawSpear();
	}

	void FixedUpdate() {
		if (!isPinned) {
			rb.MovePosition(rb.position + Vector2.up * speed * Time.fixedDeltaTime);
			CheckDistanceToRotator();
		}
	}

	void OnTriggerEnter2D (Collider2D col) {
		if (col.tag == "Pin") {
			if (col.GetComponent<Needle>().isPinned) {
				isPinned = true;
				GameManager.instance.EndGame();
				Debug.Log ("<color=red>Colisión: " + name + " " +  col.gameObject.name + "</color>");
			}
		}
	}

	void CheckDistanceToRotator() {
		float dist = (rotator.position - transform.position).magnitude;
		Debug.Log("Distance to rotator: " + dist.ToString());

		if (!isPinned) {
			if ( dist <= GameManager.instance.distanceOfPins ) {

				FixPosition();

				transform.SetParent(rotator);

				if (!GameManager.instance.gameHasEnded)
					GameManager.instance.score++;

				sr.color = Color.black;

				drawSpear = true;
				isPinned = true;
			}
		}
	}

	void FixPosition() {
		transform.position = rotator.position + (transform.position - rotator.position).normalized * GameManager.instance.distanceOfPins;
	}

	void DrawSpear() {
		if (drawSpear ) {
			lr.numPositions = 2;
			lr.SetPosition(0, transform.position);//Vector3.zero);
			lr.SetPosition(1, rotator.position);//transform.worldToLocalMatrix.MultiplyPoint(rotator.position));
		}
		else
			lr.numPositions = 0;
	}
}
