using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Handles the ship as a moving platform that players can walk on
/// Manages player attachment to ship and relative movement
/// </summary>
public class ShipPlatform : NetworkBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] private LayerMask playerLayerMask = 1 << 6; // Assuming players are on layer 6
    [SerializeField] private bool movePlayersWithShip = true;

    private Rigidbody2D shipRigidbody;
    private Vector2 lastPosition;
    private float lastRotation;

    void Awake()
    {
        shipRigidbody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.eulerAngles.z;
    }

    void FixedUpdate()
    {
        if (!IsServer || !movePlayersWithShip) return;

        // Calculate ship movement delta
        Vector2 currentPosition = transform.position;
        float currentRotation = transform.eulerAngles.z;

        Vector2 positionDelta = currentPosition - lastPosition;
        float rotationDelta = Mathf.DeltaAngle(lastRotation, currentRotation);

        // Only move players if the ship has actually moved
        if (positionDelta.magnitude > 0.001f || Mathf.Abs(rotationDelta) > 0.1f)
        {
            MovePlayersWithShip(positionDelta, rotationDelta);
        }

        lastPosition = currentPosition;
        lastRotation = currentRotation;
    }

    void MovePlayersWithShip(Vector2 positionDelta, float rotationDelta)
    {
        // Find all players on the ship
        PlayerController[] playersOnShip = FindObjectsOfType<PlayerController>();
        
        foreach (PlayerController player in playersOnShip)
        {
            if (IsPlayerOnShip(player))
            {
                // Move player with ship translation
                Vector3 newPosition = player.transform.position + (Vector3)positionDelta;
                
                // Apply rotation around ship center if there's rotation
                if (Mathf.Abs(rotationDelta) > 0.1f)
                {
                    Vector3 relativePos = player.transform.position - transform.position;
                    Vector3 rotatedPos = Quaternion.Euler(0, 0, rotationDelta) * relativePos;
                    newPosition = transform.position + rotatedPos;
                }

                // Update player position through their controller
                player.MoveWithPlatform(newPosition);
            }
        }
    }

    bool IsPlayerOnShip(PlayerController player)
    {
        // Simple bounds check - you might want to make this more sophisticated
        Collider2D shipCollider = GetComponent<Collider2D>();
        Collider2D playerCollider = player.GetComponent<Collider2D>();

        if (shipCollider != null && playerCollider != null)
        {
            return shipCollider.bounds.Contains(playerCollider.bounds.center);
        }

        // Fallback distance check
        float distance = Vector2.Distance(transform.position, player.transform.position);
        return distance < 15f; // Adjust based on your ship size
    }

    /// <summary>
    /// Called when a player lands on the ship
    /// </summary>
    public void OnPlayerEnterShip(PlayerController player)
    {
        // Set player's parent to ship for easier management
        player.SetShipPlatform(this);
    }

    /// <summary>
    /// Called when a player leaves the ship
    /// </summary>
    public void OnPlayerExitShip(PlayerController player)
    {
        player.SetShipPlatform(null);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            OnPlayerEnterShip(player);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            OnPlayerExitShip(player);
        }
    }
}