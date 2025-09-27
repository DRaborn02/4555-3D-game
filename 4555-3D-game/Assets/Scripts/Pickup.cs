using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private Item item; // assign your ScriptableObject here (e.g., HealthpackSO)

    private GameObject weaponClub;

    public Item GetItem()
    {
        return item;
    }
    public void Consume()
    {
        Destroy(gameObject);
    }
}
