using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int defaultSlotCount = 3;
    private Item[] slots;
    private Item equipmentSlot;
    private int currentIndex = 0;
    private GameObject currentlyHeldItem;
    public Transform handTransform; // Transform where the item will be held

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

    public void OnNextSlot()
    {
        if (IsInventoryEmpty())
        {
            Debug.Log("Inventory is empty.");
            return;
        }

        int startIndex = currentIndex;
        do
        {
            currentIndex = (currentIndex + 1) % slots.Length;
        }
        while (slots[currentIndex] == null && currentIndex != startIndex);

        Debug.Log($"Switched to slot {currentIndex} containing {(slots[currentIndex] != null ? slots[currentIndex].itemName : "nothing")}");
        if (slots[currentIndex] != null)
        {
            equipItem();
        }
    }

    public void OnPreviousSlot()
    {
        if (IsInventoryEmpty())
        {
            Debug.Log("Inventory is empty.");
            return;
        }

        int startIndex = currentIndex;
        do
        {
            currentIndex = (currentIndex - 1 + slots.Length) % slots.Length;
        }
        while (slots[currentIndex] == null && currentIndex != startIndex);

        Debug.Log($"Switched to slot {currentIndex} containing {(slots[currentIndex] != null ? slots[currentIndex].itemName : "nothing")}");
        if (slots[currentIndex] != null)
        {
            equipItem();
        }
    }

    private bool IsInventoryEmpty()
    {
        foreach (var slot in slots)
        {
            if (slot != null) return false;
        }
        return true;
    }

    public Item GetCurrentItem() => slots[currentIndex];

    public void equipItem()
    {
        if (currentlyHeldItem != null)
        {
            Destroy(currentlyHeldItem);
        }
        
        Item item = GetCurrentItem();
        Debug.Log("Equipped item: " + item.itemName);

        // Instantiate held prefab (if any) under the hand transform
        GameObject instance = null;
        if (item.heldPrefab != null && handTransform != null)
        {
            instance = Instantiate(item.heldPrefab, handTransform);

            instance.transform.SetParent(handTransform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;

            currentlyHeldItem = instance;
        }

        // Notify the player controller of the equipped item and the spawned instance
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetEquippedItem(item, instance);
        }
    }
}
