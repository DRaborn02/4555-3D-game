using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public NavMeshAgent navAgent;
    public Animator animator;
    public Transform targetPlayer;
    public LayerMask groundLayer, playerLayer;
    public PlayerHealth playerHealth;

    private Vector3 walkPoint;
    private bool walkPointSet;
    private bool canAttack;
    private bool wasAttacked;
    private float walkPointRange;
    private float sightRange;
    private float attackRange;
    private float enemyHealth;
    private float attackCooldown;
    private float damageCooldown;
    private int damageTaken;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        targetPlayer = GameObject.FindWithTag("Player").transform;
        navAgent = GetComponent<NavMeshAgent>();
    }

    void handleAnimation()
    {
        bool Walk = animator.GetBool("Walk");
        bool Run = animator.GetBool("Run");
        bool Attack = animator.GetBool("Attack");
    }

    void Update()
    {
        bool playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerLayer);
        bool playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        if (!playerInSightRange && !playerInAttackRange)
        {
            IdleWalking();
        }
        else if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }
        else if (playerInAttackRange && playerInSightRange)
        {
            AttackPlayer();
        }
        else if (!playerInSightRange && wasAttacked)
        {
            ChasePlayer();
        }
    }

    private void IdleWalking()
    {
        if (!walkPointSet)
        {
            SearchWalkPoint();
        }

        if (walkPointSet)
        {
            navAgent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        animator.SetFloat("Velocity", 0.2f);

        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, groundLayer))
        {
            walkPointSet = true;
        }
    }
    private void ChasePlayer()
    {
        navAgent.SetDestination(targetPlayer.position);
        animator.SetFloat("Velocity", 0.6f);
        navAgent.isStopped = false; 
    }
    private void AttackPlayer()
    {
        navAgent.SetDestination(transform.position);

        if (!wasAttacked)
        {
            transform.LookAt(targetPlayer.position);
            canAttack = true;
            animator.SetBool("Attack", true);
            StartCoroutine(AttackCooldown());

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
            {
                playerHealth.TakeDamage(1);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        enemyHealth -= damage;
        StartCoroutine(DamageCooldown());

        if (enemyHealth <= 0)
        {
            Invoke(nameof(DestroyEnemy), 0.5f);
        }
    }

    private void DestroyEnemy()
    {
        StartCoroutine(EnemyDeath());
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
        animator.SetBool("Dead", true);
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}

