using System.Collections;
using System.Collections.Generic;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;

public class SpoolController : MonoBehaviour
{
    [SerializeField] private SpoolData spoolData;
    [SerializeField] private List<SpoolItem> spoolItems;
    [SerializeField] private Transform startPosition;
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
            spoolItem.Initialize(this, spoolData);
        }
    }
    
  
    public void AddSpoolItem(SpoolItem spoolItem)
    {
        spoolItems.Add(spoolItem);
    }
    public void RemoveSpoolItem(SpoolItem spoolItem)
    {
        spoolItems.Remove(spoolItem);
    }
    public void StopSpool(SpoolItem spoolItem)
    {
       spoolItem.StateFollowPath(EndOfPathInstruction.Stop);
    }
}
