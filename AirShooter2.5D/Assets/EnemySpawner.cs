using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public string enemyPoolTag = "EnemyBasic";
    public float spawnInterval = 1.5f; // Thời gian giãn cách giữa các đợt quái
    public float xBound = 6f;          // Tọa độ ngẫu nhiên Trái/Phải
    public float spawnZ = 12f;         // Tọa độ xuất hiện phía trên màn hình nghiêng
    public Vector3 spawnOffset = new Vector3(0f, 0f, 12f); // Offset từ vị trí của Spawner

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (ObjectPooler.Instance == null) return;

        float randomX = Random.Range(-xBound, xBound);
        Vector3 spawnPosition = transform.position + new Vector3(randomX, 0f, spawnZ);

        // Lấy Enemy ra từ Pool
        ObjectPooler.Instance.SpawnFromPool(enemyPoolTag, spawnPosition, Quaternion.identity);
    }
}