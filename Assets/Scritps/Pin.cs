using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin : Circumference {
	public const float TIME_TO_DESTROY = 0.2f;

	public float speed = 20f;

	public bool isShooted = false;
	public bool isPinned = false;
	public bool drawSpear = false;

	private Circumference me;

	private LineRenderer line;
	private SpriteRenderer sr;
	Rotator rot;

	public override void Initialize() {
		sr = GetComponent<SpriteRenderer>();
		me = this;
		rot = GameManager.instance.rotator;
		colisionador.enabled = false;
		//SetupLine();
	}

	void OnEnable() {
	}

	void SetupLine() {
		line = new LineRenderer ();
		Color parentColor = GetComponent<SpriteRenderer>().color;
		line = gameObject.AddComponent<LineRenderer>();
		line.material = new Material(Shader.Find("Sprites/Default"));
		line.startColor = Color.black;//parentColor;
		line.endColor = Color.black;//parentColor;
		line.startWidth = 0.03f;
		line.endWidth = 0.03f;
	}

	void DrawTheSpear() {
		if (drawSpear) {
			line.positionCount = 2;
			line.SetPosition(0, transform.position);
			line.SetPosition(1, rot.transform.position);
		}
		else
			if (line)
			line.positionCount = 0;
	}
	
	public void DrawSpear() {
		SetupLine ();
		drawSpear = true;
	}

	Vector3 vel;

	void Update() {
		#if UNITY_EDITOR
		if ( Input.GetKeyDown(KeyCode.Space) )
			GameManager.instance.spawner.ThrowCurrentPin();
		#endif
		//#if UNITY_EDITOR
		//if (!isShooted) 
		//	if (Input.GetButtonDown("Fire1")) isShooted = true;
		//#endif
		if (!isPinned && isShooted) {
			float smoothSpeed = speed * Time.deltaTime;
			float moveInc = Mathf.Min (smoothSpeed, 2 * GetRadius ());
			vel = Vector3.up * moveInc;

			//Debug.LogFormat("Velocidad del pin {0}: {1} ----- moveInc: {2}, diametro: {3}", name, moveInc, smoothSpeed, 2 * GetRadius());

			transform.position += vel;

			RaycastHit2D hit = Physics2D.CircleCast (transform.position, GetRadius (), vel.normalized, 0);
			if (hit) {
				if ((hit.collider.tag == "Pin" && hit.collider.GetComponent<Pin> ().isPinned && hit.collider.GetComponent<Pin> ()) || hit.collider.tag == "Rotator") {
					speed = 0;
					rot.AddPin (me, hit.collider.gameObject);
					#if UNITY_EDITOR
						DrawTheGizmo (new GizmoToDraw (GizmoType.sphere, transform.position, GetRadius (), RandomColor ()));
						DrawX(hit.point, RandomColor(), GetRadius (), 3, 1);
					#endif
				}
			}
		}
	}

	void LateUpdate() {
		DrawTheSpear();
	}

	public override void Disable() {
		drawSpear = false;
		colisionador.enabled = false;
		StartCoroutine(AnimToDead());
	}

	public IEnumerator AnimToDead() {

		float t = TIME_TO_DESTROY;
		while (t > 0f) {
			t -= Time.deltaTime;
			transform.localScale *= 1.13f;
			sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, t);
			yield return null;
		}		
		Destroy(gameObject);
	}


	////////////// DEBUG //////////////
#if UNITY_EDITOR
	void DrawX(Vector2 position, Color color, float size, float duration, float shape)
	{
		Debug.DrawLine(position - Vector2.one * (size / 2f), position + Vector2.one * (size / 2f), color, duration);
		Debug.DrawLine(position + new Vector2(-1 * shape, 1) * (size / 2f), position + new Vector2(1, -1 * shape) * (size / 2f), color, duration);
	}


	Color RandomColor()
	{
		return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
	}

	void DrawTheGizmo(GizmoToDraw g) {
		if (!gizmosToDraw.Contains(g))
			gizmosToDraw.Add(g);
	}

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
#endif
}