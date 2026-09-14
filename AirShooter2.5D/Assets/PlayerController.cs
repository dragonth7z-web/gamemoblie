using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float moveSpeed = 12f;
    public float xBound = 6.5f;     // Giới hạn Trái/Phải
    public float zBoundMin = -5.5f; // Giới hạn Dưới
    public float zBoundMax = 3.5f;  // Giới hạn Trên

    [Header("Hiệu ứng nghiêng 2.5D")]
    public Transform modelHolder;      // Object con chứa Model 3D máy bay
    public float basePitchAngle = 20f; // Góc nghiêng mặc định song song nền chéo
    public float tiltAmount = 25f;     // Góc nghiêng tối đa khi rẽ Trái/Phải
    public float tiltSpeed = 8f;       // Tốc độ nghiêng mượt

    [Header("Cấu hình BẮN TỰ ĐỘNG")]
    public string bulletPoolTag = "PlayerBullet"; // Tag đạn trong ObjectPooler
    public Transform firePointLeft;               // Nòng pháo bắn sang Trái
    public Transform firePointRight;              // Nòng pháo bắn sang Phải
    public float fireRate = 0.15f;                // Tốc độ bắn (giây/viên)
    public float detectionRadius = 15f;           // Bán kính quét tìm địch
    public LayerMask enemyLayer;                  // Layer chứa kẻ địch (Enemy)

    private float nextFireTime = 0f;
    private PlayerIntro playerIntro;

    void Start()
    {
        playerIntro = GetComponent<PlayerIntro>();
    }

    void Update()
    {
        // Nếu đang chạy Intro thì chưa cho phép di chuyển và bắn
        if (playerIntro != null && !playerIntro.CanControl()) return;

        HandleMovement();
        AutoShooting(); // Chuyển sang gọi hàm AutoShooting
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

        float clampedX = Mathf.Clamp(transform.position.x, -xBound, xBound);
        float clampedZ = Mathf.Clamp(transform.position.z, zBoundMin, zBoundMax);
        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);

        if (modelHolder != null)
        {
            float targetRoll = -moveX * tiltAmount;
            Quaternion targetRotation = Quaternion.Euler(basePitchAngle, 0f, targetRoll);
            
            modelHolder.localRotation = Quaternion.Slerp(
                modelHolder.localRotation, 
                targetRotation, 
                Time.deltaTime * tiltSpeed
            );
        }
    }

    /// <summary>
    /// Hàm xử lý TỰ ĐỘNG BẮN khi phát hiện kẻ địch
    /// </summary>
    void AutoShooting()
    {
        if (Time.time < nextFireTime) return;

        // Quét tìm tất cả kẻ địch trong vùng bán kính detectionRadius
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);

        if (enemies.Length > 0)
        {
            // Tìm kẻ địch ở gần Player nhất
            Transform closestEnemy = GetClosestEnemy(enemies);

            if (closestEnemy != null)
            {
                nextFireTime = Time.time + fireRate;

                // Kiểm tra vị trí kẻ địch: Ở bên Trái hay bên Phải so với Player?
                if (closestEnemy.position.x < transform.position.x)
                {
                    // Địch nằm bên Trái -> Bắn đạn sang Trái
                    ShootBullet(FireDragonBullet.ShotDirection.RightToLeft, firePointLeft);
                }
                else
                {
                    // Địch nằm bên Phải (hoặc thẳng mặt) -> Bắn đạn sang Phải
                    ShootBullet(FireDragonBullet.ShotDirection.LeftToRight, firePointRight);
                }
            }
        }
    }

    /// <summary>
    /// Thuật toán tìm kẻ địch gần nhất trong danh sách đã quét
    /// </summary>
    Transform GetClosestEnemy(Collider[] enemies)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (Collider enemyCollider in enemies)
        {
            Vector3 directionToTarget = enemyCollider.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = enemyCollider.transform;
            }
        }

        return bestTarget;
    }

    void ShootBullet(FireDragonBullet.ShotDirection direction, Transform firePoint)
    {
        Transform spawnPoint = (firePoint != null) ? firePoint : transform;

        if (ObjectPooler.Instance == null) return;

        string poolTagForDirection = (direction == FireDragonBullet.ShotDirection.LeftToRight)
            ? "LeftToRight"
            : "RightToLeft";

        // Lấy viên đạn ra từ Object Pool theo hướng bắn
        GameObject bulletObj = ObjectPooler.Instance.SpawnFromPool(poolTagForDirection, spawnPoint.position, spawnPoint.rotation);

        if (bulletObj != null)
        {
            FireDragonBullet bulletScript = bulletObj.GetComponent<FireDragonBullet>();
            if (bulletScript != null)
            {
                bulletScript.SetPoolTag(poolTagForDirection);
                bulletScript.SetDirection(direction);
            }
        }
    }

    // Hiển thị vòng tròn tầm quét địch trong cửa sổ Scene để dễ căn chỉnh
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}