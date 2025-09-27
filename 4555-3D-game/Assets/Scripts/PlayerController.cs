using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] float rotationSpeed = 10.0f;
    [SerializeField] private float interactRange = 2f;
    private Collider[] results = new Collider[5]; // buffer for overlap checks

    private Vector2 moveInput;
    private Rigidbody rb;
    private Inventory inventory;
    [SerializeField] private Item item;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inventory = GetComponent<Inventory>();
    }

    public void OnMove()
    {
        var playerInput = GetComponent<PlayerInput>();
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
    }

    void OnInteract()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange);
        foreach (var hit in hits)
        {
            Pickup pickup = hit.GetComponent<Pickup>();
            if (pickup != null)
            {
                Inventory inventory = GetComponent<Inventory>();
                if (inventory != null && inventory.AddItem(pickup.GetItem()))
                {
                    pickup.Consume();
                }
                break; // stop after first pickup
            }
        }
    }

    void FixedUpdate()
    {
        // WASD always maps directly to world axes (XZ plane)
        Vector3 move = new Vector3(-moveInput.x, 0, -moveInput.y);

        if (move.sqrMagnitude > 0.01f)
        {
            move.Normalize();

            // Move in world space (ignores capsule's facing)
            Vector3 targetPos = rb.position + move * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPos);

            // Rotate to face direction of movement
            Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }
    }
}
