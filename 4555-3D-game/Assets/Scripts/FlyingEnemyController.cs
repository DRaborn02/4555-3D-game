using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject hurtbox;
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Animator animator;

    [Header("Flight Settings")]
    [SerializeField] private float flightHeight = 2.5f;
    [SerializeField] private float hoverRadius = 3f;
    [SerializeField] private float hoverSpeed = 2f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float sightRange = 6f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float enemyHealth = 5f;

    private Transform targetPlayer;
    private Transform orbitTarget; // usually a demon
    private Vector3 wanderTarget;
    private float orbitAngle;
    private bool canAttack = true;
    private bool playerInSightRange = false, playerInAttackRange = false;

    private enum State { Searching, Hovering, Chasing, Attacking }
    private State currentState;

    private EnemyHealth enemyHealthScript;

    void Start()
    {
        enemyHealthScript = GetComponent<EnemyHealth>();

        if (hurtbox != null) hurtbox.SetActive(false);
        ApplyData();

        FindNearestPlayer();
        currentState = State.Searching;
        StartCoroutine(StartDelayed());
    }

    void Update()
    {
        if (enemyHealthScript != null && enemyHealthScript.IsInvulnerable)
        {
            print("Invulnerable");
            // Enemy is currently invulnerable from taking damage
            currentState = State.Hovering;
            animator.SetBool("Attack", false);
            return;
        }

        if (targetPlayer == null)
            FindNearestPlayer();

        MaintainHeightAboveGround();
    }

    private void ApplyData()
    {
        if (enemyData == null) return;

        attackRange = enemyData.attackRange;
        sightRange = enemyData.sightRange;
        attackCooldown = enemyData.attackCooldown;
        enemyHealth = enemyData.health;
    }

    private IEnumerator StartDelayed()
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.5f)); // random delay so they don't all sync up
        StartCoroutine(BehaviorLoop());
    }


    private IEnumerator BehaviorLoop()
    {
        while (true)
        {
            UpdateAwareness();

            switch (currentState)
            {
                case State.Searching:
                    FindOrbitTarget();
                    if (playerInSightRange)
                        currentState = State.Chasing;
                    else if (orbitTarget != null)
                        currentState = State.Hovering;
                    else
                        Wander();
                    break;

                case State.Hovering:
                    HoverAroundTarget();
                    if (playerInSightRange)
                        currentState = State.Chasing;
                    else if (orbitTarget == null)
                        currentState = State.Searching;
                    break;

                case State.Chasing:
                    ChasePlayer();
                    if (playerInAttackRange)
                        currentState = State.Attacking;
                    else if (!playerInSightRange)
                        currentState = State.Searching;
                    break;

                case State.Attacking:
                    if (canAttack)
                        StartCoroutine(AttackSequence());
                    if (!playerInAttackRange)
                        currentState = State.Chasing;
                    break;
            }

            yield return null;
        }
    }

    private void UpdateAwareness()
    {
        if (targetPlayer == null) return;

        float dist = Vector3.Distance(transform.position, targetPlayer.position);
        playerInSightRange = dist < sightRange;
        playerInAttackRange = dist < attackRange;
    }

    private void FindOrbitTarget()
    {
        // Only search for ground enemies (demons)
        EnemyController[] groundEnemies = FindObjectsOfType<EnemyController>();
        float minDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var e in groundEnemies)
        {
            if (e == null) continue;
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = e.transform;
            }
        }

        orbitTarget = closest;
    }

    private void HoverAroundTarget()
    {
        if (orbitTarget == null)
        {
            currentState = State.Searching;
            return;
        }

        orbitAngle += hoverSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Cos(orbitAngle), 0, Mathf.Sin(orbitAngle)) * hoverRadius;
        Vector3 desiredPos = orbitTarget.position + offset + Vector3.up * flightHeight;

        MoveToward(desiredPos);
        FaceTarget(orbitTarget.position);
        animator.SetBool("Fly", true);
    }

    private void Wander()
    {
        // Occasionally pick a new random wander point
        if (wanderTarget == Vector3.zero || Vector3.Distance(transform.position, wanderTarget) < 0.5f || Random.value < 0.01f)
        {
            Vector3 randomOffset = Random.insideUnitSphere * hoverRadius * 2f;
            randomOffset.y = 0;
            wanderTarget = transform.position + randomOffset;
        }

        Vector3 desiredPos = wanderTarget + Vector3.up * flightHeight;
        MoveToward(desiredPos);
        animator.SetBool("Fly", true);
    }

    private void ChasePlayer()
    {
        if (targetPlayer == null)
        {
            currentState = State.Searching;
            return;
        }

        Vector3 desiredPos = targetPlayer.position + Vector3.up * flightHeight;

        if (enemyHealthScript != null && enemyHealthScript.IsInvulnerable)
        {

        }
        else
        {
            MoveToward(desiredPos);
        }
            
        FaceTarget(targetPlayer.position);
        animator.SetBool("Fly", true);
    }

    private IEnumerator AttackSequence()
    {
        canAttack = false;
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.4f);
        if (enemyHealthScript != null && enemyHealthScript.IsInvulnerable)
        {
            if (hurtbox != null) hurtbox.SetActive(false);
        }
        else
        { 
        if (hurtbox != null) hurtbox.SetActive(true);

        yield return new WaitForSeconds(0.2f);
        if (hurtbox != null) hurtbox.SetActive(false);

        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void MoveToward(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    [SerializeField] LayerMask groundMask;      // Assign your ground layer in Inspector

    private void MaintainHeightAboveGround()
    {
        // Cast straight down to find the ground
        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f, groundMask))
        {
            // Clamp position so it doesn’t rise higher than flightHeight above the ground
            float desiredY = hit.point.y + flightHeight;
            Vector3 pos = transform.position;
            pos.y = Mathf.Min(pos.y, desiredY);
            transform.position = pos;
        }
    }



    private void FaceTarget(Vector3 target)
    {
        Vector3 lookDir = target - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    private void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = p.transform;
            }
        }

        if (closest != null)
            targetPlayer = closest;
    }

    public void TakeDamage(float dmg)
    {
        enemyHealth -= dmg;
        if (enemyHealth <= 0)
        {
            animator.SetTrigger("Dead");
            Destroy(gameObject, 2f);
        }
    }
}
