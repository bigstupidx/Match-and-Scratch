using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods {	

	public static List<GameObject> ToList(this GameObject[] list) {
		List<GameObject> newList = new List<GameObject>();
		for (int i = 0; i < list.Length; i++) {
			newList.Add(list[i]);
		}
		return newList;
	}

	public static string ListaAsString(this GameObject[] list, string enunciado = "") {
		string ret = enunciado + " ";
		for (int i = 0; i < list.Length; i++) {
			ret += list[i] == null ? "<null>" : list[i].name;
			if (i < list.Length -1) ret += ", ";
		}
		return ret;
	}

	public static string ListaAsString(this List<GameObject> list, string enunciado = "") {
		string ret = enunciado + " ";
		for (int i = 0; i < list.Count; i++) {
			ret += list[i].name;
			if (i < list.Count -1) ret += ", ";
		}
		return ret;
	}
}

public class Pin : Circumference {
	public const float TIME_TO_DESTROY = 0.2f;

	public float speed = 20f;
	public bool isShooted = false;
	public bool isPinned = false;
	public bool drawSpear = false;

	Circumference me;
	private Transform rotator;
	private Rigidbody2D rb;
	private LineRenderer lr;
	private SpriteRenderer sr;

	public override void Initialize() {
		rb = GetComponent<Rigidbody2D>();
		sr = GetComponent<SpriteRenderer>();
		rotator = GameObject.FindGameObjectWithTag("Rotator").transform;
		me = GetComponent<Circumference>();
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

	void DrawTheSpear() {
		if (drawSpear ) {
			lr.numPositions = 2;
			lr.SetPosition(0, transform.position);
			lr.SetPosition(1, rotator.position);
		}
		else
			lr.numPositions = 0;
	}
	
	public void DrawSpear() {
		drawSpear = true;
	}

	void Update() {
		if (!isShooted) 
			if (Input.GetButtonDown("Fire1")) isShooted = true;

		if (isShooted && !isPinned)
			rb.MovePosition(rb.position + Vector2.up * speed * Time.fixedDeltaTime);
	}

	void LateUpdate() {
		DrawTheSpear();
	}

	void OnTriggerEnter2D (Collider2D col) {
		if ( isShooted && !isPinned) {
			if (col.gameObject.tag == "Rotator" || col.gameObject.tag == "Pin" ) {
				GameManager.instance.rotator.AddPin(me, col);
			}
		}
	}

	public override void Disable() {
		drawSpear = false;
		cc.enabled = false;
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
}
