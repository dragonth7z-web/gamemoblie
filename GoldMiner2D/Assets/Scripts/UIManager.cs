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
    public GameObject howToPlayPanel;

    [Header("Text trong PlayingPanel")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI levelText;

    [Header("Text trong Bảng Thắng (Level Complete)")]
    public TextMeshProUGUI completeScoreText; 
    public TextMeshProUGUI totalScoreText;    
    public TextMeshProUGUI rewardText;        

    [Header("Text trong Bảng Thua (Game Over)")]
    public TextMeshProUGUI gameOverReasonText; 
    public TextMeshProUGUI finalScoreText;     
    public TextMeshProUGUI gameOverHighScoreText; 

    [Header("Text Hiển Thị Kỷ Lục Menu")]
    public TextMeshProUGUI menuHighScoreText;  

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    void Start()
    {
        ShowScreen(GameState.Menu);
        UpdateRecordUI();
    }

    public void ShowScreen(GameState state)
    {
        if (menuPanel != null) menuPanel.SetActive(state == GameState.Menu);
        if (playingPanel != null) playingPanel.SetActive(state == GameState.Playing);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(state == GameState.LevelComplete);
        if (gameOverPanel != null) gameOverPanel.SetActive(state == GameState.GameOver);

        if (howToPlayPanel != null && state != GameState.Menu)
        {
            howToPlayPanel.SetActive(false);
        }

        UpdateRecordUI();
    }

    public void UpdateUI(int score, int target, float time, int level)
    {
        if (scoreText != null) scoreText.text = "Điểm: " + score;
        if (targetText != null) targetText.text = "Mục tiêu: " + target;
        if (timerText != null) timerText.text = "Thời gian: " + Mathf.CeilToInt(time) + "s";
        if (levelText != null) levelText.text = "Màn: " + level;
    }

    public void ShowLevelCompleteUI(int score, int totalScore, int reward)
    {
        if (completeScoreText != null) completeScoreText.text = "Điểm màn này: +" + score;
        if (totalScoreText != null) totalScoreText.text = "Tổng điểm tích lũy: " + totalScore;
        if (rewardText != null) rewardText.text = "Phần thưởng: +$" + reward;
        
        UpdateRecordUI();
    }

    // Đã cập nhật nhận đúng 3 tham số khớp hoàn toàn với GameManager.cs
    public void ShowGameOverUI(string reason, int score, int highScore)
    {
        if (gameOverReasonText != null) gameOverReasonText.text = reason;
        if (finalScoreText != null) finalScoreText.text = "Điểm đạt được: " + score;
        if (gameOverHighScoreText != null) gameOverHighScoreText.text = "Kỷ lục hiện tại: " + highScore;
        
        UpdateRecordUI();
    }

    public void UpdateRecordUI()
    {
        int currentHighScore = 0;
        if (GameManager.Instance != null)
        {
            currentHighScore = GameManager.Instance.highScore;
        }
        else
        {
            currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        }

        if (menuHighScoreText != null) 
            menuHighScoreText.text = "Kỷ lục: " + currentHighScore;
    }

    public void OnClickOpenHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
            if (menuPanel != null) menuPanel.transform.Find("ButtonsHolder")?.gameObject.SetActive(false); 
        }
    }

    public void OnClickCloseHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
            if (menuPanel != null) menuPanel.transform.Find("ButtonsHolder")?.gameObject.SetActive(true);
        }
    }

    public void OnClickPlay() 
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartLevel(1); 
        }
    }

    public void OnClickNextLevel() 
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NextLevel();
        }
    }

    public void OnClickRestart() 
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame(); 
        }
    }
}