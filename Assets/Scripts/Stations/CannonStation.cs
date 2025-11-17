using UnityEngine;
using PirateCoop;
using Photon.Pun;

/// <summary>
/// Cannon Station (Station 3): Allows player to aim and fire cannons.
/// </summary>
public class CannonStation : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject cannonballPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float reloadTime = 6.0f;
    [SerializeField] private float cannonballSpeed = 20f;
    
    // Firing arc (e.g., 45 degrees left to 45 degrees right of center)
    [SerializeField] private float minAngle = -45f;
    [SerializeField] private float maxAngle = 45f;

    private PlayerController interactingPlayer;
    private bool isBusy = false;
    private bool isDisabled = false;
    private bool isReloading = false;
    private float currentAimAngle = 0f;

    void Update()
    {
        if (interactingPlayer != null && !isDisabled)
        {
            HandleAiming();
            HandleFiring();
        }
    }

    private void HandleAiming()
    {
        // Player uses A/D to aim
        float aimInput = InputManager.Instance.StationInput.x;
        currentAimAngle -= aimInput * 50f * Time.deltaTime; // 50f is aim speed
        currentAimAngle = Mathf.Clamp(currentAimAngle, minAngle, maxAngle);
        
        // Rotate the cannon sprite
        transform.localRotation = Quaternion.Euler(0, 0, currentAimAngle);
    }

    private void HandleFiring()
    {
        if (InputManager.Instance.RangedPressed && !isReloading)
        {
            // Fire!
            isReloading = true;
            Debug.Log("Cannon FIRE!");
            
            // Call RPC to spawn cannonball on all clients
            photonView.RPC("RPC_FireCannon", RpcTarget.All, firePoint.position, firePoint.rotation);
            
            Invoke(nameof(FinishReload), reloadTime);
        }
    }

    [PunRPC]
    private void RPC_FireCannon(Vector3 position, Quaternion rotation)
    {
        // All clients spawn a visual-only cannonball
        // The host will handle damage calculation
        GameObject ball = Instantiate(cannonballPrefab, position, rotation);
        ball.GetComponent<Rigidbody2D>().velocity = ball.transform.up * cannonballSpeed;
    }

    private void FinishReload()
    {
        isReloading = false;
    }

    public void Interact(PlayerController player)
    {
        interactingPlayer = player;
        isBusy = true;
    }

    public void StopInteract()
    {
        interactingPlayer = null;
        isBusy = false;
    }

    public bool CanInteract(PlayerController player)
    {
        return !isBusy && !isDisabled && player.State.Carried_Item == CarriedItemType.None;
    }

    public string GetInteractPrompt(PlayerController player)
    {
        if (isDisabled) return "[Station Broken]";
        if (isBusy) return "[Cannon Busy]";
        if (isReloading) return "[Reloading...]";
        return "Use Cannon (E)";
    }

    public void SetDisabled(bool disabled)
    {
        isDisabled = disabled;
        if (isDisabled)
        {
            StopInteract();
        }
    }
}