using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;


public class MapController : MonoBehaviour
{
    [SerializeField] PillarController pillarController;
    [SerializeField] List<Knit> knitItems = new List<Knit>();
    [SerializeField] private RopeSetting ropeSettingPrefab;
    private List<SpoolItem> activeSpools = new List<SpoolItem>();
    private RopeSetting currentYarnRope;
    public Action<SpoolItem> OnSpoolWinding;
    private Dictionary<SpoolItem, bool> spoolWindingStates = new Dictionary<SpoolItem, bool>();
    private Dictionary<SpoolItem, RopeSetting> spoolRopes = new Dictionary<SpoolItem, RopeSetting>();
    void OnEnable()
    {
        OnSpoolWinding += IEStartWinding;
    }
    void OnDisable()
    {
        OnSpoolWinding -= IEStartWinding;
    }
    public void Initialize(LevelManager levelManager)
    {
        pillarController.Initialize(levelManager);
        foreach (var knit in knitItems)
        {
            knit.Initialize(levelManager, this);
        }
    }
    public void IEStartWinding(SpoolItem spoolItem)
    {
        if (knitItems == null || knitItems.Count == 0)
        {
            return;
        }
        
        if (spoolWindingStates.ContainsKey(spoolItem) && spoolWindingStates[spoolItem])
        {
            return;
        }
        
        // Đánh dấu spool đang cuộn
        spoolWindingStates[spoolItem] = true;
        
        StartCoroutine(ProcessSmartWinding(knitItems,spoolItem));
    }

    private IEnumerator ProcessSmartWinding(List<Knit> sortedKnits, SpoolItem itemSpool)
    {
        if (itemSpool.IsOnPillar)
        {
            activeSpools.Add(itemSpool);
        }
        foreach (var spool in activeSpools)
        {
            yield return StartCoroutine(ProcessSpoolWinding(spool, sortedKnits));
        }
    }

    private IEnumerator ProcessSpoolWinding(SpoolItem spool, List<Knit> sortedKnits)
    {
        ColorRope spoolColor = spool.color;
        Debug.Log($"🎨 Bắt đầu cuộn len màu {spoolColor}");
        List<Knit> consecutiveRows = FindConsecutiveRowsWithColor(spoolColor, sortedKnits);

        if (consecutiveRows.Count == 0)
        {
            yield break;
        }
        yield return StartCoroutine(WindThroughConsecutiveRows(spool, consecutiveRows));

        yield return StartCoroutine(MakeSpoolDisappear(spool));
    }
    private List<Knit> FindConsecutiveRowsWithColor(ColorRope color, List<Knit> sortedKnits)
    {
        List<Knit> consecutiveRows = new List<Knit>();
        bool foundFirstRow = false;
        
        for (int i = 0; i < sortedKnits.Count; i++)
        {
            Knit currentKnit = sortedKnits[i];
            if (currentKnit == null) continue;
            
            bool hasTargetColor = HasColorInRow(currentKnit, color);
            
            if (hasTargetColor)
            {
                consecutiveRows.Add(currentKnit);
                foundFirstRow = true;
                Debug.Log($"✅ Hàng {i + 1} có màu {color}");
            }
            else
            {
                // Nếu đã tìm thấy hàng đầu tiên và gặp hàng không có màu thì dừng
                if (foundFirstRow)
                {
                    Debug.Log($"🛑 Hàng {i + 1} không có màu {color}, dừng chuỗi liên tiếp");
                    break;
                }
                // Nếu chưa tìm thấy hàng đầu tiên thì tiếp tục tìm
                Debug.Log($"⏭️ Hàng {i + 1} không có màu {color}, tiếp tục tìm...");
            }
        }
        
        return consecutiveRows;
    }
    
