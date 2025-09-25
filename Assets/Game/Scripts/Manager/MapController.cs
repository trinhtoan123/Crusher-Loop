using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;


public class MapController : MonoBehaviour
{
    [SerializeField] PillarController pillarController;
    [SerializeField] List<Knit> knitItems = new List<Knit>();
    private List<SpoolItem> activeSpools = new List<SpoolItem>();
    private GameObject currentYarnObject;
    public Action<SpoolItem> OnSpoolWinding;
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
            
            // Kiểm tra xem hàng này có màu cần tìm không
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
        
        GameObject currentYarn = null;
        
        for (int i = 0; i < consecutiveRows.Count; i++)
        {
            Knit currentRow = consecutiveRows[i];
            
            // Kiểm tra hướng cuộn len
            bool isLeftToRight = (i % 2 == 0);
            string direction = isLeftToRight ? "Trái → Phải" : "Phải → Trái";
            
            Debug.Log($"📏 Hàng {i + 1}: {direction}");
            
            // Chỉ xóa sợi len cũ khi cuộn từ trái sang phải
            if (isLeftToRight && currentYarn != null)
            {
                Destroy(currentYarn);
                Debug.Log($"🗑️ Xóa sợi len cũ (cuộn từ trái sang phải)");
            }
            else if (!isLeftToRight)
            {
                Debug.Log($"🔄 Giữ nguyên sợi len (cuộn từ phải sang trái)");
            }
            
            // Tạo sợi len mới cho hàng này
            yield return StartCoroutine(WindSingleRowWithNewYarn(spool, currentRow, isLeftToRight));
            currentYarn = currentYarnObject;
            
            // Delay giữa các hàng
            yield return new WaitForSeconds(0.5f);
        }
        
        // Xóa sợi len cuối cùng sau khi hoàn thành tất cả hàng
        if (currentYarn != null)
        {
            Destroy(currentYarn);
            Debug.Log($"🗑️ Xóa sợi len cuối cùng (hoàn thành tất cả hàng)");
        }
        
