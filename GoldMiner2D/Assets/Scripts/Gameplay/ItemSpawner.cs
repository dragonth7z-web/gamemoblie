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

    private readonly List<GameObject> activeItems = new List<GameObject>();
    private readonly Dictionary<GameObject, Queue<GameObject>> pooledItems = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> sourcePrefabs = new Dictionary<GameObject, GameObject>();

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
                if (prefabToSpawn == null) continue;

                GameObject newItem = GetPooledItem(prefabToSpawn, randomPos);

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
            if (item != null && (pos - item.transform.position).sqrMagnitude < minDist * minDist)
            {
                return false;
            }
        }
        return true;
    }

    public GameObject GetPooledItem(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return null;

        if (!pooledItems.TryGetValue(prefab, out Queue<GameObject> poolQueue))
        {
            poolQueue = new Queue<GameObject>();
            pooledItems[prefab] = poolQueue;
        }

        GameObject item = poolQueue.Count > 0 ? poolQueue.Dequeue() : Instantiate(prefab, transform);
        sourcePrefabs[item] = prefab;

        item.transform.SetParent(transform);
        item.transform.position = position;
        item.transform.rotation = Quaternion.identity;
        item.SetActive(true);

        Item itemScript = item.GetComponent<Item>();
        if (itemScript != null)
        {
            itemScript.sourcePrefab = prefab;
            itemScript.OnSpawnFromPool();
        }

        return item;
    }

    public GameObject GetPooledEffect(GameObject prefab, Vector3 position, Quaternion rotation, float duration)
    {
        GameObject effect = GetPooledItem(prefab, position);
        if (effect == null) return null;

        effect.transform.rotation = rotation;
        StartCoroutine(ReturnEffectAfterDelay(effect, duration));
        return effect;
    }

    private System.Collections.IEnumerator ReturnEffectAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (effect != null) ReturnToPool(effect);
    }

    public void ReturnToPool(GameObject item)
    {
        if (item == null) return;

        Item itemScript = item.GetComponent<Item>();
        if (itemScript != null)
        {
            itemScript.OnReturnToPool();
        }

        item.SetActive(false);
        item.transform.SetParent(transform);

        GameObject prefab = itemScript != null ? itemScript.sourcePrefab : null;
        if (prefab == null) sourcePrefabs.TryGetValue(item, out prefab);
        if (prefab == null)
        {
            prefab = item; // fallback: don't pool unknown objects
        }

        if (prefab != null && prefab != item)
        {
            if (!pooledItems.TryGetValue(prefab, out Queue<GameObject> poolQueue))
            {
                poolQueue = new Queue<GameObject>();
                pooledItems[prefab] = poolQueue;
            }

            if (!poolQueue.Contains(item))
            {
                poolQueue.Enqueue(item);
            }
        }

        activeItems.Remove(item);
    }

    public void ClearCurrentItems()
    {
        while (activeItems.Count > 0)
        {
            GameObject item = activeItems[activeItems.Count - 1];
            if (item != null)
            {
                ReturnToPool(item);
            }
            else
            {
                activeItems.RemoveAt(activeItems.Count - 1);
            }
        }

        GameObject[] leftoverItems = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject extra in leftoverItems)
        {
            if (extra != null && extra.activeInHierarchy)
            {
                ReturnToPool(extra);
            }
        }
    }
}