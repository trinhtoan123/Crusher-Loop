using System.Collections;
using DG.Tweening;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;
using UnityEngine.UI;


public class SpoolItem : MonoBehaviour
{
    private SpoolController spoolController;
    private BoxCollider myCollider;
    private Vector3 posConveyor = new Vector3(0, 0.5f, 0);
    [SerializeField]  public Color color;
    [SerializeField] private Direction direction;
    [SerializeField] private MeshRenderer material;
    [SerializeField] private Image image;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float conveyorDetectionDistance = 2f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float moveDistance = 1f;
    [SerializeField] private float rotationDuration = 0.3f; 
    private bool isOnConveyor = false;
    private bool isMovingToConveyor = false;
    private bool isMoving = false;
    private PathFollower pathFollower;
    private float conveyorDistance = -1f;

    public void Initialize(SpoolController spoolController, SpoolData spoolData)
    {
        this.spoolController = spoolController;
        pathFollower = gameObject.AddComponent<PathFollower>();
        pathFollower.enabled = false;
        myCollider = GetComponent<BoxCollider>();
        material.material = spoolData.spoolColors.Find(x => x.color == color).material;
        image.sprite = spoolData.spoolDirections.Find(x => x.direction == direction).sprite;

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
        if (spoolController.LevelManager.PathCreation != null)
        {
            PathCreator pathCreator = spoolController.LevelManager.PathCreation;
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
        
        Vector3 availablePosition = spoolController.LevelManager.ConveyorController.GetAvailablePositionOnConveyor(conveyorPoint);
        conveyorDistance = spoolController.PathCreation.path.GetClosestDistanceAlongPath(availablePosition);
        
        transform.DOJump(availablePosition, jumpHeight, 1, jumpDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                StartFollowingPath();
            });
    }

    private void StartFollowingPath()
    {
        isOnConveyor = true;
        isMovingToConveyor = false;

        PathCreator pathCreator = spoolController.LevelManager.PathCreation;
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
    }

    public void StateFollowPath(EndOfPathInstruction endOfPathInstruction)
    {
        pathFollower.endOfPathInstruction = endOfPathInstruction;
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

    private void OnDestroy()
    {
        if (isOnConveyor && conveyorDistance >= 0 && spoolController?.LevelManager?.ConveyorController != null)
        {
            spoolController.LevelManager.ConveyorController.ReleasePosition(conveyorDistance);
        }
    }

    
}

