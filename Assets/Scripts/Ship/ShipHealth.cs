using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// Manages ship health, damage zones, and flooding mechanics
/// Similar to Barotrauma's hull integrity system
/// </summary>
public class ShipHealth : NetworkBehaviour
{
    [Header("Ship Health Settings")]
    [SerializeField] private float maxHullIntegrity = 100f;
    [SerializeField] private int damageZones = 6; // Number of sections that can be damaged
    [SerializeField] private float floodRate = 2f; // Water intake per second when damaged
    [SerializeField] private float pumpRate = 3f; // Water removal per second when pumping

    [Header("Damage Effects")]
    [SerializeField] private GameObject holeEffectPrefab;
    [SerializeField] private GameObject waterEffectPrefab;
    [SerializeField] private Transform[] damageZoneTransforms;

    // Network variables for synchronization
    private NetworkList<float> zoneIntegrity;
    private NetworkVariable<float> waterLevel = new NetworkVariable<float>();
    private NetworkVariable<bool> isSinking = new NetworkVariable<bool>();

    private List<GameObject> activeHoles = new List<GameObject>();
    private List<GameObject> activeWaterEffects = new List<GameObject>();

    public System.Action<float> OnWaterLevelChanged;
    public System.Action<int, float> OnZoneDamaged;
    public System.Action OnShipSunk;

    void Awake()
    {
        // Initialize damage zones
        zoneIntegrity = new NetworkList<float>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Initialize all zones to full health
            for (int i = 0; i < damageZones; i++)
            {
                zoneIntegrity.Add(maxHullIntegrity);
            }
        }

