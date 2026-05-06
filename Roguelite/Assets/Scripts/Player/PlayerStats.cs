using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Game/Player Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Health")]
    public int maxHealth = 5;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Shield")]
    public float shieldMoveSpeedMultiplier = 0.4f;
    public float shieldDrainPerSecond = 20f;
    public float shieldHitDrain = 15f;
    public float damageReduction = 0.6f;

    [Header("Attack")]
    public int normalAttackDamage = 1;
    public int airDownAttackDamage = 1;
    public float airDownBounceForce = 6f;

    [Header("Knockback")]
    public float knockbackSideForce = 6f;
    public float knockbackUpForce = 8f;

    [Header("Invulnerability")]
    public float postHitInvulnerableTime = 1f;
    public float blinkInterval = 0.1f;
}