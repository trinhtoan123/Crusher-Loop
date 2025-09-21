using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] PillarController pillarController;
    [SerializeField] List<Knit> knitItems = new List<Knit>();


    public void Initialize(LevelManager levelManager)
    {
        pillarController.Initialize(levelManager);
        
        // Subscribe to event khi SpoolItem được đặt lên PillarItem
        PillarItem.OnSpoolItemPlaced += OnSpoolItemPlacedOnPillar;
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var knitItem in knitItems)
            {
                knitItem.CreateLine();
            }
        }
    }

    /// <summary>
    /// Được gọi khi SpoolItem được đặt lên PillarItem
    /// Tự động kích hoạt tạo sợi len cho tất cả Knit items
    /// </summary>
    private void OnSpoolItemPlacedOnPillar(SpoolItem spoolItem)
    {
        Debug.Log($"SpoolItem {spoolItem.name} đã được đặt lên PillarItem - chuẩn bị tự động tạo sợi len");
        
        // Thêm delay nhỏ để đảm bảo SpoolItem đã ổn định
        StartCoroutine(CreateYarnAfterDelay(0.5f));
    }

    /// <summary>
    /// Tạo sợi len sau delay
    /// CHỈ kích hoạt 1 Knit item đầu tiên không có sợi len
    /// </summary>
    private IEnumerator CreateYarnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Debug.Log($"Tự động tìm Knit item khả dụng trong {knitItems.Count} items");
        
        // CHỈ kích hoạt 1 knit item đầu tiên không có sợi len hoạt động
        foreach (var knitItem in knitItems)
        {
            if (knitItem != null && !knitItem.IsYarnActive())
            {
                Debug.Log($"Kích hoạt {knitItem.name} tạo sợi len");
                knitItem.CreateLine();
                break; // ← CHỈ KÍCH HOẠT 1 KNIT DUY NHẤT!
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe event để tránh memory leak
        PillarItem.OnSpoolItemPlaced -= OnSpoolItemPlacedOnPillar;
    }
}

