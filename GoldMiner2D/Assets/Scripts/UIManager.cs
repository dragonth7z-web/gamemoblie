using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Các Panel Trạng Thái")]
    public GameObject menuPanel;
    public GameObject playingPanel;
    public GameObject levelCompletePanel;
    public GameObject gameOverPanel;

    [Header("Text trong PlayingPanel")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI levelText;

    [Header("Text trong Bảng Thắng / Thua")]
    public TextMeshProUGUI completeScoreText;
    public TextMeshProUGUI gameOverReasonText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Khi chạy game tự động hiển thị màn hình Menu
        ShowScreen(GameState.Menu);
    }

    public void ShowScreen(GameState state)
    {
        if (menuPanel != null) menuPanel.SetActive(state == GameState.Menu);
        if (playingPanel != null) playingPanel.SetActive(state == GameState.Playing);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(state == GameState.LevelComplete);
        if (gameOverPanel != null) gameOverPanel.SetActive(state == GameState.GameOver);

        if (state == GameState.LevelComplete && completeScoreText != null)
        {
            completeScoreText.text = "Điểm đạt được: " + GameManager.Instance.currentScore;
        }
        else if (state == GameState.GameOver && gameOverReasonText != null)
        {
            gameOverReasonText.text = "Hết giờ! Cần đạt " + GameManager.Instance.targetScore + " điểm.";
        }
    }

    public void UpdateUI(int score, int target, float time, int level)
    {
        if (scoreText != null) scoreText.text = "Điểm: " + score;
        if (targetText != null) targetText.text = "Mục tiêu: " + target;
        if (timerText != null) timerText.text = "Thời gian: " + Mathf.CeilToInt(time) + "s";
        if (levelText != null) levelText.text = "Màn: " + level;
    }

    // Gán vào nút Chơi (Btn_Play)
    public void OnClickPlay() 
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartLevel(1);
        }
        ShowScreen(GameState.Playing);
    }

    // Gán vào nút Qua Màn
    public void OnClickNextLevel() 
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NextLevel();
        }
        ShowScreen(GameState.Playing);
    }

    // Gán vào nút Chơi Lại
    public void OnClickRestart() 
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        ShowScreen(GameState.Playing);
    }
}