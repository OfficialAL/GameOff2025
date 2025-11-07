using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Handles player movement and interaction with ship systems
/// Players can move around the ship and interact with various components
/// </summary>
public class PlayerController : NetworkBehaviour
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

    // Network variables for position synchronization
    private NetworkVariable<Vector2> networkPosition = new NetworkVariable<Vector2>();
    private NetworkVariable<float> networkRotation = new NetworkVariable<float>();

    public System.Action<IInteractable> OnInteractableChanged;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        // Subscribe to network position changes for smooth interpolation
        if (!IsOwner)
        {
            networkPosition.OnValueChanged += OnNetworkPositionChanged;
            networkRotation.OnValueChanged += OnNetworkRotationChanged;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleInput();
        CheckForInteractables();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        ApplyMovement();
        SyncNetworkTransform();
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
            InteractWithObjectServerRpc();
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

    [ServerRpc]
    void InteractWithObjectServerRpc()
    {
        if (currentInteractable != null && currentInteractable.CanInteract(gameObject))
        {
            currentInteractable.Interact(gameObject);
        }
    }

    void SyncNetworkTransform()
    {
        networkPosition.Value = transform.position;
        networkRotation.Value = transform.eulerAngles.z;
    }

    void OnNetworkPositionChanged(Vector2 oldPos, Vector2 newPos)
    {
        // Smooth interpolation for non-owner clients
        if (!IsOwner)
        {
            StartCoroutine(InterpolatePosition(newPos));
        }
    }

    void OnNetworkRotationChanged(float oldRot, float newRot)
    {
        if (!IsOwner)
        {
            transform.rotation = Quaternion.AngleAxis(newRot, Vector3.forward);
        }
    }

    System.Collections.IEnumerator InterpolatePosition(Vector2 targetPos)
    {
        Vector2 startPos = transform.position;
        float elapsed = 0f;
        float duration = 0.1f; // Short interpolation time for responsiveness

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
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

/// <summary>
/// Interface for objects that players can interact with
/// </summary>
public interface IInteractable
{
    bool CanInteract(GameObject player);
    void Interact(GameObject player);
    string GetInteractionText();
}