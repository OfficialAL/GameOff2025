using UnityEngine;
using PirateCoop;
using Photon.Pun;
using System.Collections.Generic;

/// <summary>
/// Controls the ship's movement and manages its shared state, including health.
/// </summary>
public class ShipController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform worldTransform;

    [Header("Ship Stations")]
    [SerializeField] private List<CannonStation> cannonStations; // For disablement
    // Add other stations like Mast, Wheel if they can be disabled
    
    public SharedShipState ShipState { get; private set; }

    // Singleton for easy access
    public static ShipController Instance { get; private set; }

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

        // Initialize the shared state
        ShipState = new SharedShipState();
        // Define ship health locations
        ShipState.Ship_Health = new Dictionary<string, int>
        {
            { "FrontHull", 100 },
            { "LeftCannon", 100 },
            { "RightCannon", 100 },
            { "Mast", 100 },
            { "Beds", 100 }
        };
    }

    // ... (Movement methods from Phase 2) ...
	void Update()
    {
        // The host will be responsible for calculating movement
        if (PhotonNetwork.IsMasterClient)
        {
            CalculateShipMovement();
        }

        // All clients (including host) move the world based on the state
        UpdateWorldTransform();
    }

    private void CalculateShipMovement()
    {
        if (ShipState.Ship_Sail == ShipSailState.Lowered)
        {
            // Set a constant speed for MVP
            ShipState.Ship_Speed = 5f; 
        }
        else
        {
            ShipState.Ship_Speed = 0f;
        }
        // ShipState.Ship_Heading is set by WheelStation
    }

    private void UpdateWorldTransform()
    {
        if (ShipState.Ship_Speed > 0)
        {
            // Convert heading (degrees) to a direction vector
            Vector2 direction = new Vector2(
                Mathf.Cos((ShipState.Ship_Heading - 90) * Mathf.Deg2Rad),
                Mathf.Sin((ShipState.Ship_Heading - 90) * Mathf.Deg2Rad)
            );

            // Move the world in the opposite direction
            worldTransform.Translate(-direction * ShipState.Ship_Speed * Time.deltaTime, Space.World);
        }
    }
    
    // --- Health and Repair ---
    
    /// <summary>
    /// RPC called by enemies/projectiles to damage the ship.
    /// This should only be executed by the host.
    /// </summary>
    [PunRPC]
    public void RPC_TakeDamage(string location, int amount)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        if (ShipState.Ship_Health.ContainsKey(location))
        {
            int newHealth = ShipState.Ship_Health[location] - amount;
            if (newHealth < 0) newHealth = 0;
            
            // Sync this health change to all clients
            photonView.RPC("RPC_UpdateHealth", RpcTarget.All, location, newHealth);
        }
    }
    
    /// <summary>
    /// RPC called by RepairPoint to heal the ship.
    /// This should only be executed by the host.
    /// </summary>
    [PunRPC]
    public void RPC_RepairDamage(string location, int amount)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (ShipState.Ship_Health.ContainsKey(location))
        {
            int newHealth = ShipState.Ship_Health[location] + amount;
            if (newHealth > 100) newHealth = 100; // Cap at max health
            
            photonView.RPC("RPC_UpdateHealth", RpcTarget.All, location, newHealth);
        }
    }

    /// <summary>
    /// This RPC is called by the host to update all clients' health state.
    /// </summary>
    [PunRPC]
    public void RPC_UpdateHealth(string location, int newHealth)
    {
        if (ShipState.Ship_Health.ContainsKey(location))
        {
            ShipState.Ship_Health[location] = newHealth;
            Debug.Log($"Ship location {location} health is now {newHealth}");
            
            // Handle visual degradation
            // TODO: Add crack overlays, water leaks, etc.
            
            // Handle station disablement
            CheckStationStatus(location, newHealth);
        }
    }
    
    private void CheckStationStatus(string location, int health)
    {
        // Example: Disable cannons if their location is destroyed
        if (location == "LeftCannon")
        {
            var cannon = cannonStations.Find(c => c.gameObject.name.Contains("Left"));
            if (cannon != null) cannon.SetDisabled(health <= 0);
        }
        if (location == "RightCannon")
        {
            var cannon = cannonStations.Find(c => c.gameObject.name.Contains("Right"));
            if (cannon != null) cannon.SetDisabled(health <= 0);
        }
        // TODO: Add logic for other stations (Mast, Beds)
    }
    
    // ... (SetSailState and SetHeading methods, now need to be RPCs) ...
    
    public void SetSailState(ShipSailState newState)
    {
        photonView.RPC("RPC_SetSailState", RpcTarget.All, newState);
    }
    
    [PunRPC]
    private void RPC_SetSailState(ShipSailState newState)
    {
        ShipState.Ship_Sail = newState;
    }
    
    public void SetHeading(float newHeading)
    {
        photonView.RPC("RPC_SetHeading", RpcTarget.All, newHeading);
    }
    
    [PunRPC]
    private void RPC_SetHeading(float newHeading)
    {
        ShipState.Ship_Heading = newHeading;
    }
	
	[PunRPC]
    public void RPC_DepositItem(string itemKey, int amount)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (ShipState.Shared_Loot.ContainsKey(itemKey))
        {
            int newAmount = ShipState.Shared_Loot[itemKey] + amount;
            photonView.RPC("RPC_UpdateLoot", RpcTarget.All, itemKey, newAmount);
        }
        else if (itemKey == "treasure")
        {
            int newAmount = ShipState.Shared_Treasure_Count + amount;
            photonView.RPC("RPC_UpdateTreasure", RpcTarget.All, newAmount);
        }
    }

    [PunRPC]
    public void RPC_WithdrawItem(string itemKey, int amount)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (ShipState.Shared_Loot.ContainsKey(itemKey))
        {
            int newAmount = ShipState.Shared_Loot[itemKey] - amount;
            if (newAmount >= 0)
            {
                photonView.RPC("RPC_UpdateLoot", RpcTarget.All, itemKey, newAmount);
            }
        }
    }

    [PunRPC]
    public void RPC_UpdateLoot(string itemKey, int newAmount)
    {
        ShipState.Shared_Loot[itemKey] = newAmount;
    }

    [PunRPC]
    public void RPC_UpdateTreasure(int newAmount)
    {
        ShipState.Shared_Treasure_Count = newAmount;
    }
	
	//Adding RPCs for purchasing.
	
	[PunRPC]
    public void RPC_PurchaseUpgrade(string upgradeKey, int cost)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        if (ShipState.Shared_Treasure_Count >= cost && ShipState.Ship_Upgrades.ContainsKey(upgradeKey))
        {
            photonView.RPC("RPC_UpdateTreasure", RpcTarget.All, ShipState.Shared_Treasure_Count - cost);
            photonView.RPC("RPC_SetUpgrade", RpcTarget.All, upgradeKey, true);
        }
    }
    
    [PunRPC]
    public void RPC_PurchaseResource(string itemKey, int amount, int cost)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (ShipState.Shared_Treasure_Count >= cost)
        {
            photonView.RPC("RPC_UpdateTreasure", RpcTarget.All, ShipState.Shared_Treasure_Count - cost);
            photonView.RPC("RPC_UpdateLoot", RpcTarget.All, itemKey, ShipState.Shared_Loot[itemKey] + amount);
        }
    }

    [PunRPC]
    public void RPC_SetUpgrade(string upgradeKey, bool value)
    {
        ShipState.Ship_Upgrades[upgradeKey] = value;
        Debug.Log($"Ship upgrade purchased: {upgradeKey}");
    }
	
	
}