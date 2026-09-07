using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Danh sách Prefab Trái Cây")]
    public GameObject[] fruitPrefabs;

    [Header("Cấu hình Bom")]
    [Tooltip("Kéo Prefab Bom vào đây")]
    public GameObject bombPrefab;
    [Range(0f, 1f)]
    [Tooltip("Tỉ lệ xuất hiện Bom ban đầu (Ví dụ: 0.1 = 10%)")]
    public float bombChance = 0.1f;

    [Header("Danh sách Vị Trí Bắn (Spawn Points)")]
    public Transform[] spawnPoints;

    [Header("Thời Gian Giữa Các Lần Bắn (Giây)")]
    public float minDelay = 1.2f;
    public float maxDelay = 2.5f;

    [Header("Lực Bắn Trái Cây & Bom")]
    public float minForce = 12f;
    public float maxForce = 15f;

    [Header("Bắn Chùm (Nhiều vật thể cùng lúc)")]
    public int minSpawnCount = 1;
    public int maxSpawnCount = 2;

    [Header("Tăng Độ Khó Tự Động Theo Điểm Số")]
    public bool enableDifficultyIncrease = true;

    private Coroutine spawnCoroutine;
    private int currentLevel = 0;

    // Lưu lại thông số ban đầu để Reset khi chơi lại (Restart)
    private float initialMinDelay;
    private float initialMaxDelay;
    private float initialMinForce;
    private float initialMaxForce;
    private float initialBombChance;
    private int initialMinSpawnCount;
    private int initialMaxSpawnCount;

    private void Awake()
    {
        // Ghi nhớ thông số mặc định đặt từ Inspector
        initialMinDelay = minDelay;
        initialMaxDelay = maxDelay;
        initialMinForce = minForce;
        initialMaxForce = maxForce;
        initialBombChance = bombChance;
        initialMinSpawnCount = minSpawnCount;
        initialMaxSpawnCount = maxSpawnCount;
    }

    private void OnEnable()
    {
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }

    private void Update()
    {
        // Kiểm tra trạng thái game
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        if (Time.timeScale == 0f) return;

        if (enableDifficultyIncrease)
        {
            UpdateDifficultyByScore(GameManager.Instance.Score);
        }
    }

    // ✨ HÀM XỬ LÝ TĂNG ĐỘ KHÓ THEO CÁC MỐC ĐIỂM
    private void UpdateDifficultyByScore(int currentScore)
    {
        int targetLevel = GetLevelFromScore(currentScore);

        // Chỉ cập nhật khi đạt mốc Level mới
        if (targetLevel > currentLevel)
        {
            int levelGained = targetLevel - currentLevel;
            currentLevel = targetLevel;

            // 1. Giảm thời gian chờ giữa các lượt bắn (Ráp dồn dập hơn)
            minDelay = Mathf.Max(0.35f, minDelay - (0.05f * levelGained));
            maxDelay = Mathf.Max(0.7f, maxDelay - (0.08f * levelGained));

            // 2. Tăng lực bắn giúp quả bay cao/nhanh hơn
            minForce += 0.25f * levelGained;
            maxForce += 0.25f * levelGained;

            // 3. Tăng tỷ lệ ra Bom (Tối đa 45%)
            bombChance = Mathf.Min(0.45f, bombChance + (0.025f * levelGained));

            // 4. Tăng số lượng quả bắn ra cùng lúc theo từng mốc Level
            if (currentLevel >= 2) maxSpawnCount = Mathf.Max(initialMaxSpawnCount, 3);
            if (currentLevel >= 4) minSpawnCount = Mathf.Max(initialMinSpawnCount, 2);
            if (currentLevel >= 6) maxSpawnCount = Mathf.Max(maxSpawnCount, 4);
            if (currentLevel >= 8) maxSpawnCount = Mathf.Max(maxSpawnCount, 5);

            Debug.Log($"[Spawner] Tăng độ khó! Level hiện tại: {currentLevel} | Score: {currentScore} | Delay: {minDelay:F2}s-{maxDelay:F2}s | BombChance: {bombChance * 100:F1}%");
        }
    }

    // ✨ TÍNH CẤP ĐỘ (LEVEL) DỰA TRÊN MỐC ĐIỂM
    private int GetLevelFromScore(int score)
    {
        if (score < 1000) return 0;
        if (score < 2000) return 1;       // Mốc 1,000
        if (score < 5000) return 2;       // Mốc 2,000
        if (score < 10000) return 3;      // Mốc 5,000
        if (score < 20000) return 4;      // Mốc 10,000
        if (score < 50000) return 5;      // Mốc 20,000

        // Từ 50,000 trở đi: Cứ mỗi lần gấp đôi điểm số sẽ tăng thêm 1 Level
        // Mốc 50k (Lvl 6), 100k (Lvl 7), 200k (Lvl 8), 400k (Lvl 9), 800k (Lvl 10),...
        int baseScore = 50000;
        int levelOffset = 0;

        while (score >= baseScore)
        {
            levelOffset++;
            baseScore *= 2;
        }

        return 5 + levelOffset;
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        while (enabled)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                yield return null;
                continue;
            }

            if (Time.timeScale == 0f)
            {
                yield return null;
                continue;
            }

            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                continue;
            }

            if ((fruitPrefabs == null || fruitPrefabs.Length == 0) && bombPrefab == null)
            {
                Debug.LogWarning("Chưa gán fruitPrefabs/bombPrefab!");
                continue;
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("Chưa gán spawnPoints!");
                continue;
            }

            int spawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);
            List<Transform> availablePoints = new List<Transform>(spawnPoints);

            for (int i = 0; i < spawnCount; i++)
            {
                if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) break;
                if (Time.timeScale == 0f) break;

                if (availablePoints.Count == 0) availablePoints = new List<Transform>(spawnPoints);

                int randomIndex = Random.Range(0, availablePoints.Count);
                Transform spawnPoint = availablePoints[randomIndex];
                availablePoints.RemoveAt(randomIndex);

                // LOGIC CHỌN BOM HOẶC TRÁI CÂY
                GameObject prefabToSpawn = null;
                if (bombPrefab != null && Random.value < bombChance)
                {
                    prefabToSpawn = bombPrefab;
                }
                else if (fruitPrefabs != null && fruitPrefabs.Length > 0)
                {
                    prefabToSpawn = fruitPrefabs[Random.Range(0, fruitPrefabs.Length)];
                }

                if (prefabToSpawn == null) continue;

                Vector3 spawnPos = new Vector3(spawnPoint.position.x, spawnPoint.position.y, 0f);
                GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

                // Rigidbody 3D
                Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints.FreezePositionZ;
                    Vector3 force = spawnPoint.up * Random.Range(minForce, maxForce);
                    rb.AddForce(force, ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
                }
                else
                {
                    // Rigidbody 2D
                    Rigidbody2D rb2d = spawnedObject.GetComponent<Rigidbody2D>();
                    if (rb2d != null)
                    {
                        Vector2 force2D = spawnPoint.up * Random.Range(minForce, maxForce);
                        rb2d.AddForce(force2D, ForceMode2D.Impulse);
                        rb2d.AddTorque(Random.Range(-2f, 2f), ForceMode2D.Impulse);
                    }
                }

                yield return new WaitForSeconds(Random.Range(0.05f, 0.12f));
            }
        }
    }

    // ✨ HÀM RESET VỀ CÀI ĐẶT BAN ĐẦU KHI RESTART GAME
    public void ResetDifficulty()
    {
        currentLevel = 0;
        minDelay = initialMinDelay;
        maxDelay = initialMaxDelay;
        minForce = initialMinForce;
        maxForce = initialMaxForce;
        bombChance = initialBombChance;
        minSpawnCount = initialMinSpawnCount;
        maxSpawnCount = initialMaxSpawnCount;
    }
}