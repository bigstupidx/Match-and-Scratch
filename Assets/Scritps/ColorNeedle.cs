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
		}
	}

	void LateUpdate() {		
		DrawSpear();
	}

	void FixedUpdate() {
		if (isShooted && !isPinned)
			rb.MovePosition(rb.position + Vector2.up * speed * Time.fixedDeltaTime);

		if (canEvaluateCollision) {
			if (collisions.Count <= 0) {
				canEvaluateCollision = false;
				SetNeedleAsPinned();
				GameManager.instance.spawner.SpawnNeedle();
			}
			PositionCorrection();
			Debug.Log ("<color=yellow> " + gameObject.name + "colisiona con: " + collisions.Count.ToString() + "[" + collisions.ListaAsString() + "]</color>");

			//TODO: uncomment this
			//GameManager.instance.EvaluatePinnedNeedle(gameObject, collisions);
			
			//TODO: comment or delete this

		}
	}

	void OnTriggerEnter2D (Collider2D col) {
		if ( isShooted && !isPinned) {
			if (col.gameObject.tag == "Rotator") {
				AddToMyCollisions(col.gameObject);
			}
			else if (col.gameObject.tag == "Pin") {
				if (col.gameObject.GetComponent<ColorNeedle>().isShooted && col.gameObject.GetComponent<ColorNeedle>().isPinned) {
					AddToMyCollisions(col.gameObject);
				}
			}
		}
	}

	void AddToMyCollisions(GameObject go) {
		if (!collisions.Contains(go)) {
			collisions.Add(go);
			canEvaluateCollision = true;
		}
	}

	void SetNeedleAsPinned(GameObject lastTouchedNeedle = null) {
		isPinned = true;
		GetComponent<CircleCollider2D>().isTrigger = false;
		transform.SetParent(rotator);
		//GameManager.instance.EvaluatePinnedNeedle(gameObject, lastTouchedNeedle);
	}
	
	void PositionCorrection() {
		switch (collisions.Count) {
			case 1:
				FixPosition( collisions[0].transform, SumRadius(collisions[0], gameObject));
				if ( collisions[0].tag == "Rotator" ) {
					drawSpear = true;
				}
				collisions.RemoveAt(0);
			break;
			case 2:					
				Vector2 A = collisions[0].transform.position;
				Vector2 B = collisions[1].transform.position;

				float La = Vector3.Distance(collisions[0].transform.position, collisions[1].transform.position); // calcular la distancia entre los dos puntos (no sumando los radios).
				float Lb = SumRadius(collisions[1], gameObject); //SumRadius(collisions[1], gameObject);    // calcular la distancia entre los dos puntos (no sumando los radios).
				float Lc = SumRadius(gameObject, collisions[0]); //SumRadius(gameObject, collisions[0]);    // calcular la distancia entre los dos puntos (no sumando los radios).

				float arCos_AB_AC = (Lb*Lb + Lc*Lc - La*La) / 2*Lb*Lc;
				float a = Mathf.Cos(arCos_AB_AC);

				float arSin_AV_x = (A.y - B.y) / La;
				float b = Mathf.Sin(arSin_AV_x);

				transform.position = new Vector3 ( A.x + (Lc * Mathf.Cos(a - b)), A.y + (Lc * Mathf.Sin(a - b)), 0 );
				collisions.RemoveAt(1);
				collisions.RemoveAt(0);
			break;
			default:
				Debug.Log("<color=red> Exceso de colisiones: " + collisions.Count + " [" + collisions.ListaAsString() + "]</color>");
			break;
		}
	}

	float SumRadius(GameObject A, GameObject B) {
		return  (A.transform.lossyScale.x * A.GetComponent<CircleCollider2D>().radius) + (B.transform.lossyScale.x * B.GetComponent<CircleCollider2D>().radius);		
	}

	void FixPosition(Transform pinnedNeedle, float distOffset = 0f) {
		transform.position = pinnedNeedle.position + (transform.position - pinnedNeedle.position).normalized * (distOffset);
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
