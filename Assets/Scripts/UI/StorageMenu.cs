using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PirateCoop;
using Photon.Pun;
using System.Collections.Generic;

/// <summary>
/// Manages the Storage Menu UI (Screen 9).
/// </summary>
public class StorageMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI lumberCountText;
    [SerializeField] private TextMeshProUGUI blunderbussCountText;
    [SerializeField] private Button withdrawLumberButton;
    [SerializeField] private Button withdrawBlunderbussButton;

    private PlayerController localPlayer;
    private SharedShipState shipState;

    public void Initialize(PlayerController player, SharedShipState state)
    {
        localPlayer = player;
        shipState = state;
        UpdateUI();
    }

    void Update()
    {
        // Continuously update UI in case state changes
        if (shipState != null)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        int lumber = shipState.Shared_Loot["lumber"];
        int blunder = shipState.Shared_Loot["blunderbuss"];

        lumberCountText.text = $"In Storage: {lumber}";
        blunderbussCountText.text = $"In Storage: {blunder}";

        withdrawLumberButton.interactable = (lumber > 0);
        withdrawBlunderbussButton.interactable = (blunder > 0);
    }

    public void OnWithdrawLumber()
    {
        WithdrawItem("lumber", CarriedItemType.Lumber);
    }

    public void OnWithdrawBlunderbuss()
    {
        WithdrawItem("blunderbuss", CarriedItemType.Blunderbuss);
    }

    private void WithdrawItem(string itemKey, CarriedItemType itemType)
    {
        // 1. Call RPC to update state
        ShipController.Instance.photonView.RPC("RPC_WithdrawItem", RpcTarget.MasterClient, itemKey, 1);
        
        // 2. Set local player's carried item
        localPlayer.State.Carried_Item = itemType;
        
        // 3. Close menu
        CloseMenu();
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}