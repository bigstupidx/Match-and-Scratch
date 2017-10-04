using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingPointsManager : MonoBehaviour
{

    public GameObject flyingPointPrefab;
    public List<FlyingPoint> flyingPointsPool;

    private int flyingPointsCount = 5;

    public RectTransform canvasParent;

    void Start() {
        GenerateFlyingPointsPool();
    }
        
    void OnEnable()
    {
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAddScore += OnAddScore_Handle;
        }
    }

    void OnDisable()
    {/*
        if (flyingPointsPool != null)
        {
            flyingPointsPool.ForEach(fp => Destroy(fp.gameObject));
            flyingPointsPool.Clear();
        }
*/
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAddScore -= OnAddScore_Handle;
        }
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

    void OnAddScore_Handle(int points, Transform target, Color color)
    {
        FlyingPoint fp = GetAvailablePoint();
        fp.SetPositionOverTarget(target);
        fp.Setup(points, color);
        fp.IsAvailable = false;
    }
}