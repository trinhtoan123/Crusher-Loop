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
    private LevelManager levelManager;
    private MapController mapController;
    private SpoolItem targetSpool;
    private int currentKnitIndex = -1;
    private int currentChildIndex = 0;
    private Transform previousPoint = null;
    private bool isClear;
    
    // Hệ thống cuộn len tự động

    private Dictionary<ColorRope, SpoolItem> colorToSpoolMapping = new Dictionary<ColorRope, SpoolItem>();
    private bool isAutoWinding = false;
    public bool IsClear => isClear;
    
    public KnitChild[] KnitItems => knitItems;
    public void Initialize(LevelManager levelManager, MapController mapController)
    {
        this.levelManager = levelManager;
        this.mapController = mapController;
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

        if (targetSpool == null)
        {
            return;
        }

        currentKnitIndex = 0;
        currentChildIndex = 0;
        previousPoint = null;
      
    }
    public void ClearRow()
    {
        isClear = true;
    }
  
   
    private ColorRope GetKnitColor(int knitIndex)
    {
        KnitChild knit = knitItems[knitIndex];
        return knit.Color;
    }


    private void UpdateLineMaterial(ColorRope color)
    {
        // if (ropeSetting == null || ropeSetting.currentRope == null) return;

        // LineRenderer lineRenderer = ropeSetting.currentRope.GetComponent<LineRenderer>();
        // if (lineRenderer != null)
        // {
        //     Material newMaterial = GetColor(color);
        //     if (newMaterial != null)
        //     {
        //         lineRenderer.material = newMaterial;
        //     }
        // }
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

    /// <summary>
    /// Cuộn một sợi len đơn lẻ - chạy qua tất cả items cùng màu từ trái sang phải
    /// </summary>
    private IEnumerator WindSingleYarn(ColorRope color, SpoolItem targetSpool, int itemCount)
    {
        // Tìm tất cả KnitChild có màu tương ứng
        List<KnitChild> sourceKnits = new List<KnitChild>();
        foreach (var knitItem in knitItems)
        {
            if (knitItem.Color == color)
            {
                sourceKnits.Add(knitItem);
            }
        }
        
        if (sourceKnits.Count == 0)
        {
            Debug.LogError($"Không tìm thấy KnitChild nào có màu {color}");
            yield break;
        }
        
        // Sắp xếp từ trái sang phải (theo vị trí X)
        sourceKnits.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        
        Debug.Log($"Sợi len màu {color} sẽ chạy qua {sourceKnits.Count} items từ trái sang phải");
        
        // Tạo sợi len
        GameObject yarnObject = new GameObject("Yarn_" + color);
        LineRenderer yarnRenderer = yarnObject.AddComponent<LineRenderer>();
        
        // Cấu hình LineRenderer
        yarnRenderer.material = GetYarnMaterial(color);
        yarnRenderer.startWidth = 0.15f;
        yarnRenderer.endWidth = 0.15f;
        
        // Tạo đường đi từ item đầu tiên đến item cuối cùng, rồi đến SpoolItem
        List<Vector3> pathPoints = new List<Vector3>();
        
        // Thêm vị trí của tất cả items cùng màu (từ trái sang phải)
        foreach (var knit in sourceKnits)
        {
            pathPoints.Add(knit.transform.position);
        }
        
        // Thêm vị trí của SpoolItem
        pathPoints.Add(targetSpool.transform.position);
        
        // Cấu hình LineRenderer với số điểm cần thiết
        yarnRenderer.positionCount = pathPoints.Count;
        for (int i = 0; i < pathPoints.Count; i++)
        {
            yarnRenderer.SetPosition(i, pathPoints[i]);
        }
        
        // Animation len chạy từ trái sang phải
        float duration = 2f; // Thời gian chạy qua tất cả items
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // Tính toán vị trí hiện tại trên đường đi
            int segmentIndex = Mathf.FloorToInt(progress * (pathPoints.Count - 1));
            segmentIndex = Mathf.Clamp(segmentIndex, 0, pathPoints.Count - 2);
            
            float segmentProgress = (progress * (pathPoints.Count - 1)) - segmentIndex;
            Vector3 currentPos = Vector3.Lerp(pathPoints[segmentIndex], pathPoints[segmentIndex + 1], segmentProgress);
            
            // Cập nhật vị trí cuối của sợi len
            yarnRenderer.SetPosition(yarnRenderer.positionCount - 1, currentPos);
            
            yield return null;
        }
        
        // Len đã đến đích, bắt đầu cuộn
        targetSpool.StartWinding(null);
        
        // Hiệu ứng cuộn len
        yield return new WaitForSeconds(0.5f);
        
        // Xóa sợi len sau khi cuộn xong
        Destroy(yarnObject);
        
        Debug.Log($"Đã cuộn xong 1 sợi len màu {color} qua {itemCount} items");
    }
    private Material GetYarnMaterial(ColorRope color)
    {
        // Tìm material từ SpoolData
        if (GameManager.Instance?.SpoolData?.SpoolColors != null)
        {
            var colorData = GameManager.Instance.SpoolData.SpoolColors.Find(x => x.color == color);
            if (colorData != null)
            {
                return colorData.materialKnit; // Hoặc materialYarn nếu có
            }
        }
        
        // Material mặc định
        return new Material(Shader.Find("Sprites/Default"));
    }
    
    /// <summary>
    /// Dừng cuộn len tự động
    /// </summary>
    public void StopAutoWinding()
    {
        if (isAutoWinding)
        {
            StopAllCoroutines();
            isAutoWinding = false;
            Debug.Log("Đã dừng cuộn len tự động");
        }
    }
    
    /// <summary>
    /// Kiểm tra xem có đang cuộn len không
    /// </summary>
    public bool IsAutoWinding()
    {
        return isAutoWinding;
    }

}
