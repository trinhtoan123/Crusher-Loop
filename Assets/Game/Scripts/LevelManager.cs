using System.Collections;
using System.Collections.Generic;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] RoadMeshCreator roadMeshCreator;
    [SerializeField] PathCreator pathCreator;
    [SerializeField] private SpoolController spoolController;
    [SerializeField] private ConveyorController conveyorController;

    public RoadMeshCreator RoadMeshCreator => roadMeshCreator;
    public PathCreator PathCreator => pathCreator;
    public SpoolController SpoolController => spoolController;
    public ConveyorController ConveyorController => conveyorController;



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
