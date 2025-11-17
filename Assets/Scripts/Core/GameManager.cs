using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon; // For Hashtable
using PirateCoop;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the GameScene state machine, player spawning,
/// difficulty scaling, and game over conditions.
/// </summary>
public enum GameState { Sailing, Encounter, AtPort, GameOver }

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Loop")]
    public GameState CurrentState { get; private set; }
    [SerializeField] private float distanceToPort = 1000f;
    private float currentTravelProgress = 0f;
    private float totalDistanceSailed = 0f;
    private int portsVisited = 0;
    
    [Header("Spawning")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyShipPrefab; // For encounters
    
    [Header("UI")]
    [SerializeField] private GameObject gameOverScreenPrefab;

    private List<PlayerController> players = new List<PlayerController>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();
            
            if (PhotonNetwork.IsMasterClient)
            {
                SetGameState(GameState.Sailing);
            }
        }
    }

    private void SpawnPlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        // Note: We need a way to register players. This is a simple way.
        // A better way would be the player registering itself with the GameManager.
    }
    
    // Called by PlayerController.Start()
    public void RegisterPlayer(PlayerController player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
        }
    }
    
    // --- Game State Machine ---

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return; // Only host runs the game loop
        
        CheckForGameOver();

        switch (CurrentState)
        {
            case GameState.Sailing:
                UpdateSailing();
                break;
            case GameState.Encounter:
                UpdateEncounter();
                break;
            case GameState.AtPort:
                // Wait for players to be ready to leave
                // (e.g., host interacts with Mast)
                break;
        }
    }
    
    private void UpdateSailing()
    {
        // If ship is moving, increase travel progress
        if (ShipController.Instance.ShipState.Ship_Sail == ShipSailState.Lowered)
        {
            float speed = ShipController.Instance.ShipState.Ship_Speed;
            currentTravelProgress += speed * Time.deltaTime;
            totalDistanceSailed += speed * Time.deltaTime;

            // Trigger random encounter
            if (Random.value < 0.001f * GetDifficultyScalar()) // Random chance
            {
                StartEncounter();
            }

            // Check if reached port
            if (currentTravelProgress >= distanceToPort)
            {
                ArriveAtPort();
            }
        }
    }
    
    private void StartEncounter()
    {
        SetGameState(GameState.Encounter);
        
        // Spawn an enemy ship
        PhotonNetwork.Instantiate(enemyShipPrefab.name, new Vector3(50, 0, 0), Quaternion.identity);
    }
    
    private void UpdateEncounter()
    {
        // Check if encounter is over
        // (e.g., all enemy ships/enemies are defeated)
        // if (FindObjectsOfType<EnemyAI>().Length == 0 && FindObjectsOfType<HostileShipController>().Length == 0)
        // {
        //    SetGameState(GameState.Sailing);
        // }
    }
    
    private void ArriveAtPort()
    {
        SetGameState(GameState.AtPort);
        portsVisited++;
        currentTravelProgress = 0;
        
        // Force sails up
        ShipController.Instance.SetSailState(ShipSailState.Raised);

        //Set host status for port room, and share.
        if (PhotonNetwork.IsMasterClient)
        {
            SharedShipState shipState = ShipController.Instance.ShipState;
            List<string> unownedUpgrades = new List<string>();
            if (!shipState.Ship_Upgrades["reinforced"]) unownedUpgrades.Add("reinforced");
            if (!shipState.Ship_Upgrades["beds"]) unownedUpgrades.Add("beds");
            if (!shipState.Ship_Upgrades["cannons"]) unownedUpgrades.Add("cannons");

            string upgradeKey = "";
            if (unownedUpgrades.Count > 0)
            {
                upgradeKey = unownedUpgrades[Random.Range(0, unownedUpgrades.Count)];
            }

            Hashtable roomProps = new Hashtable();
            roomProps.Add("CurrentPortUpgrade", upgradeKey); // Add or overwrite
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        }
    }

    public void SetGameState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"Game state changed to: {newState}");
        // TODO: Sync state to clients if needed
    }
    
    public float GetDifficultyScalar()
    {
        // Simple scaling: 10% harder for every 1000 units sailed
        return 1.0f + (totalDistanceSailed / 1000f) * 0.1f;
    }

    // --- Game Over ---
    
    private void CheckForGameOver()
    {
        if (CurrentState == GameState.GameOver) return;
        
        // 1. All players unconscious
        bool allDown = true;
        foreach (PlayerController player in players)
        {
            if (player != null && !player.State.Is_Unconscious)
            {
                allDown = false;
                break;
            }
        }
        if (allDown && players.Count > 0)
        {
            TriggerGameOver("All players are unconscious!");
            return;
        }
        
        // 2. Ship sinks (TODO: Implement in ShipController)
        // if (ShipController.Instance.IsSunk())
        // {
        //    TriggerGameOver("The ship has sunk!");
        //    return;
        // }
    }
    
    public void TriggerGameOver(string reason)
    {
        SetGameState(GameState.GameOver);
        photonView.RPC("RPC_ShowGameOver", RpcTarget.All, reason, totalDistanceSailed, portsVisited);
    }
    
    [PunRPC]
    public void RPC_ShowGameOver(string reason, float distance, int ports)
    {
        GameObject go = Instantiate(gameOverScreenPrefab);
        go.GetComponent<GameOverScreen>().Initialize(reason, distance, ports, ShipController.Instance.ShipState.Shared_Treasure_Count);
    }
    
    // 3. Host disconnect
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer.IsMasterClient)
        {
            // Host disconnected
            TriggerGameOver("The host has disconnected.");
        }
    }
}