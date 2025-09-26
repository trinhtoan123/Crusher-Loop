using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnitChild : MonoBehaviour
{
    private bool isCompleted;
    private LevelManager levelManager;
    [SerializeField] private ColorRope color;
    [SerializeField] private Transform[] childItems;
  
    public Transform[] ChildItems => childItems;
    public bool IsCompleted => isCompleted;
    public ColorRope Color => color;
    public LevelManager LevelManager => levelManager;
    
    public void Initialize(LevelManager levelManager, Vector3[] anchorPositions)
    {
        this.levelManager = levelManager;
        Material material = GameManager.Instance.SpoolData.SpoolColors.Find(x => x.color == color).materialKnit;
        SetMaterial(material);
        
        for (int i = 0; i < childItems.Length; i++)
        {
            CreateAnchor(childItems[i], i, anchorPositions);
        }
    }

    public void SetMaterial(Material material)
    {
        foreach (Transform child in childItems)
        {
            child.GetComponent<MeshRenderer>().material = material;
        }
    }

    public void SetCompleted()
    {
        Material clearMaterial = GameManager.Instance.SpoolData.MaterialKnitClear;
        SetMaterial(clearMaterial);
        isCompleted = true;
    }

    private void CreateAnchor(Transform childTransform,int childIndex, Vector3[] anchorPositions)
    {
        GameObject anchorPoint = new GameObject("KnitAnchor");
        anchorPoint.transform.SetParent(childTransform);
        
        Vector3 anchorPosition = GetIndexAnchor(childIndex,anchorPositions);
        anchorPoint.transform.localPosition = anchorPosition;
    }
    private Vector3 GetIndexAnchor(int childIndex,Vector3[] anchorPositions)
    {
        if (anchorPositions == null || anchorPositions.Length == 0)
        {
            return Vector3.zero;
        }
        int index = Mathf.Clamp(childIndex, 0, anchorPositions.Length - 1);
        return anchorPositions[index];
    }

}
