using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the Lobby Scene (Screen 6).
[cite_start]/// Displays players, lobby code, and handles game start logic. 
/// </summary>
public class LobbyMenu : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    [cite_start][SerializeField] private TextMeshProUGUI lobbyCodeText; [cite: 32]
    [cite_start][SerializeField] private Button startGameButton; [cite: 35]
    [cite_start][SerializeField] private Button leaveLobbyButton; [cite: 36]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerListPrefab; // A simple prefab with a TextMeshProUGUI

    private const string MAIN_MENU_SCENE = "MainMenuScene";
    private const string GAME_SCENE = "GameScene"; // [cite: 36]

    private void Start()
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            // Failsafe: if not in a room, return to menu
            Debug.LogWarning("Not in a room, returning to Main Menu.");
            SceneManager.LoadScene(MAIN_MENU_SCENE);
            return;
        }

        [cite_start]// Display Lobby Code 
        lobbyCodeText.text = $"Lobby Code: {PhotonNetwork.CurrentRoom.Name}";

        UpdatePlayerList();
        UpdateStartButton();
    }

    // --- Player List Management ---

    private void UpdatePlayerList()
    {
        // Clear existing list
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        [cite_start]// Re-populate list from Photon player list 
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerEntry = Instantiate(playerListPrefab, playerListContainer);
            TextMeshProUGUI playerText = playerEntry.GetComponentInChildren<TextMeshProUGUI>();
            
            string displayName = player.NickName;
            if (player.IsMasterClient)
            {
                displayName += " (Host)"; // Mark the host 
            }
            playerText.text = displayName;
        }
    }

    private void UpdateStartButton()
    {
        [cite_start]// Show "Start Game" button for host only 
        bool isHost = PhotonNetwork.IsMasterClient;
        startGameButton.gameObject.SetActive(isHost);

        if (isHost)
        {
            [cite_start]// Enable for 3-4 players 
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            bool canStart = (playerCount >= 3 && playerCount <= 4);
            startGameButton.interactable = canStart;

            // Note: PRD says 2-4 players, Implementation Plan says 3-4.
            [cite_start]// Going with Implementation Plan (3-4) 
            // bool canStart = (playerCount >= 3 && playerCount <= 4);
            
            [cite_start]// Self-correction: PRD (Screen 6) [cite: 438] says 2-4 players.
            [cite_start]// Implementation Plan (Task 1.3.5)  says 3-4 players.
            [cite_start]// Implementation Plan (Verification) [cite: 37] says 3 other players (total 4).
            [cite_start]// I'll stick to the 3-4 player rule from the sub-task.
        }
    }

    // --- Button Clicks ---

    public void OnClickStartGame()
    {
        [cite_start]// Host starts the game, loading GameScene for all players [cite: 36]
        if (PhotonNetwork.IsMasterClient)
        [csharp]
        {
            Debug.Log("Starting game...");
            PhotonNetwork.LoadLevel(GAME_SCENE);
        }
    }

    public void OnClickLeaveLobby()
    {
        [cite_start]// All players can leave [cite: 36]
        Debug.Log("Leaving lobby...");
        PhotonNetwork.LeaveRoom();
    }

    // --- Photon Callbacks ---

    public override void OnLeftRoom()
    {
        // When we leave, go back to main menu
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} entered.");
        UpdatePlayerList();
        UpdateStartButton();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} left.");
        UpdatePlayerList();
        UpdateStartButton();
        
        [cite_start]// Host leaving dissolves the lobby [cite: 36]
        // PUN handles this by default: if MasterClient leaves, another is assigned.
        // To strictly follow "Host leaving dissolves", we need custom logic.
        // For now, we'll rely on the OnLeftRoom() callback for all clients.
        // If the MasterClient leaves, other players will get OnMasterClientSwitched.
        // If we want to force-kick, the new MasterClient could check andPhotonNetwork.LeaveRoom().
        // For this MVP, we'll assume the room persists with a new host.
        //
        [cite_start]// REVISIT: The plan says "Host leaving dissolves the lobby"[cite: 36].
        // The simplest way to implement this:
        if (otherPlayer.IsMasterClient)
        {
            Debug.Log("Host left, dissolving lobby.");
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // If the original host leaves, the new host (MasterClient) will
        // see the "Start Game" button.
        UpdatePlayerList();
        UpdateStartButton();
    }
}