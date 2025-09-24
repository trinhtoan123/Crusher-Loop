using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GogoGaga.OptimizedRopesAndCables;
using DG.Tweening;

public class Knit : MonoBehaviour
{
    [SerializeField] private KnitChild[] knitItems;
    [SerializeField] private Vector3[] anchorPositions = {
        new Vector3(0.1f, 0.05f, 0),  
        new Vector3(-0.1f, 0.05f, 0)   
    };
    [SerializeField] private Rope ropePrefab;
    [SerializeField] private Material materialClear;
    [SerializeField] private float ropeLengthDecreaseRate = 0.1f; 
    [SerializeField] private float minRopeLength = 0.5f; 
    private LevelManager levelManager;
    private Rope currentRope;
    private SpoolItem targetSpool;
    private int currentKnitIndex = -1;
    private int currentChildIndex = 0;
    private bool isYarnActive = false;
    private Transform previousPoint = null;
    private Sequence movementSequence; 
    private float initialRopeLength; 
    public void Initialize(LevelManager levelManager)
    {
        this.levelManager = levelManager;
        foreach (var knitItem in knitItems)
        {
            knitItem.Initialize(levelManager);
        }
    }
    public void CreateLine()
    {
        if (knitItems == null || knitItems.Length == 0)
        {
            return;
        }
        
        targetSpool = FindSpoolItem();
        if (targetSpool == null)
        {
            return;
        }
        CreateYarnRope();
        if (currentRope == null)
        {
            return;
        }
        currentKnitIndex = 0;
        currentChildIndex = 0;
        previousPoint = null;
        
        Transform endPosition = GetCurrentChildPosition(currentChildIndex);
        SetRopePoints(targetSpool.GetYarnConnectionPoint(), endPosition);
        targetSpool.StartWinding(currentRope);
        targetSpool.OnYarnReachKnit(currentKnitIndex, knitItems.Length);
        isYarnActive = true;
        StartMovementAfterDelay();
    }


    private SpoolItem FindSpoolItem()
    {
        if (PillarController.instance == null || PillarController.instance.PillarItems == null)
        {
            return null;
        }

        ColorRope targetColor = GetKnitColor(0);

        foreach (var pillar in PillarController.instance.PillarItems)
        {
            if (pillar.HasSpoolItem())
            {
                SpoolItem spool = pillar.GetSpoolItem();
                if (spool != null && !spool.IsWindingYarn())
                {
                    if (spool.color == targetColor)
                    {
                        return spool;
                    }
                }
            }
        }
        
        return null;
    }

    private ColorRope GetKnitColor(int knitIndex)
    {
        KnitChild knit = knitItems[knitIndex];
        return knit.Color;
    }

    private Material GetColor(ColorRope color)
    {
        if (levelManager == null || levelManager.SpoolData == null)
        {
            return null;
        }

        var spoolColor = levelManager.SpoolData.SpoolColors.Find(x => x.color == color);
        return spoolColor?.materialSpool;
    }

