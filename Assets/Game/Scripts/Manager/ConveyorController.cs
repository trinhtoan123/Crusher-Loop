using System.Collections;
using System.Collections.Generic;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;
using System;

public class ConveyorController : MonoBehaviour
{
    [SerializeField] Material materialRoad;
    [SerializeField] float scrollSpeed = 1f;
    [SerializeField] private PathCreator pathCreation;
    [SerializeField] private RoadMeshCreator roadMeshCreator;
    [SerializeField] private float conveyorHeight = 1;
    [SerializeField] private float itemSpacing = 1f;
    [SerializeField] private int maxTotalItemsOnConveyor = 20; // Giới hạn tổng số item trên băng chuyền
    [SerializeField] private float jamWarningThreshold = 0.8f; // Cảnh báo khi đạt 80% giới hạn

    private Vector2 textureOffset = Vector2.zero;
    private LevelManager levelManager;
    private List<float> occupiedPositions = new List<float>();

    // Meltdown system states
    private bool isJammed = false;
    private bool isMeltdown = false;
    private bool isWarningMode = false;

    // Events
    public static event Action<int, int> OnConveyorCapacityChanged; // current count, max count
    public static event Action OnConveyorJammed;
    public static event Action OnConveyorMeltdown;
    public static event Action OnConveyorWarning;

    public PathCreator PathCreation => pathCreation;
    public RoadMeshCreator RoadMeshCreator => roadMeshCreator;
    public float ConveyorHeight => conveyorHeight;
    public float ItemSpacing => itemSpacing;
    public int MaxTotalItemsOnConveyor => maxTotalItemsOnConveyor;

    // Meltdown system properties
    public bool IsJammed => isJammed;
    public bool IsMeltdown => isMeltdown;
    public bool IsWarningMode => isWarningMode;
    public int CurrentItemCount => occupiedPositions.Count;
    public void Initialize(LevelManager levelManager)
    {
        if (roadMeshCreator == null)
            roadMeshCreator = FindObjectOfType<RoadMeshCreator>();
        this.levelManager = levelManager;
        pathCreation.bezierPath.FlipNormals = false;
        roadMeshCreator.textureTiling = 10;
    }
    void Update()
    {
        AutoMoveConveyor();
    }
    private void AutoMoveConveyor()
    {
        if (materialRoad != null)
        {
            textureOffset.y -= scrollSpeed * Time.deltaTime;
            materialRoad.mainTextureOffset = textureOffset;
        }
    }

    public Vector3 GetAvailablePositionOnConveyor(Vector3 requestPosition)
    {
        if (pathCreation == null) return requestPosition;

        // Kiểm tra trạng thái meltdown hoặc jam
        if (isMeltdown)
        {
            Debug.LogError("MELTDOWN! Băng chuyền đã bị hỏng, không thể thêm item!");
            return requestPosition;
        }

        if (!CanAddItem())
        {
            Debug.LogWarning("Băng chuyền đã đạt giới hạn tối đa hoặc bị jam!");
            TriggerJam();
            return requestPosition;
        }

        float closestDistance = pathCreation.path.GetClosestDistanceAlongPath(requestPosition);
        float availableDistance = FindAvailableDistance(closestDistance);

        if (availableDistance < 0)
        {
            Debug.LogWarning("Không thể tìm thấy vị trí trống trên băng chuyền!");
            TriggerJam();
            return requestPosition;
        }

        Vector3 pathPosition = pathCreation.path.GetPointAtDistance(availableDistance);
        pathPosition.y += 0.4f;

        occupiedPositions.Add(availableDistance);

        CheckConveyorCapacity();

        return pathPosition;
    }

    private bool IsDistanceAvailable(float distance)
    {
        foreach (float occupiedDistance in occupiedPositions)
        {
            if (Mathf.Abs(distance - occupiedDistance) < itemSpacing)
            {
                return false;
            }
        }
        return true;
    }

    public void ReleasePosition(float distance)
    {
        occupiedPositions.Remove(distance);

        // Cập nhật trạng thái sau khi remove item
        CheckConveyorCapacity();
    }

    /// <summary>
    /// Tìm vị trí available đơn giản dựa trên itemSpacing
    /// </summary>
    private float FindAvailableDistance(float preferredDistance)
    {
        // Thử vị trí preferred trước
        if (IsDistanceAvailable(preferredDistance))
        {
            return preferredDistance;
        }

        // Tìm kiếm xung quanh vị trí preferred
        float maxSearchDistance = pathCreation.path.length;
        for (float offset = itemSpacing; offset < maxSearchDistance; offset += itemSpacing)
        {
            // Thử phía trước
            float forwardDistance = preferredDistance + offset;
            if (forwardDistance < maxSearchDistance && IsDistanceAvailable(forwardDistance))
            {
                return forwardDistance;
            }

            // Thử phía sau
            float backwardDistance = preferredDistance - offset;
            if (backwardDistance >= 0 && IsDistanceAvailable(backwardDistance))
            {
                return backwardDistance;
            }
        }

        return -1; // Không tìm thấy vị trí trống
    }

