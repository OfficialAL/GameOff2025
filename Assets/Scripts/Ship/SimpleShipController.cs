using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

/// <summary>
/// Simple ship controller without networking - for testing and single player
/// Handles ship movement, journey progress, and event triggers
/// </summary>
public class SimpleShipController : MonoBehaviour
{
    [Header("Ship Movement (Visual Effects Only)")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float rockingIntensity = 2f;
    [SerializeField] private bool enableVisualEffects = true;

    [Header("Journey System")]
    [SerializeField] private float journeyProgressSpeed = 1f; // Units per second
    [SerializeField] private float journeyTotalDistance = 100f; // Total journey length
    [SerializeField] private bool journeyActive = false;
    [SerializeField] private float[] eventThresholds = { 25f, 50f, 75f, 90f }; // Progress points for events

    [Header("Ship Components")]
    [SerializeField] private Transform rudder;
    [SerializeField] private Transform[] sails;
    [Tooltip("Visual root that will receive rocking offsets. Leave null to auto-find a child named 'Visuals'.")]
    [SerializeField] private Transform visualRoot;

    [Header("Journey Events")]
    public UnityEvent<float> OnJourneyProgressChanged; // Progress percentage (0-100)
    public UnityEvent<int> OnJourneyEventTriggered; // Event index triggered
    public UnityEvent OnJourneyStarted;
    public UnityEvent OnJourneyCompleted;
    public UnityEvent OnJourneyStopped;
    
    private SimpleShipHealth shipHealth;
    
    // Visual effect variables
    private float currentSpeed = 0f;
    private float currentTurnRate = 0f;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float rockingTime = 0f;

    // Journey variables
    private float currentJourneyProgress = 0f; // Current distance traveled
    private bool[] eventTriggered; // Track which events have been triggered
    private bool journeyCompleted = false;

    // Input values
    private float throttleInput;
    private float steerInput;

    void Awake()
    {
        shipHealth = GetComponent<SimpleShipHealth>();
    }

    void Start()
    {
        // Store original position for rocking effect
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Auto-find a child named "Visuals" for safe rocking, so colliders on the root remain static
        if (visualRoot == null)
        {
            var candidate = transform.Find("Visuals");
            if (candidate != null) visualRoot = candidate;
        }
        
        // Initialize event tracking array
        eventTriggered = new bool[eventThresholds.Length];
    }

    void Update()
    {
        HandleInput();
        UpdateVisualEffects();
        UpdateJourneyProgress();
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

    void UpdateVisualEffects()
    {
        if (!enableVisualEffects) return;

        // Update speed based on throttle input
        currentSpeed = Mathf.Lerp(currentSpeed, throttleInput * maxSpeed, acceleration * Time.deltaTime);
        currentTurnRate = Mathf.Lerp(currentTurnRate, steerInput * turnSpeed, acceleration * Time.deltaTime);

        // Create rocking motion based on speed
        rockingTime += Time.deltaTime * (1f + currentSpeed * 0.1f);
        float rockX = Mathf.Sin(rockingTime) * rockingIntensity * 0.01f * (1f + currentSpeed * 0.1f);
        float rockY = Mathf.Cos(rockingTime * 0.8f) * rockingIntensity * 0.005f * (1f + currentSpeed * 0.1f);
        
        // Keep the ship root static for colliders; apply rocking only to the visual root
        transform.position = originalPosition;
        transform.rotation = originalRotation; // never rotate the root

        if (visualRoot != null)
        {
            visualRoot.localPosition = new Vector3(rockX, rockY, 0);
        }
        else
        {
            // Fallback: if no visual root is set, minimally adjust position (may cause collisions)
            transform.position = originalPosition + new Vector3(rockX, rockY, 0);
        }

        // Rotate sails based on input (if assigned)
        if (sails != null && sails.Length > 0)
        {
            foreach (Transform sail in sails)
            {
                if (sail != null)
                {
                    // Animate sails based on throttle input
                    float sailRotation = Mathf.Sin(Time.time * 2f) * 5f * Mathf.Abs(throttleInput);
                    sail.localRotation = Quaternion.Euler(0, 0, sailRotation);
                }
            }
        }

        // Rotate rudder based on steering (if assigned)
        if (rudder != null)
        {
            float rudderAngle = steerInput * 30f; // Max 30 degree turn
            rudder.localRotation = Quaternion.Euler(0, 0, rudderAngle);
        }
    }

    void UpdateJourneyProgress()
    {
        if (!journeyActive || journeyCompleted) return;

        // Progress the journey based on current speed and time
        float speedMultiplier = Mathf.Clamp01(currentSpeed / maxSpeed);
        float progressDelta = journeyProgressSpeed * speedMultiplier * Time.deltaTime;
        currentJourneyProgress += progressDelta;

        // Calculate progress percentage
        float progressPercentage = (currentJourneyProgress / journeyTotalDistance) * 100f;
        progressPercentage = Mathf.Clamp(progressPercentage, 0f, 100f);

        // Trigger progress update event
        OnJourneyProgressChanged?.Invoke(progressPercentage);

        // Check for journey events
        CheckJourneyEvents(progressPercentage);

        // Check if journey is complete
        if (currentJourneyProgress >= journeyTotalDistance && !journeyCompleted)
        {
            CompleteJourney();
        }
    }

    void CheckJourneyEvents(float progressPercentage)
    {
        for (int i = 0; i < eventThresholds.Length; i++)
        {
            if (!eventTriggered[i] && progressPercentage >= eventThresholds[i])
            {
                eventTriggered[i] = true;
                OnJourneyEventTriggered?.Invoke(i);
                Debug.Log($"Journey Event {i} triggered at {eventThresholds[i]}% progress!");
            }
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
        return currentSpeed;
    }

    /// <summary>
    /// Get current turn rate for UI/effects
    /// </summary>
    public float GetCurrentTurnRate()
    {
        return currentTurnRate;
    }

    // ===== JOURNEY CONTROL METHODS =====

    /// <summary>
    /// Start the journey progress
    /// </summary>
    public void StartJourney()
    {
        if (!journeyCompleted)
        {
            journeyActive = true;
            OnJourneyStarted?.Invoke();
            Debug.Log("Journey started!");
        }
    }

    /// <summary>
    /// Stop the journey progress
    /// </summary>
    public void StopJourney()
    {
        if (journeyActive)
        {
            journeyActive = false;
            OnJourneyStopped?.Invoke();
            Debug.Log("Journey stopped!");
        }
    }

    /// <summary>
    /// Toggle journey state (start/stop)
    /// </summary>
    public void ToggleJourney()
    {
        if (journeyActive)
            StopJourney();
        else
            StartJourney();
    }

    /// <summary>
    /// Complete the journey
    /// </summary>
    public void CompleteJourney()
    {
        if (!journeyCompleted)
        {
            journeyCompleted = true;
            journeyActive = false;
            OnJourneyCompleted?.Invoke();
            Debug.Log("Journey completed!");
        }
    }

    /// <summary>
    /// Reset the journey to start over
    /// </summary>
    public void ResetJourney()
    {
        currentJourneyProgress = 0f;
        journeyCompleted = false;
        journeyActive = false;
        
        // Reset event triggers
        for (int i = 0; i < eventTriggered.Length; i++)
        {
            eventTriggered[i] = false;
        }
        
        OnJourneyProgressChanged?.Invoke(0f);
        Debug.Log("Journey reset!");
    }

    /// <summary>
    /// Get current journey progress percentage (0-100)
    /// </summary>
    public float GetJourneyProgressPercentage()
    {
        return (currentJourneyProgress / journeyTotalDistance) * 100f;
    }

    /// <summary>
    /// Check if journey is currently active
    /// </summary>
    public bool IsJourneyActive()
    {
        return journeyActive;
    }

    /// <summary>
    /// Check if journey is completed
    /// </summary>
    public bool IsJourneyCompleted()
    {
        return journeyCompleted;
    }

    void OnDrawGizmosSelected()
    {
        // Show interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
        
        // Show visual effect range
        if (enableVisualEffects)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(originalPosition, Vector3.one * rockingIntensity * 0.1f);
        }

        // Show journey progress
        if (Application.isPlaying)
        {
            Gizmos.color = journeyActive ? Color.green : Color.red;
            Vector3 progressStart = transform.position + Vector3.right * 3f;
            Vector3 progressEnd = progressStart + Vector3.right * 5f;
            float progressPercent = GetJourneyProgressPercentage() / 100f;
            Vector3 currentProgress = Vector3.Lerp(progressStart, progressEnd, progressPercent);
            
            Gizmos.DrawLine(progressStart, progressEnd);
            Gizmos.DrawWireSphere(currentProgress, 0.2f);
        }
    }
}