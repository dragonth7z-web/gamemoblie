using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MenuFruitButton : MonoBehaviour
{
    [Header("Sự kiện khi bị chém")]
    public UnityEvent onSliced;

    [Header("Thời gian hoãn hành động")]
    [Tooltip("Khoảng thời gian (giây) chờ hiệu ứng cắt/âm thanh chạy xong trước khi kích hoạt onSliced")]
    public float actionDelay = 0.5f;

    [Header("Hiệu Ứng VFX & Prefab")]
    public GameObject slicedPrefab;
    public GameObject splashVFX;
    public Color splashColor = Color.red;

    [Header("Cấu Hình Vật Lý Chém")]
    public float sliceForce = 500f;
    public float explosionRadius = 2f;

    private bool isSliced = false;
    private Collider fruitCollider;
    private Renderer[] fruitRenderers;

    private void Awake()
    {
        fruitCollider = GetComponent<Collider>();
        fruitRenderers = GetComponentsInChildren<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isSliced || !other.CompareTag("Blade")) return;

        isSliced = true;

        // 1. Phát âm thanh chém trúng quả
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCutSound();
        }

        // 2. Lấy hướng chém từ lưỡi dao (Blade)
        Vector3 sliceDirection = Vector3.up;
        Rigidbody bladeRb = other.GetComponent<Rigidbody>();
        if (bladeRb != null)
        {
            Vector3 velocity = GetRigidbodyVelocity(bladeRb);
            if (velocity.sqrMagnitude > 0.1f)
            {
                sliceDirection = velocity.normalized;
            }
        }

        // 3. Tạo hiệu ứng nước văng (Splash)
        if (splashVFX != null)
        {
            Quaternion splashRotation = Quaternion.LookRotation(sliceDirection);
            GameObject splash = Instantiate(splashVFX, transform.position, splashRotation);
            Destroy(splash, 2f);
        }

        // 4. Tạo 2 nửa quả bị cắt & tác động lực vật lý văng tách đôi
        if (slicedPrefab != null)
        {
            GameObject sliced = Instantiate(slicedPrefab, transform.position, transform.rotation);
            Rigidbody[] rbs = sliced.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in rbs)
            {
                rb.isKinematic = false;
                rb.AddForce(sliceDirection * sliceForce + Vector3.up * 2f, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
            }
            
            Destroy(sliced, 3f);
        }

        StartCoroutine(ExecuteAction());
    }

    private IEnumerator ExecuteAction()
    {
        // Ẩn quả ngay lập tức để lộ 2 mảnh vỡ vừa sinh ra
        if (fruitCollider != null) fruitCollider.enabled = false;
        SetRenderersEnabled(false);

        // Chờ khoảng thời gian đã cấu hình (dùng Realtime để không phụ thuộc Time.timeScale)
        yield return new WaitForSecondsRealtime(actionDelay);

        // Kích hoạt sự kiện UI/GameLogic (Start Game, Chuyển Scene, Mở Popup...)
        onSliced?.Invoke();
    }

    public void RespawnFruit()
    {
        isSliced = false;
        if (fruitCollider != null) fruitCollider.enabled = true;
        SetRenderersEnabled(true);
    }

    private void SetRenderersEnabled(bool isEnabled)
    {
        if (fruitRenderers == null) return;
        foreach (var rend in fruitRenderers)
        {
            if (rend != null) rend.enabled = isEnabled;
        }
    }

    private Vector3 GetRigidbodyVelocity(Rigidbody targetRb)
    {
        if (targetRb == null) return Vector3.zero;
        #if UNITY_6000_0_OR_NEWER
            return targetRb.linearVelocity;
        #else
            return targetRb.velocity;
        #endif
    }
}