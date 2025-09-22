using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PillarController : MonoBehaviour
{
    public static PillarController instance;
    [SerializeField] private List<PillarItem> pillarItems = new List<PillarItem>();
    public List<PillarItem> PillarItems => pillarItems;
    private LevelManager levelManager;
    
    public static event Action<int> OnPillarCapacityChanged;

    private void Awake()
    {
        instance = this;
    }
    public void Initialize(LevelManager levelManager)
    {
        this.levelManager = levelManager;

        foreach (var pillar in pillarItems)
        {
            pillar.Initialize();
        }
    }

    public PillarItem GetAvailablePillar()
    {
        foreach (var pillar in pillarItems)
        {
            if (pillar.IsEmpty)
            {
                // Đánh dấu pillar này là đang được sử dụng ngay lập tức
                // để tránh nhiều item cùng nhảy vào một pillar
                pillar.SetEmpty(false, null);
                return pillar;
            }
        }
        
        return null; 
    }


    public int GetTotalStoredItems()
    {
        int total = 0;
        // foreach (var pillar in pillarItems)
        // {
        //     total += pillar.GetStoredItemCount();
        // }
        return total;
    }

}
