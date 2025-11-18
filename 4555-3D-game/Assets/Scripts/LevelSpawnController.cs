using UnityEngine;
using System.Collections;
using System.Linq;

public class LevelSpawnController : MonoBehaviour
{
    public float waveDelay = 5f;

    private DemonSpawner[] demonSpawners;
    private ItemSpawner[] itemSpawners;

    private bool waitingForWave;

    void Awake()
    {
        demonSpawners = Object.FindObjectsByType<DemonSpawner>(FindObjectsSortMode.None);
        itemSpawners = Object.FindObjectsByType<ItemSpawner>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (!waitingForWave && AllEnemiesCleared())
        {
            StartCoroutine(StartNextWave());
        }
    }

    private bool AllEnemiesCleared()
    {
        foreach (var spawner in demonSpawners)
        {
            if (!spawner.IsWaveCleared())
                return false;
        }
        return true;
    }

    private IEnumerator StartNextWave()
    {
        waitingForWave = true;

        yield return new WaitForSeconds(waveDelay);

        // Spawn enemies
        foreach (var s in demonSpawners)
            s.TrySpawn();

        // Spawn items
        foreach (var s in itemSpawners)
            s.TrySpawn();

        waitingForWave = false;
    }
}
