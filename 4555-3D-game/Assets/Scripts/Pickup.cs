using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private Item item; // assign your ScriptableObject here (e.g., HealthpackSO)

    public Item GetItem()
    {
        return item;
    }
    public void Consume()
    {
        Destroy(gameObject);
    }
}
