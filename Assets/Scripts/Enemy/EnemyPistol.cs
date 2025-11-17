using UnityEngine;

/// <summary>
/// Pirate Pistol variant. Overrides attack to be ranged.
/// </summary>
public class EnemyPistol : EnemyAI
{
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 2f;
    private float lastAttackTime;

    protected override float GetAttackRange()
    {
        return attackRange;
    }

    protected override void PerformAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            Debug.Log("Pistol Pirate fires!");
            // TODO: Fire projectile at targetPlayer
        }
    }
}