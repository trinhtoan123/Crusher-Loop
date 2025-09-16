using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

public class SpoolController : MonoBehaviour
{
    [SerializeField] private List<SpoolItem> spoolItems;
    private LevelManager levelManager;
    public LevelManager LevelManager => levelManager;
    
    public void Initialize(LevelManager levelManager)
    {
        foreach (var spoolItem in spoolItems)
        {
            spoolItem.Initialize(this);
        }
    }
}
