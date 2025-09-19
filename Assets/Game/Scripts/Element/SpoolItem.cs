using System.Collections;
using DG.Tweening;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;
using UnityEngine.UI;


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

    public void Initialize(SpoolController spoolController, SpoolData spoolData)
    {
        this.spoolController = spoolController;
        this.levelManager = spoolController.LevelManager;
        pathFollower = gameObject.AddComponent<PathFollower>();
        pathFollower.enabled = false;
        myCollider = GetComponent<BoxCollider>();
        material.material = spoolData.spoolColors.Find(x => x.color == color).material;
        image.sprite = spoolData.spoolDirections.Find(x => x.direction == direction).sprite;
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

 
    private void OnDestroy()
    {
      
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

