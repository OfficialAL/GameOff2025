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

        // Tell the Master Client to destroy this item
        this.photonView.RPC("RPC_RequestDestroy", RpcTarget.MasterClient);
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

    [PunRPC]
    public void RPC_RequestDestroy()
    {
        // This code only runs on the Master Client
        PhotonNetwork.Destroy(gameObject);
    }
}