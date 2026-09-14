using UnityEngine;

public class MysteryBag : MonoBehaviour
{
    private Item itemScript;

    void Start()
    {
        itemScript = GetComponent<Item>();
        if (itemScript != null)
        {
            // Cho điểm ngẫu nhiên từ 50 đến 800 điểm
            itemScript.scoreValue = Random.Range(1, 17) * 50; 
            
            // Trọng lượng ngẫu nhiên (kéo nhanh hoặc kéo chậm)
            itemScript.weight = Random.Range(0.5f, 2f);
        }
    }
}