using System.Collections.Generic;
using UnityEngine;

public class Knit : MonoBehaviour
{
    [SerializeField] private KnitChild[] knitItems;
    [SerializeField]
    private Vector3[] anchorPositions = {
        new Vector3(0.1f, 0.05f, 0),
        new Vector3(-0.1f, 0.05f, 0)
    };
    private LevelManager levelManager;
    private MapController mapController;
    private bool isClear;
    public KnitChild[] KnitItems => knitItems;
    public void Initialize(LevelManager levelManager, MapController mapController)
    {
        this.levelManager = levelManager;
        this.mapController = mapController;
        foreach (var knitItem in knitItems)
        {
            knitItem.Initialize(levelManager, anchorPositions);
        }
    }

}
