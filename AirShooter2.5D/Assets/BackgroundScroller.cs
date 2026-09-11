using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    [Header("Material Settings")]
    public Material bgNormal1; // Nền thường 1
    public Material bgNormal2; // Nền thường 2
    public Material bgBoss;    // Nền Boss (Tĩnh)

    [Header("Scroll Settings")]
    public float scrollSpeed = 0.2f; // Tốc độ cuộn nền
    
    private Renderer rend;
    private Material currentMaterial;
    private bool isBossMode = false;
    private float offset = 0f;

    void Start()
    {
        rend = GetComponent<Renderer>();
        // Ban đầu gán Nền 1 làm nền mặc định
        SetBackground(bgNormal1, false);
    }

    void Update()
    {
        // Nếu đang ở chế độ Boss, không chạy hiệu ứng cuộn
        if (isBossMode) return;

        // Tăng offset theo thời gian để tạo hiệu ứng di chuyển
        offset += Time.deltaTime * scrollSpeed;

        // Tránh offset vượt quá giá trị 1 gây tràn số
        if (offset > 1f) offset -= 1f;

        // Cập nhật Offset của Material (Trục Y)
        if (currentMaterial != null)
        {
            currentMaterial.mainTextureOffset = new Vector2(0, offset);
        }
    }

    /// <summary>
    /// Thay đổi Background và chế độ Cuộn/Tĩnh
    /// </summary>
    public void SetBackground(Material newMaterial, bool isBoss)
    {
        isBossMode = isBoss;
        currentMaterial = newMaterial;
        rend.material = currentMaterial;

        // Nếu chuyển sang Boss (Tĩnh), đưa offset về 0 để hình phẳng đẹp
        if (isBoss)
        {
            currentMaterial.mainTextureOffset = Vector2.zero;
        }
    }

    // --- CÁC HÀM TIỆN ÍCH DÙNG ĐỂ GỌI TỪ GAME MANAGER HOẶC BẮT SỰ KIỆN ---

    // Chuyển sang Nền Thường 1
    public void SwitchToNormal1()
    {
        SetBackground(bgNormal1, false);
    }

    // Chuyển sang Nền Thường 2
    public void SwitchToNormal2()
    {
        SetBackground(bgNormal2, false);
    }

    // Chuyển sang Nền Boss (Dừng cuộn)
    public void SwitchToBossBackground()
    {
        SetBackground(bgBoss, true);
    }
}