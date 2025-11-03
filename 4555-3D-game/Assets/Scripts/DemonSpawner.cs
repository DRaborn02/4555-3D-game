using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject demonPrefab;          // Main demon prefab
    public GameObject impPrefab;            // Imp prefab
    public Transform spawnPoint;            // Optional: demon spawn location
    public float respawnDelay = 5f;         // Delay before next wave
    public bool autoRespawn = true;         // Should it respawn automatically?

    [Header("Imp Settings")]
    public int impCount = 3;                // How many imps per wave
    public float impSpawnRadius = 5f;       // How far from the demon they spawn

    private GameObject currentDemon;
    private readonly List<GameObject> currentImps = new List<GameObject>();
    private bool isSpawning;

    void Start()
    {
        SpawnWave();
    }

    void Update()
    {
        if (autoRespawn && !isSpawning)
        {
            // Check if demon and all imps are gone
            bool allEnemiesGone = currentDemon == null && AllImpsGone();
            if (allEnemiesGone)
            {
                StartCoroutine(RespawnAfterDelay());
            }
        }
    }

    private bool AllImpsGone()
    {
        // Clean up nulls and see if any imps still exist
        currentImps.RemoveAll(imp => imp == null);
        return currentImps.Count == 0;
    }

    private void SpawnWave()
    {
        isSpawning = false;

        // --- Spawn Demon ---
        Vector3 demonSpawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion demonSpawnRot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;
        currentDemon = Instantiate(demonPrefab, demonSpawnPos, demonSpawnRot);

        // --- Spawn Imps around Demon ---
        currentImps.Clear();
        for (int i = 0; i < impCount; i++)
        {
            Vector2 circle = Random.insideUnitCircle * impSpawnRadius;
            Vector3 impPos = demonSpawnPos + new Vector3(circle.x, 0, circle.y);
            GameObject imp = Instantiate(impPrefab, impPos, Quaternion.identity);
            currentImps.Add(imp);
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        isSpawning = true;
        yield return new WaitForSeconds(respawnDelay);
        SpawnWave();
    }
}
