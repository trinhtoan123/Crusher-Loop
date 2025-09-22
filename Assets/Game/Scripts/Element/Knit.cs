using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GogoGaga.OptimizedRopesAndCables;
using DG.Tweening;

public class Knit : MonoBehaviour
{
    [Header("Knit Settings")]
    [SerializeField] private KnitChild[] knitItems;
    [SerializeField] private Vector3[] anchorPositions = {
        new Vector3(0.1f, 0.05f, 0),  
        new Vector3(-0.1f, 0.05f, 0)   
    };
    
    [Header("Rope Settings")]
    [SerializeField] private Rope ropePrefab;
    [SerializeField] private Material materialClear;
    private Rope currentRope;
    private SpoolItem targetSpool;
    private int currentKnitIndex = -1;
    private int currentChildIndex = 0;
    private bool isMovingToNextKnit = false;
    private bool isYarnActive = false;
    private Transform previousPoint = null; // Lưu trữ điểm trước đó
    [SerializeField] private float yarnMoveSpeed = 3f;
    [SerializeField] private float moveDuration = 0.5f; // Thời gian di chuyển cho DOTween
    [SerializeField] private Ease moveEase = Ease.OutQuad; // Loại easing cho animation
    [SerializeField] private float ropeLengthDecreaseRate = 0.1f; // Tỷ lệ giảm độ dài rope mỗi lần di chuyển
    [SerializeField] private float minRopeLength = 0.5f; // Độ dài tối thiểu của rope
    
    private Sequence movementSequence; // Sequence để quản lý các animation
    private float initialRopeLength; // Độ dài ban đầu của rope
    private LevelManager levelManager;
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
        
        // Bắt đầu từ Knit đầu tiên (index 0)
        currentKnitIndex = 0;
        currentChildIndex = 0; // Bắt đầu từ phần tử con đầu tiên
        previousPoint = null; // Reset điểm trước đó khi bắt đầu
        
        // Lấy vị trí phần tử con đầu tiên của Knit đầu tiên
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

        // Lấy màu của knit đầu tiên
        ColorRope targetColor = GetKnitColor(0);

        foreach (var pillar in PillarController.instance.PillarItems)
        {
            if (pillar.HasSpoolItem())
            {
                SpoolItem spool = pillar.GetSpoolItem();
                if (spool != null && !spool.IsWindingYarn())
                {
                    // Chỉ chọn ống chỉ có màu khớp với knit
                    if (spool.color == targetColor)
                    {
                        return spool;
                    }
                }
            }
        }
        
