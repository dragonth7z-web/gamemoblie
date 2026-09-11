using UnityEngine;

public class HookTrigger : MonoBehaviour
{
    private HookController hookController;

    void Start()
    {
        // Tìm HookController ở cha hoặc trên chính nó
        hookController = GetComponentInParent<HookController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hookController != null)
        {
            hookController.OnHookHitItem(collision);
        }
    }
}