using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Interactable ship component - Ship's Wheel for controlling navigation
/// Players can interact with this to control ship movement
/// </summary>
public class ShipWheel : NetworkBehaviour, IInteractable
{
    [Header("Wheel Settings")]
    [SerializeField] private float interactionRange = 2f;

    private ShipController shipController;
    private GameObject currentOperator;
    private bool isBeingOperated = false;

    public System.Action<GameObject> OnOperatorChanged;

    void Awake()
    {
        shipController = GetComponentInParent<ShipController>();
    }

    public bool CanInteract(GameObject player)
    {
        // Check if wheel is not already being operated
        if (isBeingOperated && currentOperator != player) return false;

        // Check distance
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance > interactionRange) return false;

        // Since we don't use rigid roles, anyone can operate the wheel

        return true;
    }

    public void Interact(GameObject player)
    {
        if (!isBeingOperated)
        {
            StartOperatingWheelServerRpc(player.GetComponent<NetworkObject>().NetworkObjectId);
        }
        else if (currentOperator == player)
        {
            StopOperatingWheelServerRpc();
        }
    }

    public string GetInteractionText()
    {
        if (!isBeingOperated)
        {
            return "Press E to take the wheel";
        }
        else if (currentOperator != null)
        {
            return $"Press E to stop steering (Currently: {currentOperator.name})";
        }
        else
        {
            return "Wheel is being operated";
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void StartOperatingWheelServerRpc(ulong playerNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out var playerNetworkObject))
        {
            currentOperator = playerNetworkObject.gameObject;
            isBeingOperated = true;
            OnOperatorChanged?.Invoke(currentOperator);

            // Enable wheel control for this player
            EnableWheelControlClientRpc(playerNetworkId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void StopOperatingWheelServerRpc()
    {
        if (currentOperator != null)
        {
            ulong playerId = currentOperator.GetComponent<NetworkObject>().NetworkObjectId;
            DisableWheelControlClientRpc(playerId);
        }

        currentOperator = null;
        isBeingOperated = false;
        OnOperatorChanged?.Invoke(null);
    }

    [ClientRpc]
    void EnableWheelControlClientRpc(ulong playerNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out var playerNetworkObject))
        {
            ShipWheelInput wheelInput = playerNetworkObject.GetComponent<ShipWheelInput>();
            if (wheelInput == null)
            {
                wheelInput = playerNetworkObject.gameObject.AddComponent<ShipWheelInput>();
            }
            wheelInput.SetShipController(shipController);
            wheelInput.enabled = true;
        }
    }

    [ClientRpc]
    void DisableWheelControlClientRpc(ulong playerNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out var playerNetworkObject))
        {
            ShipWheelInput wheelInput = playerNetworkObject.GetComponent<ShipWheelInput>();
            if (wheelInput != null)
            {
                wheelInput.enabled = false;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

/// <summary>
/// Component that handles input when a player is operating the ship's wheel
/// </summary>
public class ShipWheelInput : MonoBehaviour
{
    private ShipController shipController;
    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (shipController == null) return;

        // Get steering input
        float throttle = Input.GetAxis("Vertical");
        float steer = Input.GetAxis("Horizontal");

        // Send control input to ship
        shipController.SetShipControlServerRpc(throttle, steer);

        // Disable player movement while operating wheel
        if (playerController != null)
        {
            playerController.enabled = false;
        }
    }

    public void SetShipController(ShipController controller)
    {
        shipController = controller;
    }

    void OnDisable()
    {
        // Re-enable player movement when stopping wheel operation
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }
}