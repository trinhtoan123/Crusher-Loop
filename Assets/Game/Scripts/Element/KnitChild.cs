using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnitChild : MonoBehaviour
{
    [SerializeField] private ColorRope color;
    [SerializeField] private Material material;
    private LevelManager levelManager;
    public void Initialize(LevelManager levelManager)
    {
        this.levelManager = levelManager;
        foreach (Transform child in transform)
        {
            child.GetComponent<MeshRenderer>().material = levelManager.SpoolData.spoolColors.Find(x => x.color == color).materialRoll;
        }
    }
    public void SetMaterial(Material material)
    {
        foreach (Transform child in transform)
        {
            Debug.LogError(child.name);
            child.GetComponent<MeshRenderer>().material = material;
        }
    }
    
}
