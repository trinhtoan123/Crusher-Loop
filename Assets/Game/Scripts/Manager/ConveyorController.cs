using System.Collections;
using System.Collections.Generic;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;
using System;

public class ConveyorController : MonoBehaviour
{
    private LevelManager levelManager;
    private Vector2 textureOffset = Vector2.zero;
    private bool isJammed = false;
    private bool isMeltdown = false;
    private bool isWarningMode = false;
    [SerializeField] private Transform positionStart;
    [SerializeField] Material materialRoad;
    [SerializeField] float scrollSpeed = 1f;
    [SerializeField] private PathCreator pathCreation;
    [SerializeField] private RoadMeshCreator roadMeshCreator;
    [SerializeField] private float conveyorHeight = 1;
    [SerializeField] private float itemSpacing = 1f;
    [SerializeField] private int maxTotalItemsOnConveyor = 20;
    [SerializeField] private float jamWarningThreshold = 0.8f;
    public PathCreator PathCreation => pathCreation;
    public RoadMeshCreator RoadMeshCreator => roadMeshCreator;
    public Transform PositionStart => positionStart;
    public static event Action<int, int> OnConveyorCapacityChanged;
    public static event Action OnConveyorJammed;
    public static event Action OnConveyorMeltdown;
    public static event Action OnConveyorWarning;

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
        
        // Tìm điểm gần nhất trên đường băng chuyền
        Vector3 closestPoint = pathCreation.path.GetClosestPointOnPath(requestPosition);
        return closestPoint;
    }



    public void ResetConveyor()
    {
        isJammed = false;
        isMeltdown = false;
        isWarningMode = false;
        scrollSpeed = 1f; // Reset scroll speed
    }

}
