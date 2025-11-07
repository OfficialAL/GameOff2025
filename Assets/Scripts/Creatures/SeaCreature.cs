using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Basic sea creature AI that can attack ships
/// Inspired by Barotrauma's creature system but lighter/more arcade-style
/// </summary>
public class SeaCreature : NetworkBehaviour
{
    [Header("Creature Settings")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float aggroRange = 10f;

    [Header("Behavior")]
    [SerializeField] private CreatureType creatureType = CreatureType.Aggressive;
    [SerializeField] private bool avoidsLight = false;
    [SerializeField] private float wanderRadius = 5f;

    private enum CreatureType
    {
        Passive,      // Doesn't attack unless provoked
        Aggressive,   // Attacks ships on sight
        Territorial   // Attacks if ship enters territory
    }

    private enum CreatureState
    {
        Wandering,
        Chasing,
        Attacking,
        Fleeing
    }

    private Rigidbody2D rb;
    private NetworkVariable<CreatureState> currentState = new NetworkVariable<CreatureState>();
    private NetworkVariable<Vector2> targetPosition = new NetworkVariable<Vector2>();
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>();
    
    private Transform targetShip;
    private float lastAttackTime;
    private Vector2 wanderCenter;

    public System.Action<float> OnHealthChanged;
    public System.Action OnCreatureDied;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wanderCenter = transform.position;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = health;
            currentState.Value = CreatureState.Wandering;
        }

        currentHealth.OnValueChanged += OnHealthUpdated;
    }

    void Update()
    {
        if (!IsServer) return;

        UpdateAI();
    }

    void UpdateAI()
    {
        switch (currentState.Value)
        {
            case CreatureState.Wandering:
                HandleWandering();
                CheckForThreats();
                break;

            case CreatureState.Chasing:
                HandleChasing();
                break;

            case CreatureState.Attacking:
                HandleAttacking();
                break;

            case CreatureState.Fleeing:
                HandleFleeing();
                break;
        }
    }

    void HandleWandering()
    {
        // Simple wandering behavior
        if (Vector2.Distance(transform.position, targetPosition.Value) < 1f)
        {
            // Pick new random point within wander radius
            Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
            targetPosition.Value = wanderCenter + randomDirection;
        }

        MoveTowards(targetPosition.Value, moveSpeed * 0.5f);
    }

    void CheckForThreats()
    {
        // Look for nearby ships
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, aggroRange);
        
        foreach (var collider in nearbyObjects)
        {
            ShipHealth ship = collider.GetComponent<ShipHealth>();
            if (ship != null)
            {
                // React based on creature type
                if (creatureType == CreatureType.Aggressive || 
                    (creatureType == CreatureType.Territorial && Vector2.Distance(transform.position, wanderCenter) < wanderRadius))
                {
                    targetShip = ship.transform;
                    currentState.Value = CreatureState.Chasing;
                    break;
                }
            }
        }
    }

    void HandleChasing()
    {
        if (targetShip == null)
        {
            currentState.Value = CreatureState.Wandering;
            return;
        }

        float distanceToShip = Vector2.Distance(transform.position, targetShip.position);

        if (distanceToShip > aggroRange * 1.5f)
        {
            // Lost target
            targetShip = null;
            currentState.Value = CreatureState.Wandering;
        }
        else if (distanceToShip <= attackRange)
        {
            currentState.Value = CreatureState.Attacking;
        }
        else
        {
            MoveTowards(targetShip.position, moveSpeed);
        }
    }

    void HandleAttacking()
    {
        if (targetShip == null)
        {
            currentState.Value = CreatureState.Wandering;
            return;
        }

        float distanceToShip = Vector2.Distance(transform.position, targetShip.position);

        if (distanceToShip > attackRange)
        {
            currentState.Value = CreatureState.Chasing;
        }
        else if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackShip();
            lastAttackTime = Time.time;
        }
    }

    void HandleFleeing()
    {
        // Move away from threats
        if (targetShip != null)
        {
            Vector2 fleeDirection = (transform.position - targetShip.position).normalized;
            Vector2 fleeTarget = (Vector2)transform.position + fleeDirection * 10f;
            MoveTowards(fleeTarget, moveSpeed * 1.5f);

            // Stop fleeing after getting far enough away
            if (Vector2.Distance(transform.position, targetShip.position) > aggroRange * 2f)
            {
                currentState.Value = CreatureState.Wandering;
                targetShip = null;
            }
        }
    }

    void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * speed;

        // Rotate to face movement direction
        if (direction.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void AttackShip()
    {
        if (targetShip == null) return;

        ShipHealth shipHealth = targetShip.GetComponent<ShipHealth>();
        if (shipHealth != null)
        {
            // Randomly damage a zone
            int randomZone = Random.Range(0, 6); // Assuming 6 damage zones
            shipHealth.TakeDamageServerRpc(randomZone, attackDamage);

            // Trigger attack effects
            TriggerAttackEffectsClientRpc();
        }
    }

    [ClientRpc]
    void TriggerAttackEffectsClientRpc()
    {
        // Play attack animation, sound effects, particle effects, etc.
        Debug.Log($"{gameObject.name} attacks the ship!");
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, ServerRpcParams rpcParams = default)
    {
        currentHealth.Value = Mathf.Max(0, currentHealth.Value - damage);

        if (currentHealth.Value <= 0)
        {
            Die();
        }
        else if (creatureType == CreatureType.Passive)
        {
            // Passive creatures flee when attacked
            currentState.Value = CreatureState.Fleeing;
        }
    }

    void Die()
    {
        OnCreatureDied?.Invoke();
        
        // Trigger death effects
        TriggerDeathEffectsClientRpc();
        
        // Despawn after a short delay
        Invoke(nameof(DespawnCreature), 2f);
    }

    [ClientRpc]
    void TriggerDeathEffectsClientRpc()
    {
        // Death animation, sound, particles, etc.
        Debug.Log($"{gameObject.name} has been defeated!");
    }

    void DespawnCreature()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    void OnHealthUpdated(float oldHealth, float newHealth)
    {
        OnHealthChanged?.Invoke(newHealth);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(wanderCenter, wanderRadius);
    }
}