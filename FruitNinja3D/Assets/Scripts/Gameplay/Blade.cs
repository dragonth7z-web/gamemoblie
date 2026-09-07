using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Blade : MonoBehaviour
{
    private Camera mainCamera;
    private SphereCollider bladeCollider;
    private TrailRenderer bladeTrail;
    private Rigidbody bladeRigidbody;
    private bool isSlicing;

    [Header("Cài đặt vận tốc vung dao")]
    [Tooltip("Tốc độ vuốt tối thiểu để dao chém được quả")]
    public float minSliceVelocity = 2.5f;

    [Header("Cài đặt âm thanh Chém Gió")]
    [Tooltip("Khoảng thời gian tối thiểu giữa 2 lần phát tiếng chém gió (giây)")]
    [SerializeField] private float swishCooldown = 0.35f;
    
    [Tooltip("Vận tốc vung dao tối thiểu để kích hoạt tiếng chém gió")]
    [SerializeField] private float minSwishVelocity = 5.0f;
    
    private float lastSwishTime = 0f;

    [Header("Cài đặt Combo & Âm thanh Combo")]
    [Tooltip("Thời gian chờ tối đa giữa các lần chém để giữ chuỗi Combo (giây)")]
    [SerializeField] private float comboTimeout = 0.8f;
    
    private float comboTimer = 0f;
    private bool isComboActive = false;
    private int comboCount = 0;

    [Header("Cài đặt Hiệu ứng Vệt chém (Trail & Sparkles)")]
    [Tooltip("Danh sách màu (Nếu bỏ trống, C# sẽ tự sinh 4 màu chuẩn phát sáng)")]
    [SerializeField] private Gradient[] trailGradients;

    [Tooltip("Kéo GameObject con chứa Particle System hạt sáng vào đây")]
    [SerializeField] private ParticleSystem bladeSparkles;

    public Vector3 direction { get; private set; }
    public float sliceVelocity { get; private set; }

    private void Awake()
    {
        mainCamera = Camera.main;
        bladeCollider = GetComponent<SphereCollider>();
        bladeTrail = GetComponent<TrailRenderer>();
        bladeRigidbody = GetComponent<Rigidbody>();

        bladeRigidbody.isKinematic = true;
        bladeRigidbody.interpolation = RigidbodyInterpolation.None;
        bladeCollider.isTrigger = true;

        InitDefaultGradients();
    }

    private void InitDefaultGradients()
    {
        if (trailGradients == null || trailGradients.Length == 0)
        {
            trailGradients = new Gradient[4];
            trailGradients[0] = CreateGlowGradient(Color.cyan, Color.white);               // Xanh Neon
            trailGradients[1] = CreateGlowGradient(Color.red, Color.yellow);               // Đỏ Vàng
            trailGradients[2] = CreateGlowGradient(new Color(1f, 0f, 0.5f), Color.white);   // Hồng Huỳnh Quang
            trailGradients[3] = CreateGlowGradient(Color.green, Color.yellow);              // Xanh Lá
        }
    }

    private Gradient CreateGlowGradient(Color startColor, Color endColor)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(startColor, 0.0f), new GradientColorKey(endColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.2f, 1.0f) }
        );
        return g;
    }

    private void OnEnable() => StopSlicing();
    private void OnDisable() => StopSlicing();

    private void Update()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.CurrentState == GameState.Pause || 
                GameManager.Instance.CurrentState == GameState.GameOver)
            {
                if (isSlicing) StopSlicing();
                return;
            }
        }

        if (Time.timeScale == 0f)
        {
            if (isSlicing) StopSlicing();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartSlicing();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopSlicing();
        }

        if (isSlicing)
        {
            UpdateSlicing();
        }

        // ✨ XỬ LÝ ĐẾM NGƯỢC THỜI GIAN COMBO
        if (isComboActive)
        {
            comboTimer -= Time.deltaTime;

            // Nếu quá thời gian comboTimeout mà không chém thêm trái cây -> Kết thúc Combo & Ngắt âm thanh lập tức
            if (comboTimer <= 0f)
            {
                EndCombo();
            }
        }
    }

    private void StartSlicing()
    {
        Vector3 worldPosition = GetMouseWorldPosition();

        transform.position = worldPosition;
        bladeRigidbody.position = worldPosition;

        isSlicing = true;
        bladeCollider.enabled = true;

        if (bladeTrail != null)
        {
            bladeTrail.enabled = false;
            bladeTrail.Clear();

            if (trailGradients != null && trailGradients.Length > 0)
            {
                int randomIndex = Random.Range(0, trailGradients.Length);
                bladeTrail.colorGradient = trailGradients[randomIndex];
            }

            bladeTrail.enabled = true;
        }

        if (bladeSparkles != null)
        {
            bladeSparkles.Play();
        }
    }

    private void StopSlicing()
    {
        isSlicing = false;
        if (bladeCollider != null) bladeCollider.enabled = false;

        if (bladeTrail != null)
        {
            bladeTrail.Clear();
            bladeTrail.enabled = false;
        }

        if (bladeSparkles != null)
        {
            bladeSparkles.Stop();
        }

        // ✨ Dừng hành động chém/buông tay -> Tắt âm thanh Combo ngay lập tức
        EndCombo();
    }

    private void UpdateSlicing()
    {
        Vector3 newPosition = GetMouseWorldPosition();
        direction = newPosition - transform.position;

        if (Time.deltaTime > 0f)
        {
            sliceVelocity = direction.magnitude / Time.deltaTime;
        }
        else
        {
            sliceVelocity = 0f;
        }

        bool isMovingFastEnough = sliceVelocity > minSliceVelocity;
        bladeCollider.enabled = isMovingFastEnough;

        if (sliceVelocity >= minSwishVelocity && Time.time - lastSwishTime >= swishCooldown)
        {
            lastSwishTime = Time.time;
            AudioManager.Instance?.PlaySwishSound();
        }

        transform.position = newPosition;
        bladeRigidbody.position = newPosition;
    }

    // ✨ GỌI HÀM NÀY TỪ SCRIPT TRÁI CÂY (Fruit.cs / SliceManager.cs) MỖI KHI CHÉM TRÚNG 1 TRÁI CÂY
    public void OnSliceFruit()
    {
        comboCount++;
        comboTimer = comboTimeout; // Reset lại bộ đếm thời gian cho đợt chém tiếp theo

        // Khi chém liên tiếp từ 2 trái cây trở lên -> Kích hoạt nhạc Combo chạy lặp
        if (comboCount >= 2)
        {
            isComboActive = true;
            AudioManager.Instance?.PlayComboLoop();
        }
    }

    // ✨ NGẮT VÀ RESET TRẠNG THÁI COMBO
    private void EndCombo()
    {
        if (isComboActive || comboCount > 0)
        {
            isComboActive = false;
            comboCount = 0;
            comboTimer = 0f;

            // Dừng ngay lập tức âm thanh Combo
            AudioManager.Instance?.StopComboSound();
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 inputPos = Input.mousePosition;

        if (Input.touchCount > 0)
        {
            inputPos = Input.GetTouch(0).position;
        }

        if (mainCamera == null) return Vector3.zero;

        inputPos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(inputPos);
        worldPos.z = 0f;
        return worldPos;
    }
}