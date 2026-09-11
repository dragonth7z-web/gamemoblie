using UnityEngine;

public class HookController : MonoBehaviour
{
    public Transform hookTransform;
    public float rotateSpeed = 80f;
    public float maxAngle = 70f;
    public float baseShootSpeed = 10f;
    public float baseRewindSpeed = 8f;

    [Header("Cấu hình Hiệu ứng Nổ")]
    public GameObject explosionPrefab; // Kéo Prefab Particle hiệu ứng nổ vào đây
    public AudioClip explosionSound;    // Kéo File âm thanh nổ vào đây

    private float currentRewindSpeed;
    private bool isShooting = false;
    private bool isRewinding = false;
    private int rotateDirection = 1;
    private Vector3 hookInitialLocalPos;
    private Transform caughtItem = null;

    void Start()
    {
        if (hookTransform == null && transform.childCount > 0)
            hookTransform = transform.GetChild(0);

        hookInitialLocalPos = hookTransform.localPosition;
        currentRewindSpeed = baseRewindSpeed;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameState.Playing) return;

        if (!isShooting && !isRewinding)
        {
            RotateHook();

            // Kiểm tra Touch cho di động hoặc Mouse/Space cho Editor
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || CheckTouchInput())
            {
                isShooting = true;
            }
        }
        else if (isShooting)
        {
            hookTransform.Translate(Vector3.down * baseShootSpeed * Time.deltaTime, Space.Self);

            if (hookTransform.position.y < -5f || Mathf.Abs(hookTransform.position.x) > 9f)
            {
                StartRewind();
            }
        }
        else if (isRewinding)
        {
            hookTransform.localPosition = Vector3.MoveTowards(
                hookTransform.localPosition, 
                hookInitialLocalPos, 
                currentRewindSpeed * Time.deltaTime
            );

            if (hookTransform.localPosition == hookInitialLocalPos)
            {
                ResetHook();
            }
        }
    }

    bool CheckTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Began;
        }
        return false;
    }

    void RotateHook()
    {
        float zAngle = transform.eulerAngles.z;
        if (zAngle > 180) zAngle -= 360;

        if (zAngle >= maxAngle) rotateDirection = -1;
        else if (zAngle <= -maxAngle) rotateDirection = 1;

        transform.Rotate(0, 0, rotateDirection * rotateSpeed * Time.deltaTime);
    }

    public void StartRewind()
    {
        isShooting = false;
        isRewinding = true;
    }

    void ResetHook()
    {
        isRewinding = false;
        currentRewindSpeed = baseRewindSpeed;

        if (caughtItem != null)
        {
            Item item = caughtItem.GetComponent<Item>();
            if (item != null)
            {
                GameManager.Instance.AddScore(item.scoreValue);
            }
            
            caughtItem.gameObject.SetActive(false); 
            caughtItem = null;
        }
    }

    public void OnHookHitItem(Collider2D collider)
    {
        if (isShooting && collider.CompareTag("Item"))
        {
            Item itemScript = collider.GetComponent<Item>();

            if (itemScript != null && itemScript.isBomb)
            {
                // 1. Trừ thời gian trong GameManager
                GameManager.Instance.DeductTime(itemScript.timePenalty);

                // 2. Kích hoạt hiệu ứng nổ (Đã bỏ rung màn hình)
                TriggerExplosion(collider.transform.position, itemScript.explosionRadius, collider.gameObject);

                // 3. Thu móc về nhanh lập tức
                currentRewindSpeed = baseRewindSpeed * 1.5f;
                StartRewind();
                return;
            }

            // Xử lý kéo đồ vật thông thường (Vàng, Đá...)
            caughtItem = collider.transform;
            caughtItem.SetParent(hookTransform);
            caughtItem.localPosition = Vector3.zero;

            if (itemScript != null)
            {
                currentRewindSpeed = baseRewindSpeed / itemScript.weight;
            }

            StartRewind();
        }
    }

    void TriggerExplosion(Vector3 explosionPos, float radius, GameObject bombObject)
    {
        // 1. Sinh ra Particle hiệu ứng lửa/khói tại vị trí nổ
        if (explosionPrefab != null)
        {
            GameObject expInstance = Instantiate(explosionPrefab, explosionPos, Quaternion.identity);
            Destroy(expInstance, 1.5f); // Tự hủy Particle sau 1.5s
        }

        // 2. Phát âm thanh nổ
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, explosionPos, 1.0f);
        }

        // (Đã xóa phần Camera Shake ở đây)

        // 3. Tiêu diệt quả bom chính
        Destroy(bombObject);

        // 4. Quét tiêu diệt các vật phẩm xung quanh trong bán kính
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(explosionPos, radius);
        foreach (Collider2D col in objectsInRange)
        {
            if (col != null && col.CompareTag("Item") && col.gameObject != bombObject)
            {
                Item nearbyItem = col.GetComponent<Item>();
                
                // Nếu vật phẩm xung quanh cũng là Bom, kích nổ dây chuyền
                if (nearbyItem != null && nearbyItem.isBomb)
                {
                    TriggerExplosion(col.transform.position, nearbyItem.explosionRadius, col.gameObject);
                }
                else
                {
                    Destroy(col.gameObject);
                }
            }
        }
    }
}