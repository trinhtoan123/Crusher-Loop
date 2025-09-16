using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;

public class ConveyorController : MonoBehaviour
{
    [SerializeField] Material materialRoad;
    [SerializeField] float scrollSpeed = 1f;
    private Vector2 textureOffset = Vector2.zero;
    private LevelManager levelManager;

    public void Initialize(LevelManager levelManager)
    {
        this.levelManager = levelManager;
        this.levelManager.RoadMeshCreator.thickness = 1;
        this.levelManager.PathCreator.bezierPath.FlipNormals = false;
        materialRoad.mainTextureScale = new Vector2(1, 10);
        Debug.LogError("checek");
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
