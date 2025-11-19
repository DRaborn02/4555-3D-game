using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private Item item; // assign your ScriptableObject here (e.g., HealthpackSO)

    [SerializeField] private int currentDurability = -1;

    private GameObject weaponClub;

    public void setDurability(int durability)
    {
        currentDurability = durability;
    }

    public int getDurability()
    { 
        return currentDurability; 
    }

    public Item GetItem()
    {

        return item;
    }
    public void Consume()
    {
        Destroy(gameObject);
    }
}
