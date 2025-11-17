using UnityEngine;
using PirateCoop;
using Photon.Pun;

/// <summary>
/// Mast Station (Station 2): Controls raising/lowering sails.
/// </summary>
public class MastStation : MonoBehaviour, IInteractable
{
    private PlayerController interactingPlayer;
    private ShipController shipController;
    private bool isBusy = false;

    void Start()
    {
        // Find the ship controller in the scene
        shipController = FindObjectOfType<ShipController>();
    }

    void Update()
    {
        if (interactingPlayer != null)
        {
            // Player is using the mast
            float verticalInput = InputManager.Instance.StationInput.y;
            
            if (verticalInput > 0.1f) // W or Up
            {
                shipController.SetSailState(ShipSailState.Raised);
            }
            else if (verticalInput < -0.1f) // S or Down
            {
                shipController.SetSailState(ShipSailState.Lowered);
            }
        }
    }

    public void Interact(PlayerController player)
    {
        interactingPlayer = player;
        isBusy = true;
        // TODO: Show Compass UI
        // player.State.Interacting_Station = "Mast";
        Debug.Log("Player interacting with Mast");
    }

    public void StopInteract()
    {
        interactingPlayer = null;
        isBusy = false;
        // TODO: Hide Compass UI
        Debug.Log("Player stopped interacting with Mast");
    }

    public bool CanInteract(PlayerController player)
    {
        return !isBusy;
    }

    public string GetInteractPrompt()
    {
        return isBusy ? "[Station Busy]" : "Use Mast (E)";
    }
}