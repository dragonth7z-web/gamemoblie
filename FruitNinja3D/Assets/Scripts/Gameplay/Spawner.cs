using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Danh sách Tag Prefab Trái Cây (Khớp với ObjectPooler)")]
    public string[] fruitPoolTags; // Thay vì mảng GameObject, ta dùng mảng Tag để gọi Pool

    [Header("Cấu hình Bom")]
    [Tooltip("Tag của Prefab Bom trong Pool")]
    public string bombPoolTag = "Bomb";
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
    private List<Transform> availableSpawnPoints;

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
        availableSpawnPoints = new List<Transform>();

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
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        if (Time.timeScale == 0f) return;

        if (enableDifficultyIncrease)
        {
            UpdateDifficultyByScore(GameManager.Instance.Score);
        }
    }

    private void UpdateDifficultyByScore(int currentScore)
    {
        int targetLevel = GetLevelFromScore(currentScore);

        if (targetLevel > currentLevel)
        {
            int levelGained = targetLevel - currentLevel;
            currentLevel = targetLevel;

            minDelay = Mathf.Max(0.35f, minDelay - (0.05f * levelGained));
            maxDelay = Mathf.Max(0.7f, maxDelay - (0.08f * levelGained));
            minForce += 0.25f * levelGained;
            maxForce += 0.25f * levelGained;
            bombChance = Mathf.Min(0.45f, bombChance + (0.025f * levelGained));

            if (currentLevel >= 2) maxSpawnCount = Mathf.Max(initialMaxSpawnCount, 3);
            if (currentLevel >= 4) minSpawnCount = Mathf.Max(initialMinSpawnCount, 2);
            if (currentLevel >= 6) maxSpawnCount = Mathf.Max(maxSpawnCount, 4);
            if (currentLevel >= 8) maxSpawnCount = Mathf.Max(maxSpawnCount, 5);
        }
    }

    private int GetLevelFromScore(int score)
    {
        if (score < 1000) return 0;
        if (score < 2000) return 1;
        if (score < 5000) return 2;
        if (score < 10000) return 3;
        if (score < 20000) return 4;
        if (score < 50000) return 5;

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

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) continue;

            if ((fruitPoolTags == null || fruitPoolTags.Length == 0) && string.IsNullOrEmpty(bombPoolTag))
            {
                yield break;
            }

            if (spawnPoints == null || spawnPoints.Length == 0) continue;
            if (ObjectPooler.Instance == null)
            {
                Debug.LogError("Spawner cần một ObjectPooler đang hoạt động.", this);
                yield break;
            }

            int spawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);
            availableSpawnPoints.Clear();
            availableSpawnPoints.AddRange(spawnPoints);

            for (int i = 0; i < spawnCount; i++)
            {
                if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) break;
                if (Time.timeScale == 0f) break;

                if (availableSpawnPoints.Count == 0)
                {
                    availableSpawnPoints.AddRange(spawnPoints);
                }

                int randomIndex = Random.Range(0, availableSpawnPoints.Count);
                Transform spawnPoint = availableSpawnPoints[randomIndex];
                availableSpawnPoints.RemoveAt(randomIndex);

                // ✨ THAY THẾ INSTANTIATE BẰNG OBJECT POOLER
                string poolTagToSpawn = "";
                bool isBomb = (!string.IsNullOrEmpty(bombPoolTag) && Random.value < bombChance);

                if (isBomb)
                {
                    poolTagToSpawn = bombPoolTag;
                }
                else if (fruitPoolTags != null && fruitPoolTags.Length > 0)
                {
                    poolTagToSpawn = fruitPoolTags[Random.Range(0, fruitPoolTags.Length)];
                }

                if (string.IsNullOrEmpty(poolTagToSpawn)) continue;

                Vector3 spawnPos = new Vector3(spawnPoint.position.x, spawnPoint.position.y, 0f);
                
                // Gọi từ Pool thay vì tạo mới
                GameObject spawnedObject = ObjectPooler.Instance.SpawnFromPool(poolTagToSpawn, spawnPos, Quaternion.identity);

                if (spawnedObject != null)
                {
                    // Rigidbody 3D
                    Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero; // Unity 6: Dùng linearVelocity thay cho velocity cũ nếu cần
                        rb.angularVelocity = Vector3.zero;
                        rb.constraints = RigidbodyConstraints.FreezePositionZ;
                        Vector3 force = spawnPoint.up * Random.Range(minForce, maxForce);
                        rb.AddForce(force, ForceMode.Impulse);
                        rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
                    }
                }

                yield return new WaitForSeconds(Random.Range(0.05f, 0.12f));
            }
        }
    }

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