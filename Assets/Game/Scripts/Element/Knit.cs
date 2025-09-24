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
    [SerializeField] private RopeSetting ropeSetting; 
    private LevelManager levelManager;
    private SpoolItem targetSpool;
    private int currentKnitIndex = -1;
    private int currentChildIndex = 0;
    private bool isYarnActive = false;
    private Transform previousPoint = null;
    private Sequence movementSequence; 
    public void Initialize(LevelManager levelManager)
    {
        this.levelManager = levelManager;
        foreach (var knitItem in knitItems)
        {
            knitItem.Initialize(levelManager);
        }
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            CreateLine();
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

        if (ropeSetting != null)
        {
            RopeSetting rope = Instantiate(ropeSetting, ropeSetting.transform);

        }

        currentKnitIndex = 0;
        currentChildIndex = 0;
        previousPoint = null;

        // Lấy Rope từ RopeSetting để truyền cho SpoolItem
        Rope ropeForSpool = ropeSetting.GetComponent<RopeSetting>().currentRope;
        if (ropeForSpool != null)
        {
            targetSpool.StartWinding(ropeForSpool);
        }
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
        if (levelManager == null || GameManager.Instance.SpoolData == null)
        {
            return null;
        }

        var spoolColor = GameManager.Instance.SpoolData.SpoolColors.Find(x => x.color == color);
        return spoolColor?.materialSpool;
    }

    private void UpdateLineMaterial(ColorRope color)
    {
        if (ropeSetting == null || ropeSetting.currentRope == null) return;

        LineRenderer lineRenderer = ropeSetting.currentRope.GetComponent<LineRenderer>();
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
        if (point == null || GameManager.Instance.SpoolData.MaterialKnitClear == null) return;

        Transform knitChild = point.parent;
        if (knitChild != null)
        {
            KnitChild knitChildComponent = knitChild.GetComponent<KnitChild>();
            if (knitChildComponent != null)
            {
                knitChildComponent.SetMaterial(GameManager.Instance.SpoolData.MaterialKnitClear);
            }
            else
            {
                MeshRenderer meshRenderer = point.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.material = GameManager.Instance.SpoolData.MaterialKnitClear;
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
        if (targetTransform == null || ropeSetting == null)
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

        // Sử dụng RopeSetting để di chuyển dây
        if (ropeSetting.currentRope != null)
        {
            ropeSetting.currentRope.ropeLength = newRopeLength;
        }
        
        movementSequence.AppendCallback(() =>
        {
            // Di chuyển đầu dây đến điểm mới
            ropeSetting.MoveRopeToNextKnitPoint(targetTransform);
            
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
        
        if (ropeSetting != null)
        {
            ropeSetting.StopRopeMovement();
        }
    }
}
