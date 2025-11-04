using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 10f;
    private float currentHealth;

    [Header("Invulnerability & Flash")]
    public float invulnerabilityDuration = 1f;
    public float flashInterval = 0.2f;
    private bool isInvulnerable = false;

    private Renderer[] renderers;
    private Color[] originalColors;

    // Public property for other scripts to check
    public bool IsInvulnerable => isInvulnerable;

    void Start()
    {
        currentHealth = maxHealth;

        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isInvulnerable) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage ({currentHealth}/{maxHealth})");

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(FlashAndInvulnerability());
    }

    private IEnumerator FlashAndInvulnerability()
    {
        isInvulnerable = true;
        float elapsed = 0f;

        while (elapsed < invulnerabilityDuration)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = (renderers[i].material.color == originalColors[i])
                                              ? Color.red
                                              : originalColors[i];
            }

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = originalColors[i];
        }

        isInvulnerable = false;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
