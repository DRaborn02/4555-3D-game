using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Loot/Loot Table")]
public class LootTable : ScriptableObject
{
    public List<Item> possibleItems;

    public Item GetRandomItem()
    {
        if (possibleItems.Count == 0)
            return null;

        int index = Random.Range(0, possibleItems.Count);
        return possibleItems[index];
    }
}
