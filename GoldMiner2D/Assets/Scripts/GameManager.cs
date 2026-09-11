using UnityEngine;

public enum GameState
{
    Menu,
    Playing,
    LevelComplete,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State & Variables")]
    public GameState currentState;
    public int currentLevel = 1;
    public int currentScore = 0;
    public int targetScore = 600;
    public float timeRemaining = 60f;

    private bool isGameActive = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ChangeState(GameState.Menu);
    }

    void Update()
    {
        if (currentState != GameState.Playing || !isGameActive) return;

        // Đếm ngược thời gian
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateUI(currentScore, targetScore, timeRemaining, currentLevel);
            }
        }
        else
        {
            timeRemaining = 0;
            CheckLevelResult();
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowScreen(newState);
        }
    }

    public void StartLevel(int level)
    {
        Time.timeScale = 1f;
        currentLevel = level;
        
        // Tính toán độ khó mới
        targetScore = 300 + (currentLevel * 300);
        timeRemaining = Mathf.Max(60f - (currentLevel - 1) * 5f, 30f);
        currentScore = 0; // Reset điểm màn mới
        
        currentState = GameState.Playing;
        isGameActive = true;

        // 1. Sinh vật phẩm mới
        if (ItemSpawner.Instance != null)
        {
            ItemSpawner.Instance.SpawnItemsForLevel(currentLevel);
        }

        // 2. Ép UIManager cập nhật chữ lập tức lên màn hình
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI(currentScore, targetScore, timeRemaining, currentLevel);
        }
    }

    public void AddScore(int amount)
    {
        if (currentState != GameState.Playing || !isGameActive) return;

        currentScore += amount;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI(currentScore, targetScore, timeRemaining, currentLevel);
        }

        // Chuyển màn ngay nếu vượt điểm mục tiêu
        if (currentScore >= targetScore)
        {
            isGameActive = false;
            ChangeState(GameState.LevelComplete);
        }
    }

    public void DeductTime(float penaltyAmount)
    {
        if (currentState != GameState.Playing) return;

        timeRemaining -= penaltyAmount;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            CheckLevelResult();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI(currentScore, targetScore, timeRemaining, currentLevel);
        }
    }

    void CheckLevelResult()
    {
        isGameActive = false;

        if (currentScore >= targetScore)
        {
            ChangeState(GameState.LevelComplete);
        }
        else
        {
            ChangeState(GameState.GameOver);
        }
    }

    public void NextLevel()
    {
        StartLevel(currentLevel + 1);
    }

    public void RestartGame()
    {
        StartLevel(1);
    }
}