using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner Instance;

    [Header("Danh sách Prefabs Vật Phẩm")]
    public GameObject goldSmallPrefab;
    public GameObject goldLargePrefab;
    public GameObject rockSmallPrefab;
    public GameObject rockLargePrefab;
    public GameObject diamondPrefab;
    public GameObject mysteryBagPrefab;
    public GameObject tntPrefab;

    [Header("Khu vực rải đồ (Tọa độ)")]
    public float minX = -7f;
    public float maxX = 7f;
    public float minY = -4.5f;
    public float maxY = 1.0f;

    private List<GameObject> activeItems = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void SpawnItemsForLevel(int level)
    {
        // 1. Dọn sạch vật phẩm cũ
        ClearCurrentItems();

        // Lấy targetScore từ GameManager (hoặc tính dựa theo công thức level)
        int targetScore = (GameManager.Instance != null) ? GameManager.Instance.targetScore : 600;
        
        // Thêm hệ số dư (ví dụ: +20% điểm) để người chơi không bắt buộc phải gắp 100% sạch bản đồ
        int requiredTotalScore = Mathf.CeilToInt(targetScore * 1.2f); 

        int currentTotalSpawnedScore = 0;
        float rockSpawnRatio = Mathf.Clamp(0.2f + (level * 0.05f), 0.2f, 0.5f);
        float diamondRatio = Mathf.Clamp(0.15f - (level * 0.02f), 0.05f, 0.15f);
        float minDistance = 0.8f; // Giảm khoảng cách nhẹ để dễ đặt đủ số lượng vật phẩm

        int maxAttempts = 500;
        int attempts = 0;

        // Vòng lặp sinh đồ: Tiếp tục sinh cho đến khi TỔNG ĐIỂM BẢN ĐỒ >= requiredTotalScore
        while (currentTotalSpawnedScore < requiredTotalScore && attempts < maxAttempts)
        {
            attempts++;
            Vector3 randomPos = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);

            if (IsValidPosition(randomPos, minDistance))
            {
                GameObject prefabToSpawn = SelectPrefabByLevel(rockSpawnRatio, diamondRatio, randomPos.y);
                GameObject newItem = Instantiate(prefabToSpawn, randomPos, Quaternion.identity, transform);

                // Lấy giá trị điểm của Item vừa sinh ra
                Item itemScript = newItem.GetComponent<Item>();
                if (itemScript != null)
                {
                    currentTotalSpawnedScore += itemScript.scoreValue;
                }

                activeItems.Add(newItem);
            }
        }

        Debug.Log($"[ItemSpawner] Màn {level}: Đã sinh tổng cộng {activeItems.Count} vật phẩm. Tổng điểm trên bản đồ: {currentTotalSpawnedScore} / Điểm mục tiêu: {targetScore}");
    }

    GameObject SelectPrefabByLevel(float rockRatio, float diamondRatio, float depthY)
    {
        float rand = Random.value;

        if (depthY < -3.0f && rand < diamondRatio)
        {
            return diamondPrefab;
        }

        if (rand < rockRatio)
        {
            return (Random.value > 0.5f) ? rockLargePrefab : rockSmallPrefab;
        }
        
        if (rand < rockRatio + 0.08f) return tntPrefab;
        if (rand < rockRatio + 0.15f) return mysteryBagPrefab;

        return (Random.value > 0.4f) ? goldSmallPrefab : goldLargePrefab;
    }

    bool IsValidPosition(Vector3 pos, float minDist)
    {
        foreach (GameObject item in activeItems)
        {
            if (item != null && Vector3.Distance(pos, item.transform.position) < minDist)
            {
                return false;
            }
        }
        return true;
    }

    public void ClearCurrentItems()
    {
        foreach (GameObject item in activeItems)
        {
            if (item != null) Destroy(item);
        }
        activeItems.Clear();

        GameObject[] leftoverItems = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject extra in leftoverItems)
        {
            Destroy(extra);
        }
    }
}