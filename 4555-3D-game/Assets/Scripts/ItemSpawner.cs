using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    public LevelLootConfig levelLootConfig;
    public Transform spawnPoint; // where the prefab appears
    [SerializeField] Transform spawnArea; // assign your SpawnArea child here
    private Bounds areaBounds;

    public int currentLevel = 1; // set this when the level loads
    void Awake()
    {
        // If no spawnPoint assigned, use this object's transform
        if (spawnPoint == null)
            spawnPoint = transform;
        meshRenderer = GetComponent<MeshRenderer>();

        MeshRenderer mr = spawnArea.GetComponent<MeshRenderer>();
        areaBounds = mr.bounds;
    }

    void Start()
    {
        SpawnItemForCurrentLevel();
        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }

    Vector3 GetRandomPointInArea()
    {
        Vector3 min = areaBounds.min;
        Vector3 max = areaBounds.max;

        return new Vector3(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            Random.Range(min.z, max.z)
        );
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

        GameObject newItem = Instantiate(item.prefab, spawnPoint.position, Quaternion.identity);
        newItem.transform.position = GetRandomPointInArea();
    }
}
