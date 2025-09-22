using System.Collections;
using DG.Tweening;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;
using UnityEngine.UI;
using GogoGaga.OptimizedRopesAndCables;


public class SpoolItem : MonoBehaviour
{
    [SerializeField]  public ColorRope color;
    [SerializeField] private Direction direction;
    [SerializeField] private MeshRenderer material;
    [SerializeField] private Image image;
    [SerializeField] private GameObject[] rolls;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpDuration;
    [SerializeField] private float pillarDetectionDistance;
    [SerializeField] private float conveyorDetectionDistance;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float mainRotationSpeed = 30f; 
    [SerializeField] private float yarnConnectionHeightOffset = 0.3f; 
    private bool isRotating = false;
    private Transform spoolTransform; 
    private float conveyorDistance;
    private SpoolController spoolController;
    private LevelManager levelManager;
    private BoxCollider myCollider;
    private Vector3 posConveyor = new Vector3(0, 0.5f, 0);
    private bool isOnConveyor = false;
    private bool isMovingToConveyor = false;
    private bool isMoving = false;
    private bool isMovingToPillar = false;
    private bool hasJumpedToPillar = false;
    private PathFollower pathFollower;
    private Rope attachedRope;
    private bool isWindingYarn = false;
    private int activeRolls = 0;
    private bool isBlocked = false;
    private float blockedTime = 0f;
    private Vector3 initialPosition;
 
    public void Initialize(SpoolController spoolController, SpoolData spoolData)
    {
        this.spoolController = spoolController;
        this.levelManager = spoolController.LevelManager;
        pathFollower = gameObject.AddComponent<PathFollower>();
        pathFollower.enabled = false;
        myCollider = GetComponent<BoxCollider>();
        material.material = spoolData.spoolColors.Find(x => x.color == color).material;
        image.sprite = spoolData.spoolDirections.Find(x => x.direction == direction).sprite;
        
        // Lưu vị trí ban đầu
        initialPosition = transform.position;
        
        // Reset trạng thái
        isOnConveyor = false;
        isMovingToConveyor = false;
        isMoving = false;
        isMovingToPillar = false;
        hasJumpedToPillar = false;
        isBlocked = false;
        blockedTime = 0f;
        
        if (spoolTransform == null)
        {
            spoolTransform = transform.GetChild(0);
        }
        
        foreach (var roll in rolls)
        {
            roll.GetComponent<MeshRenderer>().material = spoolData.spoolColors.Find(x => x.color == color).materialRoll;
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
            if (IsPathBlocked())
            {
                if (!isBlocked)
                {
                    StartCoroutine(ReturnToInitialPosition());
                }
                isBlocked = true;
                blockedTime += Time.deltaTime;
            }
            else
            {
             
                isBlocked = false;
                blockedTime = 0f;
                transform.position += moveDirection * moveSpeed * Time.deltaTime;
            }
            
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
                // Kiểm tra xem có item nào đang chặn đường không
                if (!IsPathBlocked())
                {
                    JumpToConveyor(closestPoint);
                }
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
        availablePosition = new Vector3(availablePosition.x, availablePosition.y + 0.7f, availablePosition.z);
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
            yield return new WaitForSeconds(0.1f); // Kiểm tra mỗi 0.1 giây thay vì mỗi frame
        }
    }

    private void CheckPillarDistance()
    {
        if (levelManager?.ConveyorController?.PositionStart == null || hasJumpedToPillar) return;
        
        Transform positionStart = levelManager.ConveyorController.PositionStart;
        float distanceToPillar = Vector3.Distance(transform.position, positionStart.position);
        
        if (distanceToPillar <= pillarDetectionDistance)
        {
            PillarItem availablePillar = PillarController.instance.GetAvailablePillar();
            if (availablePillar != null && !isMovingToPillar)
            {
                JumpToPillar(availablePillar);
            }
        }
    }
  
    private void JumpToPillar(PillarItem targetPillar)
    {
        isMovingToPillar = true;
        hasJumpedToPillar = true;
        if (pathFollower != null)
        {
            pathFollower.enabled = false;
        }
        Vector3 targetPosition = new Vector3(targetPillar.transform.position.x, targetPillar.transform.position.y + 0.2f, targetPillar.transform.position.z);
        transform.DOJump(targetPosition, jumpHeight, 1, jumpDuration)
            .OnComplete(() =>
            {
                targetPillar.SetEmpty(false, this);
                // Dừng coroutine kiểm tra pillar sau khi đã nhảy vào
                StopAllCoroutines();
            });
        isOnConveyor = false;
    }


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
        
     
    }
    public void OnYarnReachKnit(int knitIndex, int totalKnits)
    {
        if (!isWindingYarn) return;
        
        int rollIndex = totalKnits - 1 - knitIndex; 
        
        ActivateRoll(rollIndex);
    }

    
    public void OnYarnCompletedAllKnits()
    {
        // Kích hoạt tất cả rolls còn lại (nếu có)
        // ActivateAllRemainingRolls();
        
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
        if (spoolTransform != null) spoolTransform.DOKill();
        transform.DOKill();
    }
  
    private bool IsPathBlocked()
    {
        Vector3 moveDirection = GetDirection();
        Vector3 checkPosition = transform.position + moveDirection * 1f; 
        
        // Sử dụng Physics.OverlapSphere để kiểm tra va chạm
        Collider[] colliders = Physics.OverlapSphere(checkPosition, 0.5f);
        
        foreach (Collider col in colliders)
        {
            if (col == myCollider) continue;
            
            SpoolItem otherSpool = col.GetComponent<SpoolItem>();
            if (otherSpool != null)
            {
                return true; // Có item khác đang chặn đường
            }
        }
        
        return false; // Không có item nào chặn đường
    }

    private IEnumerator ReturnToInitialPosition()
    {
        isMoving = false;
        
        float returnDuration = Vector3.Distance(transform.position, initialPosition) / moveSpeed;
        
        transform.DOMove(initialPosition, returnDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                isBlocked = false;
                blockedTime = 0f;
                isMoving = false;
            });
            
        yield return new WaitForSeconds(returnDuration);
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

