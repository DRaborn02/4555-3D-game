using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 10f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage ({currentHealth}/{maxHealth})");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Optionally play animation or particle effect
        Destroy(gameObject);
    }
}
