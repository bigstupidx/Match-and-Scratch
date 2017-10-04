using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlyingPoint : MonoBehaviour
{	
    public Text textElement;
    private Transform TargetObject;

    private RectTransform UI_Element;

    private RectTransform CanvasRect;

    private bool available;

    public bool IsAvailable
    {
        get 
        {
            return available;
        } 
        set
        { 
            available = value;
            gameObject.SetActive(!value);
        }
    }

    void Awake()
    {
        UI_Element = GetComponent<RectTransform>();
    }

    public void Setup(int points, Transform target, Color color)
    {
        textElement.text = "+" + points.ToString();
        textElement.color = color;
        SetPositionOverTarget(target);
    }

    public void SetCanvasRect(RectTransform canvasParent) {
        CanvasRect = canvasParent;
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
        IsAvailable = true;       
    }
}
