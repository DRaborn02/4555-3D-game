using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public NavMeshAgent navAgent;
    public RuntimeAnimatorController controller; // assign in inspector or load at runtime
    public Animator animator;
    public Transform targetPlayer;
    public PlayerHealth playerHealth;

    private Vector3 walkPoint;
    private bool walkPointSet;
    private float walkPointRange = 10f; // default patrol radius

    private bool canAttack;
    private bool wasAttacked;
    
    private float attackRange = 1.5f;
    private float sightRange = 3f;
    private bool playerInSightRange = false, playerInAttackRange = false;
    
    private float enemyHealth = 10f;
    private float attackCooldown = 1.5f;
    private float damageCooldown = 1.0f;

    private enum State
    {
        WaitingForPlayer,
        Patrolling,
        Chasing,
        Attacking
    }
    private State currentState;
    [SerializeField] private float rotationSpeed = 5f;
    [Header("Data")]
    [SerializeField] private EnemyData enemyData;

    void Awake()
    {
        // cache components - per instance
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        // find player once (or assign from a manager)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) targetPlayer = playerObj.transform;

        // ensure agent settings are set per-instance
        if (navAgent != null)
        {
            navAgent.updatePosition = true;
            navAgent.updateRotation = false; // we rotate in code
            navAgent.speed = Mathf.Max(navAgent.speed, 2f);
        }
    }

    void OnEnable()
    {
        // Initialize per-instance runtime state (do NOT write these back to the ScriptableObject)
        if (enemyData != null)
        {
            enemyHealth = enemyData.health;
            attackCooldown = enemyData.attackCooldown;
            damageCooldown = enemyData.damageCooldown;
            attackRange = enemyData.attackRange;
            sightRange = enemyData.sightRange;
            walkPointRange = enemyData.patrolRange;
            rotationSpeed = enemyData.rotationSpeed;
            if (navAgent != null) navAgent.speed = enemyData.navSpeed;
        }

        canAttack = true;
        wasAttacked = false;
        walkPointSet = false;
        currentState = State.WaitingForPlayer;

        StartCoroutine(CheckForPlayer()); // each instance runs its own coroutine
    }

    void OnDisable()
    {
        StopAllCoroutines();

        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.ResetPath();
        }
    }

    private void Start()
    {
        // If an EnemyData ScriptableObject is assigned, apply its values
        if (enemyData != null)
        {
            enemyHealth = enemyData.health;
            attackCooldown = enemyData.attackCooldown;
            damageCooldown = enemyData.damageCooldown;
            attackRange = enemyData.attackRange;
            sightRange = enemyData.sightRange;
            walkPointRange = enemyData.patrolRange;
            rotationSpeed = enemyData.rotationSpeed;
            if (navAgent != null) navAgent.speed = enemyData.navSpeed;
            controller = enemyData.animatorController ?? controller;
        }

        navAgent = GetComponent<NavMeshAgent>();

        // Ensure NavMeshAgent is enabled and set to update position/rotation
        if (navAgent != null)
        {
            navAgent.updatePosition = true;
            // We'll handle rotation smoothly in code to avoid spinning
            navAgent.updateRotation = false;
            navAgent.speed = Mathf.Max(navAgent.speed, 2f);
        }

        currentState = State.WaitingForPlayer;
        StartCoroutine(CheckForPlayer());
    }
    void Update()
    {
        if (currentState != State.WaitingForPlayer)
        {           
            playerInSightRange = false; playerInSightRange = false;
            //print("Distance between player and enemy: " + Vector3.Distance(transform.position, targetPlayer.position));
            if (Vector3.Distance(transform.position, targetPlayer.position) < sightRange) { playerInSightRange = true; }
            if (Vector3.Distance(transform.position, targetPlayer.position) < attackRange) { playerInAttackRange = true; }

            if (playerInSightRange && !playerInAttackRange)
            {
                currentState = State.Chasing;
                //Debug.Log("Player in sight range but not attack range. Chasing.");
                ChasePlayer();
            }
            else if (playerInAttackRange && playerInSightRange)
            {
                currentState = State.Attacking;
                //Debug.Log("Player in attack or sight range. Attacking.");
                AttackPlayer();
            }
            else if (!playerInSightRange && wasAttacked)
            {
                currentState = State.Chasing;
                //Debug.Log("Enemy attacked. Chasing.");
                ChasePlayer();
            }
            else if (!playerInAttackRange && !playerInSightRange)
            {
                currentState = State.Patrolling;
                //Debug.Log("Player not in attack or sight range. Patrolling.");
                Patrolling();
            }
        }

        // Smooth rotation to face movement direction if agent is moving
        if (navAgent != null && navAgent.enabled)
        {
            Vector3 desiredVel = navAgent.desiredVelocity;
            // If agent is moving, rotate toward velocity direction
            if (desiredVel.sqrMagnitude > 0.01f)
            {
                Vector3 lookDir = desiredVel.normalized;
                lookDir.y = 0f; // keep horizontal
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                }
            }
        }
    }

    private void Patrolling()
    {
        //Debug.Log("Patrolling starting.");
        if (!walkPointSet)
        {
            SearchWalkPoint();
        }

        if (walkPointSet && navAgent != null)
        {
            if (!navAgent.pathPending)
                navAgent.SetDestination(walkPoint);
                //Debug.Log("Moving to walk point at " + walkPoint);
        }
        // Ensure animator reflects patrolling state
        animator.SetBool("Walk", true);
        animator.SetBool("Run", false);
        animator.SetBool("Attack", false);
        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        // Choose a random point in a sphere around the enemy and sample the NavMesh
        Vector3 randomDirection = Random.insideUnitSphere * walkPointRange;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, walkPointRange, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
    }
    private void ChasePlayer()
    {
        //Debug.Log("Chasing starting.");
        animator.SetBool("Walk", false);
        animator.SetBool("Run", true);
        animator.SetBool("Attack", false);
        if (navAgent != null && targetPlayer != null)
            navAgent.SetDestination(targetPlayer.position);
    }
    private void AttackPlayer()
    {
        //Debug.Log("Attacking starting.");
        // Stop moving and face the player
        if (navAgent != null)
            navAgent.SetDestination(transform.position);

        // Look at player but keep upright (lock X/Z rotation)
        if (targetPlayer != null)
        {
            Vector3 lookPos = targetPlayer.position;
            lookPos.y = transform.position.y; // keep same Y to avoid leaning
            transform.LookAt(lookPos);
        }

        if (!wasAttacked && canAttack)
        {
            canAttack = false; // prevent immediate re-entry
            animator.SetBool("Attack", true);

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
            {
                //playerHealth.TakeDamage(1);
            }

            StartCoroutine(AttackCooldown());
        }
    }

    public void TakeDamage(float damage)
    {
        //Debug.Log("Enemy took damage: " + damage);
        enemyHealth -= damage;
        StartCoroutine(DamageCooldown());

        if (enemyHealth <= 0)
        {
            // Trigger death animation, then destroy in coroutine
            if (animator != null) animator.SetTrigger("Dead");
            Invoke(nameof(DestroyEnemy), 0.5f);
        }
    }

    private void DestroyEnemy()
    {
        //Debug.Log("Enemy destroyed.");
        StartCoroutine(EnemyDeath());
    }

    IEnumerator CheckForPlayer()
    {
        while (currentState == State.WaitingForPlayer)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                targetPlayer = playerObject.transform;
                currentState = State.Patrolling;
                //Debug.Log("Player detected. Starting patrol.");
            }

            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    IEnumerator DamageCooldown()
    {
        wasAttacked = true;
        yield return new WaitForSeconds(damageCooldown);
        wasAttacked = false;
    }

    IEnumerator EnemyDeath()
    {
        if (animator != null)
        {
            animator.SetTrigger("Dead");
        }
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
