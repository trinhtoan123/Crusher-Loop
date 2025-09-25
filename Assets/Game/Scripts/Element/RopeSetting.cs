using System;
using System.Collections.Generic;
using DG.Tweening;
using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;
public class RopeSetting : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Rope rope;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        rope = GetComponent<Rope>();
    }
 
    public void SetMaterial(Material material)
    {
        lineRenderer.material = material;
    }

    public void SetStartPoint(Transform startPointTarget)
    {
        this.startPoint = startPointTarget;
    }
    public void SetEndPoint(Transform endPointTarget)
    {
        this.endPoint.DOMove(endPointTarget.position, moveDuration);
    }

    public void MoveNextEndPoint(Transform nextEndPoint)
    {
        SetEndPoint(nextEndPoint);
    }

    public void ClearRope()
    {
        lineRenderer.enabled = false;
    }
}

