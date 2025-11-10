using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Repair station for fixing ship damage
/// Players can interact to repair holes and restore ship health
/// </summary>
public class RepairStation : NetworkBehaviour, IInteractable
{
    [Header("Repair Settings")]
    [SerializeField] private readonly float interactionRange = 2f;
    [SerializeField] private readonly float repairRate = 10f; // HP per second
    [SerializeField] private readonly float repairCost = 5f; // Resources per repair point (for future resource system)
    [SerializeField] private readonly bool requiresResources = true;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject repairEffect;
    [SerializeField] private AudioClip repairSound;

    private ShipHealth shipHealth;
    private GameObject currentRepairer;
    private bool isBeingRepaired = false;
    private readonly bool canRepair = true;

    public System.Action<bool> OnRepairStateChanged;

    void Awake()
    {
        shipHealth = GetComponentInParent<ShipHealth>();
        if (shipHealth == null)
        {
            Debug.LogError("RepairStation: No ShipHealth component found on parent object!");
        }
    }

    void Update()
    {
        if (!IsServer) return;

        // Handle continuous repair while being operated
        if (isBeingRepaired && currentRepairer != null && canRepair)
        {
            PerformRepair();
        }
    }

    public bool CanInteract(GameObject player)
    {
        // Check distance
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance > interactionRange) return false;

        // Check if ship needs repair
        if (shipHealth != null && shipHealth.IsFullyRepaired()) return false;

        // Check if already being repaired by someone else
        if (isBeingRepaired && currentRepairer != player) return false;

        return true;
    }

    public void Interact(GameObject player)
    {
        if (!isBeingRepaired)
        {
            StartRepairingServerRpc(player.GetComponent<NetworkObject>().NetworkObjectId);
        }
        else if (currentRepairer == player)
        {
            StopRepairingServerRpc();
        }
    }

    public string GetInteractionText()
    {
        if (shipHealth != null && shipHealth.IsFullyRepaired())
        {
            return "Ship is fully repaired";
        }
        
        if (!isBeingRepaired)
        {
            return "Press E to start repairs";
        }
        else
        {
            return "Press E to stop repairing";
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void StartRepairingServerRpc(ulong playerNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out NetworkObject playerObj))
        {
            currentRepairer = playerObj.gameObject;
            isBeingRepaired = true;
            OnRepairStateChanged?.Invoke(true);

            // Start repair effects
            StartRepairEffectsClientRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void StopRepairingServerRpc()
    {
        currentRepairer = null;
        isBeingRepaired = false;
        OnRepairStateChanged?.Invoke(false);

        // Stop repair effects
        StopRepairEffectsClientRpc();
    }

    void PerformRepair()
    {
        if (shipHealth == null) return;

        float repairAmount = repairRate * Time.deltaTime;
        
        // Check if we have enough resources (if required)
        if (requiresResources)
        {
            // Calculate resource cost based on repair amount
            float resourceCost = repairAmount * repairCost;
            // TODO: Implement resource checking system when resource manager is added
            // For now, assume we always have resources
            Debug.Log($"Repair would cost {resourceCost:F1} resources");
        }

        // Apply repair to the ship
        shipHealth.RepairDamage(repairAmount);

        // If ship is fully repaired, stop repairing
        if (shipHealth.IsFullyRepaired())
        {
            StopRepairingServerRpc();
        }
    }

    [ClientRpc]
    void StartRepairEffectsClientRpc()
    {
        if (repairEffect != null)
        {
            repairEffect.SetActive(true);
        }

        if (repairSound != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.clip = repairSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    [ClientRpc]
    void StopRepairEffectsClientRpc()
    {
        if (repairEffect != null)
        {
            repairEffect.SetActive(false);
        }

        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}