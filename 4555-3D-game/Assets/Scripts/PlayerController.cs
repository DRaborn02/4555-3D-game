using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;
using System.Collections;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public Animator animator;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] float rotationSpeed = 10.0f;
    [SerializeField] private float interactRange = 2f;
    [SerializeField] float jumpHeight = 5.0f;
    [SerializeField] float jumpCooldown = 1.0f;
    [SerializeField] float dashSpeed = 10.0f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] float dashCooldown = 2.0f;
    [Space]

    [Header("NPC Settings")]
    [SerializeField] private Color npcHighlightColor = Color.white;
    [Space]

    [Header("Health Settings")]
    [SerializeField] private int maxQuarterHearts = 12; // 12 = 3 full hearts

    private Collider[] results = new Collider[5]; // buffer for overlap checks

    private Vector2 moveInput;
    private Rigidbody rb;
    private Inventory inventory;
    [SerializeField] private Item item;
    private Item currentlyEquippedItem;
    private GameObject currentlyEquippedObject;
    private GameObject heldItemPrefab;
    private float jumpInput;
    private bool canJump = true;
    private float dashInput;
    private bool dashRequested = false;
    private bool canDash = true;
    private bool isDashing = false;
    private float attackInput;
    private float weaponDamage;
    private float attackCooldown;
    private bool canAttack = true;

    private float secondaryAttackInput;
    private float secondaryWeaponDamage;
    private float secondaryCooldown;
    private bool canSecondaryAttack = true;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inventory = GetComponent<Inventory>();
    }

    void Start()
    {
        var input = GetComponent<PlayerInput>();
        var health = GetComponent<PlayerHealth>();
        health.setMaxHealth(maxQuarterHearts);
        PlayerUIManager.Instance.AssignUI(inventory, health, input.playerIndex);
        if (CameraFollow.Instance != null)
            CameraFollow.Instance.RegisterPlayer(transform);
    }

    void OnDestroy()
    {
        // Clean up if player leaves
        if (CameraFollow.Instance != null)
            CameraFollow.Instance.UnregisterPlayer(transform);
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

    public void SetEquippedItem(Item newItem, GameObject instance)
    {
        if (currentlyEquippedObject != null && currentlyEquippedObject != instance)
        {
            Destroy(currentlyEquippedObject);
        }

        currentlyEquippedItem = newItem;
        item = newItem; 
        currentlyEquippedObject = instance;

        Debug.Log("Equipped item: " + (currentlyEquippedItem != null ? currentlyEquippedItem.itemName : "none"));

        if (currentlyEquippedItem is Weapon weapon)
        {
            Debug.Log("Equipped weapon: " + weapon.itemName);
            weaponDamage = weapon.damage;
            attackCooldown = weapon.cooldown;
            secondaryWeaponDamage = weapon.secondaryDamage;
            secondaryCooldown = weapon.secondaryCooldown;
            heldItemPrefab = weapon.prefab;
        }
        else
        {
            // Clear weapon stats if unequipped
            weaponDamage = 0f;
            attackCooldown = 0f;
            secondaryWeaponDamage = 0f;
            secondaryCooldown = 0f;
            heldItemPrefab = null;
        }
    }

    public void OnAttack()
    {
        var playerInput = GetComponent<PlayerInput>();
        attackInput = playerInput.actions["Attack"].ReadValue<float>();
        //Debug.Log("Attack was pressed");
    }

    public void OnSecondaryAttack()
    {
        var playerInput = GetComponent<PlayerInput>();
        secondaryAttackInput = playerInput.actions["SecondaryAttack"].ReadValue<float>();
        //Debug.Log("Secondary Attack was pressed");
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

        if (attackInput > 0)
        {
            if (currentlyEquippedItem is Weapon weapon && canAttack)
            {
                Debug.Log("Attacking with " + weapon.itemName + " of type " + weapon.type);

                // Implement attack logic based on weapon type
                if (weapon.type == Weapon.WeaponType.LightMelee)
                {
                    // Light melee attack logic
                    Debug.Log("Performing light melee attack.");
                    LightMeleeAttack();
                }
                else if (weapon.type == Weapon.WeaponType.HeavyMelee)
                {
                    // Heavy melee attack logic
                    Debug.Log("Performing heavy melee attack.");
                    HeavyMeleeAttack();
                }
                else if (weapon.type == Weapon.WeaponType.Ranged)
                {
                    // Ranged attack logic
                    Debug.Log("Performing ranged attack.");
                    RangedAttack();
                }
                attackInput = 0; // Reset attack input after processing
                StartCoroutine(AttackCooldown());
            }
            else
            {
                canAttack = false;
            }
        }

        if (secondaryAttackInput > 0)
        {
            if (currentlyEquippedItem is Weapon weapon && canSecondaryAttack)
            {
                Debug.Log("Performing secondary attack with " + weapon.itemName);

                // Implement secondary attack logic based on weapon type
                if (weapon.type == Weapon.WeaponType.LightMelee)
                {
                    // Light melee secondary attack logic
                    Debug.Log("Performing light melee secondary attack.");
                    SecondaryLMAttack();
                }
                else if (weapon.type == Weapon.WeaponType.HeavyMelee)
                {
                    // Heavy melee secondary attack logic
                    Debug.Log("Performing heavy melee secondary attack.");
                    SecondaryHMAttack();
                }
                else if (weapon.type == Weapon.WeaponType.Ranged)
                {
                    // Ranged secondary attack logic
                    Debug.Log("Performing ranged secondary attack.");
                    SecondaryRangedAttack();
                }
                secondaryAttackInput = 0; // Reset secondary attack input after processing
                StartCoroutine(SecondaryCooldown());
            }
            else
            {
                canSecondaryAttack = false;
            }
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

    private void LightMeleeAttack()
    {
        //animator.SetTrigger("LightMeleeAttack");
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 2.0f);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // Apply damage to enemies within range
                Debug.Log("Hit " + enemy.name);
                Debug.Log("Dealt " + weaponDamage + " damage to " + enemy.name);
            }
        }
    }

    private void HeavyMeleeAttack()
    {
       //animator.SetTrigger("HeavyMeleeAttack");
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 2.5f);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // Apply damage to enemies within range
                Debug.Log("Hit " + enemy.name);
                 Debug.Log("Dealt " + weaponDamage + " damage to " + enemy.name);
            }
        }
    }

    private void RangedAttack()
    {
        //animator.SetTrigger("RangedAttack");
        Debug.Log("Fired a projectile.");
        // Implement projectile logic here
        // Deal 'weaponDamage' to enemy
    }

    private void SecondaryLMAttack()
    {
        //animator.SetTrigger("SecondaryLMAttack");
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 2.5f);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // Apply damage to enemies within range
                Debug.Log("Hit " + enemy.name);
                 Debug.Log("Dealt " + weaponDamage + " damage to " + enemy.name);
            }
        }
    }

    private void SecondaryHMAttack()
    {
        //animator.SetTrigger("SecondaryHMAttack");
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 3.0f);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // Apply damage to enemies within range
                Debug.Log("Hit " + enemy.name);
                 Debug.Log("Dealt " + weaponDamage + " damage to " + enemy.name);
            }
        }
    }

    private void SecondaryRangedAttack()
    {
        //animator.SetTrigger("SecondaryRangedAttack");
        Debug.Log("Fired a special projectile.");
        // Implement special projectile logic here
        // Deal 'secondaryDamage' to enemy
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    IEnumerator SecondaryCooldown()
    {
        canSecondaryAttack = false;
        yield return new WaitForSeconds(secondaryCooldown);
        canSecondaryAttack = true;
    }
}
