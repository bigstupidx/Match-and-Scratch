using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GizmoType
{
    sphere,
    line
}

public struct GizmoToDraw
{
    public GizmoType gizmoType;
    public Vector3 from;
    public Vector3 to;
    public float size;
    public Color color;

    public GizmoToDraw(GizmoType type, Vector3 origen, float radius, Color col)
    {
        gizmoType = type;
        from = origen;
        size = radius;
        color = col;
        to = Vector3.zero;
    }

    public GizmoToDraw(GizmoType type, Vector3 origen, Vector3 destiny, Color col)
    {
        gizmoType = type;
        from = origen;
        to = destiny;
        size = 0;
        color = col;
    }
}

[RequireComponent(typeof(CircleCollider2D))]
public class Circumference : MonoBehaviour
{
    #if VISUAL_DEBUG
        public List<GizmoToDraw> gizmosToDraw = new List<GizmoToDraw>();
    #endif

    public int colorType = -1;
    public CircleCollider2D colisionador;
    private float radius;

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public float GetRadius()
    {
        return radius * transform.lossyScale.x;
    }

    int _colorGroup;

    public int colorGroup
    {
        get { return _colorGroup; }
        set
        {
            _colorGroup = value;
        }
    }

    public void Awake()
    {
        if (colisionador == null)
            colisionador = GetComponent<CircleCollider2D>();
        radius = colisionador.radius;		
        Initialize();
    }
    public virtual void Initialize()
    {
    }

}
