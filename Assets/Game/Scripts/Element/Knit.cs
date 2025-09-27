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
    public bool IsClear => isClear;
    public void Initialize(LevelManager levelManager, MapController mapController)
    {
        this.levelManager = levelManager;
        this.mapController = mapController;
        foreach (var knitItem in knitItems)
        {
            knitItem.Initialize(levelManager, anchorPositions);
        }
    }

    /// <summary>
    /// Kiểm tra và cập nhật trạng thái hoàn thành của Knit
    /// </summary>
    public void UpdateClearStatus()
    {
        if (knitItems == null || knitItems.Length == 0)
        {
            isClear = false;
            return;
        }

        foreach (var knitItem in knitItems)
        {
            if (knitItem != null && !knitItem.IsCompleted)
            {
                isClear = false;
                return;
            }
        }
        isClear = true;
    }

}
