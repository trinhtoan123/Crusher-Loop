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
    public void Initialize(LevelManager levelManager)
    {
        this.levelManager = levelManager;
        pathCreation = levelManager.PathCreation;
        roadMeshCreator = levelManager.RoadMeshCreator;
        
        // Kiểm tra GameManager và SpoolData trước khi khởi tạo
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null! Vui lòng đảm bảo GameManager được khởi tạo trước LevelManager.");
            return;
        }
        
        if (GameManager.Instance.SpoolData == null)
        {
            Debug.LogError("SpoolData is null! Vui lòng gán SpoolData ScriptableObject trong GameManager Inspector.");
            return;
        }
        
        foreach (var spoolItem in spoolItems)
        {
            if (spoolItem != null)
            {
                spoolItem.Initialize(this, GameManager.Instance.SpoolData);
            }
        }
    }
  
}
