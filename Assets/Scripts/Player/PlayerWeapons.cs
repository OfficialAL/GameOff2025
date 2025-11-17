using UnityEngine;
using Photon.Pun;
using PirateCoop;

/// <summary>
/// Manages the player's personal weapons (Cutlass, Pistol, Blunderbuss).
/// Handles input, reloads, and attack logic.
/// </summary>
public class PlayerWeapons : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PhotonView photonView;
    private InputManager inputManager;

    [Header("Cutlass (LMB)")]
    [SerializeField] private float cutlassRange = 1.5f;
    [SerializeField] private int cutlassDamage = 1;
    [SerializeField] private float cutlassCooldown = 0.5f; // High rate of fire
    private float lastCutlassSwing;

    [Header("Pistol (RMB)")]
    [SerializeField] private float pistolReloadTime = 1.5f;
    [SerializeField] private int pistolDamage = 1;
    private bool isPistolReloading = false;

    [Header("Blunderbuss (RMB)")]
    [SerializeField] private float blunderbussReloadTime = 3.0f;
    [SerializeField] private int blunderbussDamage = 2;
    [SerializeField] private float blunderbussKnockback = 5f;
    [SerializeField] private float blunderbussBreakChance = 0.1f; // 10%
    private bool isBlunderbussReloading = false;
    
    // Projectile prefab for Pistol/Blunderbuss (to be created)
    // [SerializeField] private GameObject projectilePrefab; 

    void Start()
    {
        if (photonView.IsMine)
        {
            inputManager = InputManager.Instance;
        }
    }

    void Update()
    {
        if (!photonView.IsMine || playerController.State.Is_Unconscious)
        {
            return;
        }

        HandleWeaponInput();
    }

    private void HandleWeaponInput()
    {
        // --- Melee Attack (LMB) ---
        if (inputManager.MeleePressed && Time.time > lastCutlassSwing + cutlassCooldown)
        {
            // Auto-drop ALL items (Lumber, Treasure, Blunderbuss)
            if (playerController.State.Carried_Item != CarriedItemType.None)
            {
                playerController.DropItem();
            }
            
            lastCutlassSwing = Time.time;
            PerformCutlassAttack();
        }

        // --- Ranged Attack (RMB) ---
        if (inputManager.RangedPressed)
        {
            if (playerController.State.Carried_Item == CarriedItemType.Blunderbuss)
            {
                // Blunderbuss does NOT auto-drop
                if (!isBlunderbussReloading)
                {
                    PerformBlunderbussAttack();
                }
            }
            else
            {
                // Auto-drop Lumber or Treasure
                if (playerController.State.Carried_Item == CarriedItemType.Lumber || 
                    playerController.State.Carried_Item == CarriedItemType.Treasure)
                {
                    playerController.DropItem();
                }

                if (!isPistolReloading)
                {
                    PerformPistolAttack();
                }
            }
        }
    }

    private void PerformCutlassAttack()
    {
        Debug.Log("Swing Cutlass!");
        // Perform a simple OverlapCircle to find enemies
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, cutlassRange);
        foreach (Collider2D hit in hits)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                // Call RPC on the enemy to take damage
                enemy.photonView.RPC("RPC_TakeDamage", RpcTarget.All, cutlassDamage);
            }
        }
    }

    private void PerformPistolAttack()
    {
        Debug.Log("Fire Pistol!");
        isPistolReloading = true;
        // TODO: Spawn projectile or use Raycast
        // For now, just log and start reload
        Invoke(nameof(FinishPistolReload), pistolReloadTime);
    }
    private void FinishPistolReload()
    {
        isPistolReloading = false;
    }

    private void PerformBlunderbussAttack()
    {
        Debug.Log("Fire Blunderbuss!");
        isBlunderbussReloading = true;
        
        // 1. Apply knockback to user
        playerController.ApplyKnockback(-transform.up * blunderbussKnockback);

        // 2. Perform cone attack
        // (Simplified as a wide OverlapCircle for now)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 5f);
        foreach (Collider2D hit in hits)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.photonView.RPC("RPC_TakeDamage", RpcTarget.All, blunderbussDamage);
                // TODO: Apply knockback to enemy
            }
        }

        // 3. Check for break
        if (Random.value < blunderbussBreakChance)
        {
            Debug.Log("Blunderbuss broke!");
            playerController.State.Carried_Item = CarriedItemType.None;
        }

        Invoke(nameof(FinishBlunderbussReload), blunderbussReloadTime);
    }
    private void FinishBlunderbussReload()
    {
        isBlunderbussReloading = false;
    }
}