using System.Collections;
using System.Collections.Generic;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;

public class ConveyorController : MonoBehaviour
{
    [SerializeField] Material materialRoad;
    [SerializeField] float scrollSpeed = 1f;
    [SerializeField]private PathCreator pathCreation;
    [SerializeField]private RoadMeshCreator roadMeshCreator;
    private Vector2 textureOffset = Vector2.zero;
    private LevelManager levelManager;
    public PathCreator PathCreation => pathCreation;
    public RoadMeshCreator RoadMeshCreator => roadMeshCreator;
    public void Initialize(LevelManager levelManager)
    {
        this.levelManager = levelManager;
        roadMeshCreator.thickness = 1;
        pathCreation.bezierPath.FlipNormals = false;
        materialRoad.mainTextureScale = new Vector2(1, 10);
        Debug.LogError("check");
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
}
