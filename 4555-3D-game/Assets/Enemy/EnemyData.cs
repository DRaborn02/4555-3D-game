using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    public float health = 10f;
    public float attackCooldown = 1.5f;
    public float damageCooldown = 1.0f;

    [Header("Ranges")]
    public float attackRange = 1.5f;
    public float sightRange = 8f;
    public float patrolRange = 10f;

    [Header("Movement")]
    public float navSpeed = 3.5f;
    public float rotationSpeed = 5f;

    [Header("Animation")]
    public RuntimeAnimatorController animatorController;
}
