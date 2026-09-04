using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Hiệu Ứng Nổ")]
    [Tooltip("Kéo Prefab Particle nổ (Explosion VFX) vào đây")]
    public GameObject explosionVFX;

    [Header("Cài Đặt An Toàn")]
    [SerializeField] private float enableDelay = 0.15f; // Chờ 0.15s mới kích hoạt nhận chém
    [SerializeField] private float gameOverDelay = 0.3f; // Đợi nổ 0.3s rồi mới hiện UI Game Over
    [SerializeField] private float destroyYBoundary = -10f; // Hạ thấp ngưỡng hủy để phù hợp SpawnPoint thấp

    private float spawnTime;
    private bool isDetonated = false;
    private Rigidbody rb3D;

    private void Awake()
    {
        rb3D = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        spawnTime = Time.time;
        isDetonated = false;

        // Phát tiếng ngòi nổ cháy xè xè (nếu có cấu hình trong AudioManager)
        AudioManager.Instance?.PlayFuseSound();
    }

    private void Update()
    {
        // Chỉ tự hủy khi quả bom đang RƠI XUỐNG (linearVelocity.y < 0) và vượt quá đáy màn hình
        bool isFalling = (rb3D != null && rb3D.linearVelocity.y < 0) || rb3D == null;

        if (!isDetonated && isFalling && transform.position.y < destroyYBoundary)
        {
            Destroy(gameObject);
        }
    }

    // Xử lý va chạm cho Collider 3D
    private void OnTriggerEnter(Collider other)
    {
        CheckAndExplode(other.gameObject);
    }

    // Xử lý va chạm hỗ trợ thêm cho Collider 2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckAndExplode(other.gameObject);
    }

    private void CheckAndExplode(GameObject target)
    {
        if (isDetonated || Time.time - spawnTime < enableDelay) return;

        if (target.CompareTag("Blade"))
        {
            isDetonated = true;

            // Tắt Collider ngay lập tức
            Collider col3D = GetComponent<Collider>();
            if (col3D != null) col3D.enabled = false;

            Collider2D col2D = GetComponent<Collider2D>();
            if (col2D != null) col2D.enabled = false;

            // Ẩn toàn bộ Renderer của Bom
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var ren in renderers)
            {
                if (ren != null) ren.enabled = false;
            }

            StartCoroutine(ExplodeSequence());
        }
    }

    private IEnumerator ExplodeSequence()
    {
        // 1. Tạo hiệu ứng particle nổ
        if (explosionVFX != null)
        {
            GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 2.5f);
        }

        // 2. Phát tiếng nổ bom chuẩn
        AudioManager.Instance?.PlayBombSound();

        // 3. Rung camera
        CameraShake.Shake(0.25f, 0.4f);

        // 4. Chờ nổ trước khi gọi Game Over
        yield return new WaitForSecondsRealtime(gameOverDelay);

        GameManager.Instance?.TriggerGameOver();

        Destroy(gameObject);
    }
}

// Helper Class rung camera tối ưu Unity 6
public static class CameraShake
{
    private class MonoRunner : MonoBehaviour { }
    private static MonoRunner runner;
    private static Coroutine currentShakeCoroutine;
    private static Vector3 initialCamPosition;
    private static bool isPositionSaved = false;

    public static void Shake(float duration, float magnitude)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        if (runner == null)
        {
            GameObject runnerGO = new GameObject("CameraShake_Runner");
            runner = runnerGO.AddComponent<MonoRunner>();
            Object.DontDestroyOnLoad(runnerGO);
        }

        if (!isPositionSaved)
        {
            initialCamPosition = mainCam.transform.localPosition;
            isPositionSaved = true;
        }

        if (currentShakeCoroutine != null)
        {
            runner.StopCoroutine(currentShakeCoroutine);
        }

        currentShakeCoroutine = runner.StartCoroutine(DoShake(mainCam, duration, magnitude));
    }

    private static IEnumerator DoShake(Camera targetCam, float duration, float magnitude)
    {
        if (targetCam == null) yield break;

        Transform camTransform = targetCam.transform;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            if (targetCam == null) yield break;

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            camTransform.localPosition = new Vector3(initialCamPosition.x + x, initialCamPosition.y + y, initialCamPosition.z);
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (targetCam != null)
        {
            camTransform.localPosition = initialCamPosition;
        }

        currentShakeCoroutine = null;
    }
}