        // Subscribe to network variable changes for client updates
        waterLevel.OnValueChanged += OnWaterLevelUpdated;
        isSinking.OnValueChanged += OnSinkingStatusChanged;
        zoneIntegrity.OnListChanged += OnZoneIntegrityChanged;
    }

    void Update()
    {
        if (!IsServer) return;

        ProcessFlooding();
        CheckSinkingCondition();
    }

    /// <summary>
    /// Apply damage to a specific zone of the ship
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int zoneIndex, float damage, ServerRpcParams rpcParams = default)
    {
        if (zoneIndex < 0 || zoneIndex >= zoneIntegrity.Count) return;

        float currentIntegrity = zoneIntegrity[zoneIndex];
        float newIntegrity = Mathf.Max(0, currentIntegrity - damage);
        zoneIntegrity[zoneIndex] = newIntegrity;

        // Trigger damage effects on all clients
        TriggerDamageEffectsClientRpc(zoneIndex, newIntegrity);
    }

    /// <summary>
    /// Repair a damaged zone (called by crew members with repair tools)
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RepairZoneServerRpc(int zoneIndex, float repairAmount, ServerRpcParams rpcParams = default)
    {
        if (zoneIndex < 0 || zoneIndex >= zoneIntegrity.Count) return;

        float currentIntegrity = zoneIntegrity[zoneIndex];
        float newIntegrity = Mathf.Min(maxHullIntegrity, currentIntegrity + repairAmount);
        zoneIntegrity[zoneIndex] = newIntegrity;
    }

    /// <summary>
    /// Pump water out of the ship (called by crew members operating pumps)
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void PumpWaterServerRpc(float pumpEfficiency = 1f, ServerRpcParams rpcParams = default)
    {
        float currentWater = waterLevel.Value;
        float pumpAmount = pumpRate * pumpEfficiency * Time.deltaTime;
        waterLevel.Value = Mathf.Max(0, currentWater - pumpAmount);
    }

    void ProcessFlooding()
    {
        float totalFloodRate = 0f;

        // Calculate flood rate based on damaged zones
        for (int i = 0; i < zoneIntegrity.Count; i++)
        {
            float integrity = zoneIntegrity[i];
            if (integrity < maxHullIntegrity * 0.5f) // Zone is significantly damaged
            {
                float damagePercent = 1f - (integrity / maxHullIntegrity);
                totalFloodRate += floodRate * damagePercent;
            }
        }

        // Apply flooding
        if (totalFloodRate > 0)
        {
            waterLevel.Value = Mathf.Min(100f, waterLevel.Value + totalFloodRate * Time.deltaTime);
        }
    }

    void CheckSinkingCondition()
    {
        // Ship sinks if water level is too high
        bool shouldSink = waterLevel.Value > 80f;
        
        if (shouldSink && !isSinking.Value)
        {
            isSinking.Value = true;
            OnShipSunk?.Invoke();
        }
    }

    [ClientRpc]
    void TriggerDamageEffectsClientRpc(int zoneIndex, float newIntegrity)
    {
        if (damageZoneTransforms[zoneIndex] == null) return;

        // Create hole effect if zone is significantly damaged
        if (newIntegrity < maxHullIntegrity * 0.5f && activeHoles.Count <= zoneIndex)
        {
            if (holeEffectPrefab != null)
            {
                GameObject hole = Instantiate(holeEffectPrefab, damageZoneTransforms[zoneIndex]);
                activeHoles.Add(hole);
            }
        }

        OnZoneDamaged?.Invoke(zoneIndex, newIntegrity);
    }

    void OnWaterLevelUpdated(float oldValue, float newValue)
    {
        OnWaterLevelChanged?.Invoke(newValue);
    }

    void OnSinkingStatusChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            Debug.Log("Ship is sinking!");
        }
    }

    void OnZoneIntegrityChanged(NetworkListEvent<float> changeEvent)
    {
        // Handle visual updates when zone integrity changes
        UpdateDamageVisuals();
    }

    void UpdateDamageVisuals()
    {
        // Update visual effects based on current damage state
        // This would include hole effects, water effects, etc.
    }

    public float GetWaterLevel() => waterLevel.Value;
    public bool IsSinking() => isSinking.Value;
    public float GetZoneIntegrity(int zoneIndex)
    {
        if (zoneIndex < 0 || zoneIndex >= zoneIntegrity.Count) return 0f;
        return zoneIntegrity[zoneIndex];
    }

    /// <summary>
    /// Check if the ship is fully repaired
    /// </summary>
    public bool IsFullyRepaired()
    {
        if (!IsServer) return false;
        
        for (int i = 0; i < zoneIntegrity.Count; i++)
        {
            if (zoneIntegrity[i] < maxHullIntegrity)
                return false;
        }
        return waterLevel.Value <= 0f;
    }

    /// <summary>
    /// Repair damage to the ship
    /// </summary>
    public void RepairDamage(float repairAmount)
    {
        if (!IsServer) return;

        // Find the most damaged zone and repair it
        int mostDamagedZone = -1;
        float lowestIntegrity = maxHullIntegrity;
        
        for (int i = 0; i < zoneIntegrity.Count; i++)
        {
            if (zoneIntegrity[i] < lowestIntegrity)
            {
                lowestIntegrity = zoneIntegrity[i];
                mostDamagedZone = i;
            }
        }

        // Repair the most damaged zone
        if (mostDamagedZone >= 0)
        {
            float newIntegrity = Mathf.Min(zoneIntegrity[mostDamagedZone] + repairAmount, maxHullIntegrity);
            zoneIntegrity[mostDamagedZone] = newIntegrity;
            
            OnZoneDamaged?.Invoke(mostDamagedZone, newIntegrity);
        }

        // Reduce water level if all zones are repaired
        bool allZonesRepaired = true;
        for (int i = 0; i < zoneIntegrity.Count; i++)
        {
            if (zoneIntegrity[i] < maxHullIntegrity)
            {
                allZonesRepaired = false;
                break;
            }
        }

        if (allZonesRepaired && waterLevel.Value > 0)
        {
            waterLevel.Value = Mathf.Max(0f, waterLevel.Value - (repairAmount * 0.5f));
            OnWaterLevelChanged?.Invoke(waterLevel.Value);
        }
    }
}