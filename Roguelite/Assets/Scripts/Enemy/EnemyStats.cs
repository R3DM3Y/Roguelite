using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Game/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Health")]
    public int maxHealth = 3;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Detection")]
    public float detectionRadius = 5f;

    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 1;
    public float verticalAttackTolerance = 1f;

    [Header("Behaviour")]
    public bool canPatrol = true;
    public bool stopIfPlayerAbove = true;
    public float stopAboveHeight = 1.5f;

    [Header("Flip")]
    public float flipRadius = 0.5f;
    
    public enum MovementType
    {
        Ground,
        Flying
    }

    [Header("Movement Type")]
    public MovementType movementType = MovementType.Ground;
    
    public float flyingDashMultiplier = 3f;
    public float flyingHoverDistance = 2f;
}