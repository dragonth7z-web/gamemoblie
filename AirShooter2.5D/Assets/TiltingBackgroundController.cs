using UnityEngine;

public class TiltingBackgroundController : MonoBehaviour
{
    [Header("Gán 2 Material Nền")]
    public Material matNormal; // Material Nền thường (Cuộn)
    public Material matBoss;   // Material Nền Boss (Tĩnh)

    [Header("Tốc độ cuộn nền thường")]
    public float scrollSpeed = 0.15f;

    private Renderer rend;
    private Material currentMat;
    private float offset = 0f;
    private bool isBossMode = false;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void Start()
    {
        // Mặc định chạy Nền thường khi vào game
        SwitchToNormal();
    }

    void Update()
    {
        // Khi ở chế độ Boss (isBossMode = true), dừng toàn bộ hiệu ứng cuộn
        if (isBossMode || currentMat == null) return;

        // Cuộn texture nền thường trên mặt phẳng nghiêng
        offset += Time.deltaTime * scrollSpeed;
        if (offset > 1f) offset -= 1f;

        currentMat.mainTextureOffset = new Vector2(0, offset);
    }

    // --- HÀM CHUYỂN NỀN THƯỜNG (CÓ CUỘN) ---
    public void SwitchToNormal()
    {
        if (matNormal == null) return;
        isBossMode = false;
        currentMat = matNormal;
        rend.material = currentMat;
    }

    // --- HÀM CHUYỂN NỀN BOSS (TĨNH) ---
    public void SwitchToBoss()
    {
        if (matBoss == null) return;
        isBossMode = true;
        currentMat = matBoss;
        rend.material = currentMat;
        // Đưa offset về 0 để hình nền Boss nằm thẳng đẹp, không bị lệch
        currentMat.mainTextureOffset = Vector2.zero;
    }
}