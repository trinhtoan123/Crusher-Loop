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
    public SpoolController SpoolController => spoolController;
    public ConveyorController ConveyorController => conveyorController;
    public PillarController PillarController => pillarController;
    public PathCreator PathCreation => conveyorController.PathCreation;
    public RoadMeshCreator RoadMeshCreator => conveyorController.RoadMeshCreator;
    public MapController MapController => mapController;

    public void Initialize()
    {
        spoolController.Initialize(this);
        conveyorController.Initialize(this);
        pillarController.Initialize(this);
        mapController.Initialize(this);
        isGameOver = false;
    }
    void OnDisable()
    {
    }
    
    private void Update()
    {
    }
    
  

    #region Meltdown System Methods
    public void CheckCompleteLevel()
    {
        if (GameManager.Instance.CurrentGameState == GameState.GameOver) return;
        bool isMapClear = mapController.ClearMap();
        bool canAnySpoolWind = mapController.CanAnySpoolStillWind();
        bool hasSlotPillar = mapController.HasSlotPillar();

        Debug.Log($"Tất cả spool đã dừng - Kiểm tra: MapClear={isMapClear}, CanSpoolWind={canAnySpoolWind}, HasSlotPillar={hasSlotPillar}");

        if (isMapClear)
        {
            WinLevel();
        }
        else if (!hasSlotPillar)
        {
            if (!canAnySpoolWind)
            {
                LoseLevel();
            }
        }
    }

    public void LoseLevel()
    {
        GameManager.Instance.LoseGame();

    }
    
    public void WinLevel()
    {
        GameManager.Instance.WinGame();
    }
    public void RestartLevel()
    {
        if (conveyorController != null)
        {
            conveyorController.ResetConveyor();
        }
        isGameOver = false;
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
    

    public void ProcessLevel()
    {
        currentLevel = nextLevel;
        nextLevel++;
        Debug.Log($"Đã chuyển sang level {currentLevel}, thùng tiếp theo: {nextLevel}");
    }
    
 
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
    
}



