using UnityEngine;
using UnityEngine.UI;


public class Inventory : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip weaponSwapSound;
    [SerializeField] private float soundVolume = 1f;

    [SerializeField] private int defaultSlotCount = 4;
    private ItemInstance[] slots;
    private ItemInstance equipmentSlot;
    private int currentIndex = 0;
    private GameObject currentlyHeldItem;
    public Transform handTransform; // Transform where the item will be held

    private Image[] slotImages;
    private Image equipmentImage;

    private Image[] durabilityBGs;
    private Image[] durabilityFills;
    private Image equipDurabilityBG;
    private Image equipDurabilityFill;


    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [SerializeField] private Item defaultStartingItem;


    void Awake()
    {
        slots = new ItemInstance[defaultSlotCount];
        //handTransform = transform.Find("Hand"); // Default to a child named "Hand"

        // Give the player the starting item (slot 0)
        if (defaultStartingItem != null)
        {
            slots[0] = new ItemInstance(defaultStartingItem);
        }
    }
    void Start()
    {
        RefreshUI();

        if (slots[0] != null)
        {
            equipItem();
        }
    }


    public void BindUI(GameObject uiRoot)
    {
        Transform slotsParent = uiRoot.transform.Find("MainPanel/InventoryPanel");

        // Initialize arrays
        slotImages = new Image[defaultSlotCount];
        durabilityBGs = new Image[defaultSlotCount];
        durabilityFills = new Image[defaultSlotCount];

        for (int i = 0; i < defaultSlotCount; i++)
        {
            Transform slot = slotsParent.Find("Slot" + i);

            // Slot image
            slotImages[i] = slot.GetComponent<Image>();

            // Durability BG (direct child of Slot)
            durabilityBGs[i] = slot.Find("DurabilityBG")?.GetComponent<Image>();

            // Durability Fill (child of DurabilityBG)
            durabilityFills[i] = slot.Find("DurabilityBG/DurabilityFill")?.GetComponent<Image>();
        }

        // Equipment slot
        equipmentImage = uiRoot.transform.Find("MainPanel/EquipmentPanel/Slot0")
                                         .GetComponent<Image>();

        RefreshUI();
    }



    public bool AddItem(Item item)
    {
        

        ItemInstance instance = new ItemInstance(item);  // ← NEW

        if (item is Equipment)
        {
            if (equipmentSlot != null)
                DropItem(equipmentSlot.baseItem);

            equipmentSlot = instance;
            RefreshUI();
            return true;
        }

        // Find first empty slot
        for (int i = 1; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = instance;   // ← store instance, not item
                RefreshUI();
                equipItem();
                return true;
            }
        }

        // Inventory full → drop and replace
        int itemToReplace = currentIndex;
        if (currentIndex == 0) { itemToReplace++; }
        DropItem(slots[itemToReplace].baseItem);
        slots[itemToReplace] = instance;
        RefreshUI();
        equipItem();
        return true;
    }


    public void RemoveItem(Item item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].baseItem == item)
            {
                slots[i] = null;
                RefreshUI();
                return;
            }
        }

        if (equipmentSlot != null && equipmentSlot.baseItem == item)
        {
            equipmentSlot = null;
            RefreshUI();
        }
    }


    public void DropItem(Item item)
    {
        if (item == null) return;

        // Prevent dropping item in slot 0
        //if (slots[0] != null && slots[0].baseItem == item)
        //{
        //    Debug.Log("Cannot drop starting weapon.");
        //    return;
        //}

        if (item.prefab != null)
        {
            // Spawn the pickup at player's feet
            Vector3 dropPosition = transform.position + transform.forward;
            Instantiate(item.prefab, dropPosition, Quaternion.identity);
        }

        //Debug.Log("Dropped " + item.itemName);
    }

    public void RefreshUI()
    {
        if (slotImages != null)
        {
            for (int i = 0; i < slotImages.Length; i++)
            {
                if (slots[i] != null && slots[i].baseItem.icon != null)
                    slotImages[i].sprite = slots[i].baseItem.icon;
                else
                    slotImages[i].sprite = null;

                slotImages[i].color = defaultColor; //clear color
            }

            if (currentIndex >= 0 && currentIndex < slotImages.Length)
                slotImages[currentIndex].color = selectedColor;
        }

        if (equipmentImage != null)
        {
            if (equipmentSlot != null && equipmentSlot.baseItem.icon != null)
                equipmentImage.sprite = equipmentSlot.baseItem.icon;
            else
                equipmentImage.sprite = null;
        }

        // Update durability bars for each inventory slot
        for (int i = 0; i < slots.Length; i++)
        {
            ItemInstance instance = slots[i];

            // Slot 0: NEVER show durability, even for weapons
            if (i == 0)
            {
                if (durabilityBGs[i] != null) durabilityBGs[i].enabled = false;
                if (durabilityFills[i] != null) durabilityFills[i].enabled = false;
                continue;
            }

            // No item in slot
            if (instance == null || !(instance.baseItem is Weapon weapon))
            {
                if (durabilityBGs[i] != null) durabilityBGs[i].enabled = false;
                if (durabilityFills[i] != null) durabilityFills[i].enabled = false;
                continue;
            }

            // Item is a weapon → update durability
            if (durabilityBGs[i] != null) durabilityBGs[i].enabled = true;
            if (durabilityFills[i] != null)
            {
                durabilityFills[i].enabled = true;
                durabilityFills[i].fillAmount =
                    (float)instance.durability / weapon.maxDurability;
            }
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
            PlayWeaponSwapSound();
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
            PlayWeaponSwapSound();
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

    public ItemInstance GetCurrentItem() => slots[currentIndex];

    public void equipItem()
    {
        if (currentlyHeldItem != null)
        {
            Destroy(currentlyHeldItem);
        }

        ItemInstance instanceData = GetCurrentItem();
        Item item = instanceData.baseItem;
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
            playerController.SetEquippedItem(instanceData, instance);
        }
    }
    
    private void PlayWeaponSwapSound()
    {
        // Play weapon swap sound when changing slots
        if (weaponSwapSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(weaponSwapSound, soundVolume);
        }
    }
}
