using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance;

    [Header("Nhóm Hình Nền")]
    public GameObject normalBackgroundGroup;
    public GameObject bossBackgroundGroup;

    [Header("Cấu Hình Cuộn Nền")]
    public float scrollSpeed = 3.0f; // Tốc độ trôi của nền
    public float resetPositionY = -20.0f; // Vị trí Y khi nền trôi hết màn hình
    public float startPositionY = 20.0f;   // Vị trí Y để đưa nền quay lại đỉnh

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ShowNormalBackground();
    }

    void Update()
    {
        HandleBackgroundScrolling();
    }

    // Logic cuộn nền tự động trôi xuống
    private void HandleBackgroundScrolling()
    {
        // Kiểm tra nhóm nền nào đang bật thì cuộn nhóm đó
        GameObject activeGroup = normalBackgroundGroup.activeSelf ? normalBackgroundGroup : bossBackgroundGroup;

        if (activeGroup != null)
        {
            // Cho toàn bộ nhóm nền trôi xuống theo trục Y
            activeGroup.transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);

            // Tự động lặp lại vị trí khi trôi hết
            if (activeGroup.transform.position.y <= resetPositionY)
            {
                Vector3 newPos = activeGroup.transform.position;
                newPos.y = startPositionY;
                activeGroup.transform.position = newPos;
            }
        }
    }

    // Chuyển sang nền thường
    public void ShowNormalBackground()
    {
        if (normalBackgroundGroup != null) normalBackgroundGroup.SetActive(true);
        if (bossBackgroundGroup != null) bossBackgroundGroup.SetActive(false);
    }

    // Chuyển sang nền Boss
    public void ShowBossBackground()
    {
        if (normalBackgroundGroup != null) normalBackgroundGroup.SetActive(false);
        if (bossBackgroundGroup != null) bossBackgroundGroup.SetActive(true);
    }
}