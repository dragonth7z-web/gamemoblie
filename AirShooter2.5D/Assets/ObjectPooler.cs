using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;        // Tag định danh (ví dụ: "PlayerBullet", "EnemyBasic")
        public GameObject prefab; // Prefab đối tượng
        public int size;          // Số lượng tạo sẵn ban đầu trong bể
    }

    public static ObjectPooler Instance;

    public List<Pool> pools;
    [System.NonSerialized] private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
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

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            if (pool.prefab == null || string.IsNullOrWhiteSpace(pool.tag))
            {
                Debug.LogWarning("Phát hiện Pool chưa được gán Prefab hoặc thiếu Tag trong Inspector!");
                continue;
            }

            Queue<GameObject> objectPool;
            if (poolDictionary.TryGetValue(pool.tag, out Queue<GameObject> existingPool))
            {
                objectPool = existingPool;
                Debug.LogWarning($"Tag '{pool.tag}' đã bị trùng lặp trong Inspector! Đã gộp các đối tượng vào cùng một pool.");
            }
            else
            {
                objectPool = new Queue<GameObject>();
                poolDictionary[pool.tag] = objectPool;
            }

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.transform.SetParent(this.transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool với Tag '{tag}' không tồn tại hoặc chưa được khởi tạo!");
            return null;
        }

        Queue<GameObject> poolQueue = poolDictionary[tag];

        // Tự động mở rộng bể chứa nếu số lượng đạn/quái vượt quá size ban đầu
        if (poolQueue.Count == 0)
        {
            Pool poolToExpand = pools.Find(p => p.tag == tag);
            if (poolToExpand != null && poolToExpand.prefab != null)
            {
                GameObject newObj = Instantiate(poolToExpand.prefab);
                newObj.transform.SetParent(this.transform);
                newObj.SetActive(false);
                poolQueue.Enqueue(newObj);
            }
        }

        GameObject objectToSpawn = poolQueue.Dequeue();

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (objectToReturn == null) return;

        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool với Tag '{tag}' không tồn tại! Thực hiện xóa đối tượng.");
            Destroy(objectToReturn);
            return;
        }

        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}