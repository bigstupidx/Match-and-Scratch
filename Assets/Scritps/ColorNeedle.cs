using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class ExtensionMethods {
	
	public static string ListaAsString(this List<GameObject> list) {
		string ret = "";
		for (int i = 0; i < list.Count; i++) {
			ret += list[i].name;
			if (i < list.Count -1)
				ret += ", ";
		}
		return ret;
	}
}

public class ColorNeedle : MonoBehaviour {
	public const float TIME_TO_DESTROY = 0.2f;

	private Rigidbody2D rb;
	private CircleCollider2D cc;

	public float speed = 20f;
	public bool isShooted = false;
	public bool isPinned = false;
	public bool drawSpear = false;

	private bool canEvaluateCollision = false;
	private List<GameObject> collisions = new List<GameObject>();
	
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

	void OnEnable() {
		if (lr == null)
			lr = GetComponent<LineRenderer>();
	}

	void SetupLine() {
		Color parentColor = GetComponent<SpriteRenderer>().color;
		lr = gameObject.AddComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Sprites/Default"));
		lr.startColor = parentColor;
		lr.endColor = parentColor;
		lr.startWidth = 0.05f;
		lr.endWidth = 0.05f;
	}

	void Update () {
		if (Input.GetButtonDown("Fire1")) {
			isShooted = true;
			//canEvaluateCollision = true;
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
			if (canEvaluateCollision) {
				Debug.Log ("<color=yellow> " + gameObject.name + "colisiona con: " + collisions.Count.ToString() + "[" + collisions.ListaAsString() + "]</color>");

				PositionCorrection();
				SetNeedleAsPinned();
				canEvaluateCollision = false;

				//TODO: uncomment this
				//GameManager.instance.EvaluatePinnedNeedle(gameObject, collisions);

				//TODO: comment or delete this
				GameManager.instance.spawner.SpawnNeedle();
			}
		}
	}

	void OnTriggerEnter2D (Collider2D col) {
		if (!isPinned && isShooted) {
			if (!collisions.Contains (col.gameObject)) {
				collisions.Add(col.gameObject);
				canEvaluateCollision = true;
			}
		}
/*
		if (!isPinned && isShooted) {
			//Debug.Log ("<color=green>Procession Collision {" + name + "-" +  col.gameObject.name + "}</color>");
			if (col.gameObject.tag == "Pin") {
				if (col.gameObject.name.Split('-')[1] != name.Split('-')[1]){
					CheckCollisionWithPinnedNeedles();
					GameManager.instance.EndGame();
					isPinned = true;
					return;
				}
				else {
					CheckCollisionWithPinnedNeedles();
					SetNeedleAsPinned(col.gameObject);
				}
			}
			// Si despues de colocada, está a la suficiente distancia del rotator... la fijamos al rotator
			/ * else if (col.tag == "Rotator") {
				CheckDistanceToRotator();
			} * /
		}
*/
	}
	
	void CheckDistanceToRotator() {
		if (!isPinned) {
			float sqrDist = (rotator.position - transform.position).sqrMagnitude;
			if ( sqrDist <= (GameManager.instance.distanceOfPins * GameManager.instance.distanceOfPins) ) {
				if (collisions.Count == 0) {
					if (!collisions.Contains (rotator.gameObject)) {
						collisions.Add(rotator.gameObject);
						FixPosition(rotator, GameManager.instance.distanceOfPins);
						drawSpear = true;
						SetNeedleAsPinned();
						canEvaluateCollision = true;
					}
				}
			}
		}
	}
/*
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
*/
	void SetNeedleAsPinned(GameObject lastTouchedNeedle = null) {
		isPinned = true;
		GetComponent<CircleCollider2D>().isTrigger = false;
		transform.SetParent(rotator);

		//GameManager.instance.EvaluatePinnedNeedle(gameObject, lastTouchedNeedle);
	}
	
	void PositionCorrection() {
		switch (collisions.Count) {
			case 1:
				FixPosition( collisions[0].transform, GetDistanceBetween(collisions[0], gameObject));
			break;
			case 2:
					
				Vector2 A = collisions[0].transform.position;
				Vector2 B = collisions[1].transform.position;

				float La = GetDistanceBetween(collisions[0], collisions[1]); // calcular la distancia entre los dos puntos (no sumando los radios).
				float Lb = GetDistanceBetween(collisions[1], gameObject);    // calcular la distancia entre los dos puntos (no sumando los radios).
				float Lc = GetDistanceBetween(gameObject, collisions[0]);    // calcular la distancia entre los dos puntos (no sumando los radios).

				float arCos_AB_AC = (Lb*Lb + Lc*Lc - La*La) / 2*Lb*Lc;
				float a = Mathf.Cos(arCos_AB_AC);

				float arSin_AV_x = (A.y - B.y) / La;
				float b = Mathf.Sin(arSin_AV_x);

				transform.position = new Vector3 ( A.x + (Lc * Mathf.Cos(a + b)), A.y + (Lc * Mathf.Sin(a + b)), 0 );
			break;
			default:
				Debug.Log("<color=red> Exceso de colisiones: " + collisions.Count + " [" + collisions.ListaAsString() + "]</color>");
			break;
		}
	}

	float GetDistanceBetween(GameObject A, GameObject B) {

		float distanceBetween =  (A.transform.lossyScale.x * A.GetComponent<CircleCollider2D>().radius) + (B.transform.lossyScale.x * B.GetComponent<CircleCollider2D>().radius);		
		return A.tag == "Rotator" ? GameManager.instance.distanceOfPins : distanceBetween;
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
		drawSpear = false;
		cc.enabled = false;
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
