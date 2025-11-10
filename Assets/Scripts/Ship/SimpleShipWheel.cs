using UnityEngine;

/// <summary>
/// Simple ship wheel without networking - for testing
/// Players can interact with this to control ship movement
/// </summary>
public class SimpleShipWheel : MonoBehaviour, IInteractable
{
    [Header("Wheel Settings")]
    [SerializeField] private float interactionRange = 2f;

    private SimpleShipController shipController;
    private GameObject currentOperator;
    private bool isBeingOperated = false;

    public System.Action<GameObject> OnOperatorChanged;

    void Awake()
    {
        shipController = GetComponentInParent<SimpleShipController>();
    }

    public bool CanInteract(GameObject player)
    {
        // Check if wheel is not already being operated
        if (isBeingOperated && currentOperator != player) return false;

        // Check distance
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance > interactionRange) return false;

        return true;
    }

    public void Interact(GameObject player)
    {
        if (!isBeingOperated)
        {
            StartOperatingWheel(player);
        }
        else if (currentOperator == player)
        {
            StopOperatingWheel();
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

    void StartOperatingWheel(GameObject player)
    {
        currentOperator = player;
        isBeingOperated = true;
        OnOperatorChanged?.Invoke(currentOperator);

        // Disable player movement while operating wheel
        SimplePlayerController playerController = player.GetComponent<SimplePlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        Debug.Log($"{player.name} is now steering the ship!");
    }

    void StopOperatingWheel()
    {
        // Re-enable player movement
        if (currentOperator != null)
        {
            SimplePlayerController playerController = currentOperator.GetComponent<SimplePlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
            Debug.Log($"{currentOperator.name} stopped steering the ship.");
        }

        currentOperator = null;
        isBeingOperated = false;
        OnOperatorChanged?.Invoke(null);
    }

    void Update()
    {
        // Handle steering input when someone is operating the wheel
        if (isBeingOperated && currentOperator != null && shipController != null)
        {
            float throttle = Input.GetAxis("Vertical");
            float steer = Input.GetAxis("Horizontal");
            shipController.SetShipControl(throttle, steer);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}