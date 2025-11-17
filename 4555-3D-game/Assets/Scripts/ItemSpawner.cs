using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public LevelLootConfig levelLootConfig;
    public Transform spawnPoint; // where the prefab appears

    public int currentLevel = 1; // set this when the level loads
    void Awake()
    {
        // If no spawnPoint assigned, use this object's transform
        if (spawnPoint == null)
            spawnPoint = transform;
    }

    void Start()
    {
        SpawnItemForCurrentLevel();
    }

    public void SpawnItemForCurrentLevel()
    {
        var lootTable = levelLootConfig.GetLootTableForLevel(currentLevel);
        if (lootTable == null)
        {
            Debug.LogWarning($"No loot table for level {currentLevel}");
            return;
        }

        var item = lootTable.GetRandomItem();

        if (item == null)
        {
            Debug.LogWarning("Loot table has no items");
            return;
        }

        Instantiate(item.prefab, spawnPoint.position, Quaternion.identity);
    }
}
