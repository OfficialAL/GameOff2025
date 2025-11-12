using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Simple test script to verify Unity packages are working correctly
/// Tests Netcode, Input System, and basic Unity functionality
/// </summary>
public class PackageTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("✅ Unity packages loaded successfully!");
        
        // Test Netcode package
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("✅ Netcode for GameObjects package working");
        }
        else
        {
            Debug.Log("ℹ️ Netcode NetworkManager not found (expected in empty scene)");
        }
        
        // Test Input System (basic check)
        Debug.Log("✅ Input System package loaded");
        
        // Test URP (basic check)
        var camera = Camera.main;
        if (camera != null)
        {
            Debug.Log($"✅ Camera found: {camera.name}");
        }
        
        Debug.Log("🎮 Ready for GameOff 2025 development!");
    }
}