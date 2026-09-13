using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panel Tạm Dừng")]
    public GameObject pauseMenuUI; // Kéo Panel Pause (bảng chứa nút Tiếp tục, Thoát...) vào đây

    void Start()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    void Update()
    {
        // Bấm phím ESC để bật/tắt tạm dừng nhanh
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.currentState == GameState.Playing)
                {
                    PauseGame();
                }
                else if (GameManager.Instance.currentState == GameState.Paused)
                {
                    ResumeGame();
                }
            }
        }
    }

    public void PauseGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.Paused);
        }

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        Time.timeScale = 0f; // Dừng thời gian game
    }

    public void ResumeGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.Playing);
        }

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        Time.timeScale = 1f; // Khôi phục thời gian game
    }
}