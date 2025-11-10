using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Main game manager for the multiplayer pirate sailing game
/// Handles game state, session management, and coordination between systems
/// </summary>
public class GameManager : NetworkBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private float gameSessionTime = 300f; // 5 minutes default
    [SerializeField] private Transform[] playerSpawnPoints;
    [SerializeField] private GameObject playerPrefab;

    [Header("Ship Settings")]
    [SerializeField] private GameObject shipPrefab;
    [SerializeField] private Transform shipSpawnPoint;

    public enum GameState
    {
        WaitingForPlayers,
        Starting,
        InProgress,
        GameOver
    }

    private NetworkVariable<GameState> currentGameState = new NetworkVariable<GameState>();
    private NetworkVariable<float> gameTimer = new NetworkVariable<float>();
    private NetworkVariable<int> connectedPlayers = new NetworkVariable<int>();

    private GameObject spawnedShip;

    public System.Action<GameState> OnGameStateChanged;
    public System.Action<float> OnGameTimerUpdated;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentGameState.Value = GameState.WaitingForPlayers;
            gameTimer.Value = gameSessionTime;

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        currentGameState.OnValueChanged += OnGameStateUpdated;
        gameTimer.OnValueChanged += OnTimerUpdated;
    }

    void Update()
    {
        if (!IsServer) return;

        UpdateGameLogic();
    }

    void UpdateGameLogic()
    {
        switch (currentGameState.Value)
        {
            case GameState.WaitingForPlayers:
                // Check if we have enough players to start
                if (connectedPlayers.Value >= 2) // Minimum 2 players
                {
                    StartGame();
                }
                // Check if we've reached max players
                else if (connectedPlayers.Value >= maxPlayers)
                {
                    Debug.Log($"Maximum players ({maxPlayers}) reached. Starting game.");
                    StartGame();
                }
                break;

            case GameState.InProgress:
                // Update game timer
                gameTimer.Value -= Time.deltaTime;
                
                if (gameTimer.Value <= 0)
                {
                    EndGame();
                }
                break;
        }
    }

    void OnClientConnected(ulong clientId)
    {
        connectedPlayers.Value++;
        
        Debug.Log($"Player connected. Total players: {connectedPlayers.Value}");

        // Spawn player
        SpawnPlayer(clientId);
    }

    void OnClientDisconnected(ulong clientId)
    {
        connectedPlayers.Value--;
        
        Debug.Log($"Player disconnected. Total players: {connectedPlayers.Value}");

        // Handle player cleanup
        HandlePlayerDisconnection(clientId);
    }

    void SpawnPlayer(ulong clientId)
    {
        if (playerPrefab == null || playerSpawnPoints.Length == 0) return;

        // Find available spawn point
        Transform spawnPoint = GetAvailableSpawnPoint();
        if (spawnPoint == null) spawnPoint = playerSpawnPoints[0]; // Fallback

        // Spawn player
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    Transform GetAvailableSpawnPoint()
    {
        // Simple implementation - return first spawn point
        // In a real game, you'd check for occupied spawn points
        return playerSpawnPoints[0];
    }

    void StartGame()
    {
        currentGameState.Value = GameState.Starting;

        // Spawn the ship if not already spawned
        if (spawnedShip == null && shipPrefab != null)
        {
            spawnedShip = Instantiate(shipPrefab, shipSpawnPoint.position, shipSpawnPoint.rotation);
            spawnedShip.GetComponent<NetworkObject>().Spawn();
        }

        // Wait a moment then transition to in progress
        Invoke(nameof(TransitionToInProgress), 3f);
    }

    void TransitionToInProgress()
    {
        currentGameState.Value = GameState.InProgress;
        NotifyGameStartedClientRpc();
    }

    void EndGame()
    {
        currentGameState.Value = GameState.GameOver;
        NotifyGameEndedClientRpc();
    }

    [ClientRpc]
    void NotifyGameStartedClientRpc()
    {
        Debug.Log("Game Started! Set sail, ye pirates!");
        // Update UI, play sounds, etc.
    }

    [ClientRpc]
    void NotifyGameEndedClientRpc()
    {
        Debug.Log("Game Over! Time's up!");
        // Show end game UI, calculate scores, etc.
    }

    void HandlePlayerDisconnection(ulong clientId)
    {
        // Handle what happens when a player leaves
        // Maybe transfer their role to another player, etc.
    }

    void OnGameStateUpdated(GameState oldState, GameState newState)
    {
        OnGameStateChanged?.Invoke(newState);
        Debug.Log($"Game state changed: {oldState} -> {newState}");
    }

    void OnTimerUpdated(float oldTime, float newTime)
    {
        OnGameTimerUpdated?.Invoke(newTime);
    }

    // Public getters
    public GameState GetGameState() => currentGameState.Value;
    public float GetGameTime() => gameTimer.Value;
    public int GetPlayerCount() => connectedPlayers.Value;

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}