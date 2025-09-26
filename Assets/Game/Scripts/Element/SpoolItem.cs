using System.Collections;
using DG.Tweening;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;
using UnityEngine.UI;
using GogoGaga.OptimizedRopesAndCables;


public class SpoolItem : MonoBehaviour
{
    #region Fields & Properties
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
    [SerializeField] private float processRope;
    [SerializeField] private int countKnit;
    private bool isRotating = false;
    private Transform spoolTransform; 
    private float conveyorDistance;
    private SpoolController spoolController;
    private MapController mapController;
    private LevelManager levelManager;
    private BoxCollider myCollider;
    private Vector3 posConveyor = new Vector3(0, 0.5f, 0);
    private bool isOnConveyor = false;
    private bool isOnPillar = false;
    private PathFollower pathFollower;
    private Rope attachedRope;
    private bool isWindingYarn = false;
    private bool isCompletedWindingYarn;
    private int activeRolls = 0;
    private bool isBlocked = false;
    private Vector3 initialPosition;
    public bool IsOnPillar => isOnPillar;
    public bool IsCompletedWindingYarn => isCompletedWindingYarn;
    #endregion

    #region Initialization
    public void Initialize(SpoolController spoolController, SpoolData spoolData)
    {
        this.spoolController = spoolController;
        this.levelManager = spoolController.LevelManager;
        this.mapController = levelManager.MapController;
        pathFollower = gameObject.AddComponent<PathFollower>();
        pathFollower.enabled = false;
        myCollider = GetComponent<BoxCollider>();

        var spoolColorData = spoolData.SpoolColors?.Find(x => x.color == color);
        if (spoolColorData != null && material != null)
        {
            material.material = spoolColorData.materialSpool;
        }

        var spoolDirectionData = spoolData.SpoolDirections?.Find(x => x.direction == direction);
        if (spoolDirectionData != null && image != null)
        {
            image.sprite = spoolDirectionData.sprite;
        }

        initialPosition = transform.position;

        // Reset trạng thái
        isOnConveyor = false;
        isOnPillar = false;
        isBlocked = false;

        if (spoolTransform == null)
        {
            spoolTransform = transform.GetChild(0);
        }

        // Cập nhật material cho rolls
        if (spoolColorData != null && rolls != null)
        {
            foreach (var roll in rolls)
            {
                if (roll != null)
                {
                    var rollRenderer = roll.GetComponent<MeshRenderer>();
                    if (rollRenderer != null)
                    {
                        rollRenderer.material = spoolColorData.materialRoll;
                    }
                    roll.SetActive(false);
                }
            }
        }
        CreateAnchorPoint();
    }
    
    private Transform CreateAnchorPoint()
    {
        GameObject anchorPoint = new GameObject("Anchor");
        Transform targetTransform = spoolTransform != null ? spoolTransform : transform;
        anchorPoint.transform.SetParent(targetTransform);
        anchorPoint.transform.localPosition = new Vector3(0, yarnConnectionHeightOffset, 0);
        anchorPoint.transform.position = targetTransform.position + Vector3.up * yarnConnectionHeightOffset;
        return anchorPoint.transform;
    }
    #endregion

    #region Idle State - Đứng im
    public void StartMoving()
    {
        if (!isOnConveyor && !isOnPillar)
        {
            StartCoroutine(IEMoveInDirection());
        }
    }
    
    private IEnumerator IEMoveInDirection()
    {
        Vector3 moveDirection = GetDirection();
        while (!isOnConveyor && !isOnPillar)
        {
            if (TriggerSpoolOther())
            {
                if (!isBlocked)
                {
                    StartCoroutine(MoveToPath());
                }
                isBlocked = true;
            }
            else
            {
             
                isBlocked = false;
                transform.position += moveDirection * moveSpeed * Time.deltaTime;
            }
            
            CheckConveyorDistance();
            yield return null;
        }
    }

    private bool TriggerSpoolOther()
    {
        Vector3 moveDirection = GetDirection();
        Vector3 checkPosition = transform.position + moveDirection * 1f; 
        
        Collider[] colliders = Physics.OverlapSphere(checkPosition, 0.5f);
        
        foreach (Collider col in colliders)
        {
            if (col == myCollider) continue;
            
            SpoolItem otherSpool = col.GetComponent<SpoolItem>();
            if (otherSpool != null)
            {
                return true; 
            }
        }
        
        return false; 
    }