        return null;
    }

    // Lấy màu của knit theo index
    private ColorRope GetKnitColor(int knitIndex)
    {
        if (knitItems == null || knitIndex < 0 || knitIndex >= knitItems.Length)
        {
            return ColorRope.Red; // Màu mặc định
        }

        KnitChild knit = knitItems[knitIndex];
        if (knit == null) return ColorRope.Red;

        // Lấy màu từ field color của KnitChild
        var colorField = typeof(KnitChild).GetField("color", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (colorField != null)
        {
            return (ColorRope)colorField.GetValue(knit);
        }

        return ColorRope.Red;
    }

    // Lấy material theo màu sắc
    private Material GetMaterialByColor(ColorRope color)
    {
        if (levelManager == null || levelManager.SpoolData == null)
        {
            return null;
        }

        var spoolColor = levelManager.SpoolData.spoolColors.Find(x => x.color == color);
        return spoolColor?.material;
    }

    // Cập nhật material của line theo màu sắc
    private void UpdateLineMaterial(ColorRope color)
    {
        if (currentRope == null) return;

        LineRenderer lineRenderer = currentRope.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            Material newMaterial = GetMaterialByColor(color);
            if (newMaterial != null)
            {
                lineRenderer.material = newMaterial;
            }
        }
    }

    /// <summary>
    /// Set material clear cho một điểm cụ thể
    /// </summary>
    private void SetPointMaterialClear(Transform point)
    {
        if (point == null || materialClear == null) return;

        // Tìm KnitChild cha của điểm này
        Transform knitChild = point.parent;
        if (knitChild != null)
        {
            KnitChild knitChildComponent = knitChild.GetComponent<KnitChild>();
            if (knitChildComponent != null)
            {
                // Sử dụng method SetMaterial của KnitChild để set material clear cho tất cả child
                knitChildComponent.SetMaterial(materialClear);
            }
            else
            {
                // Nếu không có KnitChild component, set trực tiếp cho MeshRenderer
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
        Transform currentKnit = knitItems[currentKnitIndex].transform;
        if (currentKnit == null || currentKnit.childCount == 0)
        {
            return null;
        }
        
        // Lấy phần tử con theo index (từ 0 đến childCount-1)
        if (index < 0 || index >= currentKnit.childCount)
        {
            return null;
        }
        
        Transform childTransform = currentKnit.GetChild(index);
        
        // Tạo anchor point cho child này nếu chưa có
        return CreateAnchorPointForChild(childTransform);
    }
    
    /// <summary>
    /// Tạo anchor point cho knit child với vị trí cụ thể
    /// </summary>
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
    
    /// <summary>
    /// Lấy vị trí anchor cho child index cụ thể
    /// </summary>
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
        // Sử dụng DOTween thay vì coroutine
        DOVirtual.DelayedCall(0.3f, MoveToNextTarget);
    }

    private void MoveToNextTarget()
    {
        if (currentChildIndex < knitItems[currentKnitIndex].transform.childCount - 1)
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
        
        // Set material clear cho điểm trước đó (nếu có)
        if (previousPoint != null)
        {
            SetPointMaterialClear(previousPoint);
        }
        
        // Kiểm tra màu sắc - chỉ cuộn chỉ nếu màu khớp
        ColorRope currentKnitColor = GetKnitColor(currentKnitIndex);
        if (targetSpool != null && targetSpool.color != currentKnitColor)
        {
            // Màu không khớp, bỏ qua và chuyển sang knit tiếp theo
            MoveToNextTarget();
            return;
        }

        // Thay đổi material của line theo màu của knit hiện tại
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
            
            // Lưu điểm hiện tại làm điểm trước đó cho lần sau
            previousPoint = targetTransform;
        });
        
        // Thêm delay trước khi di chuyển tiếp
        movementSequence.AppendInterval(0.2f);
        
        // Tiếp tục di chuyển đến target tiếp theo
        movementSequence.AppendCallback(MoveToNextTarget);
    }
    
    /// <summary>
    /// Tính toán độ dài rope mới dựa trên số lần di chuyển
    /// </summary>
    private float CalculateNewRopeLength()
    {
        int totalMoves = GetTotalMovesCount();
        float decreaseAmount = ropeLengthDecreaseRate * totalMoves;
        float newLength = initialRopeLength - decreaseAmount;
        
        // Đảm bảo không nhỏ hơn độ dài tối thiểu
        return Mathf.Max(newLength, minRopeLength);
    }
    
    /// <summary>
    /// Tính tổng số lần di chuyển đã thực hiện
    /// </summary>
    private int GetTotalMovesCount()
    {
        int moves = 0;
        
        // Đếm số lần di chuyển trong các knit trước đó
        for (int i = 0; i < currentKnitIndex; i++)
        {
            if (knitItems[i] != null)
            {
                moves += knitItems[i].transform.childCount;
            }
        }
        
        // Cộng thêm số lần di chuyển trong knit hiện tại
        moves += currentChildIndex;
        
        return moves;
    }
    

    // Không cần method này nữa vì đã tích hợp vào SmoothMoveToTarget()

    private void CompletedAllKnitItems()
    {
        isYarnActive = false;
        isMovingToNextKnit = false;
        
        // if (targetSpool != null)
        // {
        //     targetSpool.OnYarnCompletedAllKnits();
        // }
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

        // Lưu độ dài ban đầu của rope
        initialRopeLength = currentRope.ropeLength;

        LineRenderer lineRenderer = currentRope.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            // Lấy material theo màu của knit đầu tiên
            ColorRope knitColor = GetKnitColor(0);
            Material yarnMaterial = GetMaterialByColor(knitColor);
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
        isMovingToNextKnit = false;
        
        // Hủy tất cả DOTween animations
        if (movementSequence != null && movementSequence.IsActive())
        {
            movementSequence.Kill();
        }
        
        StopAllCoroutines();
        currentKnitIndex = -1;
        currentChildIndex = 0; // Reset về 0 để phù hợp với logic mới
        previousPoint = null; // Reset điểm trước đó
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
