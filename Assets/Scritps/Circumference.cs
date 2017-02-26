using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GizmoType {
	sphere,
	line
}

public struct GizmoToDraw {
	public GizmoType gizmoType;
	public Vector3 from;
	public Vector3 to;
	public float size;
	public Color color;
	
	public GizmoToDraw (GizmoType type, Vector3 pos, float s, Color col) {
		gizmoType = type;
		from = pos;
		size = s;
		color = col;
		to = Vector3.zero;
	}
}

[RequireComponent (typeof (CircleCollider2D))]
public class Circumference : MonoBehaviour {
	public CircleCollider2D cc;
	private float radius;
	public Vector3 GetPosition() { return transform.position; }
	//public float GetScale() { return transform.lossyScale.x; }
	public float GetRadius() { return radius * transform.lossyScale.x; }

	public void Awake () {
		cc = GetComponent<CircleCollider2D>();
		radius = cc.radius;
		Initialize();
	}

	public virtual void Initialize(){}
}
