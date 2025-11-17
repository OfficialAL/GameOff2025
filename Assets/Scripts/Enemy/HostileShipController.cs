using UnityEngine;
using Photon.Pun;
using PirateCoop;
using System.Collections.Generic;

/// <summary>
/// Manages an enemy vessel (Pirate Ship, Raft, Barrel).
/// Handles movement, health, and spawning enemies.
/// </summary>
public class HostileShipController : MonoBehaviourPun, IPunObservable
{
    public HostileShipState State { get; private set; }

    [SerializeField] private HostileShipType shipType;
    [SerializeField] private int maxHealth = 500;
    
    // For Spawning
    [SerializeField] private List<GameObject> enemyPrefabs; // e.g., PirateCutlass, PiratePistol
    [SerializeField] private Transform boardingPlankVisual;
    [SerializeField] private Transform grapplingHookVisual;

    private Transform playerShip;
    private bool isBoarding = false;
    private float spawnTimer;

    void Awake()
    {
        State = new HostileShipState
        {
            HostileShip_ID = photonView.ViewID.ToString(),
            Enemy_Type = shipType,
            Current_Health = maxHealth
            // ... initialize enemy list
        };
    }

    void Start()
    {
        playerShip = ShipController.Instance.transform;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return; // Only host controls AI

        // --- Approach Behavior ---
        float distance = Vector2.Distance(transform.position, playerShip.position);
        if (distance > 10f) // 10f is boarding range
        {
            // Move towards player ship
            Vector2 direction = (playerShip.position - transform.position).normalized;
            transform.position += (Vector3)direction * 3f * Time.deltaTime; // 3f is approach speed
        }
        else
        {
            // --- Boarding Behavior ---
            if (!isBoarding)
            {
                StartBoarding();
            }
            HandleEnemySpawning();
        }
    }

    private void StartBoarding()
    {
        isBoarding = true;
        photonView.RPC("RPC_ShowBoardingVisuals", RpcTarget.All, true);
        
        // Spawn 50% of enemies at once
        int initialSpawn = State.Carrying_Enemies.Count / 2;
        for (int i = 0; i < initialSpawn; i++)
        {
            SpawnEnemy();
        }
    }
    
    private void HandleEnemySpawning()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0 && State.Carrying_Enemies.Count > 0)
        {
            spawnTimer = Random.Range(3f, 6f); // 1 every 3-6s
            SpawnEnemy();
        }
        
        // TODO: Handle ship leaving when empty
    }
    
    private void SpawnEnemy()
    {
        // TODO: Get enemy type from State
        GameObject enemyToSpawn = enemyPrefabs[0]; // Simple for now
        Vector3 spawnPos = transform.position; // Spawn on deck
        
        PhotonNetwork.Instantiate(enemyToSpawn.name, spawnPos, Quaternion.identity);
    }
    
    [PunRPC]
    public void RPC_ShowBoardingVisuals(bool show)
    {
        if (shipType == HostileShipType.PirateShip)
        {
            if(boardingPlankVisual) boardingPlankVisual.gameObject.SetActive(show);
        }
        else
        {
            if(grapplingHookVisual) grapplingHookVisual.gameObject.SetActive(show);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Sync health
        if (stream.IsWriting)
        {
            stream.SendNext(State.Current_Health);
        }
        else
        {
            State.Current_Health = (int)stream.ReceiveNext();
        }
    }
    
    // TODO: Add RPC_TakeDamage for cannonballs
}