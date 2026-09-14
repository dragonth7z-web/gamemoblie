using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 5f;
    public float minZBound = 0f; // Thu hồi khi máy bay vượt qua vùng nhìn thấy phía dưới 0
    public string poolTag = "EnemyBasic";

    void Update()
    {
        // Kẻ địch di chuyển tiến về phía dưới màn hình
        transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);

        // Vượt quá mép dưới màn hình thì thu hồi về Pool
        if (transform.position.z < minZBound)
        {
            Recycle();
        }
    }

    public void Die()
    {
        // Có thể thêm hiệu ứng nổ (VFX) hoặc cộng điểm ở đây
        Recycle();
    }

    private void Recycle()
    {
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}