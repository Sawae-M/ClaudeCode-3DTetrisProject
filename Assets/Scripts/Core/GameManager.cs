using UnityEngine;

public enum GameState { Playing, GameOver, Paused }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Playing;
    public int Score { get; private set; }
    public int Level { get; private set; } = 1;

    [Header("References")]
    public Board board;
    public BoardRenderer boardRenderer;
    public PieceSpawner pieceSpawner;
    public GravityManager gravityManager;
    public CubeRotator cubeRotator;
    public CameraController cameraController;
    public ObstacleSpawner obstacleSpawner;
    public UIManager uiManager;

    [Header("Difficulty")]
    public float baseFallInterval = 1.5f;
    public float minFallInterval = 0.3f;
    public float intervalDecreasePerLevel = 0.1f;
    public float levelUpInterval = 30f;

    float levelTimer;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        board.Initialize();
        boardRenderer.Initialize();
        pieceSpawner.SpawnNext();
        uiManager?.UpdateAll();
    }

    void Update()
    {
        if (State != GameState.Playing) return;

        levelTimer += Time.deltaTime;
        if (levelTimer >= levelUpInterval)
        {
            levelTimer = 0f;
            Level++;
            uiManager?.UpdateLevel(Level);
        }
    }

    public float CurrentFallInterval()
    {
        float t = baseFallInterval - (Level - 1) * intervalDecreasePerLevel;
        return Mathf.Max(t, minFallInterval);
    }

    public void AddScore(int clearedFaces)
    {
        int[] bonuses = { 0, 100, 300, 600, 1000, 1500 };
        int idx = Mathf.Clamp(clearedFaces, 0, bonuses.Length - 1);
        Score += bonuses[idx] * Level;
        uiManager?.UpdateScore(Score);
    }

    public void TriggerGameOver()
    {
        State = GameState.GameOver;
        uiManager?.ShowGameOver();
        Debug.Log("Game Over");
    }

    public void Restart()
    {
        Score = 0;
        Level = 1;
        levelTimer = 0f;
        State = GameState.Playing;
        board.Initialize();
        boardRenderer.FullRedraw();
        pieceSpawner.SpawnNext();
        uiManager?.UpdateAll();
    }
}
