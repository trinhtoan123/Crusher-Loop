using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;

public class PathController : MonoBehaviour
{
    [SerializeField] RoadMeshCreator roadMeshCreator;
    // Start is called before the first frame update
    void Start()
    {
        GeneratePath();
    }

    public void GeneratePath()
    {
        roadMeshCreator.thickness = 1;
    }
}
