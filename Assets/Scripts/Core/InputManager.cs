using UnityEngine;

/// <summary>
/// Singleton to handle all raw game input.
/// Other scripts read from this manager to get action states.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // Movement
    public Vector2 MovementInput { get; private set; }
    
    // Actions
    public bool InteractPressed { get; private set; }
    public bool MeleePressed { get; private set; }
    public bool RangedPressed { get; private set; }
    public bool PickUpPressed { get; private set; }

    // Station-specific
    public Vector2 StationInput { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Update()
    {
        // Read Movement (WASD or Arrow Keys)
        MovementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"), // A/D or Left/Right
            Input.GetAxisRaw("Vertical")  // W/S or Up/Down
        );

        // Read Action Keys
        InteractPressed = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space);
        MeleePressed = Input.GetKeyDown(KeyCode.Mouse0); // LMB
        RangedPressed = Input.GetKeyDown(KeyCode.Mouse1); // RMB
        PickUpPressed = Input.GetKeyDown(KeyCode.Q);

        // Read Station-specific keys (re-using horizontal/vertical)
        // This is for Mast (W/S) and Wheel (A/D)
        StationInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );
    }
}