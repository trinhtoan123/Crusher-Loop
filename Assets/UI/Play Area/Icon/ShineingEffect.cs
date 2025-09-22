using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShineingEffect : MonoBehaviour
{
    public float Left;
    public float right;
    public float timer;
    public float setdeley;
    private void Start()
    {
        Effect();
    }
    public void Effect()
    {
        transform.DOLocalMoveX(right, timer).SetDelay(setdeley).SetEase(Ease.Linear).OnComplete(() =>
        {
            transform.DOLocalMoveX(Left, 0.001f).OnComplete(() =>
            {
                Effect();
            });
        });
    }
}
