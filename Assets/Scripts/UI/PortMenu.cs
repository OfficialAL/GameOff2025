using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PirateCoop;
using Photon.Pun;
using System.Collections.Generic;

/// <summary>
/// Manages the Port Purchase Screen (Screen 8).
/// </summary>
public class PortMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI treasureCountText;
    [SerializeField] private TextMeshProUGUI upgradeNameText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private Button upgradePurchaseButton;
    [SerializeField] private TextMeshProUGUI lumberStockText;
    [SerializeField] private Button lumberPurchaseButton;
    // ... (add Blunderbuss UI) ...

    private PlayerController localPlayer;
    private SharedShipState shipState;
    private string randomUpgradeKey;
    private int upgradeCost = 100; // Example costs
    private int lumberCost = 10;
    
    // Example port stock
    private int lumberStock = 20;

    public void Initialize(PlayerController player, SharedShipState state)
    {
        localPlayer = player;
        shipState = state;

        // Read the synced upgrade from the Room's Custom Properties
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CurrentPortUpgrade"))
        {
            randomUpgradeKey = (string)PhotonNetwork.CurrentRoom.CustomProperties["CurrentPortUpgrade"];
        }
        else
        {
            randomUpgradeKey = ""; // Failsafe
        }

        UpdateUI();
    }
    
    void SelectRandomUpgrade()
    {
        List<string> unownedUpgrades = new List<string>();
        if (!shipState.Ship_Upgrades["reinforced"]) unownedUpgrades.Add("reinforced");
        if (!shipState.Ship_Upgrades["beds"]) unownedUpgrades.Add("beds");
        if (!shipState.Ship_Upgrades["cannons"]) unownedUpgrades.Add("cannons");
        
        if(unownedUpgrades.Count > 0)
        {
            randomUpgradeKey = unownedUpgrades[Random.Range(0, unownedUpgrades.Count)];
        }
    }

    void Update()
    {
        if (shipState != null)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        treasureCountText.text = $"Shared Treasure: {shipState.Shared_Treasure_Count}";
        
        // Update Upgrade Button
        if (string.IsNullOrEmpty(randomUpgradeKey))
        {
            upgradeNameText.text = "All Upgrades Owned";
            upgradeCostText.text = "";
            upgradePurchaseButton.interactable = false;
            upgradePurchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = "Owned";
        }
        else if (shipState.Ship_Upgrades[randomUpgradeKey])
        {
            upgradeNameText.text = GetUpgradeDisplayName(randomUpgradeKey);
            upgradeCostText.text = $"Cost: {upgradeCost}";
            upgradePurchaseButton.interactable = false;
            upgradePurchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = "Owned";
        }
        else
        {
            upgradeNameText.text = GetUpgradeDisplayName(randomUpgradeKey);
            upgradeCostText.text = $"Cost: {upgradeCost}";
            upgradePurchaseButton.interactable = (shipState.Shared_Treasure_Count >= upgradeCost);
            upgradePurchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = "Purchase";
        }
        
        // Update Resource Buttons
        lumberStockText.text = $"Available: {lumberStock}";
        lumberPurchaseButton.interactable = (shipState.Shared_Treasure_Count >= lumberCost && lumberStock > 0);
    }
    
    private string GetUpgradeDisplayName(string key)
    {
        if (key == "reinforced") return "Reinforced Boat";
        if (key == "beds") return "Better Beds";
        if (key == "cannons") return "Better Cannons";
        return "Unknown Upgrade";
    }

    public void OnPurchaseUpgrade()
    {
        if (shipState.Shared_Treasure_Count >= upgradeCost)
        {
            // Call RPC on ShipController (or GameManager)
            ShipController.Instance.photonView.RPC("RPC_PurchaseUpgrade", RpcTarget.MasterClient, randomUpgradeKey, upgradeCost);
            randomUpgradeKey = null; // Mark as purchased for this session
        }
    }

    public void OnPurchaseLumber()
    {
        if (shipState.Shared_Treasure_Count >= lumberCost && lumberStock > 0)
        {
            lumberStock--;
            ShipController.Instance.photonView.RPC("RPC_PurchaseResource", RpcTarget.MasterClient, "lumber", 1, lumberCost);
        }
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}