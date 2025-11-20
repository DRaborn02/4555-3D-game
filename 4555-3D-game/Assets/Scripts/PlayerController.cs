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
    [SerializeField] private float movementDeadzone = 0.05f;
    [Space]

    [Header("NPC Settings")]
    [SerializeField] private Color npcHighlightColor = Color.white;
    [Space]

    [Header("Health Settings")]
    [SerializeField] private int maxQuarterHearts = 12; // 12 = 3 full hearts

    private Collider[] results = new Collider[5]; // buffer for overlap checks

    [Header("Combat Settings")]
    [SerializeField] private GameObject hurtBoxPrefab; // assign in Inspector
    [SerializeField] private Transform attackSpawnPoint; // e.g. in front of player

    [SerializeField] private float meleeRange = 1.5f;
    [SerializeField] private float hurtBoxLifetime = 0.2f;
    [Space]
    
    [Header("Weapon Audio")]
    [SerializeField] private AudioClip lightMeleeSwoosh; // Assign in Inspector from Assets/Audio/SFX/
    [SerializeField] private AudioClip heavyMeleeSwoosh; // Assign in Inspector from Assets/Audio/SFX/
    [SerializeField] private AudioClip rangedAttackSound; // Assign in Inspector from Assets/Audio/SFX/
    [SerializeField] private AudioClip jumpSound; // Assign in Inspector from Assets/Audio/SFX/
    [SerializeField] private AudioClip dashSound; // Assign in Inspector from Assets/Audio/SFX/
    [SerializeField] private float weaponSoundVolume = 1f;

    private Vector2 moveInput;
    private Rigidbody rb;
    private Inventory inventory;
    [SerializeField] private Item item;
    public ItemInstance equippedItemInstance;
    public Item equippedItem => equippedItemInstance?.baseItem; // shortcut

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
    private float projectileSpeed;
    private float attackCooldown;
    private float attackRange;
    private bool canAttack = true;
    private GameObject projectilePrefab;
    private int healthGain;

    private float secondaryAttackInput;
    private float secondaryWeaponDamage;
    private float secondaryCooldown;
    private bool canSecondaryAttack = true;

    public Camera mainCamera;
    public LayerMask groundMask; // Assign your "Ground" layer in Inspector

    private int upperBodyLayer;
    private bool isGrounded = true;

    private PlayerInput playerInput;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inventory = GetComponent<Inventory>();
        
        // Auto-assign animator if not assigned in Inspector
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("PlayerController: No Animator component found! Please add an Animator component to the player GameObject.");
            }
        }
        
        if (animator != null)
        {
            upperBodyLayer = animator.GetLayerIndex("Upper Body");
        }

        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        var input = GetComponent<PlayerInput>();
        var health = GetComponent<PlayerHealth>();
        health.setMaxHealth(maxQuarterHearts);
        PlayerUIManager.Instance.AssignUI(inventory, health, input.playerIndex);
        if (CameraFollow.Instance != null)
            CameraFollow.Instance.RegisterPlayer(transform);
        if (CameraObstruction.Instance != null)
            CameraObstruction.Instance.RegisterPlayer(transform);

        if (mainCamera == null)
        {
            GameObject camObj = GameObject.FindWithTag("MainCamera");
            if (camObj != null)
                mainCamera = camObj.GetComponent<Camera>();
        }
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
        isGrounded = false;
        if (animator != null)
        {
            animator.SetBool("Jumping", true);
        }
        
        // Play jump sound
        if (jumpSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(jumpSound, weaponSoundVolume);
        }
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

    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
        if (animator != null)
        {
            animator.SetBool("Jumping", false);
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
                if (inventory != null && inventory.AddItem(pickup.GetItem(), pickup.getDurability()))
                {

                    pickup.Consume();
                }
                break; // stop after first pickup
            }
        }
    }

    public void SetEquippedItem(ItemInstance newItemInstance, GameObject instance)
    {
        // Destroy previous visual instance if needed
        if (currentlyEquippedObject != null && currentlyEquippedObject != instance)
        {
            Destroy(currentlyEquippedObject);
        }

        equippedItemInstance = newItemInstance;
        currentlyEquippedObject = instance;

        Item item = equippedItemInstance?.baseItem;

        if (item is Weapon weapon)
        {
            weaponDamage = weapon.damage;
            attackCooldown = weapon.cooldown;
            secondaryWeaponDamage = weapon.secondaryDamage;
            secondaryCooldown = weapon.secondaryCooldown;
            heldItemPrefab = weapon.prefab;
            attackRange = weapon.attackRange;

            projectileSpeed = weapon.projectileSpeed;
            projectilePrefab = weapon.projectilePrefab;

            //Debug.Log($"Equipped {weapon.itemName} with durability {equippedItemInstance.durability}/{weapon.maxDurability}");
        }
        else if (item is Consumable consumable)
        {
            healthGain = consumable.healthGain;
        }
        else
        {
            // Clear stats
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
        attackInput = 1;
        //print("test this out:");
        //print(playerInput.actions["Attack"].ReadValue<float>());
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

        // Apply deadzone to controller drift
        if (move.magnitude < movementDeadzone)
            move = Vector3.zero;

        // Animations
        if (animator != null)
        {
            if (move == Vector3.zero)
            {
                animator.SetBool("Moving", false);
            }
            else
            {
                animator.SetBool("Moving", true);
            }
        }

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
            var weapon = equippedItemInstance?.baseItem as Weapon;
            if (weapon != null && canAttack)
            {

                // Only rotate toward mouse if using Keyboard+Mouse
                if (playerInput.currentControlScheme == "Keyboard")
                {
                    RotateTowardsMouse();
                }

                if (weapon.type == Weapon.WeaponType.LightMelee)
                {
                    LightMeleeAttack();
                    // Reduce durability
                    equippedItemInstance.durability -= 1;
                    inventory.RefreshUI();
                }
                else if (weapon.type == Weapon.WeaponType.HeavyMelee)
                {
                    HeavyMeleeAttack();
                    // Reduce durability
                    equippedItemInstance.durability -= 1;
                    inventory.RefreshUI();
                }
                else if (weapon.type == Weapon.WeaponType.Ranged)
                {
                    RangedAttack();
                    // Reduce durability
                    equippedItemInstance.durability -= 1;
                    inventory.RefreshUI();
                }


                    //Debug.Log($"{weapon.itemName} durability: {equippedItemInstance.durability}/{weapon.maxDurability}");

                if (equippedItemInstance.durability <= 0)
                {
                    Debug.Log($"{weapon.itemName} broke!");
                    inventory.RemoveItem(weapon);
                    SetEquippedItem(null, null);
                }

                attackInput = 0;
                StartCoroutine(AttackCooldown());
            }

            else if (equippedItemInstance?.baseItem is Consumable consumable)
            {
                // Use consumable
                var health = GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.Heal(healthGain);
                    inventory.RemoveItem(consumable);
                    SetEquippedItem(null, null); // Unequip after use
                }
                attackInput = 0; // Reset attack input after processing
            }
            else             
            {
                attackInput = 0;
            }
        }

        //if (secondaryAttackInput > 0)
        //{
        //    if (currentlyEquippedItem is Weapon weapon && canSecondaryAttack)
        //    {
        //        //Debug.Log("Performing secondary attack with " + weapon.itemName);

        //        // Implement secondary attack logic based on weapon type
        //        if (weapon.type == Weapon.WeaponType.LightMelee)
        //        {
        //            // Light melee secondary attack logic
        //            //Debug.Log("Performing light melee secondary attack.");
        //            SecondaryLMAttack();
        //        }
        //        else if (weapon.type == Weapon.WeaponType.HeavyMelee)
        //        {
        //            // Heavy melee secondary attack logic
        //            //Debug.Log("Performing heavy melee secondary attack.");
        //            SecondaryHMAttack();
        //        }
        //        else if (weapon.type == Weapon.WeaponType.Ranged)
        //        {
        //            // Ranged secondary attack logic
        //            //Debug.Log("Performing ranged secondary attack.");
        //            SecondaryRangedAttack();
        //        }
        //        secondaryAttackInput = 0; // Reset secondary attack input after processing
        //        StartCoroutine(SecondaryCooldown());
        //    }
        //    else
        //    {
        //        canSecondaryAttack = false;
        //    }
        //}
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

        // Play dash sound
        if (dashSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(dashSound, weaponSoundVolume);
        }

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
        Weapon weapon = equippedItemInstance.baseItem as Weapon;
        if (weapon == null) return;

        if (projectilePrefab == null || attackSpawnPoint == null)
        {
            Debug.LogWarning("Missing projectile prefab or attack spawn point!");
            return;
        }

        // Play weapon swoosh sound
        if (lightMeleeSwoosh != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(lightMeleeSwoosh, weaponSoundVolume);
        }

        // Trigger animation and layer blending
        if (animator != null)
        {
            StartCoroutine(PulseLayerWeight(upperBodyLayer, 1f, 0.8f));
            animator.SetTrigger("Attack");
        }

        // Spawn at the attackSpawnPoint transform
        GameObject proj = Instantiate(
            projectilePrefab,
            attackSpawnPoint.position,
            attackSpawnPoint.rotation
        );

        // Make the hurtbox larger (3.5× the prefab’s default size)
        proj.transform.localScale *= attackRange;

        // Configure the HurtBox component
        var hb = proj.GetComponent<HurtBox>();
        hb.isProjectile = false; // stationary
        hb.direction = Vector3.zero;
        hb.damage = weapon.damage;
        hb.speed = 0f;
        hb.lifetime = hurtBoxLifetime;
        hb.owner = gameObject;

        // Make sure it doesn’t move or simulate physics
        hb.rb = proj.GetComponent<Rigidbody>();
        if (hb.rb != null)
            hb.rb.isKinematic = true;

        // Hide visuals (no mesh or particle effects)
        foreach (var mr in proj.GetComponentsInChildren<MeshRenderer>())
            mr.enabled = false;

        foreach (var ps in proj.GetComponentsInChildren<ParticleSystem>())
            ps.Stop();

        // Destroy after its lifetime ends
        Destroy(proj, hurtBoxLifetime);
    }




    private void HeavyMeleeAttack()
    {
        //print("Heavy Melee Attack Executed");
        
        // Play weapon swoosh sound
        if (heavyMeleeSwoosh != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(heavyMeleeSwoosh, weaponSoundVolume);
        }
        
        if (animator != null)
        {
            StartCoroutine(PulseLayerWeight(upperBodyLayer, 1f, 0.8f)); // fades in/out over 0.8 seconds total
            animator.SetTrigger("Attack");
            //animator.SetTrigger("HeavyMeleeAttack");
        }
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 2.5f);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // Apply damage to enemies within range
                //Debug.Log("Hit " + enemy.name);
                 //Debug.Log("Dealt " + weaponDamage + " damage to " + enemy.name);
            }
        }
    }

    private void RangedAttack()
    {
        Weapon weapon = equippedItemInstance.baseItem as Weapon;
        if (weapon == null) return;

        //animator.SetTrigger("RangedAttack");
        //Debug.Log("Fired a projectile.");
        
        // Play ranged attack sound
        if (rangedAttackSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(rangedAttackSound, weaponSoundVolume);
        }
        
        // Implement projectile logic here
        // Deal 'weaponDamage' to enemy
        GameObject proj = Instantiate(projectilePrefab, attackSpawnPoint.position, attackSpawnPoint.rotation);

        var hb = proj.GetComponent<HurtBox>();
        hb.isProjectile = true;
        hb.direction = attackSpawnPoint.forward;
        hb.damage = weapon.damage;
        hb.speed = weapon.projectileSpeed;
        hb.lifetime = 3f;
        hb.rb = proj.GetComponent<Rigidbody>();

        if (animator != null)
        {
            StartCoroutine(PulseLayerWeight(upperBodyLayer, 1f, 0.8f)); // fades in/out over 0.8 seconds total
            animator.SetTrigger("RangedAttack");
        }

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
                //Debug.Log("Hit " + enemy.name);
                 //Debug.Log("Dealt " + weaponDamage + " damage to " + enemy.name);
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
                //Debug.Log("Hit " + enemy.name);
                 //Debug.Log("Dealt " + weaponDamage + " damage to " + enemy.name);
            }
        }
    }

    private void SecondaryRangedAttack()
    {
        //animator.SetTrigger("SecondaryRangedAttack");
        //Debug.Log("Fired a special projectile.");
        // Implement special projectile logic here
        // Deal 'secondaryDamage' to enemy
    }

    private void RotateTowardsMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask))
        {
            Vector3 lookPoint = hit.point;
            Vector3 direction = (lookPoint - transform.position);
            direction.y = 0f; // keep rotation flat on the ground
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f);
            }
        }
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

    IEnumerator PulseLayerWeight(int layerIndex, float peakWeight, float totalDuration, float fadeFraction = 0.25f)
    {
        if (animator == null) yield break;
        
        // fadeFraction = fraction of time spent fading in/out (e.g., 0.25 = 25% fade-in, 50% hold, 25% fade-out)
        float fadeDuration = totalDuration * fadeFraction;
        float holdDuration = totalDuration - (fadeDuration * 2f);
        float time = 0f;

        // --- Fade In ---
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            animator.SetLayerWeight(layerIndex, Mathf.Lerp(0f, peakWeight, t));
            yield return null;
        }

        animator.SetLayerWeight(layerIndex, peakWeight);

        // --- Hold ---
        yield return new WaitForSeconds(holdDuration);

        // --- Fade Out ---
        time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            animator.SetLayerWeight(layerIndex, Mathf.Lerp(peakWeight, 0f, t));
            yield return null;
        }

        animator.SetLayerWeight(layerIndex, 0f);
    }


}
