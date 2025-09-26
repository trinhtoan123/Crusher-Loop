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
    private List<SpoolItem> activeSpools = new List<SpoolItem>();
    private RopeSetting currentYarnRope;
    public Action<SpoolItem> OnSpoolWinding;
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
        pillarController.Initialize(levelManager);
        foreach (var knit in knitItems)
        {
            knit.Initialize(levelManager, this);
        }
    }
    public void IEStartWinding(SpoolItem spoolItem)
    {
        if (knitItems == null || knitItems.Count == 0)
        {
            return;
        }
        
        
        StartCoroutine(ProcessSmartWinding(knitItems,spoolItem));
    }

    private IEnumerator ProcessSmartWinding(List<Knit> sortedKnits, SpoolItem itemSpool)
    {
        if (itemSpool.IsOnPillar)
        {
            activeSpools.Add(itemSpool);
        }
        
        yield return StartCoroutine(ProcessSpoolWinding(itemSpool, sortedKnits));

    }

    private IEnumerator ProcessSpoolWinding(SpoolItem spool, List<Knit> sortedKnits)
    {
        ColorRope spoolColor = spool.color;
        int currentRowIndex = 0;
            
        while (currentRowIndex < sortedKnits.Count && !spool.IsCompletedWindingYarn)
        {
            Knit currentRow = sortedKnits[currentRowIndex];
            if (currentRow == null) 
            {
                currentRowIndex++;
                continue;
            }

            // Kiểm tra hàng hiện tại có màu của spool không
            if (HasColorInRow(currentRow, spoolColor))
            {
                // Cuộn hàng hiện tại và đợi hoàn thành
                yield return StartCoroutine(WindThroughSingleRow(spool, currentRow, currentRowIndex));
                
                // Sau khi cuộn xong hàng này, kiểm tra hàng tiếp theo
                currentRowIndex++;
                
                // Nếu còn hàng tiếp theo và có cùng màu thì tiếp tục cuộn
                if (currentRowIndex < sortedKnits.Count && HasColorInRow(sortedKnits[currentRowIndex], spoolColor))
                {
                    // Tiếp tục cuộn hàng tiếp theo
                    continue;
                }
                else
                {
                    // Hàng tiếp theo không có cùng màu hoặc đã hết hàng
                    // Destroy dây hiện tại
                    if (currentYarnRope != null)
                    {
                        Destroy(currentYarnRope.gameObject);
                        currentYarnRope = null;
                    }
                    
                    // Đợi hàng tiếp theo hoàn thành rồi mới chuyển sang hàng sau
                    while (currentRowIndex < sortedKnits.Count && !IsRowCompleted(sortedKnits[currentRowIndex]))
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                    
                    // Chuyển sang hàng tiếp theo để kiểm tra
                    currentRowIndex++;
                }
            }
            else
            {
                // Hàng này không có màu phù hợp
                // Destroy dây hiện tại nếu có
                if (currentYarnRope != null)
                {
                    Destroy(currentYarnRope.gameObject);
                    currentYarnRope = null;
                }
                
                if (IsRowCompleted(currentRow))
                {
                    currentRowIndex++;
                }
                else
                {
                    yield return new WaitUntil(() => IsRowCompleted(currentRow));
                    currentRowIndex++;
                }
            }
        }
        
        // Nếu spool đã cuộn xong thì làm cho nó biến mất
        if (spool.IsCompletedWindingYarn)
        {
            yield return StartCoroutine(MakeSpoolDisappear(spool));
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
        // Destroy line cũ nếu có
        if (currentYarnRope != null)
        {
            Destroy(currentYarnRope.gameObject);
        }
        
        // Tạo line mới cho hàng này
        RopeSetting singleRowRope = Instantiate(ropeSettingPrefab);
        singleRowRope.SetLineRenderer(GetYarnMaterial(spool.color), spool.transform.GetChild(0));
        Dictionary<Transform, KnitChild> targetChildren = GetTargetChildrenInRow(currentRow, spool.color);
        List<Transform> sortedTransforms = new List<Transform>(targetChildren.Keys);
        
        // Logic cuộn len: Hàng lẻ (1,3,5...) từ trái sang phải, hàng chẵn (2,4,6...) từ phải sang trái
        bool isLeftToRight = (rowIndex % 2 == 0);
        if (isLeftToRight)
        {
            // Hàng lẻ: từ trái sang phải
            sortedTransforms.Sort((a, b) => a.position.x.CompareTo(b.position.x));
        }
        else
        {
            // Hàng chẵn: từ phải sang trái
            sortedTransforms.Sort((a, b) => b.position.x.CompareTo(a.position.x));
        }
        
        foreach (var transform in sortedTransforms)
        {
            singleRowRope.AddEndPoint(transform.position);
        }
        
        yield return ProcessEachPoint(sortedTransforms, targetChildren, singleRowRope);
        yield return new WaitUntil(() => singleRowRope.GetCurrentEndIndex() >= singleRowRope.GetEndPointsCount() - 1);
        yield return new WaitForSeconds(0.2f);
        
        // Bắt đầu cuộn len
        spool.StartWinding(null);
        currentYarnRope = singleRowRope;
        yield return new WaitUntil(() => IsRowCompleted(currentRow));
        // Cuộn liên tục cho đến khi hoàn thành tất cả items (ví dụ 100 items)
    }
    
    private IEnumerator ProcessEachPoint(List<Transform> sortedTransforms, Dictionary<Transform, KnitChild> targetChildren, RopeSetting singleRowRope)
    {
        Transform previousChildTransform = null;
        
        for (int i = 0; i < sortedTransforms.Count; i++)
        {
            yield return new WaitUntil(() => singleRowRope.GetCurrentEndIndex() >= i);
            
            if (previousChildTransform != null)
            {
                SetChildItemClear(previousChildTransform);
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
        
        spool.transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                spool.gameObject.SetActive(false);
            });
        
        yield return new WaitForSeconds(0.5f);
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
}
