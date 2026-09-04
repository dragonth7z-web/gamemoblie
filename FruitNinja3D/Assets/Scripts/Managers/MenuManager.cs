using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("UI Popups")]
    public GameObject difficultyPanel;
    public GameObject highScorePanel;
    public GameObject settingsPanel;

    [Header("UI References")]
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI currentDifficultyText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() => CloseAllPanels();

    // 1. BẮT ĐẦU VÁN CHƠI
    public void StartGame()
    {
        CloseAllPanels();
        GameManager.Instance?.StartGame();
    }

    // 2. MỞ BẢNG ĐỘ KHÓ
    public void OpenDifficulty()
    {
        CloseAllPanels();
        if (difficultyPanel != null) difficultyPanel.SetActive(true);
        UpdateDifficultyUI();
    }

    public void SetDifficulty(int level) // 0: Dễ, 1: Trung bình, 2: Khó
    {
        PlayerPrefs.SetInt("GameDifficulty", level);
        PlayerPrefs.Save();
        Debug.Log($"Đã chọn độ khó: {level}");
        CloseAllPanels();
    }

    // 3. MỞ BẢNG KỶ LỤC
    public void OpenHighScore()
    {
        CloseAllPanels();
        if (highScorePanel == null) return;

        highScorePanel.SetActive(true);
        int topScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
        {
            highScoreText.text = $"Điểm Cao Nhất: {topScore}";
        }
    }

    // 4. MỞ BẢNG CÀI ĐẶT
    public void OpenSettings()
    {
        CloseAllPanels();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    // 5. ĐÓNG BẢNG UI POPUP & HỒI PHỤC QUẢ
    public void CloseAllPanels()
    {
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        if (highScorePanel != null) highScorePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Đã sửa chính xác enum Unity 6: FindObjectsSortMode (có chữ 's')
        MenuFruitButton[] menuButtons = FindObjectsByType<MenuFruitButton>(FindObjectsSortMode.None);
        foreach (var btn in menuButtons)
        {
            btn.RespawnFruit();
        }
    }

    private void UpdateDifficultyUI()
    {
        if (currentDifficultyText == null) return;

        int currentLevel = PlayerPrefs.GetInt("GameDifficulty", 1);
        string levelName = currentLevel switch
        {
            0 => "Dễ",
            1 => "Trung Bình",
            2 => "Khó",
            _ => "Trung Bình"
        };
        currentDifficultyText.text = $"Độ khó hiện tại: {levelName}";
    }
}