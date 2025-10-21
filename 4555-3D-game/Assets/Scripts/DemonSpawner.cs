using System.Collections;
using UnityEngine;

public class DemonSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject demonPrefab;          // Assign your demon prefab here
    public Transform spawnPoint;            // Optional: override spawn location
    public float respawnDelay = 3f;         // Delay before a new demon spawns
    public bool autoRespawn = true;         // Should it automatically respawn demons?

    private GameObject currentDemon;
    private bool isSpawning;

    void Start()
    {
        SpawnDemon();
    }

    void Update()
    {
        // If demon was destroyed (defeated), respawn after delay
        if (autoRespawn && !isSpawning && currentDemon == null)
        {
            StartCoroutine(RespawnAfterDelay());
        }
    }

    private void SpawnDemon()
    {
        // Spawn either at a specified spawn point or this object's position
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        currentDemon = Instantiate(demonPrefab, spawnPos, spawnRot);
        isSpawning = false;
    }

    private IEnumerator RespawnAfterDelay()
    {
        isSpawning = true;
        yield return new WaitForSeconds(respawnDelay);
        SpawnDemon();
    }
}