    /// <summary>
    /// Kiểm tra xem một hàng có chứa màu cụ thể không
    /// </summary>
    private bool HasColorInRow(Knit knit, ColorRope targetColor)
    {
        if (knit == null || knit.KnitItems == null) return false;
        
        foreach (var knitChild in knit.KnitItems)
        {
            if (knitChild != null && knitChild.Color == targetColor)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Cuộn len qua các hàng liên tiếp - tạo sợi len mới cho mỗi hàng
    /// </summary>
    private IEnumerator WindThroughConsecutiveRows(SpoolItem spool, List<Knit> consecutiveRows)
    {
        Debug.Log($"🔄 Bắt đầu cuộn len qua {consecutiveRows.Count} hàng liên tiếp");
        
        // Kiểm tra xem spool đã có dây chưa, nếu có thì hủy dây cũ
        if (spoolRopes.ContainsKey(spool) && spoolRopes[spool] != null)
        {
            Destroy(spoolRopes[spool].gameObject);
            spoolRopes.Remove(spool);
            Debug.Log($"🗑️ Hủy dây cũ của spool {spool.color}");
        }
        
        // Tạo một dây duy nhất cho tất cả các hàng
        RopeSetting continuousRope = Instantiate(ropeSettingPrefab);
        continuousRope.name = $"Rope_{spool.color}";
        spoolRopes[spool] = continuousRope;
        
        // Cấu hình dây
        continuousRope.SetLineRenderer(GetYarnMaterial(spool.color));
        continuousRope.SetStart(spool.transform.GetChild(0).position);
        continuousRope.SetCurveParameters(0.5f, 1.0f, 0.05f, 0.8f);
        continuousRope.SetAutoMoveToNextEnd(true, 0.2f);
        
        // Thêm tất cả điểm đích từ tất cả các hàng vào một dây duy nhất
        for (int i = 0; i < consecutiveRows.Count; i++)
        {
            Knit currentRow = consecutiveRows[i];
            
            // Kiểm tra hướng cuộn len
            bool isLeftToRight = (i % 2 == 0);
            string direction = isLeftToRight ? "Trái → Phải" : "Phải → Trái";
            
            Debug.Log($"📏 Hàng {i + 1}: {direction}");
            
            // Lấy các điểm đích trong hàng này
            List<KnitChild> targetChildren = GetTargetChildrenInRow(currentRow, spool.color);
            
            // Sắp xếp theo hướng cuộn len
            if (isLeftToRight)
            {
                targetChildren.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
            }
            else
            {
                targetChildren.Sort((a, b) => b.transform.position.x.CompareTo(a.transform.position.x));
            }
            
            // Thêm các điểm đích vào dây (không tạo dây mới)
            foreach (var child in targetChildren)
            {
                continuousRope.AddEndPoint(child.transform.position);
            }
            
            Debug.Log($"📏 Hàng {i + 1}: {direction} - {targetChildren.Count} điểm");
        }
        
        // Chờ cho đến khi hoàn thành tất cả điểm đích
        yield return new WaitUntil(() => continuousRope.GetCurrentEndIndex() == continuousRope.GetEndPointsCount() - 1);
        yield return new WaitForSeconds(0.3f);
        
        // Bắt đầu cuộn len
        spool.StartWinding(null);
        
        yield return new WaitForSeconds(0.5f);
        
        currentYarnRope = continuousRope;
        
        Debug.Log($"✅ Hoàn thành cuộn len qua {consecutiveRows.Count} hàng với một dây liên tục");
    }
    
    /// <summary>
    /// Lấy các KnitChild có màu cụ thể trong một hàng
    /// </summary>
    private List<KnitChild> GetTargetChildrenInRow(Knit row, ColorRope targetColor)
    {
        List<KnitChild> targetChildren = new List<KnitChild>();
        
        foreach (var knitChild in row.KnitItems)
        {
            if (knitChild != null && knitChild.Color == targetColor)
            {
                targetChildren.Add(knitChild);
            }
        }
        
        return targetChildren;
    }
    
    
    /// <summary>
    /// Làm cuộn len biến mất
    /// </summary>
    private IEnumerator MakeSpoolDisappear(SpoolItem spool)
    {
        Debug.Log($"👻 Cuộn len {spool.color} biến mất");
        
        // Đánh dấu spool dừng cuộn
        spoolWindingStates[spool] = false;
        
        // Hủy dây tương ứng
        if (spoolRopes.ContainsKey(spool))
        {
            RopeSetting ropeToDestroy = spoolRopes[spool];
            if (ropeToDestroy != null)
            {
                Destroy(ropeToDestroy.gameObject);
                Debug.Log($"🗑️ Đã hủy dây cho spool {spool.color}");
            }
            spoolRopes.Remove(spool);
        }
        
        // Hiệu ứng biến mất
        spool.transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                spool.gameObject.SetActive(false);
            });
        
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// Lấy material cho sợi len
    /// </summary>
    private Material GetYarnMaterial(ColorRope color)
    {
        if (GameManager.Instance?.SpoolData?.SpoolColors != null)
        {
            var colorData = GameManager.Instance.SpoolData.SpoolColors.Find(x => x.color == color);
            if (colorData != null)
            {
                return colorData.materialKnit;
            }
        }
        
        return new Material(Shader.Find("Sprites/Default"));
    }


 
  
}
