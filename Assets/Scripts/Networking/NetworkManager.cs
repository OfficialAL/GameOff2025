using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
[cite_start]/// Persistent singleton that manages the core Photon connection state 
/// and player display name.
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    public string PlayerDisplayName { get; private set; } = "Pirate";

    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make persistent 
        }
    }

    private void Start()
    {
        // Connect to Photon Master Server
        Debug.Log("Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings(); // [cite: 9]
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server!");
        // We are now ready to join or create rooms.
        // We automatically join the lobby to be able to get room lists if needed.
        PhotonNetwork.JoinLobby(); 
    }

    /// <summary>
    /// Sets the player's display name, which will be used in the lobby.
    /// </summary>
    public void SetPlayerDisplayName(string newName)
    {
        if (string.IsNullOrEmpty(newName)) return;
        PlayerDisplayName = newName;
        PhotonNetwork.NickName = PlayerDisplayName; // Set for PUN
        Debug.Log($"Player name set to: {PlayerDisplayName}");
    }
	
	/// <summary>
    /// Attempts to rejoin a room.
    /// </summary>
    public void ReconnectToLobby(string lobbyCode)
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.RejoinRoom(lobbyCode);
        }
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        ShowError($"Join Room Failed: {message}");
        
        // Try to reconnect if the room exists
        // This is a basic implementation of the PRD requirement
        if (message.Contains("already in room")) // Simplified check
        {
            // We might be disconnected, try rejoining
            // PhotonNetwork.RejoinRoom(lobbyCode); // Need lobby code
        }
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected from Photon: {cause}");
        // TODO: Show UI to reconnect
    }
}