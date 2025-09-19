using System;
using System.Collections.Generic;
using UnityEngine;

public class WoolYarn : MonoBehaviour
{
    [SerializeField] private float minPointDist = 0.04f;
    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();

    void Awake()
    {
        SetupLineRenderer();
        lineRenderer = GetComponent<LineRenderer>();   
    }
    
    public void Initialize(Material ropeMat)
    {
        lineRenderer.material = ropeMat;
    }
    private void SetupLineRenderer()
    {
        // Tự thêm LineRenderer component nếu chưa có
        // lineRenderer = GetComponent<LineRenderer>();
        // if (lineRenderer == null)
        //     lineRenderer = gameObject.AddComponent<LineRenderer>();

        // // Cấu hình theo yêu cầu
        // lineRenderer.textureMode = LineTextureMode.Tile;
        // lineRenderer.numCornerVertices = 8;
        // lineRenderer.numCapVertices = 8;
        // lineRenderer.startWidth = 0.08f;
        // lineRenderer.endWidth = 0.08f;
        // lineRenderer.useWorldSpace = true;
    }
    
    public void Clear()
    {
        points.Clear();
        lineRenderer.positionCount = 0;
    }
    
    public void AddPoint(Vector3 worldPos)
    {
        // Kiểm tra khoảng cách với điểm trước (nếu có)
        if (points.Count > 0)
        {
            float dist = Vector3.Distance(points[points.Count - 1], worldPos);
            if (dist < minPointDist)
                return;
        }
        
        // Thêm điểm mới
        points.Add(worldPos);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(points.Count - 1, worldPos);
        
        // Cập nhật tiling theo chiều dài tổng
        UpdateTiling();
    }
    
    private void UpdateTiling()
    {
        if (points.Count < 2 || lineRenderer.material == null)
            return;
        
        // Tính tổng chiều dài
        float totalLength = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            totalLength += Vector3.Distance(points[i - 1], points[i]);
        }
        
       
    }

  
}
