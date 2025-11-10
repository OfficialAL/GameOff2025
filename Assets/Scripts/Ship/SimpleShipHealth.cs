using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple ship health without networking - for testing
/// Manages ship health and damage
/// </summary>
public class SimpleShipHealth : MonoBehaviour
{
    [Header("Ship Health Settings")]
    [SerializeField] private float maxHullIntegrity = 100f;
    [SerializeField] private int damageZones = 6;
    [SerializeField] private float floodRate = 2f;
    [SerializeField] private float pumpRate = 3f;

    [Header("Damage Effects")]
    [SerializeField] private GameObject holeEffectPrefab;
    [SerializeField] private GameObject waterEffectPrefab;
    [SerializeField] private Transform[] damageZoneTransforms;

    private List<float> zoneIntegrity = new List<float>();
    private float waterLevel = 0f;
    private bool isSinking = false;

    private List<GameObject> activeHoles = new List<GameObject>();
    private List<GameObject> activeWaterEffects = new List<GameObject>();

    public System.Action<float> OnWaterLevelChanged;
    public System.Action<int, float> OnZoneDamaged;
    public System.Action OnShipSunk;

    void Start()
    {
        // Initialize all zones to full health
        for (int i = 0; i < damageZones; i++)
        {
            zoneIntegrity.Add(maxHullIntegrity);
        }
    }

    void Update()
    {
        // Handle water level changes
        HandleWaterLevel();
        
        // Check if ship should sink
        CheckSinkingCondition();
    }

    void HandleWaterLevel()
    {
        // Increase water level based on damage
        float totalDamage = 0f;
        for (int i = 0; i < zoneIntegrity.Count; i++)
        {
            totalDamage += (maxHullIntegrity - zoneIntegrity[i]);
        }

        if (totalDamage > 0f)
        {
            waterLevel += (totalDamage / maxHullIntegrity) * floodRate * Time.deltaTime;
            OnWaterLevelChanged?.Invoke(waterLevel);
        }
    }

    void CheckSinkingCondition()
    {
        if (waterLevel >= 50f && !isSinking) // Ship sinks at 50% water level
        {
            isSinking = true;
            OnShipSunk?.Invoke();
            Debug.Log("Ship is sinking!");
        }
    }

    /// <summary>
    /// Apply damage to a specific zone
    /// </summary>
    public void DamageZone(int zoneIndex, float damage)
    {
        if (zoneIndex < 0 || zoneIndex >= zoneIntegrity.Count) return;

        zoneIntegrity[zoneIndex] = Mathf.Max(0f, zoneIntegrity[zoneIndex] - damage);
        OnZoneDamaged?.Invoke(zoneIndex, zoneIntegrity[zoneIndex]);

        // Create visual damage effects
        if (zoneIntegrity[zoneIndex] < maxHullIntegrity * 0.5f && damageZoneTransforms[zoneIndex] != null)
        {
            CreateDamageEffects(zoneIndex);
        }
    }

    void CreateDamageEffects(int zoneIndex)
    {
        if (damageZoneTransforms[zoneIndex] == null) return;

        // Create hole effect
        if (holeEffectPrefab != null)
        {
            GameObject hole = Instantiate(holeEffectPrefab, damageZoneTransforms[zoneIndex].position, Quaternion.identity);
            activeHoles.Add(hole);
        }

        // Create water effect
        if (waterEffectPrefab != null)
        {
            GameObject water = Instantiate(waterEffectPrefab, damageZoneTransforms[zoneIndex].position, Quaternion.identity);
            activeWaterEffects.Add(water);
        }
    }

    /// <summary>
    /// Check if the ship is fully repaired
    /// </summary>
    public bool IsFullyRepaired()
    {
        for (int i = 0; i < zoneIntegrity.Count; i++)
        {
            if (zoneIntegrity[i] < maxHullIntegrity)
                return false;
        }
        return waterLevel <= 0f;
    }

    /// <summary>
    /// Repair damage to the ship
    /// </summary>
    public void RepairDamage(float repairAmount)
    {
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

        if (allZonesRepaired && waterLevel > 0)
        {
            waterLevel = Mathf.Max(0f, waterLevel - (repairAmount * 0.5f));
            OnWaterLevelChanged?.Invoke(waterLevel);
        }
    }

    // Public getters
    public float GetWaterLevel() => waterLevel;
    public bool IsSinking() => isSinking;
    public float GetZoneIntegrity(int zoneIndex)
    {
        if (zoneIndex < 0 || zoneIndex >= zoneIntegrity.Count) return 0f;
        return zoneIntegrity[zoneIndex];
    }
}