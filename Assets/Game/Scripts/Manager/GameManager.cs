using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private GameState currentGameState = GameState.Playing;
    [SerializeField] private DataLevel dataLevel;
    public DataLevel DataLevel => dataLevel;
    public GameState CurrentGameState => currentGameState;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
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
    public void SetGameState(GameState state)
    {
        currentGameState = state;
    }
}
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}
