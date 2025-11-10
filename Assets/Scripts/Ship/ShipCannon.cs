using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Cannon system for the pirate ship
/// Players can interact to load, aim, and fire cannons
/// </summary>
public class ShipCannon : NetworkBehaviour, IInteractable
{
    [Header("Cannon Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private float reloadTime = 3f;
    [SerializeField] private float cannonPower = 20f;
    [SerializeField] private GameObject cannonballPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int maxAmmo = 10;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioClip fireSound;

    private NetworkVariable<bool> isLoaded = new NetworkVariable<bool>(true);
    private NetworkVariable<int> ammoCount = new NetworkVariable<int>();
    private NetworkVariable<bool> isReloading = new NetworkVariable<bool>();
    
    private GameObject currentOperator;
    private bool isBeingOperated = false;
    private float reloadTimer = 0f;

    public System.Action<int> OnAmmoChanged;
    public System.Action<bool> OnLoadedStateChanged;

    void Awake()
    {
        if (firePoint == null)
            firePoint = transform;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            ammoCount.Value = maxAmmo;
        }

        // Subscribe to network variable changes
        isLoaded.OnValueChanged += OnLoadedChanged;
        ammoCount.OnValueChanged += OnAmmoCountChanged;
    }

    void Update()
    {
        if (!IsServer) return;

        // Handle reloading
        if (isReloading.Value)
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer >= reloadTime)
            {
                CompleteReload();
            }
        }

        // Handle cannon aiming and firing input
        if (isBeingOperated && currentOperator != null)
        {
            HandleCannonControls();
        }
    }

    void HandleCannonControls()
    {
        // Simple mouse aiming for now - you might want keyboard controls
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        Vector2 aimDirection = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        
        // Clamp cannon rotation (cannons can't rotate 360 degrees)
        angle = Mathf.Clamp(angle, -90f, 90f);
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Fire cannon
        if (Input.GetMouseButtonDown(0) && isLoaded.Value && !isReloading.Value)
        {
            FireCannonServerRpc();
        }
    }

    public bool CanInteract(GameObject player)
    {
        // Check if cannon is not already being operated
        if (isBeingOperated && currentOperator != player) return false;

        // Check distance
        float distance = Vector2.Distance(transform.position, player.transform.position);
        return distance <= interactionRange;
    }

    public void Interact(GameObject player)
    {
        if (!isBeingOperated)
        {
            StartOperatingCannonServerRpc(player.GetComponent<NetworkObject>().NetworkObjectId);
        }
        else if (currentOperator == player)
        {
            StopOperatingCannonServerRpc();
        }
    }

    public string GetInteractionText()
    {
        if (!isBeingOperated)
        {
            if (ammoCount.Value <= 0)
                return "No ammunition left";
            else if (isReloading.Value)
                return "Cannon is reloading...";
            else
                return "Press E to operate cannon";
        }
        else
        {
            return "Press E to stop operating cannon";
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void StartOperatingCannonServerRpc(ulong playerNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out NetworkObject playerObj))
        {
            currentOperator = playerObj.gameObject;
            isBeingOperated = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void StopOperatingCannonServerRpc()
    {
        currentOperator = null;
        isBeingOperated = false;
    }

    [ServerRpc(RequireOwnership = false)]
    void FireCannonServerRpc()
    {
        if (!isLoaded.Value || isReloading.Value || ammoCount.Value <= 0) return;

        // Fire the cannon
        FireCannonClientRpc();
        
        // Update state
        isLoaded.Value = false;
        ammoCount.Value--;
        
        // Start reloading
        StartReload();
    }

    [ClientRpc]
    void FireCannonClientRpc()
    {
        // Visual and audio effects
        if (muzzleFlash != null)
            muzzleFlash.Play();
            
        if (fireSound != null)
            AudioSource.PlayClipAtPoint(fireSound, transform.position);

        // Create cannonball (if on server)
        if (IsServer && cannonballPrefab != null)
        {
            GameObject cannonball = Instantiate(cannonballPrefab, firePoint.position, firePoint.rotation);
            
            // Add force to cannonball
            Rigidbody2D cannonballRb = cannonball.GetComponent<Rigidbody2D>();
            if (cannonballRb != null)
            {
                Vector2 fireDirection = firePoint.right; // Assuming cannon points right
                cannonballRb.AddForce(fireDirection * cannonPower, ForceMode2D.Impulse);
            }
        }
    }

    void StartReload()
    {
        isReloading.Value = true;
        reloadTimer = 0f;
    }

    void CompleteReload()
    {
        isReloading.Value = false;
        isLoaded.Value = true;
        reloadTimer = 0f;
    }

    void OnLoadedChanged(bool oldValue, bool newValue)
    {
        OnLoadedStateChanged?.Invoke(newValue);
    }

    void OnAmmoCountChanged(int oldValue, int newValue)
    {
        OnAmmoChanged?.Invoke(newValue);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw fire direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(firePoint.position, firePoint.right * 5f);
    }
}