using System.Collections;
using UnityEngine;

public class PlayerIntro : MonoBehaviour
{
    [Header("Tọa độ xuất phát & Tọa độ chơi")]
    public Vector3 startPosition = new Vector3(0f, -2f, -8f); // Dưới đáy màn hình
    public Vector3 playPosition  = new Vector3(0f, 1f, -4f);  // Vị trí chiến đấu chính
    
    public float introDuration = 2f; // Thời gian bay vào (2 giây)
    private bool isControlAllowed = false;

    void Start()
    {
        StartCoroutine(StartIntroRoutine());
    }

    IEnumerator StartIntroRoutine()
    {
        isControlAllowed = false;
        transform.position = startPosition;

        float elapsedTime = 0f;
        while (elapsedTime < introDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / introDuration);
            transform.position = Vector3.Lerp(startPosition, playPosition, t);
            yield return null;
        }

        transform.position = playPosition;
        isControlAllowed = true; // Cho phép điều khiển máy bay
    }

    public bool CanControl() => isControlAllowed;
}