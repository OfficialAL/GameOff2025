using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;

/// <summary>
/// Manages the Game Over Screen (Screen 10).
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI reasonText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI portsText;
    [SerializeField] private TextMeshProUGUI treasureText;

    public void Initialize(string reason, float distance, int ports, int treasure)
    {
        reasonText.text = reason;
        distanceText.text = $"Distance Travelled: {distance:F0}";
        portsText.text = $"Ports Visited: {ports}";
        treasureText.text = $"Treasure Collected: {treasure}";
    }

    public void OnReturnToMenu()
    {
        // Disconnect from Photon and return to Main Menu
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
    
    // Photon callback
    private void OnLeftRoom()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
    
    void Start()
    {
        // Hook into Photon callback
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}