using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Chỉ số cơ bản")]
    public int scoreValue = 100;
    public float weight = 1f; // Trọng lượng vật phẩm

    [Header("Cấu hình Bom / TNT")]
    public bool isBomb = false; // Tích chọn nếu đây là Boom/TNT
    public float timePenalty = 10f; // Số giây bị trừ khi nổ
    public float explosionRadius = 2.5f; // Bán kính nổ làm tan biến đồ vật xung quanh

    [Header("Hiệu ứng & Âm thanh")]
    public GameObject explosionPrefab; // Kéo Prefab Particle hiệu ứng nổ vào đây
    public AudioClip explosionSound;    // Âm thanh nổ

    [HideInInspector]
    public GameObject sourcePrefab;     // Dùng cho Object Pooling

    private bool hasExploded = false;

    public void OnSpawnFromPool()
    {
        hasExploded = false;
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void OnReturnToPool()
    {
        hasExploded = false;
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // Gọi hàm này khi Móc Kéo chạm vào quả Bom
    public void TriggerExplosion()
    {
        if (!isBomb || hasExploded) return;
        hasExploded = true;

        // 1. Tạo hiệu ứng hình ảnh (Particle System)
        if (explosionPrefab != null)
        {
            if (ItemSpawner.Instance != null)
            {
                ItemSpawner.Instance.GetPooledEffect(explosionPrefab, transform.position, Quaternion.identity, 1.5f);
            }
            else
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
        }

        // 2. Phát âm thanh nổ
        if (explosionSound != null || (AudioManager.Instance != null && AudioManager.Instance.bombExplosionSound != null))
        {
            if (AudioManager.Instance != null)
            {
                if (AudioManager.Instance.bombExplosionSound != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.bombExplosionSound);
                }
                else
                {
                    AudioManager.Instance.PlaySFX(explosionSound);
                }
            }
            else
            {
                AudioSource.PlayClipAtPoint(explosionSound != null ? explosionSound : AudioManager.Instance.bombExplosionSound, transform.position, 1.0f);
            }
        }

        // (Đã xóa hiệu ứng rung camera ở đây theo yêu cầu)

        // 4. Trừ thời gian chơi
        if (GameManager.Instance != null && timePenalty > 0)
        {
            GameManager.Instance.DeductTime(timePenalty);
        }

        // 5. Quét tiêu diệt các vật phẩm xung quanh trong bán kính nổ
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Item"))
            {
                Item nearbyItem = hit.GetComponent<Item>();
                if (nearbyItem != null)
                {
                    // Nếu vật phẩm lân cận cũng là bom thì kích nổ dây chuyền
                    if (nearbyItem.isBomb)
                    {
                        nearbyItem.TriggerExplosion();
                    }
                    else
                    {
                        if (ItemSpawner.Instance != null)
                        {
                            ItemSpawner.Instance.ReturnToPool(hit.gameObject);
                        }
                        else
                        {
                            Destroy(hit.gameObject);
                        }
                    }
                }
            }
        }

        // 6. Xóa chính quả bom này
        if (ItemSpawner.Instance != null)
        {
            ItemSpawner.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Vẽ bán kính nổ trong Scene View để dễ căn chỉnh
    private void OnDrawGizmosSelected()
    {
        if (isBomb)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}