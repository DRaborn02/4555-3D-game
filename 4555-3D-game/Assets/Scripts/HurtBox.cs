using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HurtBox : MonoBehaviour
{
    [Header("Damage Info")]
    public float damage = 0;
    public float speed = 0;
    public GameObject owner; // who spawned this (player, enemy, etc.)
    public Vector3 direction;

    [Header("Lifetime")]
    public float lifetime = 0.5f; // auto-destroy after some time

    private bool hasHit = false;
    public bool isProjectile = false;
    public Rigidbody rb;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        Destroy(gameObject, lifetime); // safety cleanup
    }

    void FixedUpdate()
    {
        // Only move if this is marked as a projectile
        if (isProjectile && rb != null)
        {
            rb.MovePosition(transform.position + direction.normalized * speed * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //print("damage: " + damage);
        //print("other tag: " + other.gameObject.tag);
        if (other.CompareTag("Enemy"))
        {
            //print("hit: " + other.gameObject.name);
            EnemyHealth target = other.GetComponent<EnemyHealth>();
            target.TakeDamage(damage);
            Destroy(gameObject); // despawn after hit
        }
    }
}
