using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using PathCreation;
using PathCreation.Examples;

[System.Serializable]
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public class SpoolItem : MonoBehaviour
{
    private SpoolController spoolController;
    [SerializeField] private Direction direction;
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

    public void Initialize(SpoolController spoolController)
    {
        this.spoolController = spoolController;
        pathFollower = gameObject.AddComponent<PathFollower>();
        pathFollower.enabled = false; // Tắt ban đầu
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
        
        // Di chuyển thẳng theo hướng cho đến khi gặp băng chuyền hoặc ra khỏi giới hạn
        while (isMoving && !isOnConveyor && !isMovingToConveyor)
        {
            // Di chuyển theo hướng
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
            
            // Kiểm tra khoảng cách đến băng chuyền mỗi frame
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
        isMoving = false; // Dừng di chuyển thẳng
        
        // Lấy vị trí phù hợp trên băng chuyền (tránh trùng lặp)
        Vector3 availablePosition = spoolController.LevelManager.ConveyorController.GetAvailablePositionOnConveyor(conveyorPoint);
        conveyorDistance = spoolController.LevelManager.ConveyorController.PathCreation.path.GetClosestDistanceAlongPath(availablePosition);
        Vector3 jumpTarget = availablePosition + Vector3.up * jumpHeight;
        Sequence jumpSequence = DOTween.Sequence();
        
        jumpSequence.Append(transform.DOMove(jumpTarget, jumpDuration * 0.6f).SetEase(Ease.OutQuad));
        
        // Rơi xuống băng chuyền với Y phù hợp
        jumpSequence.Append(transform.DOMove(availablePosition, jumpDuration * 0.4f).SetEase(Ease.InQuad));
        
        jumpSequence.OnComplete(() => {
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
            // Sử dụng reflection để set distanceTravelled trong PathFollower
            var field = typeof(PathFollower).GetField("distanceTravelled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(pathFollower, conveyorDistance);
            }
            transform.rotation = Quaternion.identity;
        }
        
        
        pathFollower.enabled = true;
    }

    private void AlignWithConveyorDirection()
    {
        if (spoolController?.LevelManager?.PathCreation != null)
        {
            PathCreator pathCreator = spoolController.LevelManager.PathCreation;
            
            // Lấy hướng của băng chuyền tại vị trí hiện tại
            Vector3 conveyorDirection = pathCreator.path.GetDirectionAtDistance(conveyorDistance);
            
            // Tính toán rotation để item nằm thẳng theo hướng băng chuyền
            Quaternion targetRotation = Quaternion.LookRotation(conveyorDirection, Vector3.up);
            
            // Xoay item mượt mà về rotation đúng
            transform.DORotateQuaternion(targetRotation, rotationDuration).SetEase(Ease.OutQuart);
        }
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

