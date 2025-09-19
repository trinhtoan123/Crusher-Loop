using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] PillarController pillarController;
    [SerializeField] List<Knit> knitItems = new List<Knit>();


    public void Initialize(LevelManager levelManager)
    {
        pillarController.Initialize(levelManager);

    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var knitItem in knitItems)
            {
                knitItem.CreateLine();
            }
        }
    }

}

