using UnityEngine;

public class DamageCube : MonoBehaviour
{
    [SerializeField] private int damageHalfHearts = 1; // 1 = half a heart

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object has a PlayerHealth component
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageHalfHearts);
        }
    }
}
