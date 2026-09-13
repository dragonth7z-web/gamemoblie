using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HookController : MonoBehaviour
{
    public Transform hookTransform;
    public float rotateSpeed = 80f;
    public float maxAngle = 70f;
    public float baseShootSpeed = 10f;
    public float baseRewindSpeed = 8f;

    [Header("Cấu hình Hiệu ứng Nổ")]
    public GameObject explosionPrefab; 
    public AudioClip explosionSound;    

    private float currentRewindSpeed;
    private bool isShooting = false;
    private bool isRewinding = false;
    private int rotateDirection = 1;
    private Vector3 hookInitialLocalPos;
    private Transform caughtItem = null;

    void Awake()
    {
        if (hookTransform == null && transform.childCount > 0)
            hookTransform = transform.GetChild(0);

        if (hookTransform == null)
        {
            Debug.LogError("HookController cần một hookTransform hoặc một object con làm móc.", this);
            enabled = false;
            return;
        }

        hookInitialLocalPos = hookTransform.localPosition;
        currentRewindSpeed = baseRewindSpeed;
    }

    void Update()
    {
        // Kiểm tra an toàn GameManager
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameState.Playing) return;

        if (!isShooting && !isRewinding)
        {
            // Luôn gọi hàm xoay móc ở mọi khung hình khi đang rảnh
            RotateHook();

            // Lệnh kiểm tra nút bấm (Phím Space hoặc Click chuột trái hoặc Chạm màn hình)
            bool isInputPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || CheckTouchInput();

            if (isInputPressed)
            {
                // Chỉ cho phép bắn móc NẾU NGƯỜI CHƠI KHÔNG BẤM VÀO GIAO DIỆN (UI)
                if (!IsPointerOverUI())
                {
                    isShooting = true;
                }
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
                Mathf.Max(0f, currentRewindSpeed) * Time.deltaTime
            );

            if (hookTransform.localPosition == hookInitialLocalPos)
            {
                ResetHook();
            }
        }
    }

    // Hàm kiểm tra UI an toàn
    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // Kiểm tra chuột PC
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        // Kiểm tra cảm ứng điện thoại
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                {
                    return true;
                }
            }
        }
        return false;
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
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddScore(item.scoreValue);
                }
            }
            
            caughtItem.gameObject.SetActive(false); 
            caughtItem = null;
        }
    }

    public void OnHookHitItem(Collider2D collider)
    {
        if (!isShooting || collider == null) return;

        Item itemScript = collider.GetComponentInParent<Item>();
        if (itemScript == null) return;

        if (itemScript.isBomb)
        {
            TriggerExplosion(itemScript);
            currentRewindSpeed = baseRewindSpeed * 1.5f;
            StartRewind();
            return;

        }

        caughtItem = itemScript.transform;
        caughtItem.SetParent(hookTransform);
        caughtItem.localPosition = Vector3.zero;

        currentRewindSpeed = baseRewindSpeed / Mathf.Max(0.01f, itemScript.weight);
        StartRewind();
    }

    void TriggerExplosion(Item firstBomb)
    {
        var pendingBombs = new Queue<Item>();
        var processedBombs = new HashSet<Item>();
        pendingBombs.Enqueue(firstBomb);

        while (pendingBombs.Count > 0)
        {
            Item bomb = pendingBombs.Dequeue();
            if (bomb == null || !processedBombs.Add(bomb)) continue;

            Vector3 currentPosition = bomb.transform.position;
            float currentRadius = Mathf.Max(0f, bomb.explosionRadius);

            if (explosionPrefab != null)
            {
                GameObject effect = Instantiate(explosionPrefab, currentPosition, Quaternion.identity);
                Destroy(effect, 1.5f);
            }

            if (explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(explosionSound, currentPosition);
            }

            if (bomb == firstBomb && GameManager.Instance != null)
            {
                GameManager.Instance.DeductTime(bomb.timePenalty);
            }

            Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(currentPosition, currentRadius);
            foreach (Collider2D nearbyCollider in objectsInRange)
            {
                if (nearbyCollider == null || !nearbyCollider.CompareTag("Item")) continue;

                Item nearbyItem = nearbyCollider.GetComponentInParent<Item>();
                if (nearbyItem == null || nearbyItem == bomb) continue;

                if (nearbyItem.isBomb)
                {
                    pendingBombs.Enqueue(nearbyItem);
                }
                else
                {
                    Destroy(nearbyItem.gameObject);
                }
            }

            Destroy(bomb.gameObject);
        }
    }
}