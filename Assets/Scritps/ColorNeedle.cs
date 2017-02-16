using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorNeedle : MonoBehaviour {
	public const float TIME_TO_DESTROY = 0.2f;

	public float speed = 20f;

	private Rigidbody2D rb;
	private CircleCollider2D cc;

	public bool isShooted = false;
	public bool isPinned = false;
	private bool drawSpear = false;

	private Transform rotator;
	private LineRenderer lr;
	private SpriteRenderer sr;

	void Awake() {
		rb = GetComponent<Rigidbody2D>();
		cc = GetComponent<CircleCollider2D>();
		sr = GetComponent<SpriteRenderer>();
		rotator = GameObject.FindGameObjectWithTag("Rotator").transform;
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
			//Debug.Log ("<color=green>Procession Collision {" + name + "-" +  col.gameObject.name + "}</color>");

			//transform.SetParent(rotator);
			CheckCollisionWithPinnedNeedles();
			// Si despues de colocada, está a la suficiente distancia del rotator... la fijamos al rotator
			if (col.tag == "Rotator") {
				CheckDistanceToRotator();
			}
			else {
				SetNeedleAsPinned(col.gameObject);
			}
		}
	}
	
	bool CheckDistanceToRotator() {
		if (!isPinned) {
			float sqrDist = (rotator.position - transform.position).sqrMagnitude;
			if ( sqrDist <= (GameManager.instance.distanceOfPins * GameManager.instance.distanceOfPins) ) {
				CheckCollisionWithPinnedNeedles();
				FixPosition(rotator, GameManager.instance.distanceOfPins);
				drawSpear = true;
				SetNeedleAsPinned();
				return true;
			}
		}
		return false;
	}

	void CheckCollisionWithPinnedNeedles() {
		foreach (GameObject cn in GameObject.FindGameObjectsWithTag("Pin")){
			if ( cn.GetComponent<ColorNeedle>().isPinned ) {
				// dist es la suma de los radios => dist = (radio1 * escala_objeto1) + (radio2 * escala_objeto2)
				float dist = (cn.transform.lossyScale.x * cn.GetComponent<CircleCollider2D>().radius) + (transform.lossyScale.x * cc.radius);
				if ( (transform.position - cn.transform.position).sqrMagnitude <= (dist * dist)) {
					FixPosition(cn.transform, dist);
				}
			}
		}
	}

	void SetNeedleAsPinned(GameObject lastTouchedNeedle = null) {
		isPinned = true;
		GetComponent<CircleCollider2D>().isTrigger = false;
		transform.SetParent(rotator);

		GameManager.instance.EvaluatePinnedNeedle(gameObject, lastTouchedNeedle);
		GameManager.instance.spawner.SpawnNeedle();
	}
	
	void FixPosition(Transform pinnedNeedle, float distOffset = 0f) {
		transform.position = pinnedNeedle.position + (transform.position - pinnedNeedle.position).normalized * distOffset;
	}

	void DrawSpear() {
		if (drawSpear ) {
			lr.numPositions = 2;
			lr.SetPosition(0, transform.position);
			lr.SetPosition(1, rotator.position);
		}
		else
			lr.numPositions = 0;
	}

	public void Autodestroy() {
		cc.enabled = false;
		drawSpear = false;
		StartCoroutine(AnimToDead());
	}

	public IEnumerator AnimToDead() {

		float t = 1f;
		while (t > 0f) {
			t -= Time.deltaTime/TIME_TO_DESTROY;
			transform.localScale *= 1.13f;
			sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, t);
			yield return null;
		}
		
		Destroy(gameObject);

	}
}
