using UnityEngine;
using PirateCoop;
using System.Collections;
using Photon.Pun;

/// <summary>
/// Repair Point (Station 4): Allows players with Lumber to repair a ship location.
/// </summary>
public class RepairPoint : MonoBehaviour, IInteractable
{
    [Tooltip("The name of the ship location this point repairs (must match ShipState.Ship_Health key)")]
    [SerializeField] private string repairLocationName;
    
    [SerializeField] private float repairTime = 3.0f;
    [SerializeField] private int repairAmount = 25; // Amount healed per action

    private PlayerController interactingPlayer;
    private Coroutine repairCoroutine;
    private bool isBusy = false;
    
    public void Interact(PlayerController player)
    {
        if (isBusy || player.State.Carried_Item != CarriedItemType.Lumber)
        {
            return;
        }

        interactingPlayer = player;
        isBusy = true;
        repairCoroutine = StartCoroutine(RepairProgress());
    }

    private IEnumerator RepairProgress()
    {
        Debug.Log("Starting repair...");
        yield return new WaitForSeconds(repairTime);

        // Success!
        Debug.Log("Repair complete!");
        
        // Call Host RPC on ShipController to heal
        ShipController.Instance.photonView.RPC("RPC_RepairDamage", RpcTarget.MasterClient, repairLocationName, repairAmount);
        
        // Consume Lumber
        interactingPlayer.State.Carried_Item = CarriedItemType.None;
        
        StopInteract();
    }

    public void StopInteract()
    {
        if (repairCoroutine != null)
        {
            StopCoroutine(repairCoroutine);
            repairCoroutine = null;
        }
        interactingPlayer = null;
        isBusy = false;
    }

    public bool CanInteract(PlayerController player)
    {
        // Can interact if not busy AND the location is damaged
        bool isDamaged = ShipController.Instance.ShipState.Ship_Health[repairLocationName] < 100;
        return !isBusy && isDamaged;
    }

    public string GetInteractPrompt(PlayerController player)
    {
        if (isBusy) return "[Repairing...]";
        
        bool isDamaged = ShipController.Instance.ShipState.Ship_Health[repairLocationName] < 100;
        if (!isDamaged) return "[Fully Repaired]";
        
        if (player.State.Carried_Item != CarriedItemType.Lumber)
        {
            return "[Need Lumber]";
        }
        
        return "Repair (E)";
    }
}