using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageCube : MonoBehaviour
{
    [SerializeField] private int damageQuarterHearts = 1;
    [SerializeField] private float damageInterval = 1f;

    // Keeps track of running coroutines per player
    private Dictionary<PlayerHealth, Coroutine> activeCoroutines = new();

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null && !activeCoroutines.ContainsKey(player))
        {
            // start coroutine and store it
            Coroutine c = StartCoroutine(DamageOverTime(player));
            activeCoroutines[player] = c;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null && activeCoroutines.TryGetValue(player, out Coroutine c))
        {
            // stop the *correct* coroutine
            StopCoroutine(c);
            activeCoroutines.Remove(player);
        }
    }

    private IEnumerator DamageOverTime(PlayerHealth player)
    {
        // deal initial hit
        player.TakeDamage(damageQuarterHearts);

        while (true)
        {
            yield return new WaitForSeconds(damageInterval);

            if (player == null || player.IsDead)
                break;

            player.TakeDamage(damageQuarterHearts);
        }

        // auto-cleanup in case player died while inside
        activeCoroutines.Remove(player);
    }
}
