using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple ship controller without networking - for testing and single player
/// Handles ship movement and physics
/// </summary>
public class SimpleShipController : MonoBehaviour
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
    private SimpleShipHealth shipHealth;

    // Input values
    private float throttleInput;
    private float steerInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        shipHealth = GetComponent<SimpleShipHealth>();
    }

    void Start()
    {
        // Set physics properties
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
    }

    void Update()
    {
        HandleInput();
        ApplyMovement();
    }

    void HandleInput()
    {
        // Basic input for testing using new Input System
        throttleInput = 0f;
        steerInput = 0f;
        
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) throttleInput += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) throttleInput -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) steerInput -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) steerInput += 1f;
        }
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

    /// <summary>
    /// Called by crew members to control the ship
    /// </summary>
    public void SetShipControl(float throttle, float steer)
    {
        throttleInput = Mathf.Clamp(throttle, -1f, 1f);
        steerInput = Mathf.Clamp(steer, -1f, 1f);
    }

    /// <summary>
    /// Get current ship speed for UI/effects
    /// </summary>
    public float GetCurrentSpeed()
    {
        return rb.linearVelocity.magnitude;
    }
}