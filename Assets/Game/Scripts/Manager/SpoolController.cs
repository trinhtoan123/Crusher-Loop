using System.Collections;
using System.Collections.Generic;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;

public class SpoolController : MonoBehaviour
{
    [SerializeField] private List<SpoolItem> spoolItems;
    private PathCreator pathCreation;
    private RoadMeshCreator roadMeshCreator;
    private LevelManager levelManager;
    public LevelManager LevelManager => levelManager;
    public PathCreator PathCreation => pathCreation;
    public RoadMeshCreator RoadMeshCreator => roadMeshCreator;


    public void Initialize(LevelManager levelManager)
    {
        this.levelManager = levelManager;
        pathCreation = levelManager.PathCreation;
        roadMeshCreator = levelManager.RoadMeshCreator;
        foreach (var spoolItem in spoolItems)
        {
            spoolItem.Initialize(this, levelManager.SpoolData);
        }
    }
  
}
