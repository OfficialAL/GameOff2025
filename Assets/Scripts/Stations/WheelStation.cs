using UnityEngine;
using PirateCoop;
using Photon.Pun;

/// <summary>
/// Steering Wheel Station (Station 1): Controls ship direction.
/// </summary>
public class WheelStation : MonoBehaviour, IInteractable
{
    [SerializeField] private float turnSpeed = 50f;

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
            // Player is using the wheel
            float horizontalInput = InputManager.Instance.StationInput.x; // A/D
            
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // Get current heading and apply turn
                float currentHeading = shipController.ShipState.Ship_Heading;
                currentHeading -= horizontalInput * turnSpeed * Time.deltaTime;
                
                // Normalize heading to 0-360
                shipController.SetHeading(currentHeading % 360f);
            }
        }
    }

    public void Interact(PlayerController player)
    {
        interactingPlayer = player;
        isBusy = true;
        // TODO: Show Compass UI
        // player.State.Interacting_Station = "Wheel";
        Debug.Log("Player interacting with Wheel");
    }

    public void StopInteract()
    {
        interactingPlayer = null;
        isBusy = false;
        // TODO: Hide Compass UI
        Debug.Log("Player stopped interacting with Wheel");
    }

    public bool CanInteract(PlayerController player)
    {
        return !isBusy;
    }

    public string GetInteractPrompt()
    {
        return isBusy ? "[Station Busy]" : "Use Wheel (E)";
    }
}