using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("UI Main Menu")]
    public GameObject menuPanel; // Panel chính chứa các nút hoặc giao diện menu

    [Header("UI Popups")]
    public GameObject difficultyPanel;
    public GameObject highScorePanel;
    public GameObject settingsPanel;
    public GameObject tutorialPanel;

    [Header("UI References")]
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI currentDifficultyText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start() => ShowMainMenuView();

    // Hiển thị menu chính và ẩn các popup khác
    public void ShowMainMenuView()
    {
        CloseAllPopups();
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void StartGame()
    {
        CloseAllPopups();
        GameManager.Instance?.StartGame();
    }

    public void OpenDifficulty()
    {
        HideMainMenuAndOpenPopup(difficultyPanel);
        UpdateDifficultyUI();
    }

    public void SetDifficulty(int level) 
    {
        PlayerPrefs.SetInt("GameDifficulty", level);
        PlayerPrefs.Save();
        ShowMainMenuView(); // Chọn xong quay lại menu chính và hồi sinh quả
    }

    public void OpenHighScore()
    {
        HideMainMenuAndOpenPopup(highScorePanel);
        int topScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
        {
            highScoreText.text = $"Điểm Cao Nhất: {topScore}";
        }
    }

    public void OpenSettings()
    {
        HideMainMenuAndOpenPopup(settingsPanel);
    }

    public void OpenTutorial()
    {
        HideMainMenuAndOpenPopup(tutorialPanel);
    }

    // Hàm phụ trợ để ẩn Menu chính, mở popup tương ứng
    private void HideMainMenuAndOpenPopup(GameObject targetPopup)
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        CloseAllPopups();
        if (targetPopup != null) targetPopup.SetActive(true);
    }

    // Đóng toàn bộ các popup phụ
    public void CloseAllPopups()
    {
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        if (highScorePanel != null) highScorePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        // Hồi sinh lại các quả menu để người chơi tiếp tục chém
        MenuFruitButton[] menuButtons = FindObjectsByType<MenuFruitButton>(FindObjectsInactive.Include);
        foreach (var btn in menuButtons)
        {
            if (btn == null) continue;
            btn.RespawnFruit();
        }
    }

    // Hàm gọi khi nhấn nút "Quay lại" (Back) từ các popup phụ về Menu chính
    public void BackToMenu()
    {
        ShowMainMenuView();
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