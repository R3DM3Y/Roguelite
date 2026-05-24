using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Game/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    public enum MovementType
    {
        Ground,
        Flying
    }
    

    [Header("=== GENERAL ===")]

    public MovementType movementType = MovementType.Ground;

    public int maxHealth = 3;

    public float detectionRadius = 6f;

    public float moveSpeed = 2f;

    public float flipRadius = 0.5f;

    [Header("=== ATTACK ===")]

    public int contactDamage = 1;

    public float attackRange = 1.2f;

    public float attackCooldown = 1.5f;

    public int attackDamage = 1;

    public float verticalAttackTolerance = 1f;

    [Header("=== GROUND ENEMY ===")]

    public bool canPatrol = true;

    public float stopAboveHeight = 1.5f;
    
    public float aboveOffsetRange = 1.5f;

    public float patrolSpeed = 2f;
    
    public float maxUpVision = 3f;

    public float maxDownVision = 2f;

    [Header("=== FLYING ENEMY ===")]

    public float hoverRadius = 2.5f;

    public float approachSpeed = 4f;

    public float orbitSpeed = 2f;

    public float dashMultiplier = 3f;

    public float dashDuration = 0.35f;

    public float dashChance = 0.003f;

    public float orbitHeightVariation = 0.5f;

    public float returnToOrbitSpeed = 5f;
    
}