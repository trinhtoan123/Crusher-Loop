using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GogoGaga.OptimizedRopesAndCables;

public class Knit : MonoBehaviour
{
    [Header("Knit Settings")]
    [SerializeField] private Transform[] knitItems;
    [SerializeField] private Material yarnMaterial;
    
    [Header("Rope Settings")]
    [SerializeField] private Rope ropePrefab;
    private Rope currentRope;
    private SpoolItem targetSpool;
    private int currentKnitIndex = -1;
    private int currentChildIndex = 1;
    private bool isMovingToNextKnit = false;
    private bool isYarnActive = false;
    [SerializeField] private float yarnMoveSpeed = 3f;  
    
    public void CreateLine()
    {
        if (knitItems == null || knitItems.Length == 0)
        {
            return;
        }
        
        targetSpool = FindAvailableSpoolItem();
        if (targetSpool == null)
        {
            return;
        }
        
        CreateYarnRope();
        if (currentRope == null)
        {
            return;
        }
        
        currentKnitIndex = knitItems.Length - 1;
        currentChildIndex = 1;
        
        Transform endPosition = GetCurrentChildPosition();
   
        SetRopePoints(targetSpool.GetYarnConnectionPoint(), endPosition);
        targetSpool.StartWinding(currentRope);
        targetSpool.OnYarnReachKnit(currentKnitIndex, knitItems.Length);

        isYarnActive = true;
        StartCoroutine(StartMovementAfterDelay());
    }


    private SpoolItem FindAvailableSpoolItem()
    {
        // Tìm SpoolItem có sẵn trong các PillarItem
        if (PillarController.instance == null || PillarController.instance.PillarItems == null)
        {
            return null;
        }

        foreach (var pillar in PillarController.instance.PillarItems)
        {
            if (pillar.HasSpoolItem())
            {
                SpoolItem spool = pillar.GetSpoolItem();
                if (spool != null && !spool.IsWindingYarn())
                {
                    return spool;
                }
            }
        }
        
        return null;
    }

    void Update()
    {
        if (isYarnActive && isMovingToNextKnit && currentRope != null)
        {
            MoveYarnHeadToTarget();
        }
    }

    #region Yarn Movement Logic

    private Transform GetCurrentChildPosition()
    {
        if (knitItems == null || currentKnitIndex < 0 || currentKnitIndex >= knitItems.Length)
        {
            Debug.LogError($"currentKnitIndex không hợp lệ: {currentKnitIndex}");
            return null;
        }
        
        Transform currentKnit = knitItems[currentKnitIndex];
        if (currentKnit == null)
        {
            Debug.LogError($"knitItems[{currentKnitIndex}] là null!");
            return null;
        }
        
        if (currentChildIndex < 0 || currentChildIndex >= currentKnit.childCount)
        {
            Debug.LogError($"currentChildIndex không hợp lệ: {currentChildIndex}, childCount: {currentKnit.childCount}");
            return null;
        }
        
        Transform childTransform = currentKnit.GetChild(currentChildIndex);
        return childTransform;
    }

    private IEnumerator StartMovementAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        MoveToNextTarget();
    }

    private void MoveToNextTarget()
    {
        if (currentChildIndex > 0)
        {
            currentChildIndex--;
            isMovingToNextKnit = true;
            Debug.LogError($"Di chuyển đến child {currentChildIndex} của knit {currentKnitIndex}");
        }
        else if (currentKnitIndex > 0)
        {
            currentKnitIndex--;
            currentChildIndex = 1;
            isMovingToNextKnit = true;
            Debug.Log($"Chuyển sang knit {currentKnitIndex}, child {currentChildIndex}");
        }
        else
        {
            // Hoàn thành tất cả
            Debug.Log("Hoàn thành tất cả knit items");
            CompletedAllKnitItems();
        }
    }


    private void MoveYarnHeadToTarget()
    {
        Transform targetTransform = GetCurrentChildPosition();
        Debug.LogError(targetTransform.gameObject.name);
        if (targetTransform == null || currentRope == null || currentRope.EndPoint == null)
        {
            return;
        }
        
        Vector3 currentPos = currentRope.EndPoint.position;
        Vector3 targetPos = targetTransform.position;
        
        // Kiểm tra nếu đã đến gần target
        float distance = Vector3.Distance(currentPos, targetPos);
        if (distance < 0.1f)
        {
            // Đã đến target, chuyển sang vị trí tiếp theo
            isMovingToNextKnit = false;
            Debug.Log($"Đầu sợi len đã đến knit {currentKnitIndex}, child {currentChildIndex}");
            
            // Set endpoint thành target transform chính thức
            currentRope.SetEndPoint(targetTransform, false);
            
            if (targetSpool != null)
            {
                targetSpool.OnYarnReachKnit(currentKnitIndex, knitItems.Length);
            }
            
            StartCoroutine(DelayedMoveToNext());
            return;
        }
        
        // Di chuyển đầu sợi len về phía target
        Vector3 direction = (targetPos - currentPos).normalized;
        Vector3 newPosition = currentPos + direction * yarnMoveSpeed * Time.deltaTime;
        
        // Đảm bảo có temp endpoint để di chuyển
        EnsureTempEndpoint(newPosition);
        
        // Cập nhật độ dài rope để tự nhiên
        float ropeDistance = Vector3.Distance(currentRope.StartPoint.position, newPosition);
    }
    
    /// <summary>
    /// Đảm bảo có temp endpoint để di chuyển
    /// </summary>
    private void EnsureTempEndpoint(Vector3 position)
    {
        GameObject currentEndpoint = currentRope.EndPoint.gameObject;
        
        if (currentEndpoint.name == "TempYarnEndpoint")
        {
            // Đã có temp endpoint, chỉ cần update position
            currentEndpoint.transform.position = position;
        }
        else
        {
            // Tạo temp endpoint mới
            GameObject newTempEndpoint = new GameObject("TempYarnEndpoint");
            newTempEndpoint.transform.position = position;
            currentRope.SetEndPoint(newTempEndpoint.transform, false);
        }
    }

    private IEnumerator DelayedMoveToNext()
    {
        yield return new WaitForSeconds(0.3f);
        MoveToNextTarget();
    }

    private void CompletedAllKnitItems()
    {
        isYarnActive = false;
        isMovingToNextKnit = false;
        
        if (targetSpool != null)
        {
            targetSpool.OnYarnCompletedAllKnits();
        }
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

        LineRenderer lineRenderer = currentRope.GetComponent<LineRenderer>();
        if (lineRenderer != null && yarnMaterial != null)
        {
            lineRenderer.material = yarnMaterial;
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
        StopAllCoroutines();
        currentKnitIndex = -1;
        currentChildIndex = 1;
        targetSpool = null;
        
        CleanupTempEndpoints();
    }
    

    private void CleanupTempEndpoints()
    {
        GameObject[] tempEndpoints = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject obj in tempEndpoints)
        {
            if (obj != null && obj.name == "TempYarnEndpoint")
            {
                Destroy(obj);
            }
        }
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
