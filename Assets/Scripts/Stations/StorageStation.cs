using UnityEngine;
using PirateCoop;
using Photon.Pun;

/// <summary>
/// Storage Station (Station 6): Deposit items or open menu to withdraw.
/// </summary>
public class StorageStation : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject storageMenuPrefab; // Screen 9
    private StorageMenu storageMenuInstance;

    public void Interact(PlayerController player)
    {
        if (player.State.Carried_Item != CarriedItemType.None)
        {
            // --- Deposit Item ---
            CarriedItemType item = player.State.Carried_Item;
            
            // Only Treasure, Lumber, and Blunderbuss are storable
            if (item == CarriedItemType.Lumber || 
                item == CarriedItemType.Treasure || 
                item == CarriedItemType.Blunderbuss)
            {
                // Call RPC on ShipController (or GameManager) to update Shared_Loot
                string itemKey = item.ToString().ToLower();
                ShipController.Instance.photonView.RPC("RPC_DepositItem", RpcTarget.MasterClient, itemKey, 1);
                
                // Clear player's hands
                player.State.Carried_Item = CarriedItemType.None;
            }
        }
        else
        {
            // --- Withdraw (Open Menu) ---
            OpenStorageMenu(player);
        }
    }

    private void OpenStorageMenu(PlayerController player)
    {
        if (storageMenuInstance == null)
        {
            GameObject menuObject = Instantiate(storageMenuPrefab);
            storageMenuInstance = menuObject.GetComponent<StorageMenu>();
            storageMenuInstance.Initialize(player, ShipController.Instance.ShipState);
        }
        storageMenuInstance.gameObject.SetActive(true);
    }

    public void StopInteract() { }

    public bool CanInteract(PlayerController player)
    {
        // Can always interact (unless carrying a teammate)
        return player.State.Carried_Item != CarriedItemType.Teammate;
    }

    public string GetInteractPrompt(PlayerController player)
    {
        if (player.State.Carried_Item == CarriedItemType.Teammate)
        {
            return "[Cannot Use]";
        }
        
        if (player.State.Carried_Item != CarriedItemType.None)
        {
            return $"Deposit {player.State.Carried_Item} (E)";
        }
        
        return "Open Storage (E)";
    }
}