    private IEnumerator MoveToPath()
    {
        float returnDuration = Vector3.Distance(transform.position, initialPosition) / moveSpeed;
        
        transform.DOMove(initialPosition, returnDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                isBlocked = false;
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
    #endregion

    #region Conveyor State - Trên băng chuyền
    private void CheckConveyorDistance()
    {
        if (levelManager.PathCreation != null)
        {
            PathCreator pathCreator = levelManager.PathCreation;
            Vector3 closestPoint = pathCreator.path.GetClosestPointOnPath(transform.position);
            float distanceToConveyor = Vector3.Distance(transform.position, closestPoint);
            
            if (distanceToConveyor <= conveyorDetectionDistance)
            {
                if (!TriggerSpoolOther())
                {
                    JumpToConveyor(closestPoint);
                }
            }
        }
    }
    
    private void JumpToConveyor(Vector3 conveyorPoint)
    {
        if (isOnConveyor) return;

        Vector3 availablePosition = levelManager.ConveyorController.GetAvailablePositionOnConveyor(conveyorPoint);
        conveyorDistance = levelManager.PathCreation.path.GetClosestDistanceAlongPath(availablePosition);
        availablePosition = new Vector3(availablePosition.x, availablePosition.y + 0.7f, availablePosition.z);
        
        transform.DOJump(availablePosition, jumpHeight, 1, jumpDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                StartCoroutine(DelayedStartFollowingPath());
            });
    }

    private IEnumerator DelayedStartFollowingPath()
    {
        yield return new WaitForEndOfFrame();
        StartFollowingPath();
    }

    private void StartFollowingPath()
    {
        isOnConveyor = true;

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
        transform.DOKill();
        StartCoroutine(CheckPillarDistanceCoroutine());
    }

    private IEnumerator CheckPillarDistanceCoroutine()
    {
        while (isOnConveyor && !isOnPillar && pathFollower != null && pathFollower.enabled)
        {
            CheckPillarDistance();
            yield return new WaitForSeconds(0.2f); // Kiểm tra mỗi 0.1 giây thay vì mỗi frame
        }
    }

    private void CheckPillarDistance()
    {
        if (levelManager?.ConveyorController?.PositionStart == null || isOnPillar) return;
        
        Transform positionStart = levelManager.ConveyorController.PositionStart;
        float distanceToPillar = Vector3.Distance(transform.position, positionStart.position);
        
        if (distanceToPillar <= pillarDetectionDistance)
        {
            PillarItem availablePillar = PillarController.instance.GetAvailablePillar();
            if (availablePillar != null && !isOnPillar)
            {
                JumpToPillar(availablePillar);
            }
        }
    }
    #endregion

    #region Pillar State - Trên pillar
    private void JumpToPillar(PillarItem targetPillar)
    {
        isOnPillar = true;
        if (pathFollower != null)
        {
            pathFollower.enabled = false;
        }
        Vector3 targetPosition = new Vector3(targetPillar.transform.position.x, targetPillar.transform.position.y + 0.2f, targetPillar.transform.position.z);
        transform.DOJump(targetPosition, jumpHeight, 1, jumpDuration)
            .OnComplete(() =>
            {
                targetPillar.SetEmpty(false, this);
                StopAllCoroutines();
                mapController.OnSpoolWinding?.Invoke(this);
            });
        isOnConveyor = false;

    }
    #endregion

    #region Winding State - Cuốn sợi len
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
        StartCoroutine(WindingYarnCoroutine());
    }

    public void OnYarnCompletedAllKnits()
    {
        // Hiệu ứng hoàn thành
        Debug.Log($"Cuộn len hoàn thành cho spool màu {color}");
        StopSpoolRotation();
        
        // Thông báo cho MapController rằng đã hoàn thành cuộn một hàng
        if (mapController != null)
        {
            // Có thể thêm callback hoặc event ở đây nếu cần
        }
    }

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
        if (rollIndex >= rolls.Length) 
        {
            return;
        }
        if (!rolls[rollIndex].activeInHierarchy)
        {
            rolls[rollIndex].SetActive(true);
            activeRolls++;
        }
    }

    private void StartSpoolRotation()
    {
        if (isRotating) return;
        isRotating = true;
        isCompletedWindingYarn = false;
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
    public bool CheckCompletedWindingYarn()
    {
        if (countKnit == 100)
        {
            isCompletedWindingYarn = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Coroutine mô phỏng việc thu sợi len
    /// </summary>
    private IEnumerator WindingYarnCoroutine()
    {
        float windingDuration = 2f; // Thời gian thu sợi len
        float elapsedTime = 0f;

        while (elapsedTime < windingDuration && isWindingYarn)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / windingDuration;


            for (int i = activeRolls; i < rolls.Length; i++)
            {
                ActivateRoll(i);
            }

            yield return null;
        }
        // OnYarnCompletedAllKnits();
    }
    #endregion
    
}