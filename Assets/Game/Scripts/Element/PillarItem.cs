using UnityEngine;
using DG.Tweening;

public class PillarItem : MonoBehaviour
{
    [SerializeField] private bool isEmpty;
    private SpoolItem spoolItem;
    public void Initialize()
    {
    }

    public void SetEmpty(bool isEmpty, SpoolItem spoolItem)
    {
        this.isEmpty = isEmpty;
        this.spoolItem = spoolItem;
    }
 
}
