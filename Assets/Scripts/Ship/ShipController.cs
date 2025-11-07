using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Main ship controller for multiplayer pirate game
/// Handles ship movement, physics, and basic ship state
/// </summary>
public class ShipController : NetworkBehaviour
{
    [Header("Ship Movement")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float drag = 0.98f;
    [SerializeField] private float angularDrag = 0.95f;

    [Header("Ship Components")]
    [SerializeField] private Transform rudder;
    [SerializeField] private Transform[] sails;
    
    private Rigidbody2D rb;
    private ShipHealth shipHealth;
    private NetworkVariable<float> networkSpeed = new NetworkVariable<float>();
    private NetworkVariable<float> networkRotation = new NetworkVariable<float>();

    // Input values - only the server/host processes these
    private float throttleInput;
    private float steerInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        shipHealth = GetComponent<ShipHealth>();
    }

    void Start()
    {
        if (IsServer)
        {
            // Server sets initial physics properties
            rb.linearDamping = drag;
            rb.angularDamping = angularDrag;
        }
    }

    void Update()
    {
        if (!IsServer) return;

        HandleInput();
        ApplyMovement();
        SyncNetworkValues();
    }

    void HandleInput()
    {
        // In multiplayer, this would come from the player controlling the wheel
        // For now, using basic input for testing
        throttleInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    void ApplyMovement()
    {
        // Apply forward/backward thrust
        Vector2 thrust = transform.up * throttleInput * acceleration;
        rb.AddForce(thrust);

        // Apply steering (only when moving)
        if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f)
        {
            float turn = steerInput * turnSpeed * Time.deltaTime;
            rb.AddTorque(-turn);
        }

        // Clamp to max speed
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void SyncNetworkValues()
    {
        networkSpeed.Value = rb.linearVelocity.magnitude;
        networkRotation.Value = transform.eulerAngles.z;
    }

    /// <summary>
    /// Called by crew members to control the ship
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SetShipControlServerRpc(float throttle, float steer, ServerRpcParams rpcParams = default)
    {
        // Verify the player has permission to control this ship component
        throttleInput = Mathf.Clamp(throttle, -1f, 1f);
        steerInput = Mathf.Clamp(steer, -1f, 1f);
    }

    /// <summary>
    /// Get current ship speed for UI/effects
    /// </summary>
    public float GetCurrentSpeed()
    {
        return IsServer ? rb.linearVelocity.magnitude : networkSpeed.Value;
    }
}