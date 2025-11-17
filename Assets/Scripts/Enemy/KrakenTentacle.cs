using UnityEngine;
using System.Collections;

/// <summary>
/// AI for the Kraken Tentacle.
/// Slams the deck, pauses, and retreats.
/// </summary>
public class KrakenTentacle : EnemyAI
{
    [SerializeField] private float slamAttackRange = 5f;
    [SerializeField] private float slamDuration = 3f; // Time vulnerable on deck
    [SerializeField] private float retreatTime = 5f; // Time hidden
    
    private Vector3 retreatPosition;
    private Vector3 slamPosition;
    private bool isSlamming = false;

    void Start()
    {
        retreatPosition = transform.position;
        // Disable NavMeshAgent, as this AI is scripted
        agent.enabled = false;
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SlamCycle());
        }
    }

    private IEnumerator SlamCycle()
    {
        while (State.Current_Health > 0)
        {
            // 1. Wait while retreated
            yield return new WaitForSeconds(retreatTime);
            
            // 2. Pick a target slam position (near a player or ship part)
            slamPosition = FindSlamTarget();
            
            // 3. Move to slam position (Slam!)
            // TODO: Add slam animation/visual
            transform.position = slamPosition;
            isSlamming = true;
            
            // TODO: Deal damage in an area
            Debug.Log("Tentacle SLAM!");

            // 4. Wait on deck (vulnerable)
            yield return new WaitForSeconds(slamDuration);
            
            // 5. Retreat
            transform.position = retreatPosition;
            isSlamming = false;
        }
    }
    
    private Vector3 FindSlamTarget()
    {
        // Simple: pick a random point on the upper deck
        // TODO: Make this target players or damaged ship parts
        return ShipController.Instance.transform.position + new Vector3(Random.Range(-5, 5), Random.Range(-2, 2), 0);
    }

    protected override float GetAttackRange()
    {
        return slamAttackRange;
    }
    
    protected override void PerformAttack()
    {
        // Attack is handled by the SlamCycle coroutine
    }
}