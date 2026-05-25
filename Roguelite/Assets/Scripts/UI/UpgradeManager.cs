using System.Collections.Generic;
using UI;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    private PlayerController player;

    [Header("RARITY CHANCES")]
    public int commonWeight = 75;
    public int rareWeight = 15;
    public int epicWeight = 7;
    public int legendaryWeight = 3;

    [Header("UPGRADE BASE VALUES")]
    public int swordDamageBase = 1;
    public int airDamageBase = 1;
    public int airBounceBase = 1;

    public int hpBase = 2;
    public float speedBase = 0.1f;

    public float shieldDefenseBase = 0.02f;
    public float shieldEfficiencyBase = 0.5f;

    [Header("LEVELS")]
    private int airBounceLevel;
    private int swordDamageLevel;
    private int airDamageLevel;

    private int hpLevel;
    private int speedLevel;

    private int shieldLevel;
    private int shieldEfficiencyLevel;

    private int jumpLevel;
    private bool dashUnlocked;

    [Header("MAX LEVELS")]
    public int maxAirBounceLevel = 5;
    public int maxSwordDamageLevel = 10;
    public int maxAirDamageLevel = 10;

    public int maxHpLevel = 10;
    public int maxSpeedLevel = 5;

    public int maxShieldLevel = 5;
    public int maxShieldEfficiencyLevel = 5;

    public int maxJumpLevel = 2;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
    }

    // ---------------- RARITY ----------------
    private UpgradeRarity RollRarity()
    {
        int total = commonWeight + rareWeight + epicWeight + legendaryWeight;
        int roll = Random.Range(0, total);

        if (roll < commonWeight) return UpgradeRarity.Common;
        roll -= commonWeight;

        if (roll < rareWeight) return UpgradeRarity.Rare;
        roll -= rareWeight;

        if (roll < epicWeight) return UpgradeRarity.Epic;

        return UpgradeRarity.Legendary;
    }

    // ---------------- MAIN ----------------
    public void ApplyRandomUpgrade()
    {
        UpgradeRarity rarity = RollRarity();

        List<UpgradeType> pool = new List<UpgradeType>();

        switch (rarity)
        {
            case UpgradeRarity.Common:
                pool.Add(UpgradeType.SwordDamage);
                pool.Add(UpgradeType.Speed);
                pool.Add(UpgradeType.MaxHealth);
                break;

            case UpgradeRarity.Rare:
                pool.Add(UpgradeType.AirAttackDamage);
                pool.Add(UpgradeType.ShieldDefense);
                break;

            case UpgradeRarity.Epic:
                pool.Add(UpgradeType.AirBounce);
                pool.Add(UpgradeType.ShieldEfficiency);
                break;

            case UpgradeRarity.Legendary:
                pool.Add(UpgradeType.DoubleJump);
                pool.Add(UpgradeType.Dash);
                break;
        }

        UpgradeType upgrade = pool[Random.Range(0, pool.Count)];
        ApplyUpgrade(upgrade, rarity);
    }

    // ---------------- APPLY ----------------
    public void ApplyUpgrade(UpgradeType type, UpgradeRarity rarity)
    {
        float mult = rarity switch
        {
            UpgradeRarity.Rare => 1.2f,
            UpgradeRarity.Epic => 1.5f,
            UpgradeRarity.Legendary => 2.2f,
            _ => 1f
        };

        string color = GetRarityColor(rarity);

        switch (type)
        {
            case UpgradeType.SwordDamage:
                swordDamageLevel++;

                int sword = Mathf.RoundToInt(swordDamageBase * mult);
                player.bonusDamage += sword;

                UpgradePopupUI.Instance.Show(
                    $"<color={color}>Sword Damage {ToRoman(swordDamageLevel)}"
                );
                break;

            case UpgradeType.AirAttackDamage:
                airDamageLevel++;

                int air = Mathf.RoundToInt(airDamageBase * mult);
                player.bonusAirDamage += air;

                UpgradePopupUI.Instance.Show(
                    $"<color={color}>Air Damage {ToRoman(airDamageLevel)}"
                );
                break;

            case UpgradeType.AirBounce:
                airBounceLevel++;

                int bounce = Mathf.RoundToInt(airBounceBase * mult);
                player.bonusBounceForce += bounce;

                UpgradePopupUI.Instance.Show(
                    $"<color={color}>Air Bounce {ToRoman(airBounceLevel)}"
                );
                break;

            case UpgradeType.MaxHealth:
                hpLevel++;

                int hp = Mathf.RoundToInt(hpBase * mult);
                player.bonusHealth += hp;
                player.Heal(hp);

                UpgradePopupUI.Instance.Show(
                    $"<color={color}>Max HP {ToRoman(hpLevel)}"
                );
                break;

            case UpgradeType.Speed:
                speedLevel++;

                float sp = speedBase * mult;
                player.bonusSpeed += sp;

                UpgradePopupUI.Instance.Show(
                    $"<color={color}>Speed {ToRoman(speedLevel)}"
                );
                break;

            case UpgradeType.ShieldDefense:
                shieldLevel++;

                float sd = shieldDefenseBase * mult;
                player.bonusShieldReduction += sd;

                UpgradePopupUI.Instance.Show(
                    $"<color={color}>Shield Defense {ToRoman(shieldLevel)}"
                );
                break;

            case UpgradeType.ShieldEfficiency:
                shieldEfficiencyLevel++;

                float se = shieldEfficiencyBase * mult;
                player.bonusShieldDrainReduction += se;

                UpgradePopupUI.Instance.Show(
                    $"<color={color}>Shield Efficiency {ToRoman(shieldEfficiencyLevel)}"
                );
                break;

            case UpgradeType.DoubleJump:
                jumpLevel++;
                player.extraJumps += (rarity == UpgradeRarity.Legendary) ? 2 : 1;

                UpgradePopupUI.Instance.Show(
                    $"<color={color}>Extra Jump {ToRoman(jumpLevel)}</color>"
                );
                break;

            case UpgradeType.Dash:
                dashUnlocked = true;
                player.canDash = true;

                UpgradePopupUI.Instance.Show(
                    $"<color={color}>Shadow Dash</color>"
                );
                break;
        }
    }

    // ---------------- UTILS ----------------
    private string ToRoman(int n)
    {
        return n switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            6 => "VI",
            7 => "VII",
            8 => "VIII",
            9 => "IX",
            10 => "X",
            _ => n.ToString()
        };
    }

    private string GetRarityColor(UpgradeRarity r)
    {
        return r switch
        {
            UpgradeRarity.Common => "#FFFFFF",
            UpgradeRarity.Rare => "#4DA6FF",
            UpgradeRarity.Epic => "#C266FF",
            UpgradeRarity.Legendary => "#FFB347",
            _ => "#FFFFFF"
        };
    }
}