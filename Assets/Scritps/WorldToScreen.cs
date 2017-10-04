using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldToScreen : MonoBehaviour
{
    //this is your object that you want to have the UI element hovering over
    public GameObject WorldObject;

    //this is the ui element
    RectTransform UI_Element;

    //first you need the RectTransform component of your canvas
    public RectTransform CanvasRect;

    void Start()
    {
        UI_Element = GetComponent<RectTransform>();
        Relocate();
    }

    void Relocate()
    {
        //then you calculate the position of the UI element
        //0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(WorldObject.transform.position);
        Vector2 WorldObject_ScreenPosition = new Vector2(
                                                 ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
                                                 ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f))
                                             );

        //now you can set the position of the ui element
        UI_Element.anchoredPosition = WorldObject_ScreenPosition;
    }
}
