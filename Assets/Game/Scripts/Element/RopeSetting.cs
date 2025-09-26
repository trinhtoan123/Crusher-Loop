using System.Collections.Generic;
using UnityEngine;
public class RopeSetting : MonoBehaviour
{
    [Range(4,128)] [SerializeField] private int segments = 40;
    [SerializeField] private float sagFactor = 0.08f;
    [SerializeField] private Vector3 gravityDir = Vector3.down;
    [SerializeField] private float curveIntensity = 1.0f;
    [SerializeField] private float waveFrequency = 2.0f;
    [SerializeField] private float waveAmplitude = 0.1f;
    [SerializeField] private float smoothness = 0.5f;

    private Transform startPoint; 
    private List<Transform> endTargets = new List<Transform>();
    private int currentEndIndex = 0;
    public float moveSpeed = 6f;
    private LineRenderer lr;
    private Vector3 currentEnd;
    private Transform _startDummy;
    private Transform _endDummy;
    private bool isMovingToNextEnd = false;
    private bool autoMoveToNextEnd = false;
    private float delayBetweenMoves = 1.0f;
    private float lastMoveTime = 0f;
    private Vector3 previousEndPosition;
    private Vector3 velocity;
    private float timeSinceLastMove = 0f;
    private float ropeTension = 0f;

    void Awake(){
        lr = GetComponent<LineRenderer>();
        lr.positionCount = segments;
        UpdateCurrentEnd();
    }
    public void SetLineRenderer(Material color){
        lr.material = color;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
    }

    Transform MakeDummy(string n){
        var go = new GameObject($"Rope{n}");
        go.hideFlags = HideFlags.HideInHierarchy;
        return go.transform;
    }

    void Update(){
        if (!startPoint && _startDummy == null) return;

        var p0 = (startPoint ? startPoint : _startDummy).position;
        var target = GetCurrentTargetPosition();

        Vector3 newEndPosition = Vector3.MoveTowards(currentEnd, target, moveSpeed * Time.deltaTime);
        velocity = (newEndPosition - currentEnd) / Time.deltaTime;
        currentEnd = newEndPosition;
        
        float distance = Vector3.Distance(p0, currentEnd);
        ropeTension = Mathf.Clamp01(distance / 10f); 
        timeSinceLastMove += Time.deltaTime;
        
        DrawRopeWithCurve(p0, currentEnd);
        
        if (autoMoveToNextEnd && Time.time - lastMoveTime >= delayBetweenMoves)
        {
            if (Vector3.Distance(currentEnd, target) < 0.1f)
            {
                MoveToNextEnd();
                lastMoveTime = Time.time;
                timeSinceLastMove = 0f;
            }
        }
        
        previousEndPosition = currentEnd;
    }

    void DrawRopeWithCurve(Vector3 p0, Vector3 p2){
        float dist = Vector3.Distance(p0, p2);
        
        // Tính toán điểm kiểm soát chính với hiệu ứng trọng lực
        Vector3 gravityEffect = gravityDir.normalized * (dist * sagFactor * curveIntensity);
        Vector3 p1 = (p0 + p2) * 0.5f + gravityEffect;
        
        // Thêm hiệu ứng sóng và cong dựa trên vận tốc
        Vector3 waveEffect = CalculateWaveEffect(p0, p2, dist);
        p1 += waveEffect;
        
        // Tạo đường cong mượt mà với nhiều điểm kiểm soát
        for (int i = 0; i < segments; i++){
            float t = i / (segments - 1f);
            
            // Sử dụng Bezier curve với nhiều điểm kiểm soát
            Vector3 point = CalculateBezierPoint(p0, p1, p2, t);
            
            // Thêm hiệu ứng sóng nhỏ cho tự nhiên
            Vector3 waveOffset = CalculateSmallWave(p0, p2, t, dist);
            point += waveOffset;
            
            lr.SetPosition(i, point);
        }
    }
    
    // Tính toán hiệu ứng sóng dựa trên vận tốc và thời gian
    Vector3 CalculateWaveEffect(Vector3 p0, Vector3 p2, float distance)
    {
        Vector3 direction = (p2 - p0).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, gravityDir.normalized).normalized;
        
        // Hiệu ứng sóng dựa trên vận tốc
        float waveStrength = velocity.magnitude * waveAmplitude * 0.1f;
        float wavePhase = Time.time * waveFrequency + distance * 0.1f;
        
        return perpendicular * Mathf.Sin(wavePhase) * waveStrength;
    }
    
    Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        // Quadratic Bezier curve
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector3 point = uu * p0 + 2 * u * t * p1 + tt * p2;
        
        // Thêm hiệu ứng căng thẳng
        float tensionEffect = ropeTension * smoothness * 0.1f;
        Vector3 tensionOffset = (p1 - point) * tensionEffect;
        
        return point + tensionOffset;
    }
    
    // Tính toán sóng nhỏ cho hiệu ứng tự nhiên
    Vector3 CalculateSmallWave(Vector3 p0, Vector3 p2, float t, float distance)
    {
        Vector3 direction = (p2 - p0).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
        
        // Sóng nhỏ với tần số cao hơn
        float smallWavePhase = Time.time * waveFrequency * 3f + t * Mathf.PI * 4f;
        float smallWaveAmplitude = waveAmplitude * 0.3f * (1f - ropeTension);
        
        return perpendicular * Mathf.Sin(smallWavePhase) * smallWaveAmplitude;
    }
    
    public void AddEndPoint(Vector3 position)
    {
        if (_endDummy == null) _endDummy = MakeDummy("_end");
        _endDummy.position = position;
        if (!endTargets.Contains(_endDummy))
        {
            endTargets.Add(_endDummy);
            UpdateCurrentEnd();
        }
    }
    public void MoveToNextEnd()
    {
        if (endTargets.Count > 1)
        {
            currentEndIndex = (currentEndIndex + 1) % endTargets.Count;
            UpdateCurrentEnd();
            isMovingToNextEnd = false;
        }
    }
    public int GetEndPointsCount()
    {
        return endTargets.Count;
    }
    
    public int GetCurrentEndIndex()
    {
        return currentEndIndex;
    }

    public void SetStart(Vector3 pos){
        if (_startDummy == null) _startDummy = MakeDummy("_start");
        _startDummy.position = pos;
        startPoint = null;
        UpdateCurrentEnd();
    }
    
    void UpdateCurrentEnd(){
        if (endTargets.Count > 0 && currentEndIndex < endTargets.Count)
        {
            currentEnd = endTargets[currentEndIndex].position;
        }
        else if (_endDummy != null)
        {
            currentEnd = _endDummy.position;
        }
    }
    Vector3 GetCurrentTargetPosition()
    {
        if (endTargets.Count > 0 && currentEndIndex < endTargets.Count)
        {
            return endTargets[currentEndIndex].position;
        }
        else if (_endDummy != null)
        {
            return _endDummy.position;
        }
        return currentEnd;
    }
    
    public void SetAutoMoveToNextEnd(bool enable, float delay = 1.0f)
    {
        autoMoveToNextEnd = enable;
        delayBetweenMoves = delay;
        lastMoveTime = Time.time;
    }
    public void SetCurveParameters(float intensity, float frequency, float amplitude, float smooth)
    {
        curveIntensity = intensity;
        waveFrequency = frequency;
        waveAmplitude = amplitude;
        smoothness = smooth;
    }

}
