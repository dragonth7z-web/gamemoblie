using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float destroyDelay = 1.5f;
    void Start()
    {
        Destroy(gameObject, destroyDelay);
    }
}