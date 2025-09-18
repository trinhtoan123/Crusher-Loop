using System.Collections;
using System.Collections.Generic;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;

public class ConveyorController : MonoBehaviour
{
    [SerializeField] Material materialRoad;
    [SerializeField] float scrollSpeed = 1f;
    [SerializeField] private PathCreator pathCreation;
    [SerializeField] private RoadMeshCreator roadMeshCreator;
    [SerializeField] private float conveyorHeight = 1; 
    [SerializeField] private float itemSpacing = 1f;
    
    private Vector2 textureOffset = Vector2.zero;
    private LevelManager levelManager;
    private List<float> occupiedPositions = new List<float>(); 
    
    public PathCreator PathCreation => pathCreation;
    public RoadMeshCreator RoadMeshCreator => roadMeshCreator;
    public float ConveyorHeight => conveyorHeight;
    public void Initialize(LevelManager levelManager)
    {
        this.levelManager = levelManager;
        roadMeshCreator.thickness = 1;
        pathCreation.bezierPath.FlipNormals = false;
        materialRoad.mainTextureScale = new Vector2(1, 10);
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
        float closestDistance = pathCreation.path.GetClosestDistanceAlongPath(requestPosition);
        float availableDistance = FindAvailableDistance(closestDistance);
        Vector3 pathPosition = pathCreation.path.GetPointAtDistance(availableDistance);
        pathPosition.y += 3;
        occupiedPositions.Add(availableDistance);
        return pathPosition;
    }

    private float FindAvailableDistance(float preferredDistance)
    {
        occupiedPositions.Sort();
        if (IsDistanceAvailable(preferredDistance))
        {
            return preferredDistance;
        }
        float bestDistance = preferredDistance;
        float minDifference = float.MaxValue;
        for (float offset = itemSpacing; offset <= pathCreation.path.length; offset += itemSpacing)
        {
            float forwardDistance = preferredDistance + offset;
            if (forwardDistance <= pathCreation.path.length && IsDistanceAvailable(forwardDistance))
            {
                if (offset < minDifference)
                {
                    minDifference = offset;
                    bestDistance = forwardDistance;
                }
            }
            
            // Kiểm tra phía sau
            float backwardDistance = preferredDistance - offset;
            if (backwardDistance >= 0 && IsDistanceAvailable(backwardDistance))
            {
                if (offset < minDifference)
                {
                    minDifference = offset;
                    bestDistance = backwardDistance;
                }
            }
        }
        
        return bestDistance;
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
    }

   
}
