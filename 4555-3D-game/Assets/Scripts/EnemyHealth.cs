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

    [Header("Audio")]
    [SerializeField] private AudioClip demonHurtSound;
    
    [SerializeField] private AudioClip demonDeathSound;
    [SerializeField] private AudioClip impHurtSound;



    [SerializeField] private AudioClip impDeathSound;
    [SerializeField] private float soundVolume = 1f;

    private Renderer[] renderers;
    private Color[] originalColors;

    // Public property for other scripts to check
    public bool IsInvulnerable => isInvulnerable;
    
    //check for demon/imp
    private bool IsDemon()
    {
        return GetComponent<EnemyController>() != null;
    }
    
    private bool IsImp()
    {
        return GetComponent<FlyingEnemyController>() != null;
    }

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

        // Play hurt sound based on enemy type
        if (IsDemon() && demonHurtSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(demonHurtSound, soundVolume);
        }
        else if (IsImp() && impHurtSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(impHurtSound, soundVolume);
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
                Material mat = renderers[i].material;

                // Toggle between flash white and original
                Color newColor = (mat.color == originalColors[i]) ? Color.white : originalColors[i];

                // Use correct shader property
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", newColor);
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", newColor);
            }

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // Reset colors
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_BaseColor"))
                renderers[i].material.SetColor("_BaseColor", originalColors[i]);
            else if (renderers[i].material.HasProperty("_Color"))
                renderers[i].material.SetColor("_Color", originalColors[i]);
        }

        isInvulnerable = false;
    }


    private void Die()
    {
        
        if (IsDemon() && demonDeathSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(demonDeathSound, soundVolume);
        }
        else if (IsImp() && impDeathSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(impDeathSound, soundVolume);
        }
        
        Destroy(gameObject);
    }
}
