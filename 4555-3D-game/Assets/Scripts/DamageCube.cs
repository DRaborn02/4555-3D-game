using UnityEngine;
using System.Collections;

public class DamageCube : MonoBehaviour
{
    [SerializeField] private int damageQuarterHearts = 1;
    [SerializeField] private float damageInterval = 1f; // time between damage ticks

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Start damaging this player as long as they're inside
            StartCoroutine(DamageOverTime(playerHealth));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Stop damaging this player
            StopCoroutine(DamageOverTime(playerHealth));
        }
    }

    private IEnumerator DamageOverTime(PlayerHealth player)
    {
        // Initial hit immediately
        player.TakeDamage(damageQuarterHearts);

        // Continue dealing damage every interval while player remains inside
        while (true)
        {
            yield return new WaitForSeconds(damageInterval);

            // If the player died or was destroyed, stop
            if (player == null || player.IsDead)
                yield break;

            player.TakeDamage(damageQuarterHearts);
        }
    }
}
