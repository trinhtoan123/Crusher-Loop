using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnitChild : MonoBehaviour
{
    private LevelManager levelManager;
    [SerializeField] private ColorRope color;
    [SerializeField] private Transform[] childItems;

    public Transform[] ChildItems => childItems;
    public ColorRope Color => color;
    public LevelManager LevelManager => levelManager;
    public void Initialize(LevelManager levelManager)
    {
        this.levelManager = levelManager;
        Material material = levelManager.SpoolData.SpoolColors.Find(x => x.color == color).materialRoll;
        SetMaterial(material);
    }

    public void SetMaterial(Material material)
    {
        foreach (Transform child in childItems)
        {
            child.GetComponent<MeshRenderer>().material = material;
        }
    }
    public Transform GetChildItem(int index)
    {
        return childItems[index];
    }
    
}
