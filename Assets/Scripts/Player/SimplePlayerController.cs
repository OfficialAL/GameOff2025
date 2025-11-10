using UnityEngine;

/// <summary>
/// Simple player controller without networking - for testing
/// Handles player movement and interaction with ship systems
/// </summary>
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float interactionRange = 2f;

    [Header("Components")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private LayerMask interactableLayerMask = 1;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private IInteractable currentInteractable;
    private SimpleShipPlatform currentShipPlatform;

    public System.Action<IInteractable> OnInteractableChanged;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleInput();
        CheckForInteractables();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    void HandleInput()
    {
        // Movement input
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        movementInput = movementInput.normalized;

        // Interaction input
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            if (currentInteractable.CanInteract(gameObject))
            {
                currentInteractable.Interact(gameObject);
            }
        }
    }

    void ApplyMovement()
    {
        Vector2 movement = movementInput * moveSpeed;
        rb.linearVelocity = movement;

        // Rotate player to face movement direction
        if (movement.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void CheckForInteractables()
    {
        // Cast a circle to find nearby interactable objects
        Collider2D nearestInteractable = Physics2D.OverlapCircle(
            interactionPoint.position, 
            interactionRange, 
            interactableLayerMask
        );

        IInteractable newInteractable = null;
        if (nearestInteractable != null)
        {
            newInteractable = nearestInteractable.GetComponent<IInteractable>();
        }

        // Update current interactable if it changed
        if (newInteractable != currentInteractable)
        {
            currentInteractable = newInteractable;
            OnInteractableChanged?.Invoke(currentInteractable);
        }
    }

    /// <summary>
    /// Called by SimpleShipPlatform to move player with the ship
    /// </summary>
    public void MoveWithPlatform(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    /// <summary>
    /// Set the current ship platform this player is on
    /// </summary>
    public void SetShipPlatform(SimpleShipPlatform platform)
    {
        currentShipPlatform = platform;
    }

    void OnDrawGizmosSelected()
    {
        if (interactionPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactionPoint.position, interactionRange);
        }
    }
}