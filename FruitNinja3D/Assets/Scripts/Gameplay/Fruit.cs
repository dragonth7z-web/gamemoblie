using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Fruit : MonoBehaviour
{
    [Header("Cài đặt Quả Bị Cắt")]
    [Tooltip("Kéo Prefab 2 nửa quả trong thư mục Assets/Prefabs/SlicedFruits vào đây")]
    public GameObject slicedFruitPrefab;

    [Header("Cài đặt Hiệu Ứng (VFX & SFX)")]
    [Tooltip("Prefab Particle System hiệu ứng nước bắn tóe khi chém")]
    public GameObject splashEffectPrefab;
    [Tooltip("Màu nước bắn tóe (Đỏ cho Dưa hấu, Vàng cho Cam...)")]
    public Color splashColor = Color.red;

    [Header("Cài đặt Lực Nổ Cắt Quả")]
    public float explosionForce = 180f;
    public float explosionRadius = 3.5f;

    [Header("Cài đặt Điểm Số & Loại Quả")]
    public int scoreValue = 1;
    public bool isSpecialFruit = false;

    [Header("Cài đặt An Toàn Rơi Mạng")]
    public float destroyYBoundary = -6f;
    public float safeTimeBeforeCheckDrop = 0.5f;

    private bool isSliced = false;
    private float spawnTime;
    private Rigidbody rb;
    private Collider fruitCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        fruitCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        spawnTime = Time.time;
        isSliced = false;

        if (fruitCollider != null) 
        {
            fruitCollider.enabled = true;
        }
    }

    private void Update()
    {
        // 1. Chống mất mạng nhầm khi quả vừa bay lên
        if (Time.time - spawnTime < safeTimeBeforeCheckDrop) return;

        // 2. Kiểm tra nếu quả chưa bị chém mà rơi quá đáy màn hình
        if (!isSliced && transform.position.y < destroyYBoundary)
        {
            Vector3 currentVel = GetRigidbodyVelocity(rb);

            if (currentVel.y <= 0f)
            {
                isSliced = true;

                if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
                {
                    GameManager.Instance.LoseLife();
                }

                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
        if (isSliced || !other.CompareTag("Blade")) return;

        isSliced = true;

        Vector3 slicePosition = transform.position;

        Blade blade = other.GetComponent<Blade>();
        Quaternion sliceRotation = Quaternion.identity;
        if (blade != null && blade.direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(blade.direction.y, blade.direction.x) * Mathf.Rad2Deg;
            sliceRotation = Quaternion.Euler(0f, 0f, angle);

            // ✨ KÍCH HOẠT VÀ TÍNH TOÁN BỘ ĐẾM COMBO TRÊN LƯỠI DAO
            blade.OnSliceFruit();
        }
        else
        {
            sliceRotation = other.transform.rotation;
        }

        if (fruitCollider != null) fruitCollider.enabled = false;

        // 1. ÂM THANH CHÉM TRÚNG QUẢ
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCutSound();
        }

        // 2. HIỆU ỨNG NƯỚC BẮN
        SpawnJuiceSplash(slicePosition, sliceRotation);

        // 3. TẠO & PHÂN TÁCH 2 NỬA QUẢ
        SpawnSlicedParts(slicePosition);

        // 4. CỘNG ĐIỂM
        if (GameManager.Instance != null)
        {
            int finalScore = isSpecialFruit ? scoreValue * 2 : scoreValue;
            GameManager.Instance.AddScore(finalScore);
        }

        // 5. XÓA QUẢ GỐC
        Destroy(gameObject);
    }

    private void SpawnJuiceSplash(Vector3 position, Quaternion rotation)
    {
        if (splashEffectPrefab == null) return;

        GameObject splash = Instantiate(splashEffectPrefab, position, rotation);

        ParticleSystem ps = splash.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = splashColor;
        }

        Destroy(splash, 2f);
    }

    private void SpawnSlicedParts(Vector3 position)
    {
        if (slicedFruitPrefab == null) return;

        GameObject sliced = Instantiate(slicedFruitPrefab, position, transform.rotation);
        Vector3 parentVelocity = GetRigidbodyVelocity(rb);

        Rigidbody[] rbs = sliced.GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rbs.Length; i++)
        {
            Rigidbody childRb = rbs[i];
            
            SetRigidbodyVelocity(childRb, parentVelocity);
            childRb.AddExplosionForce(explosionForce, position, explosionRadius);

            float sideDirection = (i % 2 == 0) ? -1.2f : 1.2f;
            childRb.AddForce(Vector3.right * sideDirection * 3f, ForceMode.Impulse);

            childRb.AddTorque(Random.insideUnitSphere * 15f, ForceMode.Impulse);
        }

        Destroy(sliced, 2.5f);
    }

    private Vector3 GetRigidbodyVelocity(Rigidbody targetRb)
    {
        if (targetRb == null) return Vector3.zero;
        #if UNITY_6000_0_OR_NEWER
            return targetRb.linearVelocity;
        #else
            return targetRb.velocity;
        #endif
    }

    private void SetRigidbodyVelocity(Rigidbody targetRb, Vector3 vel)
    {
        if (targetRb == null) return;
        #if UNITY_6000_0_OR_NEWER
            targetRb.linearVelocity = vel;
        #else
            targetRb.velocity = vel;
        #endif
    }
}