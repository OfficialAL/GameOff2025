using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/// <summary>
/// Handles the logic for creating and joining Photon rooms. 
/// This script lives in the MainMenuScene and is called by UI buttons.
/// </summary>
public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField joinLobbyCodeField; [cite: 28]
    [SerializeField] private TextMeshProUGUI errorMessageText;

    private const string LOBBY_SCENE_NAME = "LobbyScene"; // 

    private void Start()
    {
        // Ensure error message is clear at start
        if (errorMessageText) errorMessageText.text = "";
    }

    /// <summary>
    /// Called by the "Create Lobby" button. [cite: 29]
    /// </summary>
    public void OnCreateLobby()
    {
        if (!IsPlayerNameValid()) return;
        
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4, // 3-4 players, so max is 4 
            IsVisible = true,
            IsOpen = true
        };
        
        // Using null for room name tells Photon to generate a unique code
        PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default); [cite: 30]
    }

    /// <summary>
    /// Called by the "Join Lobby" button. [cite: 29]
    /// </summary>
    public void OnJoinLobby()
    {
        if (!IsPlayerNameValid()) return;

        string lobbyCode = joinLobbyCodeField.text;
        if (string.IsNullOrEmpty(lobbyCode))
        {
            ShowError("Lobby Code cannot be empty.");
            return;
        }

        PhotonNetwork.JoinRoom(lobbyCode); [cite: 30]
    }

    private bool IsPlayerNameValid()
    {
        if (string.IsNullOrEmpty(NetworkManager.Instance.PlayerDisplayName))
        {
            ShowError("Player Display Name cannot be empty.");
            return false;
        }
        // Name is set in NetworkManager via MainMenu.cs
        PhotonNetwork.NickName = NetworkManager.Instance.PlayerDisplayName;
        return true;
    }

    // --- Photon Callbacks ---

    public override void OnJoinedRoom()
    {
        // Success! Transition all players to the Lobby Scene.
        Debug.Log($"Successfully joined room: {PhotonNetwork.CurrentRoom.Name}");
        PhotonNetwork.LoadLevel(LOBBY_SCENE_NAME); // 
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        ShowError($"Create Room Failed: {message}"); [cite: 31]
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        ShowError($"Join Room Failed: {message}"); // [cite: 31]
        // Common errors: 32765 (RoomFull), 32758 (GameDoesNotExist)
    }

    private void ShowError(string message)
    {
        if (errorMessageText)
        {
            errorMessageText.text = $"Error: {message}";
            Debug.LogError(message);
        }
    }
}