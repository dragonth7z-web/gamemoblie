using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Header("Cài đặt tự động dọn dẹp")]
    [Tooltip("Tự đo thời lượng Particle System nếu được bật")]
    public bool autoDetectParticleDuration = true;

    [Header("Thời gian sống thủ công (giây)")]
    public float lifetime = 1.5f;

    private void Start()
    {
        float delay = lifetime;

        if (autoDetectParticleDuration)
        {
            ParticleSystem particleSystem = GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                var main = particleSystem.main;
                delay = main.duration + main.startLifetime.constantMax;
            }
        }

        Destroy(gameObject, delay);
    }
}