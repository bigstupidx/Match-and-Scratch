using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin : Circumference
{
    public const float TIME_TO_DESTROY = 0.2f;

    public float speed = 20f;

    public bool isShooted = false;
    public bool isPinned = false;
    public bool drawSpear = false;

    public GameObject pointsGameObject;

    private LineRenderer line;
    private SpriteRenderer sr;
    private Rotator rot;
    private Transform GameScreenParent;
    private Vector3 vel;

    public int pointsValue
    { 
        get; 
        set; 
    }

    public override void Initialize()
    {
        sr = GetComponent<SpriteRenderer>();
        rot = GameManager.Instance.rotator;
        colisionador.enabled = false;
        GameScreenParent = GameObject.Find("Game Screen").transform;
        pointsValue = 0;
        line = GetComponent<LineRenderer>();
        line.enabled = false;
    }

    public void SetColor(Color32 color)
    {
        sr.color = color;
    }

    void SetupLine()
    {        
        line.startColor = sr.color;
        line.endColor = sr.color;
        line.startWidth = 0.03f;
        line.endWidth = 0.03f;
        line.enabled = true;
    }

    void DrawTheSpear()
    {
        if (drawSpear)
        {
            line.positionCount = 2;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, rot.transform.position);
        }
        else if (line)
        {
            line.positionCount = 0;
        }
    }

    public void DrawSpear()
    {
        SetupLine();
        drawSpear = true;
    }

    public void SetPinned()
    {
        isPinned = true;
    }

    void Update()
    {
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.Instance.spawner.ThrowCurrentPin();
        }
        #endif

        if (!isPinned && isShooted)
        {
            float smoothSpeed = speed * Time.deltaTime;
            float moveInc = Mathf.Min(smoothSpeed, 2 * GetRadius());
            vel = Vector3.up * moveInc;

            transform.position += vel;

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, GetRadius(), vel.normalized, 0);
            if (hit)
            {
                if ((hit.collider.tag == "Pin" && hit.collider.GetComponent<Pin>().isPinned && hit.collider.GetComponent<Pin>()) || hit.collider.tag == "Rotator")
                {
                    speed = 0;
                    rot.AddPin(this, hit.collider.gameObject);

                    #if VISUAL_DEBUG
                        DrawTheGizmo(new GizmoToDraw(GizmoType.sphere, transform.position, GetRadius(), RandomColor()));
                        DrawX(hit.point, RandomColor(), GetRadius(), 3, 1);
                    #endif
                }
            }
        }
    }

    void LateUpdate()
    {
        DrawTheSpear();
    }

    public void Autodestroy()
    {
        drawSpear = false;
        colisionador.enabled = false;
        if (pointsValue > 0)
        {
            // TODO: Pooling of floating points
            GameObject point = Instantiate(pointsGameObject, GameScreenParent) as GameObject;
            point.GetComponent<PointsSumsUp>().SetPositionOverTarget(transform);
            UnityEngine.UI.Text txtPoint = point.GetComponentInChildren<UnityEngine.UI.Text>();
            txtPoint.text = pointsValue.ToString();
            txtPoint.color = sr.color;
            GameManager.Instance.AddScore(pointsValue);
        }
        StartCoroutine(AnimToDead());
    }

    public IEnumerator AnimToDead()
    {
		
        float t = TIME_TO_DESTROY;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            transform.localScale *= 1.13f;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, t);
            yield return null;
        }	

        Destroy(gameObject);
    }
        
    #if VISUAL_DEBUG
        void DrawX(Vector2 position, Color color, float size, float duration, float shape)
        {
            Debug.DrawLine(position - Vector2.one * (size / 2f), position + Vector2.one * (size / 2f), color, duration);
            Debug.DrawLine(position + new Vector2(-1 * shape, 1) * (size / 2f), position + new Vector2(1, -1 * shape) * (size / 2f), color, duration);
        }


        Color RandomColor()
        {
            return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
        }


        void DrawTheGizmo(GizmoToDraw g)
        {
            if (!gizmosToDraw.Contains(g))
                gizmosToDraw.Add(g);
        }

        void OnDrawGizmos()
        {
            foreach (GizmoToDraw gtd in gizmosToDraw)
            {
                switch (gtd.gizmoType)
                {
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