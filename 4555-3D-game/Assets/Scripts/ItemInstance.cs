using UnityEngine;
[System.Serializable]
public class ItemInstance
{
    public Item baseItem; // ScriptableObject reference
    public int durability;

    public ItemInstance(Item baseItem)
    {
        this.baseItem = baseItem;

        if (baseItem is Weapon w)
            durability = w.maxDurability;
    }

    public void ReduceDurability(int amount)
    {
        durability = Mathf.Max(0, durability - amount);
    }

    public bool IsBroken => durability <= 0;
}
