using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingPointsManager : MonoBehaviour
{

    public GameObject flyingPointPrefab;
    public List<FlyingPoint> flyingPointsPool;

    private int flyingPointsCount = 5;

    private RectTransform canvasParent;


    void OnEnable()
    {
        GenerateFlyingPointsPool();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAddScore += OnAddScore_Handle;
        }
    }

    void OnDisable()
    {
        if (flyingPointsPool != null)
        {
            flyingPointsPool.ForEach(fp => Destroy(fp.gameObject));
            flyingPointsPool.Clear();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAddScore -= OnAddScore_Handle;
        }
    }

    void Awake()
    {
        canvasParent = GetComponent<RectTransform>();
    }

    void Start()
    {
    }

    void Update()
    {
    }

    void GenerateFlyingPointsPool()
    {
        if (flyingPointsPool == null)
        {
            flyingPointsPool = new List<FlyingPoint>();
        }
        else
        {
            flyingPointsPool.ForEach(p => Destroy(p.gameObject));
            flyingPointsPool.Clear();
        }

        for (int i = 0; i < flyingPointsCount; i++)
        {
            flyingPointsPool.Add(CreateNewFlyingPoint());
        }
    }

    FlyingPoint CreateNewFlyingPoint()
    {
        GameObject point = Instantiate(flyingPointPrefab, canvasParent) as GameObject;
        FlyingPoint newFP = point.GetComponent<FlyingPoint>();
        newFP.SetCanvasRect(canvasParent);
        newFP.transform.SetParent(transform);
        newFP.IsAvailable = true;
        return newFP;
    }

    FlyingPoint GetAvailablePoint()
    {
        FlyingPoint available = flyingPointsPool.Find(p => p.IsAvailable);
        if (available == null)
        {
            available = CreateNewFlyingPoint();
        }

        return available;
    }

    void OnAddScore_Handle(int points, Transform position, Color color)
    {
        FlyingPoint fp = GetAvailablePoint();
        fp.Setup(points, position, color);
        fp.IsAvailable = false;
    }
}