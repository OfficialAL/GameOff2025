using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple player controller without networking - for testing
/// Handles player movement and interaction with ship systems
/// </summary>
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private bool keepUpright = true; // Keep player upright (no rotation)
    [SerializeField] private bool zeroGravity = true; // Top-down: disable gravity
    [SerializeField] private bool freezeRotationZ = true; // Physics constraint backup
    [SerializeField] private bool debugCollisions = false; // Enable to find hidden colliders
    [SerializeField] private float debugOriginScanRadius = 2f; // Radius around (0,0) to scan
    [SerializeField] private float debugScanInterval = 1.0f; // Seconds between scans
    [Header("Collision Quality")]
    [SerializeField] private CollisionDetectionMode2D collisionDetection = CollisionDetectionMode2D.ContinuousSpeculative;

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

        // Ensure top-down friendly physics defaults
        if (rb != null)
        {
            if (zeroGravity) rb.gravityScale = 0f;
            if (freezeRotationZ) rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = collisionDetection;
        }
    }

    void Update()
    {
        HandleInput();
        CheckForInteractables();

        if (debugCollisions)
        {
            DebugScanOriginArea();
        }
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    void HandleInput()
    {
        // Movement input using new Input System
        Vector2 moveInput = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1f;
        }
        movementInput = moveInput.normalized;

        // Interaction input
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && currentInteractable != null)
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

        // Keep players upright by default
        if (keepUpright)
        {
            // Reset rotation each physics step
            rb.MoveRotation(0f);
            transform.rotation = Quaternion.identity;
        }
    }

    void CheckForInteractables()
    {
        // Skip if interaction point not assigned
        if (interactionPoint == null) return;
        
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

        if (debugCollisions)
        {
            // Visualize origin scan area
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Vector3.zero, debugOriginScanRadius);
        }
    }

    // --- Debug helpers to locate hidden colliders near world origin (0,0) ---
    private float _nextScanTime = 0f;
    void DebugScanOriginArea()
    {
        if (Time.time < _nextScanTime) return;
        _nextScanTime = Time.time + debugScanInterval;

        // Scan for any colliders near (0,0)
        Collider2D[] hits = Physics2D.OverlapCircleAll(Vector2.zero, debugOriginScanRadius, ~0);
        if (hits != null && hits.Length > 0)
        {
            foreach (var h in hits)
            {
                if (h == null) continue;
                string layerName = LayerMask.LayerToName(h.gameObject.layer);
                string tagName = h.gameObject.tag;
                Vector3 pos = h.bounds.center;
                Debug.Log($"[OriginScan] Collider: {h.name}, Layer: {layerName}, Tag: {tagName}, Pos: {pos}", h);
            }
        }
        else
        {
            Debug.Log("[OriginScan] No colliders found near (0,0)");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!debugCollisions) return;
        Debug.Log($"[PlayerCollision] ENTER with {collision.collider.name} on layer {LayerMask.LayerToName(collision.collider.gameObject.layer)} at contact {(collision.contacts.Length>0? (Vector3)collision.contacts[0].point : (Vector3)Vector2.zero)}", collision.collider);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!debugCollisions) return;
        if (collision.contacts != null && collision.contacts.Length > 0)
        {
            var p = collision.contacts[0].point;
            if (Vector2.Distance(p, Vector2.zero) <= debugOriginScanRadius)
            {
                Debug.Log($"[PlayerCollision] STAY near origin with {collision.collider.name} at {p}", collision.collider);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!debugCollisions) return;
        Debug.Log($"[PlayerTrigger] ENTER with {other.name} on layer {LayerMask.LayerToName(other.gameObject.layer)} at {(Vector3)other.bounds.center}", other);
    }
}