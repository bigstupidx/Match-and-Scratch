using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin : Circumference
{
    public const float TIME_TO_DESTROY = 0.2f;

    public GameObject pointsGameObject;

    [SerializeField]
    private float speed = 20f;
    [SerializeField]
    private bool isShooted;
    [SerializeField]
    private bool isPinned;
    [SerializeField]
    private bool drawSpear;

    private bool isAlive;
    private LineRenderer lr;
    private SpriteRenderer sr;
    private Rotator rot;
    //private Transform GameScreenParent;
    private Vector3 vel;
    private Vector3 originalScale;

    public bool IsAlive
    {
        get
        {
            return isAlive;
        }
        set {
            isAlive = value;
            gameObject.SetActive(value);
        }
    }

    public int pointsValue
    { 
        get; 
        set;
    }

    public override void Initialize()
    {
        sr = GetComponent<SpriteRenderer>();

        lr = GetComponent<LineRenderer>();
        lr.enabled = false;

        rot = GameManager.Instance.rotator;

        //GameScreenParent = GameObject.Find("Game Screen").transform;
        originalScale = transform.localScale;
    }

    public void Setup(Vector3 position, int color, string theName)
    {
        transform.localScale = originalScale;
        transform.position = position;
        SetColor(color);
        name = theName;

        pointsValue = 0;
        IsAlive = true;
    }

    public void SetColor(int color)
    {
        colorType = color;
        sr.color = GameManager.Instance.posibleColors[colorType];
    }

    public void SetAvailable() {
        name = "Available Pin";
        transform.SetParent(null);
        speed = 20f;
        isShooted = false;
        isPinned = false;
        drawSpear = false;
        colisionador.enabled = false;
        IsAlive = false;
    }

    void SetupLine()
    {        
        lr.startColor = sr.color;
        lr.endColor = sr.color;
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;
        lr.enabled = true;
    }

    void DrawTheSpear()
    {
        if (drawSpear)
        {
            lr.positionCount = 2;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, rot.transform.position);
        }
        else if (lr)
        {
            lr.positionCount = 0;
        }
    }

    public void Shoot()
    {
        isShooted = true;
    }

    public void PinIt()
    {
        colisionador.enabled = true;
        isPinned = true;
    }

    public void EnableSpear()
    {
        SetupLine();
        drawSpear = true;
    }

    //TODO: Cambiar esto a dejar el pin disponible (Alive = false;)
    public void Autodestroy()
    {
        drawSpear = false;
        colisionador.enabled = false;
        if (pointsValue > 0)
        {
            GameManager.Instance.AddScore(pointsValue, transform, sr.color);
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
        SetAvailable();
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