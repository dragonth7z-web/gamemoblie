using UnityEngine;

public class RopeRenderer : MonoBehaviour
{
    public Transform originPoint; // Điểm bắt đầu (HookPivot)
    public Transform hookPoint;   // Điểm kết thúc (Hook)
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("RopeRenderer cần một LineRenderer trên cùng GameObject.", this);
            enabled = false;
            return;
        }

        // Đảm bảo Line Renderer luôn có đúng 2 điểm nối
        lineRenderer.positionCount = 2; 
    }

    void Update()
    {
        if (originPoint != null && hookPoint != null)
        {
            // Cập nhật vị trí điểm đầu và điểm cuối liên tục theo vị trí thực tế
            lineRenderer.SetPosition(0, originPoint.position);
            lineRenderer.SetPosition(1, hookPoint.position);
        }
    }
}