using UnityEngine;

public class FireDragonBullet : MonoBehaviour
{
    public enum ShotDirection { LeftToRight, RightToLeft }

    [Header("Cấu hình Đạn")]
    public float speed = 20f;
    public float lifeTime = 3f;
    public string poolTag = "PlayerBullet";

    [Header("Hình ảnh & Hướng bắn")]
    public SpriteRenderer spriteRenderer;
    public ShotDirection currentDirection = ShotDirection.LeftToRight;

    private float timer = 0f;
    private string currentPoolTag;

    void OnEnable()
    {
        timer = 0f;
        currentPoolTag = string.IsNullOrEmpty(currentPoolTag) ? poolTag : currentPoolTag;
        UpdateBulletOrientation();
    }

    void Update()
    {
        Vector3 moveDir = (currentDirection == ShotDirection.LeftToRight) ? Vector3.right : Vector3.left;
        transform.Translate(moveDir * speed * Time.deltaTime, Space.World);

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Deactivate();
        }
    }

    public void SetDirection(ShotDirection dir)
    {
        currentDirection = dir;
        UpdateBulletOrientation();
    }

    public void SetPoolTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;

        currentPoolTag = tag;
        poolTag = tag;
    }

    private void UpdateBulletOrientation()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (currentDirection == ShotDirection.LeftToRight)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.Die();
            Deactivate();
        }
    }

    private void Deactivate()
    {
        string poolName = string.IsNullOrEmpty(currentPoolTag) ? poolTag : currentPoolTag;

        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(poolName, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}