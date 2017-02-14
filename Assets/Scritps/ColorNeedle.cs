using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorNeedle : MonoBehaviour {
	public float speed = 20f;

	private Rigidbody2D rb;
	private CircleCollider2D cc;

	public bool isShooted = false;
	public bool isPinned = false;
	private bool drawSpear = false;

	private Transform rotator;
	private SpriteRenderer sr;
	private LineRenderer lr;

	void Awake() {
		rb = GetComponent<Rigidbody2D>();
		cc = GetComponent<CircleCollider2D>();
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

	void Update () {
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

			if (CheckDistanceToRotator()){
				FixPosition(col.gameObject.transform, GameManager.instance.distanceOfPins);
				OnTriggerEnter2D(col);
			}
			else {
				transform.SetParent(rotator);
				CheckCollisionWithPinnedNeedles();
				isPinned = true;
				GetComponent<CircleCollider2D>().isTrigger = false;
				GameManager.instance.spawner.SpawnNeedle();
			}
		}

		Debug.Log ("<color=red>Colisión: " + name + " " +  col.gameObject.name + "</color>");
	}
	
	bool CheckDistanceToRotator() {
		float dist = (rotator.position - transform.position).magnitude;
		Debug.Log("Distance to rotator: " + dist.ToString());

		if (!isPinned) {
			if ( dist <= GameManager.instance.distanceOfPins ) {


				CheckCollisionWithPinnedNeedles();
				FixPosition(rotator, GameManager.instance.distanceOfPins);

				transform.SetParent(rotator);

				if (GameManager.instance.gameType == GameType.Free)
					sr.color = Color.black;

				drawSpear = true;
				isPinned = true;
				GetComponent<CircleCollider2D>().isTrigger = false;
				GameManager.instance.spawner.SpawnNeedle();
				return true;
			}
		}
		return false;
	}

	void CheckCollisionWithPinnedNeedles() {
		foreach (GameObject cn in GameObject.FindGameObjectsWithTag("Pin")){
			if ( cn.GetComponent<ColorNeedle>().isPinned ) {
				// offset = (radio1 * escala_objeto1) + (radio2 * escala_objeto2)
				float offset = (cn.transform.lossyScale.x * cn.GetComponent<CircleCollider2D>().radius) + (transform.lossyScale.x * cc.radius);

				if ( (transform.position - cn.transform.position).sqrMagnitude < (offset * offset)) {
					FixPosition(cn.transform, offset);
				}
			}
		}
	}

	void FixPosition(Transform pinnedNeedle, float distOffset = 0f) {
		transform.position = pinnedNeedle.position + (transform.position - pinnedNeedle.position).normalized * distOffset;
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
