using System.Collections;
using System.Collections.Generic;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private SpoolController spoolController;
    [SerializeField] private ConveyorController conveyorController;
    [SerializeField] private PillarController pillarController;
    [SerializeField] private MapController mapController;

    private bool isGameOver = false;
    private Coroutine restartCoroutine;
    private int currentLevel = 0;
    private int nextLevel = 1;
    public SpoolData SpoolData => GameManager.Instance.SpoolData;
    public SpoolController SpoolController => spoolController;
    public ConveyorController ConveyorController => conveyorController;
    public PillarController PillarController => pillarController;
    public PathCreator PathCreation => conveyorController.PathCreation;
    public RoadMeshCreator RoadMeshCreator => conveyorController.RoadMeshCreator;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        spoolController.Initialize(this);
        conveyorController.Initialize(this);
        pillarController.Initialize(this);
        mapController.Initialize(this);
        SubscribeToConveyorEvents();
        isGameOver = false;
    }
    
    private void Update()
    {
        HandleDebugInput();
    }
    
    #region Conveyor Segment Management
    
    private void HandleDebugInput()
    {
      
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ManualRestart();
        }
    }

    
    #endregion
    
    #region Meltdown System Methods
  
    private void SubscribeToConveyorEvents()
    {
        ConveyorController.OnConveyorMeltdown += OnConveyorMeltdown;
        ConveyorController.OnConveyorJammed += OnConveyorJammed;
    }
   
    private void UnsubscribeFromConveyorEvents()
    {
        ConveyorController.OnConveyorMeltdown -= OnConveyorMeltdown;
        ConveyorController.OnConveyorJammed -= OnConveyorJammed;
    }
    private void OnConveyorJammed()
    {
        Debug.LogWarning("🚫 BĂNG CHUYỀN BỊ TẮC! Giải quyết nhanh chóng để tránh meltdown!");
    }
    
 
    public void OnConveyorMeltdown()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        // Auto restart nếu được enable
        // if (autoRestartOnMeltdown)
        // {
        //     restartCoroutine = StartCoroutine(RestartAfterDelay());
        // }
    }
    
    public void RestartLevel()
    {
        
        // Reset conveyor state
        if (conveyorController != null)
        {
            conveyorController.ResetConveyor();
        }
        isGameOver = false;
        
        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ManualRestart()
    {
        if (restartCoroutine != null)
        {
            StopCoroutine(restartCoroutine);
        }
        
        RestartLevel();
    }
    
    /// <summary>
    /// Xử lý chuyển level - chuyển sang thùng tiếp theo
    /// </summary>
    public void ProcessLevel()
    {
        currentLevel = nextLevel;
        nextLevel++;
        Debug.Log($"Đã chuyển sang level {currentLevel}, thùng tiếp theo: {nextLevel}");
    }
    
    /// <summary>
    /// Lấy chỉ số thùng tiếp theo
    /// </summary>
    public int GetNextLevel()
    {
        return nextLevel;
    }
    
    /// <summary>
    /// Lấy chỉ số thùng hiện tại
    /// </summary>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    
    #endregion
    
    #region Unity Events
    
    private void OnDisable()
    {
        UnsubscribeFromConveyorEvents();
    }
    
    #endregion
}



