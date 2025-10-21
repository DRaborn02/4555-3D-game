using UnityEngine;
using UnityEngine.UI;


public class Inventory : MonoBehaviour
{
    [SerializeField] private int defaultSlotCount = 3;
    private Item[] slots;
    private Item equipmentSlot;
    private int currentIndex = 0;
    private GameObject currentlyHeldItem;
    public Transform handTransform; // Transform where the item will be held

    private Image[] slotImages;
    private Image equipmentImage;

    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    void Awake()
    {
        slots = new Item[defaultSlotCount];
        //handTransform = transform.Find("Hand"); // Default to a child named "Hand"
    }

    public void BindUI(GameObject uiRoot)
    {
        Transform slotsParent = uiRoot.transform.Find("MainPanel").Find("InventoryPanel");

        // Make sure we only pull the direct child slot images (Slot0, Slot1, Slot2)
        slotImages = new Image[defaultSlotCount];
        for (int i = 0; i < defaultSlotCount; i++)
        {
            slotImages[i] = slotsParent.Find("Slot" + i).GetComponent<Image>();
        }

        equipmentImage = uiRoot.transform.Find("MainPanel").Find("EquipmentPanel").Find("Slot0").GetComponent<Image>();

        RefreshUI();
    }


    public bool AddItem(Item item)
    {
        if (item is Equipment)
        {
            if (equipmentSlot != null)
            {
                // Drop current equipment
                DropItem(equipmentSlot);
            }

            equipmentSlot = item;
            //Debug.Log("Equipped " + item.itemName);
            RefreshUI();
            return true;
        }

        // Find first empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                //Debug.Log("Picked up " + item.itemName);
                RefreshUI();
                equipItem();
                return true;
            }
        }

        // Inventory full → drop current item
        DropItem(slots[currentIndex]);
        slots[currentIndex] = item;
        RefreshUI();
        equipItem();
        return true;
    }

    public void DropItem(Item item)
    {
        if (item == null) return;

        if (item.prefab != null)
        {
            // Spawn the pickup at player's feet
            Vector3 dropPosition = transform.position + transform.forward;
            Instantiate(item.prefab, dropPosition, Quaternion.identity);
        }

        //Debug.Log("Dropped " + item.itemName);
    }

    private void RefreshUI()
    {
        if (slotImages != null)
        {
            for (int i = 0; i < slotImages.Length; i++)
            {
                if (slots[i] != null && slots[i].icon != null)
                    slotImages[i].sprite = slots[i].icon;
                else
                    slotImages[i].sprite = null; // clear slot

                slotImages[i].color = defaultColor; //clear color
            }

            if (currentIndex >= 0 && currentIndex < slotImages.Length)
                slotImages[currentIndex].color = selectedColor;
        }

        if (equipmentImage != null)
        {
            if (equipmentSlot != null && equipmentSlot.icon != null)
                equipmentImage.sprite = equipmentSlot.icon;
            else
                equipmentImage.sprite = null;
        }
    }

    public void OnNextSlot()
    {
        if (IsInventoryEmpty())
        {
            //Debug.Log("Inventory is empty.");
            return;
        }

        int startIndex = currentIndex;
        do
        {
            currentIndex = (currentIndex + 1) % slots.Length;
        }
        while (slots[currentIndex] == null && currentIndex != startIndex);

        RefreshUI();
        //Debug.Log($"Switched to slot {currentIndex} containing {(slots[currentIndex] != null ? slots[currentIndex].itemName : "nothing")}");
        if (slots[currentIndex] != null)
        {
            //print("We have an item in the current slot, equipping it");
            equipItem();
        }
    }

    public void OnPreviousSlot()
    {
        if (IsInventoryEmpty())
        {
            //Debug.Log("Inventory is empty.");
            return;
        }

        int startIndex = currentIndex;
        do
        {
            currentIndex = (currentIndex - 1 + slots.Length) % slots.Length;
        }
        while (slots[currentIndex] == null && currentIndex != startIndex);

        RefreshUI();
        //Debug.Log($"Switched to slot {currentIndex} containing {(slots[currentIndex] != null ? slots[currentIndex].itemName : "nothing")}");
        if (slots[currentIndex] != null)
        {
            //print("We have an item in the current slot, equipping it");
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
        //Debug.Log("Equipped item: " + item.itemName);

        // Instantiate held prefab (if any) under the hand transform
        GameObject instance = null;
        if (item.heldPrefab != null && handTransform != null)
        {
            //print("We got an item prefab and a hand transform");
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
