using System.Collections.Generic;
using UnityEngine;

// This file defines the core data structures used for network synchronization,
// based on the PRD Data Requirements section .

namespace PirateCoop
{
    /// <summary>
    /// Enum definitions used across multiple data structures.
    /// </summary>
    public enum DeckPosition { Upper, Lower } [cite: 606, 617, 622]
    public enum FacingDirection { Up, Down, Left, Right } [cite: 607]
    public enum CarriedItemType { None, Lumber, Treasure, Blunderbuss, Teammate } [cite: 609]
    public enum ShipSailState { Raised, Lowered } [cite: 594]
    public enum DroppedItemType { Lumber, Treasure, Blunderbuss } [cite: 615]
    public enum EnemyType { PirateCutlass, PiratePistol, PirateBlunderbuss, KrakenTentacle, Octopus } [cite: 620]
    public enum HostileShipType { PirateShip, KrakenBody, Raft, Barrel } [cite: 627]


    /// <summary>
    /// Data Object 1: SharedShipState [cite: 592]
    /// Synced state of the player's ship, owned by the host.
    /// </summary>
    [System.Serializable]
    public class SharedShipState
    {
        public Dictionary<string, int> Ship_Health; // [cite: 592]
        public float Ship_Heading; // [cite: 593]
        public float Ship_Speed; // [cite: 593]
        public ShipSailState Ship_Sail; // [cite: 594]
        public int Shared_Treasure_Count; // [cite: 595]
        public Dictionary<string, int> Shared_Loot; // [cite: 596]
        public Dictionary<string, bool> Ship_Upgrades; // [cite: 596]

        public SharedShipState()
        {
            Ship_Health = new Dictionary<string, int>(); // E.g., {"mast": 10, "fronthull": 8}
            Ship_Heading = 0f;
            Ship_Speed = 0f;
            Ship_Sail = ShipSailState.Raised;
            Shared_Treasure_Count = 0;
            Shared_Loot = new Dictionary<string, int> { { "lumber", 0 }, { "blunderbuss", 0 } }; // [cite: 596]
            Ship_Upgrades = new Dictionary<string, bool> { { "reinforced", false }, { "beds", false }, { "cannons", false } }; // [cite: 312, 596]
        }
    }

    /// <summary>
    /// Data Object 2: SharedGameState [cite: 597]
    /// Overall state of the game session, managed by the host.
    /// </summary>
    [System.Serializable]
    public class SharedGameState
    {
        public float Travel_Progress; // [cite: 597]
        public List<EnemyState> Current_Enemies_List; // [cite: 598]
        public List<HostileShipState> Current_HostileShip_List; // [cite: 599]
        public List<DroppedItem> Current_DroppedItems_List; // [cite: 600]
        public float Total_Distance; // [cite: 601]
        public int Ports_Visited; // [cite: 601]

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
    /// Data Object 3: PlayerState [cite: 603]
    /// Synced state for an individual player.
    /// </summary>
    [System.Serializable]
    public class PlayerState
    {
        public string Player_ID; // [cite: 603]
        public string Display_Name; // [cite: 604]
        public Vector2 Position; // [cite: 605]
        public DeckPosition Position_deck; // [cite: 606]
        public FacingDirection Facing_Direction; // [cite: 607]
        public int Current_Health; // [cite: 608]
        public CarriedItemType Carried_Item; // [cite: 609]
        public bool Is_Unconscious; // [cite: 610]
        public string Interacting_Station; // [cite: 611]
        public bool Is_Disconnected; // [cite: 612]
    }

    /// <summary>
    /// Data Object 4: DroppedItem [cite: 614]
    /// Represents a physical item on the deck.
    /// </summary>
    [System.Serializable]
    public class DroppedItem
    {
        public string Item_ID; // [cite: 614]
        public DroppedItemType Item_Type; // [cite: 615]
        public Vector2 Position; // [cite: 616]
        public DeckPosition Position_deck; // [cite: 617]
    }

    /// <summary>
    /// Data Object 5: EnemyState [cite: 619]
    /// Represents an individual AI enemy.
    /// </summary>
    [System.Serializable]
    public class EnemyState
    {
        public string Enemy_ID; // [cite: 619]
        public EnemyType Enemy_Type; // [cite: 620]
        public Vector2 Position; // [cite: 621]
        public DeckPosition Position_deck; // [cite: 622]
        public int Current_Health; // [cite: 623]
        public bool Is_Boarding; // [cite: 624]
    }

    /// <summary>
    /// Data Object 6: HostileShipState [cite: 626]
    /// Represents an enemy vessel.
    /// </summary>
    [System.Serializable]
    public class HostileShipState
    {
        public string HostileShip_ID; // [cite: 626]
        public HostileShipType Enemy_Type; // [cite: 627]
        public Vector2 Position; // [cite: 628]
        public float Ship_Heading; // [cite: 629]
        public int Current_Health; // [cite: 630]
        public bool Is_Boarding; // [cite: 631]
        public List<EnemyState> Carrying_Enemies; // [cite: 632]
    }
}