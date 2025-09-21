using System.Collections;
using DG.Tweening;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;
using UnityEngine.UI;
using GogoGaga.OptimizedRopesAndCables;


public class SpoolItem : MonoBehaviour
{
    [Header("Component")]
    [SerializeField]  public Color color;
    [SerializeField] private Direction direction;
    [SerializeField] private MeshRenderer material;
    [SerializeField] private Image image;
    [SerializeField] private GameObject[] rolls;
    [Space(10)]
    [Header("Setting Spool")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpDuration;
    [SerializeField] private float pillarDetectionDistance;
    [SerializeField] private float conveyorDetectionDistance;
    [SerializeField] private float jumpHeight;
    private float conveyorDistance;
    private SpoolController spoolController;
    private LevelManager levelManager;
    private BoxCollider myCollider;
    private Vector3 posConveyor = new Vector3(0, 0.5f, 0);
    private bool isOnConveyor = false;
    private bool isMovingToConveyor = false;
    private bool isMoving = false;
    private bool isMovingToPillar = false;
    private PathFollower pathFollower;
    
    [Header("Yarn Winding")]
    [SerializeField] private float windingSpeed = 2f;
    private Rope attachedRope;
    private bool isWindingYarn = false;
    private int activeRolls = 0;
    
    [Header("Rotation Effects")]
    [SerializeField] private Transform spoolTransform; // Transform của ống chỉ chính
    [SerializeField] private float baseRotationSpeed = 90f; // Tốc độ xoay cơ bản (độ/giây)
    [SerializeField] private Vector3 rotationAxis = Vector3.forward; // Trục xoay
    [SerializeField] private float mainRotationSpeed = 30f; // Tốc độ xoay quanh trục Y (độ/giây)
    private bool isRotating = false;
    
    [Header("Yarn Connection")]
    [SerializeField] private float yarnConnectionHeightOffset = 0.3f; // Offset Y cho điểm kết nối sợi len
    
    [Header("Wave Effect Settings")]
    [SerializeField] private bool enableWaveEffect = true; // Bật hiệu ứng wave khi cuộn len
    [SerializeField] private float waveAmplitude = 0.2f;   // Độ lớn của sóng
    [SerializeField] private float waveFrequency = 3f;     // Tần số sóng  
    [SerializeField] private float waveSpeed = 4f;         // Tốc độ sóng
    
    // Wave effect variables
    private Vector3[] originalRopePositions;
    private LineRenderer ropeLineRenderer;
    private float waveTime = 0f;
    private bool isWaveActive = false;

    public void Initialize(SpoolController spoolController, SpoolData spoolData)
    {
        this.spoolController = spoolController;
        this.levelManager = spoolController.LevelManager;
        pathFollower = gameObject.AddComponent<PathFollower>();
        pathFollower.enabled = false;
        myCollider = GetComponent<BoxCollider>();
        material.material = spoolData.spoolColors.Find(x => x.color == color).material;
        image.sprite = spoolData.spoolDirections.Find(x => x.direction == direction).sprite;
        
        // Tự động tìm spoolTransform nếu chưa được gán
        if (spoolTransform == null)
        {
            spoolTransform = transform.GetChild(0); // Giả sử ống chỉ là child đầu tiên
        }
        
        foreach (var roll in rolls)
        {
            // roll.GetComponent<MeshRenderer>().material = spoolData.spoolColors.Find(x => x.color == color).materialRoll;
            roll.SetActive(false);
        }
    }

    public void StartMoving()
    {
        if (!isOnConveyor && !isMovingToConveyor && !isMoving)
        {
            StartCoroutine(MoveInDirectionCoroutine());
        }
    }
    private IEnumerator MoveInDirectionCoroutine()
    {
        isMoving = true;
        Vector3 moveDirection = GetDirection();
        while (isMoving && !isOnConveyor && !isMovingToConveyor)
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
            CheckConveyorDistance();
            yield return null;
        }
    }
    private void CheckConveyorDistance()
    {
        if (levelManager.PathCreation != null)
        {
            PathCreator pathCreator = levelManager.PathCreation;
            Vector3 closestPoint = pathCreator.path.GetClosestPointOnPath(transform.position);
            float distanceToConveyor = Vector3.Distance(transform.position, closestPoint);
            
            if (distanceToConveyor <= conveyorDetectionDistance)
            {
                JumpToConveyor(closestPoint);
            }
        }
    }
    private void JumpToConveyor(Vector3 conveyorPoint)
    {
        if (isMovingToConveyor) return;

        isMovingToConveyor = true;
        isMoving = false;

        Vector3 availablePosition = levelManager.ConveyorController.GetAvailablePositionOnConveyor(conveyorPoint);
        conveyorDistance = levelManager.PathCreation.path.GetClosestDistanceAlongPath(availablePosition);

        transform.DOJump(availablePosition, jumpHeight, 1, jumpDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                StartFollowingPath();
            });
    }

    private void StartFollowingPath()
    {
        isOnConveyor = true;
        isMovingToConveyor = false;

        PathCreator pathCreator = levelManager.PathCreation;
        pathFollower.pathCreator = pathCreator;
        pathFollower.speed = moveSpeed;
        pathFollower.endOfPathInstruction = EndOfPathInstruction.Loop;

        // Đặt vị trí bắt đầu trên path
        if (conveyorDistance >= 0)
        {
            var field = typeof(PathFollower).GetField("distanceTravelled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(pathFollower, conveyorDistance);
            }
            
        }
        transform.GetChild(0).localPosition = posConveyor;
        myCollider.center = posConveyor;
        pathFollower.enabled = true;
       
        StartCoroutine(CheckPillarDistanceCoroutine());
    }


    private IEnumerator CheckPillarDistanceCoroutine()
    {
        while (isOnConveyor && !isMovingToPillar && pathFollower != null && pathFollower.enabled)
        {
            CheckPillarDistance();
            yield return null;
        }
    }

    private void CheckPillarDistance()
    {
        if (levelManager?.ConveyorController?.PositionStart == null) return;
        
        Transform positionStart = levelManager.ConveyorController.PositionStart;
        float distanceToPillar = Vector3.Distance(transform.position, positionStart.position);
        
        if (distanceToPillar <= pillarDetectionDistance)
        {
            JumpToPillar();
        }
    }
  
    private void JumpToPillar()
    {
        if (isMovingToPillar) return;
        PillarItem targetPillar = levelManager.PillarController.PillarItems[0];
        
        if (targetPillar == null)
        {
            return;
        }
        
        isMovingToPillar = true;
        
        if (pathFollower != null)
        {
            pathFollower.enabled = false;
        }
        Vector3 targetPosition = new Vector3(targetPillar.transform.position.x, targetPillar.transform.position.y + 0.2f, targetPillar.transform.position.z);
        transform.DOJump(targetPosition, jumpHeight, 1, jumpDuration)
            .OnComplete(() =>
            {
                targetPillar.SetEmpty(true,this);
            });
        isOnConveyor = false;
    }

    /// <summary>
    /// Bắt đầu thu sợi len vào cuộn
    /// </summary>
    public void StartWinding(Rope rope)
    {
        if (attachedRope != null)
        {
            StopWindingYarn();
        }
        
        attachedRope = rope;
        isWindingYarn = true;
        activeRolls = 0;
        
        ResetAllRolls();
        StartSpoolRotation();
        
        // Khởi tạo wave effect cho rope
        if (enableWaveEffect)
        {
            StartWaveEffect();
        }
    }
    public void OnYarnReachKnit(int knitIndex, int totalKnits)
    {
        if (!isWindingYarn) return;
        
        int rollIndex = totalKnits - 1 - knitIndex; 
        
        ActivateRoll(rollIndex);
    }

    /// <summary>
    /// Được gọi khi sợi len đã chạy hết tất cả knit items
    /// </summary>
    public void OnYarnCompletedAllKnits()
    {
        // Kích hoạt tất cả rolls còn lại (nếu có)
        ActivateAllRemainingRolls();
        
        // Hiệu ứng hoàn thành
        transform.DOPunchScale(Vector3.one * 0.15f, 0.8f, 8, 0.4f);
        
        // Có thể thêm hiệu ứng particle hoặc sound effect ở đây
    }

    /// <summary>
    /// Dừng thu sợi len
    /// </summary>
    public void StopWindingYarn()
    {
        isWindingYarn = false;
        if (attachedRope != null)
        {
            attachedRope = null;
        }
        
        // Dừng hiệu ứng xoay
        StopSpoolRotation();
        
        // Dừng wave effect
        StopWaveEffect();
        
        StopAllCoroutines();
    }

    private void ResetAllRolls()
    {
        if (rolls == null || rolls.Length == 0) return;
        
        activeRolls = 0;
        foreach (var roll in rolls)
        {
            roll.SetActive(false);
        }
    }

    private void ActivateRoll(int rollIndex)
    {
        if (rolls == null || rolls.Length == 0 || rollIndex < 0 || rollIndex >= rolls.Length || rolls[rollIndex] == null) 
        {
            return;
        }

        if (!rolls[rollIndex].activeInHierarchy)
        {
            rolls[rollIndex].SetActive(true);
            activeRolls++;
            Vector3 scale = rolls[rollIndex].transform.localScale;
            rolls[rollIndex].transform.localScale = Vector3.zero;
            rolls[rollIndex].transform.DOScale(scale, 0.4f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    // Hiệu ứng bounce nhẹ
                    if (rolls[rollIndex] != null)
                    {
                        rolls[rollIndex].transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 3, 0.2f);
                    }
                });
        }
    }

    private void ActivateAllRemainingRolls()
    {
        if (rolls == null) return;
        
        for (int i = 0; i < rolls.Length; i++)
        {
            if (!rolls[i].activeInHierarchy)
            {
                // Delay nhỏ giữa các roll để tạo hiệu ứng cascade
                StartCoroutine(ActivateRollWithDelay(i, i * 0.1f));
            }
        }
    }

    private IEnumerator ActivateRollWithDelay(int rollIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        ActivateRoll(rollIndex);
    }

  
    public bool IsWindingYarn()
    {
        return isWindingYarn;
    }
    
    /// <summary>
    /// Lấy điểm kết nối sợi len trên spool
    /// </summary>
    public Transform GetYarnConnectionPoint()
    {
        // Tạo empty GameObject làm điểm anchor với offset chính xác
        GameObject anchorPoint = new GameObject("YarnAnchor");
        Transform targetTransform = spoolTransform != null ? spoolTransform : transform;
        
        anchorPoint.transform.SetParent(targetTransform);
        anchorPoint.transform.localPosition = new Vector3(0, yarnConnectionHeightOffset, 0);
        anchorPoint.transform.position = targetTransform.position + Vector3.up * yarnConnectionHeightOffset;
        
        return anchorPoint.transform;
    }
    private void StartSpoolRotation()
    {
        if (isRotating) return;
        isRotating = true;
        
        float mainTime = 360f / mainRotationSpeed;
        transform.DORotate(new Vector3(0, 360f, 0), mainTime, RotateMode.LocalAxisAdd)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }
    
    private void StopSpoolRotation()
    {
        isRotating = false;
        // Dừng tất cả hiệu ứng xoay
        if (spoolTransform != null) spoolTransform.DOKill();
        transform.DOKill();
    }
    
    private void StartWaveEffect()
    {
        if (!enableWaveEffect || attachedRope == null) return;
        
        ropeLineRenderer = attachedRope.GetComponent<LineRenderer>();
        if (ropeLineRenderer == null) return;
        
        isWaveActive = true;
        waveTime = 0f;
        
        // Lưu vị trí gốc
        if (originalRopePositions == null || originalRopePositions.Length != ropeLineRenderer.positionCount)
        {
            originalRopePositions = new Vector3[ropeLineRenderer.positionCount];
        }
    }
    
    /// <summary>
    /// Dừng hiệu ứng wave
    /// </summary>
    private void StopWaveEffect()
    {
        isWaveActive = false;
        ropeLineRenderer = null;
        originalRopePositions = null;
    }
    
    
    private Vector3 GetDirection()
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector3.forward;
            case Direction.Down:
                return Vector3.back;
            case Direction.Left:
                return Vector3.left;
            case Direction.Right:
                return Vector3.right;
        }
        return Vector3.zero;
    }
    
}

