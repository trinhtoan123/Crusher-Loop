using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GogoGaga.OptimizedRopesAndCables;

public class RopeSetting : MonoBehaviour
{
     public Transform StartTarget;
    public Transform EndTarget;

    [Header("Tùy chọn")]
    public Vector3 StartOffset;   // lệch nhẹ khỏi tâm
    public Vector3 EndOffset;
    public bool UseWorldSpace = true; // thường để true khi nối giữa 2 object khác nhau
    public bool UpdateInLate = true;  // LateUpdate giảm jitter khi có Rigidbody

    LineRenderer lr;

    void Reset()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.widthMultiplier = 0.02f;
    }

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = UseWorldSpace;
    }

    void Update()
    {
        if (!UpdateInLate) UpdateLine();
    }

    void LateUpdate()
    {
        if (UpdateInLate) UpdateLine();
    }

    void UpdateLine()
    {
        if (StartTarget == null || EndTarget == null) return;

        if (lr.useWorldSpace)
        {
            lr.SetPosition(0, StartTarget.position + StartOffset);
            lr.SetPosition(1, EndTarget.position + EndOffset);
        }
        else
        {
            // nếu muốn vẽ trong local-space của chính GameObject chứa LineRenderer
            Vector3 p0 = transform.InverseTransformPoint(StartTarget.position + StartOffset);
            Vector3 p1 = transform.InverseTransformPoint(EndTarget.position + EndOffset);
            lr.SetPosition(0, p0);
            lr.SetPosition(1, p1);
        }
    }

    // Cho phép đổi option lúc đang chạy
    void OnValidate()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        if (lr) lr.useWorldSpace = UseWorldSpace;
    }
}
