using UnityEngine;

/// <summary>
/// Simple test script to verify ship systems work without errors
/// Attach this to any GameObject in a test scene
/// </summary>
public class ShipSystemTest : MonoBehaviour
{
    [Header("Test Components")]
    public ShipController shipController;
    public ShipHealth shipHealth;
    public ShipPlatform shipPlatform;
    public PlayerController[] players;

    void Start()
    {
        Debug.Log("=== Ship System Test Started ===");
        
        // Test 1: Check if ship components exist
        TestShipComponents();
        
        // Test 2: Check if player components exist
        TestPlayerComponents();
        
        // Test 3: Check if network components are set up
        TestNetworkComponents();
        
        Debug.Log("=== Ship System Test Completed ===");
    }

    void TestShipComponents()
    {
        Debug.Log("Testing Ship Components...");
        
        if (shipController != null)
            Debug.Log("✓ ShipController found");
        else
            Debug.LogWarning("✗ ShipController missing");
            
        if (shipHealth != null)
            Debug.Log("✓ ShipHealth found");
        else
            Debug.LogWarning("✗ ShipHealth missing");
            
        if (shipPlatform != null)
            Debug.Log("✓ ShipPlatform found");
        else
            Debug.LogWarning("✗ ShipPlatform missing");
    }

    void TestPlayerComponents()
    {
        Debug.Log("Testing Player Components...");
        
        if (players != null && players.Length > 0)
        {
            Debug.Log($"✓ Found {players.Length} players");
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                    Debug.Log($"  ✓ Player {i + 1} found");
                else
                    Debug.LogWarning($"  ✗ Player {i + 1} missing");
            }
        }
        else
        {
            Debug.LogWarning("✗ No players assigned");
        }
    }

    void TestNetworkComponents()
    {
        Debug.Log("Testing Network Components...");
        
        // Check if Unity Netcode is properly set up
        if (Unity.Netcode.NetworkManager.Singleton != null)
            Debug.Log("✓ NetworkManager found");
        else
            Debug.LogWarning("✗ NetworkManager missing - Add NetworkManager to scene for multiplayer");
    }

    void Update()
    {
        // Simple runtime test - check if ship is moving
        if (shipController != null && Time.frameCount % 60 == 0) // Every second
        {
            float speed = shipController.GetCurrentSpeed();
            if (speed > 0.1f)
                Debug.Log($"Ship moving at speed: {speed:F2}");
        }
    }
}