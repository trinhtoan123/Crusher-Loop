using UnityEngine;
using DG.Tweening;
using System;

public class PillarItem : MonoBehaviour
{
    [SerializeField] private bool isEmpty ;
    public bool IsEmpty => isEmpty;
    private SpoolItem spoolItem;
    public static event Action<SpoolItem> OnSpoolItemPlaced;
    public void Initialize()
    {
        isEmpty = true;
        spoolItem = null;
    }

    public void SetEmpty(bool isEmpty, SpoolItem spoolItem)
    {
        this.isEmpty = isEmpty;
        this.spoolItem = spoolItem;
        
        if (!isEmpty && spoolItem != null)
        {
            OnSpoolItemPlaced?.Invoke(spoolItem);
        }
    }
   
    public bool CanAcceptSpoolItem()
    {
        return isEmpty && spoolItem == null;
    }


    public bool HasSpoolItem()
    {
        return spoolItem != null;
    }
 
}