    private void UpdateLineMaterial(ColorRope color)
    {
        if (currentRope == null) return;

        LineRenderer lineRenderer = currentRope.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            Material newMaterial = GetColor(color);
            if (newMaterial != null)
            {
                lineRenderer.material = newMaterial;
            }
        }
    }

    private void SetPointMaterialClear(Transform point)
    {
        if (point == null || materialClear == null) return;

        Transform knitChild = point.parent;
        if (knitChild != null)
        {
            KnitChild knitChildComponent = knitChild.GetComponent<KnitChild>();
            if (knitChildComponent != null)
            {
                knitChildComponent.SetMaterial(materialClear);
            }
            else
            {
                MeshRenderer meshRenderer = point.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.material = materialClear;
                }
            }
        }
    }

    #region Yarn Movement Logic
    private Transform GetCurrentChildPosition(int index)
    {
        if (knitItems == null || currentKnitIndex < 0 || currentKnitIndex >= knitItems.Length)
        {
            return null;
        }
        KnitChild currentKnitChild = knitItems[currentKnitIndex];
        if (currentKnitChild == null || currentKnitChild.ChildItems == null || currentKnitChild.ChildItems.Length == 0)
        {
            return null;
        }
        
        if (index < 0 || index >= currentKnitChild.ChildItems.Length)
        {
            return null;
        }
        
        Transform childTransform = currentKnitChild.GetChildItem(index);
        
        return CreateAnchorPointForChild(childTransform);
    }

    private Transform CreateAnchorPointForChild(Transform childTransform)
    {
        if (childTransform == null) return null;
        
        Transform existingAnchor = childTransform.Find("KnitAnchor");
        if (existingAnchor != null)
        {
            return existingAnchor;
        }
        
        GameObject anchorPoint = new GameObject("KnitAnchor");
        anchorPoint.transform.SetParent(childTransform);
        
        Vector3 anchorPosition = GetAnchorPositionForChildIndex(currentChildIndex);
        anchorPoint.transform.localPosition = anchorPosition;
        return anchorPoint.transform;
    }
    
    private Vector3 GetAnchorPositionForChildIndex(int childIndex)
    {
        if (anchorPositions == null || anchorPositions.Length == 0)
        {
            return Vector3.zero;
        }
        
        int index = Mathf.Clamp(childIndex, 0, anchorPositions.Length - 1);
        return anchorPositions[index];
    }

    private void StartMovementAfterDelay()
    {
        DOVirtual.DelayedCall(0.3f, MoveToNextTarget);
    }

    private void MoveToNextTarget()
    {
        if (currentChildIndex < knitItems[currentKnitIndex].ChildItems.Length - 1)
        {
            currentChildIndex++;
            SmoothMoveToTarget();
        }
        else if (currentKnitIndex < knitItems.Length - 1)
        {
            currentKnitIndex++;
            currentChildIndex = 0; 
            SmoothMoveToTarget();
        }
        else
        {
            CompletedAllKnitItems();
        }
    }


    private void SmoothMoveToTarget()
    {
        Transform targetTransform = GetCurrentChildPosition(currentChildIndex);
        if (targetTransform == null || currentRope == null)
        {
            return;
        }
        
        
        if (previousPoint != null)
        {
            SetPointMaterialClear(previousPoint);
        }
        
        
        ColorRope currentKnitColor = GetKnitColor(currentKnitIndex);
        if (targetSpool != null && targetSpool.color != currentKnitColor)
        {
            
            MoveToNextTarget();
            return;
        }

        
        UpdateLineMaterial(currentKnitColor);
        
        if (movementSequence != null && movementSequence.IsActive())
        {
            movementSequence.Kill();
        }
        
        movementSequence = DOTween.Sequence();
        
        float newRopeLength = CalculateNewRopeLength();

        currentRope.ropeLength = newRopeLength;
        movementSequence.AppendCallback(() =>
        {
            currentRope.SetEndPoint(targetTransform, true);
            if (targetSpool != null)
            {
                targetSpool.OnYarnReachKnit(currentKnitIndex, knitItems.Length);
            }
            
            
            previousPoint = targetTransform;
        });
        
        
        movementSequence.AppendInterval(0.2f);
        
        
        movementSequence.AppendCallback(MoveToNextTarget);
    }
    
    private float CalculateNewRopeLength()
    {
        int totalMoves = GetTotalMovesCount();
        float decreaseAmount = ropeLengthDecreaseRate * totalMoves;
        float newLength = initialRopeLength - decreaseAmount;
        
        
        return Mathf.Max(newLength, minRopeLength);
    }
    

    private int GetTotalMovesCount()
    {
        int moves = 0;
        for (int i = 0; i < currentKnitIndex; i++)
        {
            if (knitItems[i] != null && knitItems[i].ChildItems != null)
            {
                moves += knitItems[i].ChildItems.Length;
            }
        }
        
        moves += currentChildIndex;
        
        return moves;
    }

    private void CompletedAllKnitItems()
    {
        isYarnActive = false;
    }

    #endregion

    private void CreateYarnRope()
    {
        if (ropePrefab == null)
        {
            return;
        }
        
        if (currentRope != null)
        {
            Destroy(currentRope.gameObject);
            currentRope = null;
        }
        currentRope = Instantiate(ropePrefab);
        if (currentRope == null)
        {
            return;
        }


        initialRopeLength = currentRope.ropeLength;

        LineRenderer lineRenderer = currentRope.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {

            ColorRope knitColor = GetKnitColor(0);
            Material yarnMaterial = GetColor(knitColor);
            if (yarnMaterial != null)
            {
                lineRenderer.material = yarnMaterial;
            }
        }
    }
    
    private void SetRopePoints(Transform startPoint, Transform endPoint)
    {
        if (currentRope == null || startPoint == null || endPoint == null)
        {
            return;
        }
  
        currentRope.SetStartPoint(startPoint, false);
        currentRope.SetEndPoint(endPoint, false);
    }
    public bool IsYarnActive()
    {
        return isYarnActive;
    }
    
   
    public void ClearLine()
    {
        isYarnActive = false;
        

        if (movementSequence != null && movementSequence.IsActive())
        {
            movementSequence.Kill();
        }
        
        StopAllCoroutines();
        currentKnitIndex = -1;
        currentChildIndex = 0;
        previousPoint = null;
        targetSpool = null;
    }
    public void DestroyLine()
    {
        ClearLine();
        
        if (currentRope != null)
        {
            Destroy(currentRope.gameObject);
            currentRope = null;
        }
    }
}
