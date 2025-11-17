using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the Main Menu UI (Screen 2).
/// Handles opening and closing modals for Host, Join, Controls, and Credits. [cite: 21]
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Modals")]
    [SerializeField] private GameObject hostModal; [cite: 28]
    [SerializeField] private GameObject joinModal; [cite: 28]
    [SerializeField] private GameObject controlsModal; [cite: 23]
    [SerializeField] private GameObject creditsModal; // [cite: 23]

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField hostDisplayNameInput; [cite: 28]
    [SerializeField] private TMP_InputField joinDisplayNameInput; [cite: 28]

    private void Start()
    {
        // Ensure all modals are closed on start
        CloseAllModals();
    }

    private void CloseAllModals()
    {
        if (hostModal) hostModal.SetActive(false);
        if (joinModal) joinModal.SetActive(false);
        if (controlsModal) controlsModal.SetActive(false);
        if (creditsModal) creditsModal.SetActive(false);
    }

    // --- Button Clicks [cite: 21] ---

    public void OnHostGameButton()
    {
        CloseAllModals();
        hostModal.SetActive(true);
    }

    public void OnJoinGameButton()
    {
        CloseAllModals();
        joinModal.SetActive(true);
    }

    public void OnControlsButton()
    {
        CloseAllModals();
        controlsModal.SetActive(true);
    }

    public void OnCreditsButton()
    {
        CloseAllModals();
        creditsModal.SetActive(true);
    }

    public void OnCloseModalButton()
    {
        CloseAllModals();
    }

    // --- Display Name Logic ---

    // Called by Host and Join input fields to update the persistent NetworkManager 
    public void OnHostNameChanged()
    {
        string validatedName = ValidateDisplayName(hostDisplayNameInput.text);
        hostDisplayNameInput.text = validatedName;
        if (NetworkManager.Instance)
        {
            NetworkManager.Instance.SetPlayerDisplayName(validatedName);
        }
    }

    public void OnJoinNameChanged()
    {
        string validatedName = ValidateDisplayName(joinDisplayNameInput.text);
        joinDisplayNameInput.text = validatedName;
        if (NetworkManager.Instance)
        {
            NetworkManager.Instance.SetPlayerDisplayName(validatedName);
        }
    }

    private string ValidateDisplayName(string name)
    {
        // Enforce 16 character limit 
        if (name.Length > 16)
        {
            name = name.Substring(0, 16);
        }
        return name;
    }
}