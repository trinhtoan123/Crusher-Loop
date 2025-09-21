using UnityEngine;
using DG.Tweening;
using System;

public class PillarItem : MonoBehaviour
{
    [SerializeField] private bool isEmpty;
    private SpoolItem spoolItem;
    
    // Event được gọi khi SpoolItem được đặt lên PillarItem
    public static event Action<SpoolItem> OnSpoolItemPlaced;
    public void Initialize()
    {
    }

    public void SetEmpty(bool isEmpty, SpoolItem spoolItem)
    {
        this.isEmpty = isEmpty;
        this.spoolItem = spoolItem;
        
        // Nếu SpoolItem mới được đặt lên (không empty và có spoolItem)
        if (!isEmpty && spoolItem != null)
        {
            Debug.Log($"SpoolItem {spoolItem.name} đã được đặt lên PillarItem {this.name}");
            // Trigger event để thông báo có SpoolItem mới
            OnSpoolItemPlaced?.Invoke(spoolItem);
        }
    }

    /// <summary>
    /// Lấy SpoolItem hiện tại trong Pillar
    /// </summary>
    public SpoolItem GetSpoolItem()
    {
        return spoolItem;
    }

    /// <summary>
    /// Kiểm tra Pillar có trống không
    /// </summary>
    public bool IsEmpty()
    {
        return isEmpty || spoolItem == null;
    }

    /// <summary>
    /// Kiểm tra có SpoolItem không
    /// </summary>
    public bool HasSpoolItem()
    {
        return spoolItem != null;
    }
 
}
