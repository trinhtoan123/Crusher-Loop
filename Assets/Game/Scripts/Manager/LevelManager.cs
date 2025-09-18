using System.Collections;
using System.Collections.Generic;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private SpoolController spoolController;
    [SerializeField] private ConveyorController conveyorController;
    public SpoolController SpoolController => spoolController;
    public ConveyorController ConveyorController => conveyorController;
     public PathCreator PathCreation => conveyorController.PathCreation;
    public RoadMeshCreator RoadMeshCreator => conveyorController.RoadMeshCreator;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        spoolController.Initialize(this);
        conveyorController.Initialize(this);
    }
   

}
