using UnityEngine;

public class HookCollision : MonoBehaviour
{
    private HookController controller;

    void Start()
    {
        controller = GetComponentInParent<HookController>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (controller != null)
        {
            controller.OnHookHitItem(collider);
        }
    }
}