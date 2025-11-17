using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

/// <summary>
/// Manages the Kraken boss encounter.
/// Spawns Tentacles and Octopuses.
/// </summary>
public class KrakenController : MonoBehaviourPun
{
    [Header("Spawning")]
    [SerializeField] private GameObject tentaclePrefab;
    [SerializeField] private GameObject octopusPrefab;
    [SerializeField] private List<Transform> tentacleSpawnPoints;
    
    [SerializeField] private int maxTentacles = 3;
    [SerializeField] private float tentacleRespawnTime = 10f;
    [SerializeField] private float octopusSpawnRate = 5f;

    private HostileShipController shipState;
    private List<GameObject> activeTentacles = new List<GameObject>();
    private float octopusSpawnTimer;

    void Awake()
    {
        shipState = GetComponent<HostileShipController>();
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // --- Tentacle Spawning ---
        // Clean up destroyed tentacles
        activeTentacles.RemoveAll(item => item == null);
        
        if (activeTentacles.Count < maxTentacles)
        {
            // TODO: Add respawn timer
            SpawnTentacle();
        }

        // --- Octopus Spawning ---
        octopusSpawnTimer -= Time.deltaTime;
        if (octopusSpawnTimer <= 0)
        {
            octopusSpawnTimer = octopusSpawnRate;
            SpawnOctopus();
        }
    }

    private void SpawnTentacle()
    {
        Transform spawnPoint = tentacleSpawnPoints[Random.Range(0, tentacleSpawnPoints.Count)];
        GameObject tentacle = PhotonNetwork.Instantiate(tentaclePrefab.name, spawnPoint.position, spawnPoint.rotation);
        activeTentacles.Add(tentacle);
    }

    private void SpawnOctopus()
    {
        // TODO: Pick spawn point
        PhotonNetwork.Instantiate(octopusPrefab.name, transform.position, Quaternion.identity);
    }
    
    // TODO: Add logic for Kraken leaving
}