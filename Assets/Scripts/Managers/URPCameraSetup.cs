using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Ensures proper camera setup for Universal Render Pipeline
/// Attach this to your Main Camera to auto-configure URP settings
/// </summary>
[RequireComponent(typeof(Camera))]
public class URPCameraSetup : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private bool autoConfigureOnStart = true;
    [SerializeField] private CameraRenderType renderType = CameraRenderType.Base;

    void Start()
    {
        if (autoConfigureOnStart)
        {
            SetupURPCamera();
        }
    }

    [ContextMenu("Setup URP Camera")]
    public void SetupURPCamera()
    {
        Camera cam = GetComponent<Camera>();
        
        // Check if Universal Additional Camera Data exists
        UniversalAdditionalCameraData cameraData = GetComponent<UniversalAdditionalCameraData>();
        
        if (cameraData == null)
        {
            // Add the Universal Additional Camera Data component
            cameraData = gameObject.AddComponent<UniversalAdditionalCameraData>();
            Debug.Log("Added Universal Additional Camera Data to " + gameObject.name);
        }

        // Configure camera for 2D pirate game
        cam.orthographic = true;
        cam.orthographicSize = 10f; // Adjust based on your game scale
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 1000f;
        
        // Set render type
        cameraData.renderType = renderType;
        
        // Enable post-processing if needed
        cameraData.renderPostProcessing = true;
        
        Debug.Log("URP Camera configured successfully!");
    }

    void Reset()
    {
        // Auto-setup when component is added
        SetupURPCamera();
    }
}