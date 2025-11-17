using UnityEngine;
using Photon.Pun;
using PirateCoop;

/// <summary>
/// Represents a physical item on the deck that can be picked up.
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class DroppedItem : MonoBehaviour, IInteractable
{
    [SerializeField] private DroppedItemType itemType;

    public DroppedItemType GetItemType()
    {
        return itemType;
    }

    public void Interact(PlayerController player)
    {
        // Player picks this up. The player controller handles setting Carried_Item.
        player.PickUpItem(this);

        // This item is now "collected," so destroy it across the network.
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void StopInteract() { }

    public bool CanInteract(PlayerController player)
    {
        // Player can pick this up if their hands are empty
        return player.State.Carried_Item == CarriedItemType.None;
    }

    public string GetInteractPrompt(PlayerController player)
    {
        if (player.State.Carried_Item == CarriedItemType.None)
        {
            return $"Pick Up {itemType} (E)";
        }
        return "[Hands Full]";
    }
}