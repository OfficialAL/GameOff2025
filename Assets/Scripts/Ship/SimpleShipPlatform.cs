using UnityEngine;

/// <summary>
/// Simple ship platform without networking - for testing
/// Handles the ship as a moving platform that players can walk on
/// </summary>
public class SimpleShipPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] private LayerMask playerLayerMask = 1 << 6;
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
        if (!movePlayersWithShip) return;

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
        SimplePlayerController[] playersOnShip = FindObjectsByType<SimplePlayerController>(FindObjectsSortMode.None);
        
        foreach (SimplePlayerController player in playersOnShip)
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

    bool IsPlayerOnShip(SimplePlayerController player)
    {
        // Simple bounds check
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
    public void OnPlayerEnterShip(SimplePlayerController player)
    {
        player.SetShipPlatform(this);
    }

    /// <summary>
    /// Called when a player leaves the ship
    /// </summary>
    public void OnPlayerExitShip(SimplePlayerController player)
    {
        player.SetShipPlatform(null);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        SimplePlayerController player = other.GetComponent<SimplePlayerController>();
        if (player != null)
        {
            OnPlayerEnterShip(player);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        SimplePlayerController player = other.GetComponent<SimplePlayerController>();
        if (player != null)
        {
            OnPlayerExitShip(player);
        }
    }
}