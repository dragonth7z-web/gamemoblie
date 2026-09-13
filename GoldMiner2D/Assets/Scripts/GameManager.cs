using UnityEngine;

public enum GameState
{
    Menu,
    Playing,
    LevelComplete,
    GameOver,
    Paused
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
    public int highScore = 0;          
    public int totalScore = 0;         // Tổng điểm tích lũy các màn

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

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        totalScore = PlayerPrefs.GetInt("TotalScore", 0);
    }

    void Start()
    {
        ChangeState(GameState.Menu);
    }

    void Update()
    {
        if (currentState != GameState.Playing || !isGameActive) return;

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

        if (newState == GameState.LevelComplete || newState == GameState.GameOver || newState == GameState.Paused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowScreen(newState);

            if (newState == GameState.LevelComplete)
            {
                totalScore += currentScore; // Cộng dồn điểm khi thắng màn
                int reward = 100 + (currentLevel * 50); 
                
                PlayerPrefs.SetInt("TotalScore", totalScore);
                PlayerPrefs.Save();

                UIManager.Instance.ShowLevelCompleteUI(currentScore, totalScore, reward);
            }
            else if (newState == GameState.GameOver)
            {
                // Khi thua hiển thị điểm đạt được và kỷ lục hiện tại
                UIManager.Instance.ShowGameOverUI("Hết thời gian! Chưa đạt điểm mục tiêu.", currentScore, highScore);
            }
        }
    }

    public void StartLevel(int level)
    {
        Time.timeScale = 1f;
        currentLevel = level;
        
        targetScore = 300 + (currentLevel * 300);
        timeRemaining = Mathf.Max(60f - (currentLevel - 1) * 5f, 30f);
        currentScore = 0; 
        
        currentState = GameState.Playing;
        isGameActive = true;

        if (ItemSpawner.Instance != null)
        {
            ItemSpawner.Instance.SpawnItemsForLevel(currentLevel);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowScreen(GameState.Playing);
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

        if (currentScore >= targetScore)
        {
            isGameActive = false;
            CheckLevelResult(); // Đồng bộ hóa qua CheckLevelResult để kiểm tra kỷ lục chuẩn xác
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

        // Tính tổng điểm tiềm năng để so sánh kỷ lục (Tổng điểm tích lũy + điểm màn hiện tại)
        int finalCalculatedScore = totalScore + currentScore;

        // Kiểm tra và cập nhật kỷ lục chung (áp dụng cho cả Thắng và Thua)
        if (finalCalculatedScore > highScore)
        {
            highScore = finalCalculatedScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

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
        totalScore = 0;
        PlayerPrefs.SetInt("TotalScore", 0);
        PlayerPrefs.Save();
        StartLevel(1);
    }
}