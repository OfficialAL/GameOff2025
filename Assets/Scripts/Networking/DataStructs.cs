using System.Collections.Generic;
using UnityEngine;

// This file defines the core data structures used for network synchronization,
// based on the PRD Data Requirements section .

namespace PirateCoop
{
    /// <summary>
    /// Enum definitions used across multiple data structures.
    /// </summary>
    public enum DeckPosition { Upper, Lower } 
    public enum FacingDirection { Up, Down, Left, Right } 
    public enum CarriedItemType { None, Lumber, Treasure, Blunderbuss, Teammate } 
    public enum ShipSailState { Raised, Lowered } 
    public enum DroppedItemType { Lumber, Treasure, Blunderbuss }
    public enum EnemyType { PirateCutlass, PiratePistol, PirateBlunderbuss, KrakenTentacle, Octopus }
    public enum HostileShipType { PirateShip, KrakenBody, Raft, Barrel }


    /// <summary>
    /// Data Object 1: SharedShipState
    /// Synced state of the player's ship, owned by the host.
    /// </summary>
    [System.Serializable]
    public class SharedShipState
    {
        public Dictionary<string, int> Ship_Health; 
        public float Ship_Heading; 
        public float Ship_Speed; 
        public ShipSailState Ship_Sail; 
        public int Shared_Treasure_Count; 
        public Dictionary<string, int> Shared_Loot; 
        public Dictionary<string, bool> Ship_Upgrades; 

        public SharedShipState()
        {
            Ship_Health = new Dictionary<string, int>(); // E.g., {"mast": 10, "fronthull": 8}
            Ship_Heading = 0f;
            Ship_Speed = 0f;
            Ship_Sail = ShipSailState.Raised;
            Shared_Treasure_Count = 0;
            Shared_Loot = new Dictionary<string, int> { { "lumber", 0 }, { "blunderbuss", 0 } }; 
            Ship_Upgrades = new Dictionary<string, bool> { { "reinforced", false }, { "beds", false }, { "cannons", false } }; 
        }
    }

    /// <summary>
    /// Data Object 2: SharedGameState 
    /// Overall state of the game session, managed by the host.
    /// </summary>
    [System.Serializable]
    public class SharedGameState
    {
        public float Travel_Progress; 
        public List<EnemyState> Current_Enemies_List; 
        public List<HostileShipState> Current_HostileShip_List; 
        public List<DroppedItem> Current_DroppedItems_List; 
        public float Total_Distance;
        public int Ports_Visited; 

        public SharedGameState()
        {
            Travel_Progress = 0f;
            Current_Enemies_List = new List<EnemyState>();
            Current_HostileShip_List = new List<HostileShipState>();
            Current_DroppedItems_List = new List<DroppedItem>();
            Total_Distance = 0f;
            Ports_Visited = 0;
        }
    }

    /// <summary>
    /// Data Object 3: PlayerState 
    /// Synced state for an individual player.
    /// </summary>
    [System.Serializable]
    public class PlayerState
    {
        public string Player_ID; 
        public string Display_Name; 
        public Vector2 Position; 
        public DeckPosition Position_deck; 
        public FacingDirection Facing_Direction; 
        public int Current_Health; 
        public CarriedItemType Carried_Item; 
        public bool Is_Unconscious; 
        public string Interacting_Station; 
        public bool Is_Disconnected; 
    }

    /// <summary>
    /// Data Object 4: DroppedItem [cite: 614]
    /// Represents a physical item on the deck.
    /// </summary>
    [System.Serializable]
    public class DroppedItem
    {
        public string Item_ID; 
        public DroppedItemType Item_Type; 
        public Vector2 Position; 
        public DeckPosition Position_deck; 
    }

    /// <summary>
    /// Data Object 5: EnemyState 
    /// Represents an individual AI enemy.
    /// </summary>
    [System.Serializable]
    public class EnemyState
    {
        public string Enemy_ID; 
        public EnemyType Enemy_Type; 
        public Vector2 Position; 
        public DeckPosition Position_deck; 
        public int Current_Health; 
        public bool Is_Boarding; 
    }

    /// <summary>
    /// Data Object 6: HostileShipState 
    /// Represents an enemy vessel.
    /// </summary>
    [System.Serializable]
    public class HostileShipState
    {
        public string HostileShip_ID; 
        public HostileShipType Enemy_Type; 
        public Vector2 Position; 
        public float Ship_Heading; 
        public int Current_Health; 
        public bool Is_Boarding; 
        public List<EnemyState> Carrying_Enemies; 
    }
}