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
	private Rigidbody2D rb;
	private LineRenderer line;
	private SpriteRenderer sr;
	Rotator rot;

	public override void Initialize() {
		rb = GetComponent<Rigidbody2D>();
		sr = GetComponent<SpriteRenderer>();
		me = this;//GetComponent<Circumference>();
		rot = GameManager.instance.rotator;
		SetupLine();
	}

	void OnEnable() {
		if (line == null)	line = GetComponent<LineRenderer>();
	}

	void SetupLine() {
		Color parentColor = GetComponent<SpriteRenderer>().color;
		line = gameObject.AddComponent<LineRenderer>();
		line.material = new Material(Shader.Find("Sprites/Default"));
		line.startColor = parentColor;
		line.endColor = parentColor;
		line.startWidth = 0.05f;
		line.endWidth = 0.05f;
	}

	void DrawTheSpear() {
		if (drawSpear ) {
			line.positionCount = 2;
			line.SetPosition(0, transform.position);
			line.SetPosition(1, rot.transform.position);
		}
		else
			line.positionCount = 0;
	}
	
	public void DrawSpear() {
		drawSpear = true;
	}

	void Update() {
		if (!isShooted) 
			if (Input.GetButtonDown("Fire1")) isShooted = true;

		if (isShooted && !isPinned)
			rb.MovePosition(rb.position + Vector2.up * speed * Time.deltaTime);

		DrawTheSpear();
	}

	void OnTriggerEnter2D (Collider2D col) {
		if ( isShooted && !isPinned) {
			try {
				rot.AddPin(me, col);
			}
			catch (MissingReferenceException e) {
				Debug.LogError (e.Source + ": " + e.Message);
			}
		}
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
}
