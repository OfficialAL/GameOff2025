using UnityEngine;
using Photon.Pun;
using PirateCoop;
using UnityEngine.AI; // Assuming NavMesh for pathfinding

/// <summary>
/// Base class for all enemies that board the player ship.
/// Handles health, movement, and taking damage.
/// </summary>
[RequireComponent(typeof(PhotonView), typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviourPun, IPunObservable
{
    public EnemyState State { get; private set; }

    [SerializeField] private EnemyType enemyType;
    [SerializeField] private int maxHealth = 10;
    
    protected NavMeshAgent agent;
    protected Transform targetPlayer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        State = new EnemyState
        {
            Enemy_ID = photonView.ViewID.ToString(),
            Enemy_Type = enemyType,
            Current_Health = maxHealth
        };
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            // Only host controls AI
            agent.enabled = false;
            return;
        }

        if (State.Current_Health <= 0) return;

        FindNearestPlayer();
        
        if (targetPlayer != null)
        {
            agent.SetDestination(targetPlayer.position);
            
            // Check distance to attack
            float distance = Vector2.Distance(transform.position, targetPlayer.position);
            if (distance < GetAttackRange())
            {
                PerformAttack();
            }
        }
    }

    private void FindNearestPlayer()
    {
        // TODO: Improve this to query a GameManager list
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        float closestDist = float.MaxValue;
        targetPlayer = null;

        foreach (PlayerController player in players)
        {
            if (!player.State.Is_Unconscious)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    targetPlayer = player.transform;
                }
            }
        }
    }

    // --- Virtual methods for subclasses ---
    
    protected virtual float GetAttackRange()
    {
        return 1.5f; // Default melee range
    }

    protected virtual void PerformAttack()
    {
        // Base melee attack
        Debug.Log("Enemy attacks!");
        // TODO: Implement attack cooldown
    }
    
    // --- Health and Damage ---

    [PunRPC]
    public void RPC_TakeDamage(int amount)
    {
        if (State.Current_Health <= 0) return;

        State.Current_Health -= amount;
        if (State.Current_Health <= 0)
        {
            Die();
        }
    }
	
	[Header("Loot")]
    [SerializeField] private GameObject droppedItemPrefab; // e.g., DroppedLumber.prefab
    [SerializeField] private DroppedItemType lootType = DroppedItemType.Treasure;
    [SerializeField] [Range(0, 1)] private float lootDropChance = 0.4f; // 40%

    private void Die()
    {
        Debug.Log("Enemy died!");
        
        if (PhotonNetwork.IsMasterClient)
        {
            // --- Implement Loot Drops ---
            if (droppedItemPrefab != null && Random.value <= lootDropChance)
            {
                // TODO: Need to configure prefab based on loot table
                PhotonNetwork.Instantiate(droppedItemPrefab.name, transform.position, Quaternion.identity);
            }
            
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(State.Current_Health);
        }
        else
        {
            State.Current_Health = (int)stream.ReceiveNext();
        }
    }
}