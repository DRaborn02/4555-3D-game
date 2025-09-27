using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] float rotationSpeed = 10.0f;
    [SerializeField] private float interactRange = 2f;
    [SerializeField] float jumpHeight = 5.0f;
    [SerializeField] float jumpCooldown = 1.0f;
    [SerializeField] float dashSpeed = 5.0f;
    [SerializeField] float dashDuration = 0.1f;
    [SerializeField] float dashCooldown = 2.0f;
    [SerializeField] private Color npcHighlightColor = Color.white; 

    private Collider[] results = new Collider[5]; // buffer for overlap checks

    private Vector2 moveInput;
    private Rigidbody rb;
    private Inventory inventory;
    [SerializeField] private Item item;
    private float jumpInput;
    private bool canJump = true;
    private float dashInput;
    private bool dashRequested = false;
    private bool canDash = true;
    private bool isDashing = false;

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

    public void OnJump()
    {
        var playerInput = GetComponent<PlayerInput>();
        jumpInput = playerInput.actions["Jump"].ReadValue<float>();
    }

    public void OnDash()
    {
        var playerInput = GetComponent<PlayerInput>();
        dashInput = playerInput.actions["Dash"].ReadValue<float>();

        if (dashInput > 0)
        {
            dashRequested = true;
        }
    }

    void OnInteract()
    {

        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange);
        foreach (var hit in hits)
        {


            if (hit.CompareTag("NPC")) 
            {

                Renderer rend = hit.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = npcHighlightColor;
                }
                return;
            }

            
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
        if (isDashing) return; // Prevent movement while dashing

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

        // Handle jumping
        if (jumpInput > 0)
        {
            if (IsGrounded() && canJump)
            {
                rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
                jumpInput = 0; // Reset jump input after applying jump
                StartCoroutine(JumpCooldown());
            }
        }

        // Handle dashing
        if (dashRequested)
        {
            if (canDash && !isDashing)
            {
                StartCoroutine(Dash());
            }
            dashRequested = false;
        }
    }

    // Check if the player is grounded using a raycast
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    // Cooldown coroutine for jumping
    IEnumerator JumpCooldown()
    {
        canJump = false;
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    // Dash coroutine
    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // Use move input for dash direction
        Vector3 dashDirection = new Vector3(-moveInput.x, 0f, -moveInput.y);

        // If no movement input, use facing direction
        if (dashDirection.sqrMagnitude < 0.01f)
        {
            dashDirection = transform.forward;
        }
        dashDirection.Normalize();

        // Apply dash by setting velocity 
        float preservedY = rb.linearVelocity.y;
        rb.linearVelocity = new Vector3(dashDirection.x * dashSpeed, preservedY, dashDirection.z * dashSpeed);

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
