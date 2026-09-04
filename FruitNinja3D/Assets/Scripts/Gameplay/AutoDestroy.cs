using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Header("Thời gian sống (giây)")]
    public float lifetime = 1.5f;

    private void Start()
    {
        // Tự động xóa GameObject này sau thời gian lifetime
        Destroy(gameObject, lifetime);
    }
}