    #region Meltdown System Methods

    /// <summary>
    /// Kiểm tra xem có thể thêm item vào băng chuyền không
    /// </summary>
    private bool CanAddItem()
    {
        if (isJammed || isMeltdown) return false;
        return occupiedPositions.Count < maxTotalItemsOnConveyor;
    }

    /// <summary>
    /// Kiểm tra và cập nhật trạng thái băng chuyền dựa trên số lượng item hiện tại
    /// </summary>
    private void CheckConveyorCapacity()
    {
        int currentCount = occupiedPositions.Count;
        int maxCount = maxTotalItemsOnConveyor;
        float currentRatio = (float)currentCount / maxCount;

        // Trigger events
        OnConveyorCapacityChanged?.Invoke(currentCount, maxCount);

        // Kiểm tra warning mode
        bool shouldWarn = currentRatio >= jamWarningThreshold;
        if (shouldWarn != isWarningMode)
        {
            isWarningMode = shouldWarn;
            if (isWarningMode)
            {
                Debug.LogWarning($"CẢNH BÁO: Băng chuyền đang gần đầy! {currentCount}/{maxCount} ({currentRatio:P0})");
                OnConveyorWarning?.Invoke();
            }
        }

        // Kiểm tra jam condition
        if (currentCount >= maxCount && !isJammed)
        {
            TriggerJam();
        }
        else if (currentCount < maxCount && isJammed && !isMeltdown)
        {
            // Unjam nếu số lượng giảm xuống dưới giới hạn
            ClearJam();
        }
    }

    /// <summary>
    /// Kích hoạt trạng thái jam
    /// </summary>
    private void TriggerJam()
    {
        if (isJammed) return;

        isJammed = true;
        Debug.LogWarning($"BĂNG CHUYỀN BI TẮC! Số lượng item: {occupiedPositions.Count}/{maxTotalItemsOnConveyor}");
        OnConveyorJammed?.Invoke();

        // Bắt đầu countdown để meltdown
        StartCoroutine(MeltdownCountdown());
    }

    /// <summary>
    /// Xóa trạng thái jam
    /// </summary>
    private void ClearJam()
    {
        if (!isJammed || isMeltdown) return;

        isJammed = false;
        Debug.Log("Băng chuyền đã được giải phóng khỏi trạng thái tắc!");
    }

    /// <summary>
    /// Countdown để trigger meltdown nếu jam không được giải quyết
    /// </summary>
    private IEnumerator MeltdownCountdown()
    {
        float countdown = 5f; // 5 giây để giải quyết jam

        while (countdown > 0 && isJammed && !isMeltdown)
        {
            Debug.LogWarning($"MELTDOWN trong {countdown:F1} giây nếu không giải quyết jam!");
            countdown -= Time.deltaTime;
            yield return null;
        }

        // Nếu vẫn jam sau khi hết thời gian
        if (isJammed && !isMeltdown)
        {
            TriggerMeltdown();
        }
    }

    /// <summary>
    /// Kích hoạt trạng thái meltdown - game over
    /// </summary>
    private void TriggerMeltdown()
    {
        if (isMeltdown) return;

        isMeltdown = true;
        isJammed = true; // Ensure jam state is also set

        Debug.LogError("!!! MELTDOWN !!! Băng chuyền đã bị hỏng hoàn toàn!");
        OnConveyorMeltdown?.Invoke();

        // Dừng băng chuyền
        scrollSpeed = 0f;
    }

    /// <summary>
    /// Reset băng chuyền về trạng thái ban đầu (dùng cho restart)
    /// </summary>
    public void ResetConveyor()
    {
        isJammed = false;
        isMeltdown = false;
        isWarningMode = false;

        occupiedPositions.Clear();

        scrollSpeed = 1f; // Reset scroll speed

        Debug.Log("Băng chuyền đã được reset!");
    }

    /// <summary>
    /// Lấy thông tin chi tiết về trạng thái băng chuyền
    /// </summary>
    public ConveyorStatusInfo GetConveyorStatus()
    {
        return new ConveyorStatusInfo
        {
            currentItemCount = occupiedPositions.Count,
            maxItemCount = maxTotalItemsOnConveyor,
            capacityRatio = (float)occupiedPositions.Count / maxTotalItemsOnConveyor,
            isJammed = isJammed,
            isMeltdown = isMeltdown,
            isWarningMode = isWarningMode
        };
    }

    #endregion

}
[System.Serializable]
public struct ConveyorStats
{
    public float totalLength;
    public float itemSpacing;
    public int maxTotalItems;
    public int activeItems;
    
}

/// <summary>
/// Struct chứa thông tin chi tiết về trạng thái băng chuyền
/// </summary>
[System.Serializable]
public struct ConveyorStatusInfo
{
    public int currentItemCount;
    public int maxItemCount;
    public float capacityRatio;
    public bool isJammed;
    public bool isMeltdown;
    public bool isWarningMode;
 
}
