using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

public class SpoolController : MonoBehaviour
{
    [SerializeField] private List<SpoolItem> spoolItems;
    private List<SpoolItem> spoolItemsMoving;
    private LevelManager levelManager;
    public LevelManager LevelManager => levelManager;

    public void Initialize(LevelManager levelManager)
    {
        this.levelManager = levelManager;
        foreach (var spoolItem in spoolItems)
        {
            spoolItem.Initialize(this);
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