        Debug.Log($"✅ Hoàn thành cuộn len qua {consecutiveRows.Count} hàng");
    }
    
    /// <summary>
    /// Cuộn len qua một hàng cụ thể với sợi len mới
    /// </summary>
    private IEnumerator WindSingleRowWithNewYarn(SpoolItem spool, Knit row, bool leftToRight)
    {
        GameObject yarnObject = null;
        
        // Tìm tất cả KnitChild có màu của spool trong hàng này
        List<KnitChild> targetChildren = new List<KnitChild>();
        
        foreach (var knitChild in row.KnitItems)
        {
            if (knitChild != null && knitChild.Color == spool.color)
            {
                targetChildren.Add(knitChild);
            }
        }
        
        if (targetChildren.Count == 0)
        {
            Debug.LogWarning($"⚠️ Không tìm thấy KnitChild nào có màu {spool.color} trong hàng");
            yield break;
        }
        
        // Sắp xếp theo hướng
        if (leftToRight)
        {
            targetChildren.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        }
        else
        {
            targetChildren.Sort((a, b) => b.transform.position.x.CompareTo(a.transform.position.x));
        }
        
        string direction = leftToRight ? "trái sang phải" : "phải sang trái";
        Debug.Log($"🎯 Cuộn len qua {targetChildren.Count} items từ {direction}");
        
        // Tạo hiệu ứng cuộn len với sợi len mới
        yield return StartCoroutine(CreateWindingEffectWithNewYarn(spool, targetChildren));
        yarnObject = currentYarnObject;
    }
    
    /// <summary>
    /// Tạo hiệu ứng cuộn len với sợi len mới
    /// </summary>
    private IEnumerator CreateWindingEffectWithNewYarn(SpoolItem spool, List<KnitChild> targetChildren)
    {
        // Tạo sợi len mới
        GameObject yarnObject = new GameObject($"Yarn_{spool.color}_{Time.time}");
        LineRenderer yarnRenderer = yarnObject.AddComponent<LineRenderer>();
        
        // Cấu hình LineRenderer
        yarnRenderer.material = GetYarnMaterial(spool.color);
        yarnRenderer.startWidth = 0.1f;
        yarnRenderer.endWidth = 0.1f;
        
        // Tạo đường đi
        List<Vector3> pathPoints = new List<Vector3>();
        
        // Thêm vị trí của tất cả target children
        foreach (var child in targetChildren)
        {
            pathPoints.Add(child.transform.position);
        }
        
        // Thêm vị trí của spool
        pathPoints.Add(spool.transform.position);
        
        // Cấu hình LineRenderer
        yarnRenderer.positionCount = pathPoints.Count;
        for (int i = 0; i < pathPoints.Count; i++)
        {
            yarnRenderer.SetPosition(i, pathPoints[i]);
        }
        
        // Animation len chạy
        float duration = 1.5f;
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
        
        // Bắt đầu cuộn len
        spool.StartWinding(null);
        
        // Hiệu ứng cuộn len
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log($"✨ Hoàn thành cuộn len cho hàng này");
        
        // Lưu yarnObject vào biến class để có thể truy cập từ bên ngoài
        currentYarnObject = yarnObject;
    }
    
    /// <summary>
    /// Làm cuộn len biến mất
    /// </summary>
    private IEnumerator MakeSpoolDisappear(SpoolItem spool)
    {
        Debug.Log($"👻 Cuộn len {spool.color} biến mất");
        
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


    /// <summary>
    /// Dừng tất cả quá trình cuộn len đang diễn ra
    /// </summary>
    public void StopAllYarnWinding()
    {
        Debug.Log("🛑 Dừng tất cả quá trình cuộn len...");
        
        foreach (var knit in knitItems)
        {
            if (knit != null)
            {
                knit.StopAutoWinding();
            }
        }
    }

    /// <summary>
    /// Kiểm tra input debug để test chức năng
    /// </summary>
    private void Update()
    {
        HandleDebugInput();
    }

    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("🎯 Nhấn R - Bắt đầu cuộn len thông minh");
        }
        
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("🛑 Nhấn Y - Dừng tất cả quá trình cuộn len");
            StopAllYarnWinding();
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("📋 Nhấn P - Hiển thị thông tin chi tiết về hệ thống cuộn len");
            ShowSmartWindingDetails();
        }
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("🔄 Nhấn L - Kiểm tra chuỗi màu liên tiếp");
            CheckConsecutiveColorSequences();
        }
    }


    /// <summary>
    /// Hiển thị thông tin chi tiết về hệ thống cuộn len thông minh
    /// </summary>
    public void ShowSmartWindingDetails()
    {
        Debug.Log("🧠 === THÔNG TIN HỆ THỐNG CUỘN LEN THÔNG MINH ===");
        
        // Tìm tất cả cuộn len có sẵn
        SpoolItem[] availableSpools = FindObjectsOfType<SpoolItem>();
        Debug.Log($"🎯 Tìm thấy {availableSpools.Length} cuộn len trong scene");
        
        // Sắp xếp knitItems theo vị trí Y (từ trên xuống dưới)
        List<Knit> sortedKnits = new List<Knit>(knitItems);
        sortedKnits.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
        
        foreach (var spool in availableSpools)
        {
            if (spool != null)
            {
                ColorRope spoolColor = spool.color;
                List<Knit> consecutiveRows = FindConsecutiveRowsWithColor(spoolColor, sortedKnits);
                
                Debug.Log($"🎨 Cuộn len {spoolColor}:");
                Debug.Log($"   📋 Có {consecutiveRows.Count} hàng liên tiếp");
                
                for (int i = 0; i < consecutiveRows.Count; i++)
                {
                    bool isLeftToRight = (i % 2 == 0);
                    string direction = isLeftToRight ? "Trái → Phải" : "Phải → Trái";
                    Debug.Log($"   📏 Hàng {i + 1}: {direction}");
                }
                
                if (consecutiveRows.Count == 0)
                {
                    Debug.Log($"   ❌ Không có hàng nào có màu {spoolColor}");
                }
            }
        }
        
        Debug.Log("🧠 === KẾT THÚC THÔNG TIN ===");
    }
    
    /// <summary>
    /// Kiểm tra chuỗi màu liên tiếp cho tất cả cuộn len
    /// </summary>
    public void CheckConsecutiveColorSequences()
    {
        Debug.Log("🔍 === KIỂM TRA CHUỖI MÀU LIÊN TIẾP ===");
        
        // Sắp xếp knitItems theo vị trí Y (từ trên xuống dưới)
        List<Knit> sortedKnits = new List<Knit>(knitItems);
        sortedKnits.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
        
        // Tìm tất cả cuộn len có sẵn
        SpoolItem[] availableSpools = FindObjectsOfType<SpoolItem>();
        
        foreach (var spool in availableSpools)
        {
            if (spool != null)
            {
                ColorRope spoolColor = spool.color;
                List<Knit> consecutiveRows = FindConsecutiveRowsWithColor(spoolColor, sortedKnits);
                
                Debug.Log($"🎨 Cuộn len {spoolColor}: {consecutiveRows.Count} hàng liên tiếp");
                
                if (consecutiveRows.Count > 0)
                {
                    Debug.Log($"   ✅ Có thể cuộn len qua {consecutiveRows.Count} hàng");
                    for (int i = 0; i < consecutiveRows.Count; i++)
                    {
                        bool isLeftToRight = (i % 2 == 0);
                        string direction = isLeftToRight ? "Trái → Phải" : "Phải → Trái";
                        Debug.Log($"   📏 Hàng {i + 1}: {direction}");
                    }
                }
                else
                {
                    Debug.Log($"   ❌ Không thể cuộn len - không có hàng liên tiếp");
                }
            }
        }
        
        Debug.Log("🔍 === KẾT THÚC KIỂM TRA ===");
    }

}

