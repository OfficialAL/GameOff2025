using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

/// <summary>
/// Main network manager for WebGL + PUN2 setup
/// Handles connection, room creation/joining, and basic networking
/// </summary>
public class NetworkManager : MonoBehaviourPunPV, IConnectionCallbacks, IMatchmakingCallbacks
{
    [Header("Connection Settings")]
    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private byte maxPlayersPerRoom = 4;
    
    [Header("UI References")]
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRandomButton;
    [SerializeField] private InputField roomNameInput;
    [SerializeField] private InputField playerNameInput;
    
    [Header("Status")]
    [SerializeField] private Text statusText;
    [SerializeField] private Text roomInfoText;

    void Start()
    {
        // Set up PUN2
        PhotonNetwork.AutomaticallySyncScene = true;
        
        // Set default player name
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);
            if (playerNameInput != null)
                playerNameInput.text = PhotonNetwork.NickName;
        }
        
        SetupUI();
        UpdateStatus("Ready to connect");
    }

    void SetupUI()
    {
        // Setup button listeners
        if (connectButton != null)
            connectButton.onClick.AddListener(ConnectToPhoton);
        
        if (createRoomButton != null)
            createRoomButton.onClick.AddListener(CreateRoom);
            
        if (joinRandomButton != null)
            joinRandomButton.onClick.AddListener(JoinRandomRoom);

        // Show appropriate panels
        ShowPanel(menuPanel);
    }

    public void ConnectToPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            UpdateStatus("Already connected!");
            return;
        }

        // Update player name if changed
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
        {
            PhotonNetwork.NickName = playerNameInput.text;
        }

        UpdateStatus("Connecting to Photon...");
        ShowPanel(connectingPanel);
        
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            UpdateStatus("Not connected to Photon!");
            return;
        }

        string roomName = roomNameInput != null && !string.IsNullOrEmpty(roomNameInput.text) 
            ? roomNameInput.text 
            : "Room_" + Random.Range(1000, 9999);

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true
        };

        UpdateStatus($"Creating room: {roomName}");
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinRandomRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            UpdateStatus("Not connected to Photon!");
            return;
        }

        UpdateStatus("Looking for available room...");
        PhotonNetwork.JoinRandomRoom();
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            UpdateStatus("Leaving room...");
        }
    }

    void ShowPanel(GameObject panel)
    {
        // Hide all panels
        if (connectingPanel != null) connectingPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        
        // Show target panel
        if (panel != null) panel.SetActive(true);
    }

    void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
        
        Debug.Log($"[NetworkManager] {message}");
    }

    void UpdateRoomInfo()
    {
        if (roomInfoText != null && PhotonNetwork.InRoom)
        {
            roomInfoText.text = $"Room: {PhotonNetwork.CurrentRoom.Name}\nPlayers: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        }
    }

    #region Photon Callbacks

    public void OnConnectedToMaster()
    {
        UpdateStatus("Connected to Photon!");
        ShowPanel(menuPanel);
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        UpdateStatus($"Disconnected: {cause}");
        ShowPanel(menuPanel);
    }

    public void OnJoinedRoom()
    {
        UpdateStatus($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
        ShowPanel(lobbyPanel);
        UpdateRoomInfo();
    }

    public void OnLeftRoom()
    {
        UpdateStatus("Left room");
        ShowPanel(menuPanel);
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        UpdateStatus($"Create room failed: {message}");
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        UpdateStatus($"Join room failed: {message}");
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        UpdateStatus("No rooms available, creating new room...");
        CreateRoom();
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateStatus($"{newPlayer.NickName} joined the room");
        UpdateRoomInfo();
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateStatus($"{otherPlayer.NickName} left the room");
        UpdateRoomInfo();
    }

    #endregion

    #region Unused Callbacks
    public void OnConnected() { }
    public void OnRegionListReceived(RegionHandler regionHandler) { }
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string debugMessage) { }
    public void OnCreatedRoom() { }
    public void OnJoinedLobby() { }
    public void OnLeftLobby() { }
    public void OnRoomListUpdate(List<RoomInfo> roomList) { }
    #endregion
}