using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameState currentGameState = GameState.Playing;
    [SerializeField] private DataLevel dataLevel;
    [SerializeField] private SpoolData spoolData;
    [SerializeField] LevelManager levelManager;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;
    public SpoolData SpoolData => spoolData;
    public DataLevel DataLevel => dataLevel;
    public GameState CurrentGameState => currentGameState;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        OnLoadLevel();
        Initialize();
    }
    public void Initialize()
    {
        levelManager.Initialize();
    }
    public void OnLoadLevel()
    {
    }
    public void StartGame()
    {
    }
    public void PauseGame()
    {
    }
    public void EndGame()
    {
    }

    public void LoseGame()
    {
        currentGameState = GameState.GameOver;
        losePanel.SetActive(true);
    }
    public void WinGame()
    {
        currentGameState = GameState.GameOver;
        winPanel.SetActive(true);
    }
    public void SetGameState(GameState state)
    {
        currentGameState = state;
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void NextGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void HomeGame()
    {
        SceneManager.LoadScene("Home");
    }
}
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}
