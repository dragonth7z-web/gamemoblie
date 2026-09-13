using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pool
{
    public string tag;
    public GameObject prefab;
    public int size;
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, GameObject> prefabDictionary;
    private Dictionary<GameObject, string> objectTags;
    private HashSet<GameObject> activeObjects;

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabDictionary = new Dictionary<string, GameObject>();
        objectTags = new Dictionary<GameObject, string>();
        activeObjects = new HashSet<GameObject>();

        foreach (Pool pool in pools)
        {
            if (string.IsNullOrWhiteSpace(pool.tag) || pool.prefab == null || pool.size <= 0)
            {
                Debug.LogWarning("Pool không hợp lệ, đã bỏ qua.", this);
                continue;
            }

            if (poolDictionary.ContainsKey(pool.tag))
            {
                Debug.LogWarning($"Pool trùng tag '{pool.tag}', đã bỏ qua.", this);
                continue;
            }

            Queue<GameObject> objectQueue = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
                objectTags.Add(obj, pool.tag);
            }

            poolDictionary.Add(pool.tag, objectQueue);
            prefabDictionary.Add(pool.tag, pool.prefab);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (poolDictionary == null || prefabDictionary == null || objectTags == null)
        {
            Debug.LogWarning("ObjectPooler chưa được khởi tạo đầy đủ!");
            return null;
        }

        if (!poolDictionary.TryGetValue(tag, out Queue<GameObject> objectQueue))
        {
            Debug.LogWarning("Pool với tag " + tag + " không tồn tại!");
            return null;
        }

        CleanupDestroyedObjectsInQueue(tag, objectQueue);

        GameObject objectToSpawn = null;
        while (objectQueue.Count > 0)
        {
            objectToSpawn = objectQueue.Dequeue();
            if (objectToSpawn == null) continue;

            if (objectTags.ContainsKey(objectToSpawn))
            {
                break;
            }

            objectTags[objectToSpawn] = tag;
            break;
        }

        if (objectToSpawn == null)
        {
            if (!prefabDictionary.TryGetValue(tag, out GameObject prefab)) return null;

            objectToSpawn = Instantiate(prefab);
            objectTags[objectToSpawn] = tag;
        }

        if (objectToSpawn == null) return null;

        objectToSpawn.transform.SetPositionAndRotation(position, rotation);
        objectToSpawn.SetActive(true);

        if (activeObjects != null)
        {
            activeObjects.Remove(objectToSpawn);
            activeObjects.Add(objectToSpawn);
        }

        return objectToSpawn;
    }

    public bool ReturnToPool(GameObject objectToReturn)
    {
        if (objectToReturn == null || objectTags == null || activeObjects == null || poolDictionary == null)
        {
            return false;
        }

        if (!objectTags.TryGetValue(objectToReturn, out string tag))
        {
            return false;
        }

        if (!activeObjects.Remove(objectToReturn))
        {
            return false;
        }

        if (!poolDictionary.TryGetValue(tag, out Queue<GameObject> objectQueue))
        {
            return false;
        }

        // Loại bỏ object đã bị destroy hoặc đang ở queue để tránh re-use sai đối tượng.
        while (objectQueue.Count > 0 && objectQueue.Peek() == null)
        {
            objectQueue.Dequeue();
        }

        if (objectQueue.Contains(objectToReturn))
        {
            return false;
        }

        objectTags.Remove(objectToReturn);
        objectToReturn.SetActive(false);
        objectQueue.Enqueue(objectToReturn);
        return true;
    }

    private void CleanupDestroyedObjectsInQueue(string tag, Queue<GameObject> objectQueue)
    {
        if (objectQueue == null || objectQueue.Count == 0) return;

        Queue<GameObject> validQueue = new Queue<GameObject>();
        while (objectQueue.Count > 0)
        {
            GameObject queuedObject = objectQueue.Dequeue();
            if (queuedObject == null) continue;

            if (objectTags != null && !objectTags.ContainsKey(queuedObject))
            {
                objectTags[queuedObject] = tag;
            }

            validQueue.Enqueue(queuedObject);
        }

        while (validQueue.Count > 0)
        {
            objectQueue.Enqueue(validQueue.Dequeue());
        }
    }
}