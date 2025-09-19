using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knit : MonoBehaviour
{
    [SerializeField] private Transform[] knitItems;
    [SerializeField] private Material material;
    [SerializeField] private WoolYarn woolYarns;
    [SerializeField] private float pointCreationDelay = 0.1f;
    private WoolYarn currentWoolYarn;
    private Coroutine lineCreationCoroutine;

    public void CreateLine()
    {
        if (knitItems == null || knitItems.Length == 0)
        {
            Debug.LogWarning("KnitItems array is null or empty!");
            return;
        }

        if (lineCreationCoroutine != null)
        {
            StopCoroutine(lineCreationCoroutine);
        }
        
      
        WoolYarn currentWoolYarn = Instantiate(woolYarns, transform.position, Quaternion.identity);
        currentWoolYarn.Initialize(material);
        lineCreationCoroutine = StartCoroutine(CreateLineCoroutine(currentWoolYarn));
    }

    private IEnumerator CreateLineCoroutine(WoolYarn woolYarn)
    {
        woolYarn.Clear();
        
        for (int i = 0; i < knitItems.Length; i++)
        {
            if (knitItems[i] != null)
            {
                woolYarn.AddPoint(knitItems[i].position);
                
                Debug.Log($"Added point {i}: {knitItems[i].position}");
                yield return new WaitForSeconds(pointCreationDelay);
            }
            else
            {
                Debug.LogWarning($"KnitItem at index {i} is null!");
            }
        }
        
        Debug.Log("Line creation completed!");
        lineCreationCoroutine = null;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            CreateLine();
        }
    }

    /// <summary>
    /// Dừng việc tạo line hiện tại
    /// </summary>
    public void StopLineCreation()
    {
        if (lineCreationCoroutine != null)
        {
            StopCoroutine(lineCreationCoroutine);
            lineCreationCoroutine = null;
            Debug.Log("Line creation stopped!");
        }
    }

    public void ClearLine()
    {
        if (currentWoolYarn != null)
        {
            currentWoolYarn.Clear();
        }
    }

    /// <summary>
    /// Xóa hoàn toàn WoolYarn object
    /// </summary>
    public void DestroyLine()
    {
        StopLineCreation();
        
        if (currentWoolYarn != null)
        {
            DestroyImmediate(currentWoolYarn.gameObject);
            currentWoolYarn = null;
        }
    }

    /// <summary>
    /// Thiết lập tốc độ tạo điểm mới
    /// </summary>
    /// <param name="delay">Thời gian chờ giữa các điểm (giây)</param>
    public void SetPointCreationDelay(float delay)
    {
        pointCreationDelay = Mathf.Max(0f, delay);
    }

    /// <summary>
    /// Lấy số lượng điểm hiện tại trong line
    /// </summary>
    /// <returns>Số điểm trong line</returns>
    public int GetPointCount()
    {
        return knitItems?.Length ?? 0;
    }

    /// <summary>
    /// Kiểm tra line có đang được tạo không
    /// </summary>
    /// <returns>True nếu đang tạo line</returns>
    public bool IsCreatingLine()
    {
        return lineCreationCoroutine != null;
    }

    private void OnDisable()
    {
        StopLineCreation();
    }

    private void OnDestroy()
    {
        DestroyLine();
    }

}
