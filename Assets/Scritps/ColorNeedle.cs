using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods {	
	public static string ListaAsString(this List<GameObject> list) {
		string ret = "";
		for (int i = 0; i < list.Count; i++) {
			ret += list[i].name;
			if (i < list.Count -1) ret += ", ";
		}
		return ret;
	}
}

public class ColorNeedle : Circumference {
	public const float TIME_TO_DESTROY = 0.2f;

	public float speed = 20f;
	public bool isShooted = false;
	public bool isPinned = false;
	public bool drawSpear = false;

	private bool canEvaluateCollision = false;
	private List<GameObject> collisions = new List<GameObject>();	
	private Transform rotator;
	private Rigidbody2D rb;
	private LineRenderer lr;
	private SpriteRenderer sr;

	private List<GizmoToDraw> gizmosToDraw = new List<GizmoToDraw>();

	void OnDrawGizmos() {
		foreach (GizmoToDraw gtd in gizmosToDraw) {
			switch (gtd.gizmoType) {
			case GizmoType.line:
				Gizmos.color = gtd.color;
				Gizmos.DrawLine(gtd.from, gtd.to);
				break;
			case GizmoType.sphere:
				Gizmos.color = gtd.color;
				Gizmos.DrawWireSphere(gtd.from, gtd.size);
				break;
			}
		}
	}

	public override void Initialize() {
		rb = GetComponent<Rigidbody2D>();
		sr = GetComponent<SpriteRenderer>();
		rotator = GameObject.FindGameObjectWithTag("Rotator").transform;
		SetupLine();
	}

	void OnEnable() {
		if (lr == null)	lr = GetComponent<LineRenderer>();
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

	void FixedUpdate() {
		if (!isShooted) 
			if (Input.GetButtonDown("Fire1")) isShooted = true;

		if (isShooted && !isPinned)
			rb.MovePosition(rb.position + Vector2.up * speed * Time.fixedDeltaTime);

		if (canEvaluateCollision) {
			if (collisions.Count > 0) {
				Debug.Log ("<color=yellow> " + gameObject.name + "colisiona con: " + collisions.Count.ToString() + "[" + collisions.ListaAsString() + "]</color>");
				PositionCorrection();
			}
			else {				
				SetNeedleAsPinned();
				canEvaluateCollision = false;
				GameManager.instance.spawner.SpawnNeedle();
			}
			//TODO: uncomment this
			//GameManager.instance.EvaluatePinnedNeedle(gameObject, collisions);
		}

		DrawSpear();
	}

	void OnTriggerEnter2D (Collider2D col) {
		if ( isShooted && !isPinned) {
			if (col.gameObject.tag == "Rotator") {
				AddToMyCollisions(col.gameObject);
			}
			else if (col.gameObject.tag == "Pin") {
				if (/*col.gameObject.GetComponent<ColorNeedle>().isShooted && */col.gameObject.GetComponent<ColorNeedle>().isPinned) {
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
	}

	void DrawTheGizmo(GizmoToDraw g) {
		if (!gizmosToDraw.Contains(g))
			gizmosToDraw.Add(g);
	}
	
	void PositionCorrection() {
		switch (collisions.Count) {
			case 1:
				transform.SetParent(rotator);
				FixPosition( collisions[0].transform, SumRadius(collisions[0], gameObject));
				if ( collisions[0].tag == "Rotator" ) {
					drawSpear = true;
				}
				collisions.RemoveAt(0);
				transform.SetParent(null);
			break;
			case 2:

				Vector3 A = collisions[0].transform.position;
				Vector3 B = collisions[1].transform.position;

				DrawTheGizmo (new GizmoToDraw(GizmoType.sphere, collisions[0].transform.position, collisions[0].GetComponent<Circumference>().radius * collisions[0].transform.lossyScale.x, Color.blue));
				DrawTheGizmo (new GizmoToDraw(GizmoType.sphere, collisions[1].transform.position, collisions[1].GetComponent<Circumference>().radius * collisions[1].transform.lossyScale.x, Color.blue));


				float La = Vector3.Distance(A, B); // calcular la distancia entre los dos puntos (no sumando los radios).
				float Lb = SumRadius(collisions[1], gameObject); //SumRadius(collisions[1], gameObject);    // calcular la distancia entre los dos puntos (no sumando los radios).
				float Lc = SumRadius(gameObject, collisions[0]); //SumRadius(gameObject, collisions[0]);    // calcular la distancia entre los dos puntos (no sumando los radios).

				float arCos_AB_AC = (Lb*Lb + Lc*Lc - La*La) / 2*Lb*Lc;
				float a = Mathf.Cos(arCos_AB_AC);

				float arSin_AV_x = (A.y - B.y) / La;
				float b = Mathf.Sin(arSin_AV_x);

				transform.position = new Vector3 ( A.x + (Lc * Mathf.Cos(a - b)), A.y + (Lc * Mathf.Sin(a - b)), 0 );

				DrawTheGizmo (new GizmoToDraw(GizmoType.sphere, transform.position, radius * transform.lossyScale.x, Color.blue));
/*
				// 1
				float AB = Vector3.Distance (collisions[1].transform.position, collisions[0].transform.position);
				// 2
				Debug.Assert(AB <= 4 * radius * transform.lossyScale.x);
				// 3	
				Vector2 H = new Vector2( collisions[0].transform.position.x + ((collisions[1].transform.position.x - collisions[0].transform.position.x)/2),
			    	                     collisions[0].transform.position.y + ((collisions[1].transform.position.y - collisions[0].transform.position.y)/2)
			                           );
				// 4
				Vector2 HC_perp_norm = new Vector2( -(collisions[1].transform.position.y - collisions[0].transform.position.y) / AB, 
			    	                               (collisions[1].transform.position.x - collisions[0].transform.position.x) / AB );
				// 5		                           
				float HC = 0.5f * Mathf.Sqrt( (16*(radius * radius)) - (AB*AB) );

                transform.position = new Vector3 (H.x + (HC * HC_perp_norm.x), H.y + (HC * HC_perp_norm.y), 0);
*/	
				collisions.RemoveAt(1);
				collisions.RemoveAt(0);
			break;
			default:				
				Debug.Log("<color=red> Exceso de colisiones: " + collisions.Count + " [" + collisions.ListaAsString() + "]</color>");
			break;
		}
	}

	float SumRadius(GameObject A, GameObject B) {
		float radA = A.transform.lossyScale.x * GetRadius(A);
		float radB = B.transform.lossyScale.x * GetRadius(B);
		return  (radA + radB);		
	}

	float GetRadius(GameObject g) {
		float rad = 0f;
		rad = g.GetComponent<Circumference>().radius;
		return rad;
	}

	void FixPosition(Transform pinnedNeedle, float distOffset = 0f) {
		DrawTheGizmo(new GizmoToDraw(GizmoType.sphere, pinnedNeedle.position, pinnedNeedle.gameObject.GetComponent<Circumference>().radius * transform.lossyScale.x, Color.gray));
		DrawTheGizmo(new GizmoToDraw(GizmoType.sphere, transform.position, radius * transform.lossyScale.x, Color.yellow));
		transform.position = pinnedNeedle.position + ((transform.position - pinnedNeedle.position).normalized * distOffset);
		DrawTheGizmo(new GizmoToDraw(GizmoType.sphere, transform.position, radius * transform.lossyScale.x, Color.red));
		DrawTheGizmo(new GizmoToDraw(GizmoType.sphere, pinnedNeedle.position, pinnedNeedle.gameObject.GetComponent<Circumference>().radius * transform.lossyScale.x, Color.gray));
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
