using System.Collections;
using System.Collections.Generic;
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

public class SpoolItem : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    [SerializeField] private Direction direction;
    [SerializeField] private float speed;
    [SerializeField] private float checkDistance = 1f;
    [SerializeField] private LayerMask obstacleLayer = -1;
    [SerializeField] private SpoolController spoolController;
    [SerializeField] private float moveToConveyorSpeed = 2f;
    private PathCreator pathCreation => spoolController.LevelManager.PathCreator;
    private bool isOnConveyor = false;
    private float distanceAlongPath = 0f;
    private Renderer itemRenderer;
    private Color originalColor;

    public void Initialize(SpoolController spoolController)
    {
        this.spoolController = spoolController;
        itemRenderer = GetComponent<Renderer>();
        if (itemRenderer != null)
        {
            originalColor = itemRenderer.material.color;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isOnConveyor && !CheckForObstacles())
        {
            ShowVisualFeedback();
            MoveToConveyor();
        }
        else if (CheckForObstacles())
        {
            ShowErrorFeedback();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }

    private Vector3 GetDirection()
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector3.up;
            case Direction.Down:
                return Vector3.down;
            case Direction.Left:
                return Vector3.left;
            case Direction.Right:
                return Vector3.right;
        }
        return Vector3.zero;
    }

    private bool CheckForObstacles()
    {
        Vector3 checkDirection = GetDirection();
        Vector3 checkPosition = transform.position + checkDirection * checkDistance;
        
        // Kiểm tra va chạm với các khối khác
        Collider[] colliders = Physics.OverlapSphere(checkPosition, 0.5f, obstacleLayer);
        
        if (colliders.Length > 0)
        {
            Debug.Log("Có vật cản, không thể di chuyển!");
            return true;
        }
        
        return false;
    }

    private void MoveToConveyor()
    {
        if (pathCreation == null)
        {
            Debug.LogWarning("PathCreator chưa được gán!");
            return;
        }

        // Tìm điểm gần nhất trên path
        Vector3 nearestPoint = pathCreation.path.GetClosestPointOnPath(transform.position);
        distanceAlongPath = pathCreation.path.GetClosestDistanceAlongPath(transform.position);
        
        // Di chuyển đến điểm gần nhất trên băng chuyền
        transform.DOMove(nearestPoint, moveToConveyorSpeed).OnComplete(() =>
        {
            isOnConveyor = true;
            Debug.Log("Đã di chuyển lên băng chuyền!");
        });
    }

    void Update()
    {
        if (isOnConveyor && spoolController.LevelManager.PathCreator != null)
        {
            // Di chuyển theo path của băng chuyền
            distanceAlongPath += speed * Time.deltaTime;
            Vector3 newPosition = spoolController.LevelManager.PathCreator.path.GetPointAtDistance(distanceAlongPath);
            transform.position = newPosition;
            
            // Kiểm tra nếu đã đến cuối path
            if (distanceAlongPath >= pathCreation.path.length)
            {
                isOnConveyor = false;
                distanceAlongPath = 0f;
            }
        }
    }

    private void ShowVisualFeedback()
    {
        transform.DOScale(1.1f, 0.1f).OnComplete(() =>
        {
            transform.DOScale(1f, 0.1f);
        });
    }

    private void ShowErrorFeedback()
    {
        if (itemRenderer != null)
        {
            // Hiệu ứng đỏ khi có lỗi
            Color errorColor = Color.red;
            itemRenderer.material.color = errorColor;
            itemRenderer.material.DOColor(originalColor, 0.3f);
        }
        
        // Hiệu ứng rung nhẹ
        transform.DOShakePosition(0.3f, 0.1f, 10, 90, false, true);
    }
}
