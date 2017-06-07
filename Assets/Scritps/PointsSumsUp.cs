using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsSumsUp : MonoBehaviour {
	
	//this is your object that you want to have the UI element hovering over
	Transform TargetObject;

	//this is the ui element
	RectTransform UI_Element;

	//first you need the RectTransform component of your canvas
	RectTransform CanvasRect;

	void Awake() {
		CanvasRect = GameObject.FindGameObjectWithTag ("UICanvas").GetComponent<RectTransform> ();
		UI_Element = GetComponent<RectTransform> ();
	}

	public void SetTargetObject(Transform target) {
		TargetObject = target;
		Vector2 ViewportPosition=Camera.main.WorldToViewportPoint(TargetObject.position);
		Vector2 WorldObject_ScreenPosition = new Vector2(
			((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
			((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));

		// now you can set the position of the ui element
		UI_Element.anchoredPosition = WorldObject_ScreenPosition;
	}

	public void Autodrestroy() {
		Destroy (gameObject);
	}
}
