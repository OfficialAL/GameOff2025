using UnityEngine;
using PirateCoop;
using System.Collections;
using Photon.Pun;

/// <summary>
/// Bed Station (Station 5): Heals conscious players and revives unconscious players.
/// </summary>
public class BedStation : MonoBehaviour, IInteractable
{
    [SerializeField] private float reviveTime = 10.0f;
    [SerializeField] private int healthPerSecond = 3;

    private PlayerController interactingPlayer;
    private PlayerController revivingPlayer; // The player *in* the bed
    private Coroutine activeCoroutine;
    private bool isBusy = false;
    private bool isDisabled = false;

    public void Interact(PlayerController player)
    {
        if (isBusy || isDisabled) return;

        if (player.State.Carried_Item == CarriedItemType.Teammate)
        {
            // --- Place Teammate in Bed ---
            isBusy = true;
            interactingPlayer = player;
            revivingPlayer = player.StopCarryingPlayer(); // Un-parents the player
            
            if(revivingPlayer != null)
            {
                // Tell the placed player to snap to the bed's position on all clients
                revivingPlayer.photonView.RPC("RPC_SnapToBed", RpcTarget.All, transform.position); 
                
                activeCoroutine = StartCoroutine(ReviveProgress());
            }
        }
        else if (player.State.Carried_Item == CarriedItemType.None)
        {
            // --- Conscious Player Healing ---
            isBusy = true;
            interactingPlayer = player;
            activeCoroutine = StartCoroutine(HealProgress());
        }
    }

    private IEnumerator ReviveProgress()
    {
        Debug.Log($"Reviving {revivingPlayer.State.Display_Name}...");
        yield return new WaitForSeconds(reviveTime);

        // Success!
        if (revivingPlayer != null)
        {
            // Call RPC on the revived player
            revivingPlayer.photonView.RPC("RPC_SetUnconscious", RpcTarget.All, false);
        }
        
        StopInteract();
    }

    private IEnumerator HealProgress()
    {
        Debug.Log($"Healing {interactingPlayer.State.Display_Name}...");
        while (interactingPlayer != null)
        {
            // Heal 3% per second (approximated)
            interactingPlayer.TakeDamage(-healthPerSecond); // Take "negative" damage
            yield return new WaitForSeconds(1.0f);
        }
    }

    public void StopInteract()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        // If we were reviving someone, don't just leave them in limbo
        // The revive was interrupted.
        if (revivingPlayer != null)
        {
            Debug.Log("Revive interrupted!");
        }

        interactingPlayer = null;
        revivingPlayer = null;
        isBusy = false;
    }

    public bool CanInteract(PlayerController player)
    {
        if (isDisabled) return false;

        // Can interact if the bed is free AND
        // (player is carrying a teammate OR player has empty hands)
        return !isBusy && 
            (player.State.Carried_Item == CarriedItemType.Teammate || 
             player.State.Carried_Item == CarriedItemType.None);
    }

    public string GetInteractPrompt(PlayerController player)
    {
        if (isDisabled) return "[Station Broken]";
        if (isBusy) return "[Bed Occupied]";
        
        if (player.State.Carried_Item == CarriedItemType.Teammate)
        {
            return "Place Teammate (E)";
        }
        return "Rest (E)";
    }
    
    // Called by ShipController when location is destroyed
    public void SetDisabled(bool disabled)
    {
        isDisabled = disabled;
        if (isDisabled)
        {
            // Interrupt any active revive/heal
            StopInteract();
        }
    }
}