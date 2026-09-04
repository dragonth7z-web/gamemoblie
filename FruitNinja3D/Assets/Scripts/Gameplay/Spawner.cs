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
    [Tooltip("Tỉ lệ xuất hiện Bom (Ví dụ: 0.2 = 20%)")]
    public float bombChance = 0.2f;

    [Header("Danh sách Vị Trí Bắn (Spawn Points)")]
    public Transform[] spawnPoints;

    [Header("Thời Gian Giữa Các Lần Bắn (Giây)")]
    public float minDelay = 1f;
    public float maxDelay = 2.5f;

    [Header("Lực Bắn Trái Cây & Bom")]
    public float minForce = 12f;
    public float maxForce = 15f;

    [Header("Bắn Chùm (Nhiều vật thể cùng lúc)")]
    public int minSpawnCount = 1;
    public int maxSpawnCount = 3;

    [Header("Tăng Độ Khó Tự Động")]
    public bool enableDifficultyIncrease = true;
    public float difficultyInterval = 10f; // Mỗi 10s tăng độ khó

    private float gameTimer = 0f;
    private Coroutine spawnCoroutine;

    // Lưu lại thông số ban đầu để Reset
    private float initialMinDelay;
    private float initialMaxDelay;
    private float initialMinForce;
    private float initialMaxForce;
    private float initialBombChance;

    private void Awake()
    {
        // Ghi nhớ thông số mặc định đặt từ Inspector
        initialMinDelay = minDelay;
        initialMaxDelay = maxDelay;
        initialMinForce = minForce;
        initialMaxForce = maxForce;
        initialBombChance = bombChance;
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
        // Chỉ đếm thời gian tăng độ khó khi đang trong trạng thái Playing
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        if (Time.timeScale == 0f) return;

        if (enableDifficultyIncrease)
        {
            gameTimer += Time.deltaTime;
            if (gameTimer >= difficultyInterval)
            {
                gameTimer = 0f;
                minDelay = Mathf.Max(0.4f, minDelay - 0.1f);
                maxDelay = Mathf.Max(0.8f, maxDelay - 0.15f);
                minForce += 0.2f;
                maxForce += 0.2f;

                // Tăng nhẹ tỉ lệ ra bom theo thời gian (tối đa 40%)
                bombChance = Mathf.Min(0.4f, bombChance + 0.02f);
            }
        }
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        while (enabled)
        {
            // Bỏ qua việc sinh quả nếu không ở trạng thái Playing hoặc đang Pause
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

            // Tạo danh sách tạm để tránh việc bắn 2 quả tại cùng 1 vị trí trong 1 đợt
            List<Transform> availablePoints = new List<Transform>(spawnPoints);

            for (int i = 0; i < spawnCount; i++)
            {
                if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) break;
                if (Time.timeScale == 0f) break;

                if (availablePoints.Count == 0) availablePoints = new List<Transform>(spawnPoints);

                int randomIndex = Random.Range(0, availablePoints.Count);
                Transform spawnPoint = availablePoints[randomIndex];
                availablePoints.RemoveAt(randomIndex); // Loại bỏ điểm vừa chọn để quả sau xuất hiện ở điểm khác

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

                // Hỗ trợ Rigidbody 3D
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
                    // Hỗ trợ Rigidbody2D
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

    // Hàm reset về cài đặt ban đầu từ Inspector khi Restart
    public void ResetDifficulty()
    {
        gameTimer = 0f;
        minDelay = initialMinDelay;
        maxDelay = initialMaxDelay;
        minForce = initialMinForce;
        maxForce = initialMaxForce;
        bombChance = initialBombChance;
    }
}