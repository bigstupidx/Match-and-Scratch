using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle : MonoBehaviour {
	public float speed = 20f;
	private Rigidbody2D rb;	

	public bool isShooted = false;
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
		Color parentColor = GetComponent<SpriteRenderer>().color;
		lr = gameObject.AddComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.startColor = parentColor;
		lr.endColor = parentColor;
		lr.startWidth = 0.05f;
		lr.endWidth = 0.05f;
	}

	void Update() {
		if (Input.GetButtonDown("Fire1")) {
			isShooted = true;
		}
	}

	void LateUpdate() {		
		DrawSpear();
	}

	void FixedUpdate() {
		if (isShooted) {
			if (!isPinned) {
				rb.MovePosition(rb.position + Vector2.up * speed * Time.fixedDeltaTime);
				CheckDistanceToRotator();
			}
		}
	}

	void OnTriggerEnter2D (Collider2D col) {
		if (!isPinned && isShooted) {
			if (col.tag == "Pin") {
				isPinned = true;
				GameManager.instance.EndGame();
			}
		}
	}

	void CheckDistanceToRotator() {
		float dist = (rotator.position - transform.position).magnitude;
		Debug.Log("Distance to rotator: " + dist.ToString());

		if (!isPinned) {
			if ( dist <= GameManager.instance.distanceOfPins ) {

				FixPosition(rotator, GameManager.instance.distanceOfPins);
				transform.SetParent(rotator);

				if (!GameManager.instance.gameHasEnded)
					GameManager.instance.score++;

				if (GameManager.instance.gameType == GameType.Free)
					sr.color = Color.black;

				drawSpear = true;
				isPinned = true;
				GetComponent<CircleCollider2D>().isTrigger = false;
				GameManager.instance.spawner.SpawnNeedle();
			}
		}
	}

	void FixPosition(Transform stickyObject, float distOffset = 0f) {
		transform.position = stickyObject.position + (transform.position - stickyObject.position).normalized * distOffset;
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
