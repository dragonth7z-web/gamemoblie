using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameState
{
    Menu,
    Playing,
    Pause,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    [Header("Panels UI")]
    public GameObject menuPanel;
    public GameObject playingHUDPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    [Header("UI References - HUD Playing")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI comboText;
    public Image[] heartImages;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    [Header("UI References - Game Over")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalHighScoreText;

    [Header("Cấu hình Mạng")]
    public int maxLives = 3;
    private int currentLives;

    private int score = 0;
    private int highScore = 0;

    // Quản lý Combo
    private int fruitsSlicedInCurrentStroke = 0;
    private float strokeTimer = 0f;
    [SerializeField] private float comboWindow = 0.25f;
    private Coroutine hideComboCoroutine;

    [Header("Spawner Reference (Tùy chọn)")]
    public Spawner spawner;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        ChangeState(GameState.Menu); // Bắt đầu game ở màn hình Menu
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing && strokeTimer > 0f)
        {
            strokeTimer -= Time.deltaTime;
            if (strokeTimer <= 0f)
            {
                EvaluateCombo();
            }
        }
    }

    // -------------------------------------------------------------
    // QUẢN LÝ TẬP TRUNG TRẠNG THÁI GAME & ÂM THANH
    // -------------------------------------------------------------
    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        if (menuPanel != null) menuPanel.SetActive(false);
        if (playingHUDPanel != null) playingHUDPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Đảm bảo đóng sạch các Popup phụ
        MenuManager.Instance?.CloseAllPanels();

        switch (newState)
        {
            case GameState.Menu:
                Time.timeScale = 1f;
                if (menuPanel != null) menuPanel.SetActive(true);
                SetMenuFruitsActive(true); // Hiện các quả Menu

                // 🎵 Bật Nhạc Menu Mở Đầu
                AudioManager.Instance?.PlayMenuMusic();
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                if (playingHUDPanel != null) playingHUDPanel.SetActive(true);
                SetMenuFruitsActive(false); // Ẩn hoàn toàn các quả Menu

                // 🎵 Bật Nhạc Nền Trò Chơi
                AudioManager.Instance?.PlayGameplayMusic();
                break;

            case GameState.Pause:
                Time.timeScale = 0f;
                if (pausePanel != null) pausePanel.SetActive(true);
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
                if (finalScoreText != null) finalScoreText.text = $"Score: {score}";
                if (finalHighScoreText != null) finalHighScoreText.text = $"Best: {highScore}";

                // 🔊 Phát tiếng Thua Game (SFX) & Nhạc Game Over
                AudioManager.Instance?.PlayGameOverSound();
                AudioManager.Instance?.PlayGameOverMusic();
                break;
        }
    }

    // -------------------------------------------------------------
    // LOGIC CHƠI GAME & ĐIỂM SỐ
    // -------------------------------------------------------------
    public void StartGame()
    {
        score = 0;
        currentLives = maxLives;
        fruitsSlicedInCurrentStroke = 0;
        strokeTimer = 0f;

        if (comboText != null) comboText.gameObject.SetActive(false);

        // 🛑 Stop toàn bộ âm thanh SFX dở dang (tiếng nổ, xè xè bom, chém gió)
        AudioManager.Instance?.StopSFX();

        // 🧹 Clear sạch sẽ toàn bộ Quả, Bom, Mảnh vỡ và Hiệu ứng nổ VFX
        ClearAllActiveObjects();

        // Reset độ khó spawner
        if (spawner != null) spawner.ResetDifficulty();

        UpdateUI();
        ChangeState(GameState.Playing);
    }

    public void AddScore(int points)
    {
        if (CurrentState != GameState.Playing) return;

        score += points;

        if (strokeTimer <= 0f) fruitsSlicedInCurrentStroke = 0;
        fruitsSlicedInCurrentStroke++;
        strokeTimer = comboWindow;

        UpdateUI();

        if (scoreText != null)
        {
            StartCoroutine(PunchScale(scoreText.transform, 1.2f, 0.1f));
        }
    }

    private void EvaluateCombo()
    {
        if (fruitsSlicedInCurrentStroke >= 3)
        {
            int bonusScore = fruitsSlicedInCurrentStroke * 2;
            score += bonusScore;

            // 🔔 Phát hiệu ứng âm thanh Combo
            AudioManager.Instance?.PlayComboSound();

            if (comboText != null)
            {
                comboText.text = $"<color=#FFD700>COMBO x{fruitsSlicedInCurrentStroke}</color>\n<size=80%>(+{bonusScore}!)</size>";
                comboText.gameObject.SetActive(true);

                if (hideComboCoroutine != null) StopCoroutine(hideComboCoroutine);
                hideComboCoroutine = StartCoroutine(HideComboText(1.2f));

                StartCoroutine(PunchScale(comboText.transform, 1.4f, 0.15f));
            }
        }

        fruitsSlicedInCurrentStroke = 0;
        UpdateUI();
    }

    private IEnumerator HideComboText(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (comboText != null) comboText.gameObject.SetActive(false);
    }

    public void LoseLife()
    {
        if (CurrentState != GameState.Playing) return;

        currentLives--;
        UpdateUI();

        if (heartImages != null && currentLives >= 0 && currentLives < heartImages.Length)
        {
            if (heartImages[currentLives] != null)
            {
                StartCoroutine(PunchScale(heartImages[currentLives].transform, 1.4f, 0.15f));
            }
        }

        if (currentLives <= 0)
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (CurrentState == GameState.GameOver) return;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        ChangeState(GameState.GameOver);
    }

    // -------------------------------------------------------------
    // THAO TÁC NÚT BẤM (BUTTON EVENTS)
    // -------------------------------------------------------------
    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            ChangeState(GameState.Pause);
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Pause)
        {
            ChangeState(GameState.Playing);
        }
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void GoToMenu()
    {
        AudioManager.Instance?.StopSFX();
        ClearAllActiveObjects();
        ChangeState(GameState.Menu);
    }

    // 🧹 Dọn dẹp triệt để Quả, Bom, Mảnh vỡ & VFX khi Reset/Chơi lại
    private void ClearAllActiveObjects()
    {
        // 1. Xóa tất cả trái cây
        Fruit[] fruits = FindObjectsByType<Fruit>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var f in fruits)
        {
            Destroy(f.gameObject);
        }

        // 2. Xóa tất cả quả bom (Nếu bạn dùng script Bomb riêng)
        Bomb[] bombs = FindObjectsByType<Bomb>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var b in bombs)
        {
            Destroy(b.gameObject);
        }

        // 3. Xóa các mảnh vỡ trái cây
        GameObject[] slicedParts = GameObject.FindGameObjectsWithTag("SlicedFruit");
        foreach (var part in slicedParts)
        {
            Destroy(part);
        }

        // 4. Xóa tất cả Particle System (Hoạt họa nổ bom, khói, tia lửa còn sót trên màn hình)
        ParticleSystem[] particles = FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in particles)
        {
            Destroy(p.gameObject);
        }
    }

    // Bật/Tắt tất cả quả Menu 3D
    private void SetMenuFruitsActive(bool isActive)
    {
        MenuFruitButton[] menuFruits = FindObjectsByType<MenuFruitButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var btn in menuFruits)
        {
            btn.gameObject.SetActive(isActive);
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (highScoreText != null) highScoreText.text = $"Best: {highScore}";

        if (heartImages != null && heartImages.Length > 0)
        {
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] == null) continue;

                if (i < currentLives)
                {
                    heartImages[i].enabled = true;
                    if (fullHeartSprite != null) heartImages[i].sprite = fullHeartSprite;
                }
                else
                {
                    if (emptyHeartSprite != null) heartImages[i].sprite = emptyHeartSprite;
                    else heartImages[i].enabled = false;
                }
            }
        }
    }

    private IEnumerator PunchScale(Transform targetTransform, float targetScale, float duration)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 maxScaleVector = Vector3.one * targetScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            targetTransform.localScale = Vector3.Lerp(originalScale, maxScaleVector, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            targetTransform.localScale = Vector3.Lerp(maxScaleVector, originalScale, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        targetTransform.localScale = originalScale;
    }
}