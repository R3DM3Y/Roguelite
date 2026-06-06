using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int currentHealth;
    private float baseSpeed;
    private float baseCooldown;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float aggroRange = 14f;
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float tooCloseRange = 2f;
    private bool facingRight = true;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 2.5f;
    private bool canAttack = true;
    private bool isAttacking;

    [Header("Counter")]
    [SerializeField] private float counterChance = 0.4f;
    [SerializeField] private float counterCooldown = 4f;
    private bool canCounter = true;
    private int hitsSinceLastCounter;

    [Header("Dodge")]
    [SerializeField] private float dodgeChance = 0.3f;
    [SerializeField] private float dodgeCooldown = 3f;
    private bool canDodge = true;

    [Header("Loot")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinsToDrop = 15;
    [SerializeField] private float coinLaunchForce = 6f;
    [SerializeField] private Transform lootPoint;

    [Header("Damage FX")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Transform headPoint;

    [Header("Attacks")]
    [SerializeField] private BossSpikeAttack spikeAttack;
    [SerializeField] private BossSummonAttack summonAttack;
    [SerializeField] private Transform spikeStartPoint;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    private PlayerController player;
    private Rigidbody2D rb;
    private Animator animator;
    
    private float lastAntiAirTime;
    private float antiAirCooldown = 5f;

    private enum BossState { Idle, Chase, Attack, Dodge, Stunned, Dead }
    private BossState state;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    // =============================================
    // UNITY
    // =============================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = FindFirstObjectByType<PlayerController>();

        state = BossState.Idle;
        currentHealth = maxHealth;
        baseSpeed = moveSpeed;
        baseCooldown = attackCooldown;
    }

    private void Update()
    {
        if (player == null || state == BossState.Dead || state == BossState.Stunned) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);
        float distX = Mathf.Abs(transform.position.x - player.transform.position.x);
        float distY = player.transform.position.y - transform.position.y;

        UpdatePhases();
        UpdateState(dist, distX, distY);
    }

    // =============================================
    // STATE MACHINE
    // =============================================

    private float aggroTime;
    private bool justAggrod;

    private void UpdateState(float dist, float distX, float distY)
    {
        if (distY > 2.5f)
        {
            Stop();
            if (Time.time - lastAntiAirTime > antiAirCooldown)
            {
                lastAntiAirTime = Time.time;
                summonAttack.SummonFlyingEnemies();
            }
            return;
        }

        if (distY < -5f)
        {
            Stop();
            return;
        }

        switch (state)
        {
            case BossState.Idle:
                if (dist < aggroRange)
                {
                    state = BossState.Chase;
                    aggroTime = Time.time;
                    justAggrod = true;
                }
                break;

            case BossState.Chase:
                if (isAttacking) break;

                // Задержка перед первой атакой — босс просыпается
                if (justAggrod && Time.time - aggroTime < 1.5f)
                {
                    Face(Mathf.Sign(player.transform.position.x - transform.position.x));
                    Stop();
                    return;
                }
                justAggrod = false;

                Chase(distX, distY);
                break;

            case BossState.Attack:
                Stop();
                break;

            case BossState.Dodge:
                break;
        }
    }

    private void Chase(float distX, float distY)
    {
        float dirToPlayer = Mathf.Sign(player.transform.position.x - transform.position.x);

        // Игрок в зоне атаки
        if (distX <= attackRange && Mathf.Abs(distY) < 3f)
        {
            Stop();
            if (canAttack)
                StartCoroutine(AttackRoutine());
            return;
        }

        // Игрок слишком близко — отходим
        if (distX < tooCloseRange)
        {
            float awayDir = Mathf.Sign(transform.position.x - player.transform.position.x);
    
            if (IsGroundAhead(awayDir))
            {
                Face(-dirToPlayer);
                rb.linearVelocity = new Vector2(awayDir * moveSpeed * 0.8f, rb.linearVelocity.y);
                animator.SetBool("Walk", true);
                return;
            }
            else
            {
                // Не можем отойти — атакуем вблизи
                Stop();
                if (canAttack)
                    StartCoroutine(AttackRoutine());
                return;
            }
        }

        // Идём к игроку
        Face(dirToPlayer);
        if (IsGroundAhead(dirToPlayer))
        {
            rb.linearVelocity = new Vector2(dirToPlayer * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            Stop();
        }

        animator.SetBool("Walk", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
    }

    // =============================================
    // ATTACK
    // =============================================

    private IEnumerator AttackRoutine()
    {
        state = BossState.Attack;
        isAttacking = true;
        canAttack = false;
        Stop();

        // Телеграф — показываем что сейчас будет атака
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.5f);

        // Выполняем атаку
        PerformAttack();

        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
        state = BossState.Chase;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void PerformAttack()
    {
        float hpPercent = (float)currentHealth / maxHealth;

        // Ярость: двойные шипы + призыв
        if (hpPercent <= 0.25f)
        {
            CastSpikeAttack();
            Invoke(nameof(CastSummonAttack), 0.4f);
            return;
        }

        // Комбо: шипы + призыв
        if (hpPercent <= 0.5f)
        {
            if (Random.value < 0.6f)
            {
                CastSpikeAttack();
                Invoke(nameof(CastSummonAttack), 0.3f);
            }
            else
            {
                CastSpikeAttack();
            }
            return;
        }

        // Базовая атака
        if (Random.value < 0.6f)
            CastSpikeAttack();
        else
            CastSummonAttack();
    }

    private bool spikeLaunching;

    private void CastSpikeAttack()
    {
        if (spikeLaunching) return;
        if (spikeAttack == null) return;
    
        spikeLaunching = true;
        spikeAttack.Launch(spikeStartPoint.position);
    
        // Сбрасываем через время достаточное для завершения анимации шипов
        StartCoroutine(ResetSpikeFlag());
    }

    private IEnumerator ResetSpikeFlag()
    {
        yield return new WaitForSeconds(1f);
        spikeLaunching = false;
    }

    private void CastSummonAttack()
    {
        if (summonAttack == null) return;
        summonAttack.SummonEnemies();
    }

    // =============================================
    // COUNTER
    // =============================================

    private void TryCounter()
    {
        if (isAttacking) return; // ← уже атакует
    
        hitsSinceLastCounter++;

        if (!canCounter) return;

        float chance = counterChance + hitsSinceLastCounter * 0.15f;

        if (Random.value < chance)
        {
            StartCoroutine(CounterAttack());
        }
    }

    private IEnumerator CounterAttack()
    {
        canCounter = false;
        isAttacking = true; // ← блокируем другие атаки
        hitsSinceLastCounter = 0;

        yield return new WaitForSeconds(0.15f);

        int dir = player.transform.position.x > transform.position.x ? 1 : -1;
        Face(dir);
        CastSpikeAttack();

        yield return new WaitForSeconds(0.3f);
        isAttacking = false; // ← разблокируем

        yield return new WaitForSeconds(counterCooldown);
        canCounter = true;
    }

    // =============================================
    // DODGE
    // =============================================

    private IEnumerator DodgeBack()
    {
        state = BossState.Dodge;
        canDodge = false;

        float dir = Mathf.Sign(transform.position.x - player.transform.position.x);

        rb.linearVelocity = new Vector2(dir * moveSpeed * 1.5f, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.25f);

        Stop();
        state = BossState.Chase;

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    // =============================================
    // PHASES
    // =============================================

    private void UpdatePhases()
    {
        float hpPercent = (float)currentHealth / maxHealth;

        if (hpPercent <= 0.66f)
        {
            moveSpeed = baseSpeed * 1.2f;
            attackCooldown = baseCooldown * 0.85f;
            dodgeChance = 0.4f;
            counterChance = 0.5f;
        }

        if (hpPercent <= 0.33f)
        {
            moveSpeed = baseSpeed * 1.5f;
            attackCooldown = baseCooldown * 0.6f;
            dodgeChance = 0.5f;
            counterChance = 0.6f;
        }

        if (hpPercent <= 0.25f)
        {
            moveSpeed = baseSpeed * 1.8f;
            attackCooldown = baseCooldown * 0.35f;
            dodgeChance = 0.6f;
            counterChance = 0.7f;
        }
    }

    // =============================================
    // DAMAGE
    // =============================================

    public void TakeDamage(int dmg)
    {
        if (state == BossState.Dead) return;

        // Шанс увернуться
        if (state != BossState.Attack && canDodge && Random.value < dodgeChance)
        {
            StartCoroutine(DodgeBack());
            SpawnDamageText(0); // MISS
            return;
        }

        currentHealth -= dmg;
        SpawnDamageText(dmg);
        animator.SetTrigger("Hurt");

        TryCounter();

        // Стан на секунду при большом уроне
        if (dmg >= 10)
            StartCoroutine(StunRoutine(0.4f));

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator StunRoutine(float duration)
    {
        state = BossState.Stunned;
        yield return new WaitForSeconds(duration);
        if (state == BossState.Stunned)
            state = BossState.Chase;
    }

    private void SpawnDamageText(int dmg)
    {
        var obj = Instantiate(damageTextPrefab, headPoint.position, Quaternion.identity);
        obj.GetComponent<DamageText>().SetDamage(dmg);
    }

    // =============================================
    // DEATH
    // =============================================

    private void Die()
    {
        state = BossState.Dead;
        StopAllCoroutines();
        Stop();

        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        rb.simulated = false;
        animator.SetTrigger("Death");

        StartCoroutine(DropCoins());
    }

    public void OnDeathAnimationFinished()
    {
        Destroy(gameObject);
    }

    private IEnumerator DropCoins()
    {
        for (int i = 0; i < coinsToDrop; i++)
        {
            Vector3 pos = lootPoint.position + Random.insideUnitSphere * 0.5f;
            pos.z = 0;

            var coin = Instantiate(coinPrefab, pos, Quaternion.identity);
            var coinRb = coin.GetComponent<Rigidbody2D>();
            if (coinRb != null)
                coinRb.AddForce(Vector2.up * coinLaunchForce + (Vector2)Random.insideUnitSphere * 2f, ForceMode2D.Impulse);

            yield return new WaitForSeconds(0.04f);
        }
    }

    // =============================================
    // HELPERS
    // =============================================

    private void Stop()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("Walk", false);
    }

    private void Face(float dir)
    {
        if (dir > 0 && !facingRight) Flip();
        if (dir < 0 && facingRight) Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    private bool IsGroundAhead(float dir)
    {
        if (groundCheck == null) return true;
        Vector2 pos = (Vector2)groundCheck.position + Vector2.right * (dir * 0.5f);
        return Physics2D.OverlapBox(pos, new Vector2(0.4f, 0.1f), 0f, groundLayer);
    }
}