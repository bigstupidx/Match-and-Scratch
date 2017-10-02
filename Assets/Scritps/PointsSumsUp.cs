using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsSumsUp : MonoBehaviour
{	
    Transform TargetObject;

    RectTransform UI_Element;

    RectTransform CanvasRect;

    void Awake()
    {
        CanvasRect = GameObject.FindGameObjectWithTag("UICanvas").GetComponent<RectTransform>();
        UI_Element = GetComponent<RectTransform>();
    }

    public void SetPositionOverTarget(Transform target)
    {
        TargetObject = target;
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(TargetObject.position);
        Vector2 WorldObject_ScreenPosition = new Vector2(
                                           ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
                                           ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));

        UI_Element.anchoredPosition = WorldObject_ScreenPosition;
    }

    public void Autodrestroy()
    {
        Destroy(gameObject);
    }
}
