using UnityEngine;

[CreateAssetMenu(menuName = "Loot/Level Loot Config")]
public class LevelLootConfig : ScriptableObject
{
    [System.Serializable]
    public class LevelLoot
    {
        public int levelNumber;
        public LootTable lootTable;
    }

    public LevelLoot[] levels;

    public LootTable GetLootTableForLevel(int level)
    {
        foreach (var entry in levels)
        {
            if (entry.levelNumber == level)
                return entry.lootTable;
        }
        return null;
    }
}
