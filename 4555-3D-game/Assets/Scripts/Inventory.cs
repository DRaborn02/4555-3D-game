using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int defaultSlotCount = 3;
    private Item[] slots;
    private Item equipmentSlot;
    private int currentIndex = 0;

    void Awake()
    {
        slots = new Item[defaultSlotCount];
    }

    public bool AddItem(Item item)
    {
        if (item is Equipment)
        {
            equipmentSlot = item;
            Debug.Log("Equipped " + item.itemName);
            return true;
        }

        // Find first empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                Debug.Log("Picked up " + item.itemName);
                return true;
            }
        }

        // Inventory full → drop current item
        DropItem(slots[currentIndex]);
        slots[currentIndex] = item;
        return true;
    }

    public void DropItem(Item item)
    {
        if (item == null) return;

        if (item.pickupPrefab != null)
        {
            // Spawn the pickup at player's feet
            Vector3 dropPosition = transform.position + transform.forward;
            Instantiate(item.pickupPrefab, dropPosition, Quaternion.identity);
        }

        Debug.Log("Dropped " + item.itemName);
    }

    public void NextSlot()
    {
        currentIndex = (currentIndex + 1) % slots.Length;
        Debug.Log("Switched to slot " + currentIndex);
    }

    public void PreviousSlot()
    {
        currentIndex = (currentIndex - 1 + slots.Length) % slots.Length;
        Debug.Log("Switched to slot " + currentIndex);
    }

    public Item GetCurrentItem() => slots[currentIndex];
}
