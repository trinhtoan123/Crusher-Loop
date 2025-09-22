using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoAnimation : MonoBehaviour
{

    [SerializeField] private Transform flwoerT;
    [SerializeField] private Transform hexaT;
    [SerializeField] private Transform sortT;

    void Start()
    {
        StartCoroutine(LogoAnimationStart());
    }


    public IEnumerator LogoAnimationStart()
    {
        transform.DOMoveY(transform.position.y + 50, 1)
     .SetEase(Ease.Linear)
     .SetLoops(-1, LoopType.Yoyo); 
        Sequence scaleSequence = DOTween.Sequence();

        scaleSequence.Append(flwoerT.DOScaleY(1.1f, 1f).SetEase(Ease.Linear))
            .Append(flwoerT.DOScaleY(1f, 1f).SetEase(Ease.Linear))
            .Append(flwoerT.DOScaleY(1.1f, 1f).SetEase(Ease.Linear))
            .Append(flwoerT.DOScaleY(1f, 1f).SetEase(Ease.OutElastic, 10f)) 
            .OnComplete(() => scaleSequence.Restart()); 

        scaleSequence.SetLoops(-1, LoopType.Restart);

        Sequence scaleSequence_H = DOTween.Sequence();

        scaleSequence_H.Append(hexaT.DOScaleY(1.1f, 1f).SetEase(Ease.Linear))
            .Append(hexaT.DOScaleY(1f, 1f).SetEase(Ease.Linear))
            .Append(hexaT.DOScaleY(1.1f, 1f).SetEase(Ease.Linear))
            .Append(hexaT.DOScaleY(1f, 1f).SetEase(Ease.OutElastic, 10f)) 
             .OnComplete(() => scaleSequence_H.Restart()); 
        scaleSequence_H.SetLoops(-1, LoopType.Restart);



        Sequence scaleSequence_S = DOTween.Sequence();

        scaleSequence_S.Append(sortT.DOScaleY(1.1f, 1f).SetEase(Ease.Linear))
            .Append(sortT.DOScaleY(1f, 1f).SetEase(Ease.Linear))
            .Append(sortT.DOScaleY(1.1f, 1f).SetEase(Ease.Linear))
            .Append(sortT.DOScaleY(1f, 1f).SetEase(Ease.OutElastic, 10f))
            .OnComplete(() => scaleSequence_S.Restart());
        scaleSequence_S.SetLoops(-1, LoopType.Restart);

        yield return null;
    }
}
