using UnityEngine;

public class DamageCube : MonoBehaviour
{
    [SerializeField] private int damageQuarterHearts = 1; // 1 =  1/4th heart

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object has a PlayerHealth component
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageQuarterHearts);
        }
    }
}
