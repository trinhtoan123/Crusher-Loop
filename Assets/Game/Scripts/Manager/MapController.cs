using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class MapController : MonoBehaviour
{
    [SerializeField] PillarController pillarController;
    [SerializeField] List<Knit> knitItems = new List<Knit>();
    [SerializeField] private RopeSetting ropeSettingPrefab;
    private List<SpoolItem> spoolItems = new List<SpoolItem>();
    private RopeSetting currentYarnRope;
    public Action<SpoolItem> OnSpoolWinding;
    private Dictionary<ColorRope, Queue<SpoolItem>> colorQueues = new Dictionary<ColorRope, Queue<SpoolItem>>();
    private Dictionary<ColorRope, bool> isColorProcessing = new Dictionary<ColorRope, bool>();
    private LevelManager levelManager;
    void OnEnable()
    {
        OnSpoolWinding += IEStartWinding;
    }
    void OnDisable()
    {
        OnSpoolWinding -= IEStartWinding;
    }
    public void Initialize(LevelManager levelManager)
    {
        this.levelManager = levelManager;
        pillarController.Initialize(levelManager);
        foreach (var knit in knitItems)
        {
            knit.Initialize(levelManager, this);
        }
    }

 
   
    public bool ClearMap()
    {
        foreach (var knit in knitItems)
        {
            foreach (var knitChild in knit.KnitItems)
            {
                if (!knitChild.IsCompleted)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool HasSlotPillar()
    {
       var availablePillar = pillarController.GetAvailablePillar();
       if( availablePillar != null && availablePillar.IsEmpty)
       {
            return true; 
       }
        return false; 
    }
    
    public bool CanAnySpoolStillWind()
    {
        foreach (var spool in spoolItems)
        {
            if (spool.IsWindingYarn)
            {
                return true; // Có spool đang cuộn
            }
        }
        
        foreach (var spool in spoolItems)
        {
            if (HasColorInRow(currentRow, spool.color) && CanWindInRow(currentRow, spool.color))
            {
                return true;
            }
        }
        return false; 
    }

    public void IEStartWinding(SpoolItem spoolItem)
    {
        if (knitItems == null || knitItems.Count == 0)
        {
            return;
        }
        AddSpoolToQueue(spoolItem);
    }

    private IEnumerator ProcessSmartWinding(List<Knit> sortedKnits, SpoolItem itemSpool)
    {
        if (itemSpool.IsOnPillar)
        {
            spoolItems.Add(itemSpool);
            
            // Kiểm tra trạng thái game sau khi thêm spool mới
          
        }
        
        yield return StartCoroutine(ProcessSpoolWinding(itemSpool, sortedKnits));

    }
    Knit currentRow;
    private IEnumerator ProcessSpoolWinding(SpoolItem spool, List<Knit> sortedKnits)
    {
        ColorRope spoolColor = spool.color;
        int currentRowIndex = 0;
            
        while (currentRowIndex < sortedKnits.Count && !spool.CheckCompletedWindingYarn())
        {
             Knit currentRow = sortedKnits[currentRowIndex];
             this.currentRow = currentRow;
            if (currentRow == null)
            {
                currentRowIndex++;
                continue;
            }

            // Kiểm tra hàng hiện tại có màu của spool không
            if (HasColorInRow(currentRow, spoolColor))
            {
                // Kiểm tra xem có thể cuộn được trong hàng này không
                if (CanWindInRow(currentRow, spoolColor))
                {
                    // Cuộn hàng hiện tại và đợi hoàn thành
                    yield return StartCoroutine(WindThroughSingleRow(spool, currentRow, currentRowIndex));
                }
                else
                {
                   StopWindingYarn(spool);
                   
                   // Kiểm tra trạng thái game khi spool không thể cuộn được nữa
                   if (levelManager != null)
                   {
                       levelManager.CheckCompleteLevel();
                   }
                }
                
                // Đợi hàng hiện tại hoàn thành trước khi chuyển sang hàng tiếp theo
                yield return new WaitUntil(() => IsRowCompleted(currentRow));
                currentRowIndex++;
            }
            else
            {
                StopWindingYarn(spool);
                
                // Kiểm tra trạng thái game khi spool không có màu phù hợp
                if (levelManager != null)
                {
                    levelManager.CheckCompleteLevel();
                }
                
                yield return new WaitUntil(() => IsRowCompleted(currentRow));
                currentRowIndex++;
            }
        }
        
        if (spool.CheckCompletedWindingYarn())
        {
            yield return StartCoroutine(MakeSpoolDisappear(spool));
        }
    }
    private void StopWindingYarn(SpoolItem spool)
    {
        if (currentYarnRope != null)
        {
            Destroy(currentYarnRope.gameObject);
            currentYarnRope = null;
        }
        if (spool.IsWindingYarn)
        {
            spool.StopWindingYarn();
        }
      
    }

    private bool HasColorInRow(Knit knit, ColorRope targetColor)
    {
        if (knit == null || knit.KnitItems == null) return false;
        
        foreach (var knitChild in knit.KnitItems)
        {
            if (knitChild != null && knitChild.Color == targetColor)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Kiểm tra xem có thể cuộn được trong hàng này không
    /// </summary>
    private bool CanWindInRow(Knit knit, ColorRope targetColor)
    {
        if (knit == null || knit.KnitItems == null) return false;
        
        foreach (var knitChild in knit.KnitItems)
        {
            if (knitChild != null && knitChild.Color == targetColor && !knitChild.IsCompleted)
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Kiểm tra hàng đã hoàn thành chưa (tất cả KnitChild đã được đánh dấu hoàn thành)
    /// </summary>
    private bool IsRowCompleted(Knit row)
    {
        if (row == null || row.KnitItems == null) return false;
        
        foreach (var knitChild in row.KnitItems)
        {
            if (knitChild != null && !knitChild.IsCompleted)
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator WindThroughSingleRow(SpoolItem spool, Knit currentRow, int rowIndex)
    {
        // Destroy line cũ nếu có - đảm bảo destroy hoàn toàn
        if (currentYarnRope != null)
        {
            DestroyImmediate(currentYarnRope.gameObject);
            currentYarnRope = null;
        }
        
        // Kiểm tra lại xem có thể cuộn được không trước khi tạo dây
        if (!CanWindInRow(currentRow, spool.color))
        {
            yield break; // Không thể cuộn được, thoát khỏi hàm
        }
        
        // Tạo line mới cho hàng này - đảm bảo tạo mới hoàn toàn
        RopeSetting singleRowRope = Instantiate(ropeSettingPrefab);
        singleRowRope.SetLineRenderer(GetYarnMaterial(spool.color), spool.transform.GetChild(0));
        Dictionary<Transform, KnitChild> targetChildren = GetTargetChildrenInRow(currentRow, spool.color);
        List<Transform> sortedTransforms = new List<Transform>(targetChildren.Keys);
        
        bool isLeftToRight = (rowIndex % 2 == 0);
        if (isLeftToRight)
        {
            sortedTransforms.Sort((a, b) => a.position.x.CompareTo(b.position.x));
        }
        else
        {
            sortedTransforms.Sort((a, b) => b.position.x.CompareTo(a.position.x));
        }
        
        foreach (var transform in sortedTransforms)
        {
            singleRowRope.AddEndPoint(transform.position);
        }
        
        if (!spool.IsWindingYarn)
        {
            spool.StartWinding(null);
        }
        
        yield return ProcessEachPoint(sortedTransforms, targetChildren, singleRowRope, spool);
        yield return new WaitUntil(() => singleRowRope.GetCurrentEndIndex() >= singleRowRope.GetEndPointsCount() - 1);
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => !CanWindInRow(currentRow, spool.color));
        if (singleRowRope != null)
        {
            Destroy(singleRowRope.gameObject);
        }
    }
    
    private IEnumerator ProcessEachPoint(List<Transform> sortedTransforms, Dictionary<Transform, KnitChild> targetChildren, RopeSetting singleRowRope, SpoolItem spool)
    {
        Transform previousChildTransform = null;
        
        for (int i = 0; i < sortedTransforms.Count; i++)
        {
            yield return new WaitUntil(() => singleRowRope.GetCurrentEndIndex() >= i);
            
            if (previousChildTransform != null)
            {
                SetChildItemClear(previousChildTransform);
                spool.UpdateRoll();
            }

            previousChildTransform = sortedTransforms[i].parent;
            
            yield return new WaitForSeconds(0.1f);
        }
        if (previousChildTransform != null)
        {
            yield return new WaitUntil(() => singleRowRope.GetCurrentEndIndex() >= sortedTransforms.Count - 1);
            SetChildItemClear(previousChildTransform);
        }
    }
    
    private void SetChildItemClear(Transform childTransform)
    {
        KnitChild knitPrevious = childTransform.parent.GetComponent<KnitChild>();
        if (knitPrevious != null)
        {
            knitPrevious.SetCompleted();
        }
    }
 
    private Dictionary<Transform, KnitChild> GetTargetChildrenInRow(Knit row, ColorRope targetColor)
    {
        Dictionary<Transform, KnitChild> targetChildren = new Dictionary<Transform, KnitChild>();
        
        foreach (var knitChild in row.KnitItems)
        {
            if (knitChild != null && knitChild.Color == targetColor)
            {
                foreach (var child in knitChild.ChildItems)
                {
                    Transform childTransform = child.GetChild(0);
                    targetChildren[childTransform] = knitChild;
                }
            }
        }
        return targetChildren;
    }
    
    private IEnumerator MakeSpoolDisappear(SpoolItem spool)
    {
        if (currentYarnRope != null)
        {
            Destroy(currentYarnRope.gameObject);
            currentYarnRope = null;
        }
        spool.CompleteSpool();
        
        if (spoolItems.Contains(spool))
        {
            spoolItems.Remove(spool);
        }
        yield return new WaitForSeconds(0.2f);
        
        // Kiểm tra trạng thái game sau khi spool hoàn thành và được loại bỏ
        if (levelManager != null)
        {
            levelManager.CheckCompleteLevel();
        }
     
    }
    
    private Material GetYarnMaterial(ColorRope color)
    {
        if (GameManager.Instance?.SpoolData?.SpoolColors != null)
        {
            var colorData = GameManager.Instance.SpoolData.SpoolColors.Find(x => x.color == color);
            if (colorData != null)
            {
                return colorData.materialKnit;
            }
        }

        return null;
    }

    #region Queue Management System

    private void AddSpoolToQueue(SpoolItem spoolItem)
    {
        ColorRope spoolColor = spoolItem.color;

        // Khởi tạo queue cho màu này nếu chưa có
        if (!colorQueues.ContainsKey(spoolColor))
        {
            colorQueues[spoolColor] = new Queue<SpoolItem>();
            isColorProcessing[spoolColor] = false;
        }

        colorQueues[spoolColor].Enqueue(spoolItem);
        if (!isColorProcessing[spoolColor])
        {
            StartCoroutine(ProcessColorQueue(spoolColor));
        }
     
    }
    
    private IEnumerator ProcessColorQueue(ColorRope color)
    {
        isColorProcessing[color] = true;
        
        while (colorQueues[color].Count > 0)
        {
            SpoolItem currentSpool = colorQueues[color].Dequeue();
            
            yield return StartCoroutine(ProcessSmartWinding(knitItems, currentSpool));
            yield return new WaitForSeconds(0.1f);
        }
        
        isColorProcessing[color] = false;
    }
    
    
    #endregion
}
