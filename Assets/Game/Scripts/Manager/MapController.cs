using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] PillarController pillarController;
    [SerializeField] List<Knit> knitItems = new List<Knit>();
    public void Initialize(LevelManager levelManager)
    {
        pillarController.Initialize(levelManager);
        foreach (var knit in knitItems)
        {
            knit.Initialize(levelManager);
        }
        // Subscribe to event khi SpoolItem được đặt lên PillarItem
     
    }
    void Update()
    {
    
    }

  
   

    private void OnDestroy()
    {
    }
}

