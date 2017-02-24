using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : Circumference {

	public float speed = 100f;

	void FixedUpdate() {
		transform.Rotate(0f, 0f, speed * Time.deltaTime);
	}
}
