using UnityEngine;
using Photon.Pun;
using PirateCoop;
using System.Collections;

/// <summary>
/// Main controller for the player avatar.
/// Handles movement, state, interaction, health, and unconsciousness.
/// </summary>
[RequireComponent(typeof(PhotonView), typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviourPun, IPunObservable, IInteractable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform spriteTransform;

    [Header("Sway")]
    [SerializeField] private float swayAmount = 15f;
    [SerializeField] private float swaySpeed = 3f;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 1.5f;

    [Header("State")]
    [SerializeField] private int maxHealth = 100;
    
    // Components
    private Rigidbody2D rb;
    private InputManager inputManager;
    private Collider2D col;
	private PlayerWeapons playerWeapons;

    // State
    public PlayerState State { get; private set; }
    private IInteractable currentStation;
    private IInteractable nearestInteractable;

    // Carrying
    private PlayerController carriedPlayer;
    private Transform originalParent;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
		playerWeapons = GetComponent<PlayerWeapons>();
        originalParent = transform.parent;
        
        // Initialize State
        State = new PlayerState
        {
            Player_ID = photonView.Owner.ActorNumber.ToString(),
            Display_Name = PhotonNetwork.NickName,
            Current_Health = maxHealth,
            Is_Unconscious = false,
            Carried_Item = CarriedItemType.None
        };

        if (!photonView.IsMine)
        {
            rb.isKinematic = true;
        }
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            inputManager = InputManager.Instance;
            
            // Set up local player's camera and HUD
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 0, -10);
            
            GameHUD.Instance.SetMaxHealth(State.Current_Health);
        }
    }
	
	
	
	

    void Update()
    {
        if (!photonView.IsMine)
        {
            // This object is controlled by another player
            return;
        }

        if (State.Is_Unconscious)
        {
            // Cannot do anything while unconscious
            rb.velocity = Vector2.zero;
            return;
        }

        // --- Handle Input ---
        HandleMovement();
        HandleInteraction();

        // --- Debug ---
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TakeDamage(30);
        }
    }

    private void HandleMovement()
    {
        if (currentStation != null)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 moveInput = inputManager.MovementInput;
        rb.velocity = moveInput.normalized * moveSpeed;

        // ... (Sway and Facing Direction logic as before) ...
    }

    private void HandleInteraction()
    {
        FindNearestInteractable();
        
        // Handle UI prompt
        // if (nearestInteractable != null) { ShowPrompt(nearestInteractable.GetInteractPrompt(this)); }
        // else { HidePrompt(); }

        if (inputManager.InteractPressed)
        {
            if (currentStation != null)
            {
                currentStation.StopInteract();
                currentStation = null;
                State.Interacting_Station = null;
            }
            else if (nearestInteractable != null && nearestInteractable.CanInteract(this))
            {
                // If we are carrying a teammate, the only interactable
                // we care about is a Bed.
                if (State.Carried_Item == CarriedItemType.Teammate)
                {
                    BedStation bed = nearestInteractable as BedStation;
                    if (bed != null)
                    {
                        bed.Interact(this); // This will handle placing the player
                    }
                }
                else
                {
                    // Standard interaction
                    currentStation = nearestInteractable;
                    currentStation.Interact(this);
                }
            }
        }
    }

    private void FindNearestInteractable()
    {
        // Simple circle cast to find interactables
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange);
        nearestInteractable = null;
        float closestDist = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    nearestInteractable = interactable;
                }
            }
        }
    }

    /// <summary>
    /// Drops the currently carried item onto the ground.
    /// </summary>
    public void DropItem()
    {
        if (State.Carried_Item == CarriedItemType.None) return;

        // If carrying a player, un-parent them
        if (State.Carried_Item == CarriedItemType.Teammate && carriedPlayer != null)
        {
            carriedPlayer.photonView.RPC("RPC_SetPhysicsActive", RpcTarget.All, true, false);
            carriedPlayer = null;
        }
        else
        {
            // TODO: Instantiate a DroppedItem prefab
            Debug.Log($"Dropped item: {State.Carried_Item}");
        }

        State.Carried_Item = CarriedItemType.None;
    }
    
    // --- Health and Damage ---

    /// <summary>
    /// Public method to apply damage. Called by projectiles, AI, etc.
    /// This will run on the client that owns this player.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (!photonView.IsMine || State.Is_Unconscious) return;

        // Call an RPC to sync health across all clients
        photonView.RPC("RPC_TakeDamage", RpcTarget.All, amount);
    }

    [PunRPC]
    public void RPC_TakeDamage(int amount)
    {
        State.Current_Health -= amount;
        if (State.Current_Health <= 0)
        {
            State.Current_Health = 0;
            if (!State.Is_Unconscious)
            {
                // Set unconscious state
                photonView.RPC("RPC_SetUnconscious", RpcTarget.All, true);
            }
        }

        if (photonView.IsMine)
        {
            GameHUD.Instance.UpdateHealth(State.Current_Health);
            // TODO: Interrupt any 3-second actions (e.g., repair)
        }
    }
	
	/// <summary>
	/// Applies a physics force to the player (e.g., Blunderbuss knockback).
	/// </summary>
	public void ApplyKnockback(Vector2 force)
	{
		if (photonView.IsMine)
		{
			rb.AddForce(force, ForceMode2D.Impulse);
		}
	}

    [PunRPC]
    public void RPC_SetUnconscious(bool isUnconscious)
    {
        State.Is_Unconscious = isUnconscious;
        col.enabled = isUnconscious; // As an interactable
        
        if (isUnconscious)
        {
            Debug.Log($"{State.Display_Name} is unconscious!");
            // Change sprite to "Knocked-Out"
            spriteTransform.rotation = Quaternion.Euler(0, 0, 90f); 
            
            if (photonView.IsMine)
            {
                // Stop all actions
                if(currentStation != null)
                {
                    currentStation.StopInteract();
                    currentStation = null;
                }
                DropItem();
            }
        }
        else
        {
            Debug.Log($"{State.Display_Name} has been revived!");
            // Change sprite back to normal
            spriteTransform.rotation = Quaternion.identity;
            
            if (photonView.IsMine)
            {
                State.Current_Health = (int)(maxHealth * 0.3f); // Revive at 30%
                GameHUD.Instance.UpdateHealth(State.Current_Health);
            }
        }
    }
    
    [PunRPC]
    public void RPC_SetPhysicsActive(bool active, bool isCarried)
    {
        // Disables Rigidbody and Collider for carried player
        rb.isKinematic = !active;
        col.enabled = !isCarried; // Collider is off if carried
        
        if (isCarried)
        {
            transform.SetParent(null); // Becomes child of carrier later
            transform.localPosition = new Vector3(0, 0.5f, 0); // Position on carrier
        }
        else
        {
            transform.SetParent(originalParent); // Return to root
        }
    }

    // --- IInteractable Implementation (for being picked up) ---

    public void Interact(PlayerController player)
    {
        // This is called when another player interacts with *this* unconscious player
        if (!State.Is_Unconscious) return;
        
        player.StartCarryingPlayer(this);
    }

    public void StopInteract() { } // Not used
	
	

    public bool CanInteract(PlayerController player)
    {
        // Can be interacted with if unconscious and player has free hands
        return State.Is_Unconscious && player.State.Carried_Item == CarriedItemType.None;
    }

    public string GetInteractPrompt(PlayerController player)
    {
        return State.Is_Unconscious ? "Carry Teammate (E)" : "";
    }
    
    // --- Carrying Logic ---
    
    public void StartCarryingPlayer(PlayerController targetPlayer)
    {
        State.Carried_Item = CarriedItemType.Teammate;
        carriedPlayer = targetPlayer;
        
        // Sync physics state and parenting
        targetPlayer.photonView.RPC("RPC_SetPhysicsActive", RpcTarget.All, false, true);
        targetPlayer.transform.SetParent(transform);
    }
    
    public PlayerController StopCarryingPlayer()
    {
        if (carriedPlayer == null) return null;
        
        PlayerController droppedPlayer = carriedPlayer;
        droppedPlayer.photonView.RPC("RPC_SetPhysicsActive", RpcTarget.All, true, false);
        
        carriedPlayer = null;
        State.Carried_Item = CarriedItemType.None;
        return droppedPlayer;
    }
	
	public void PickUpItem(DroppedItem item)
    {
        if (State.Carried_Item != CarriedItemType.None) return;

        // Convert DroppedItemType to CarriedItemType
        switch (item.GetItemType())
        {
            case DroppedItemType.Lumber:
                State.Carried_Item = CarriedItemType.Lumber;
                break;
            case DroppedItemType.Treasure:
                State.Carried_Item = CarriedItemType.Treasure;
                break;
            case DroppedItemType.Blunderbuss:
                State.Carried_Item = CarriedItemType.Blunderbuss;
                break;
        }
        
        Debug.Log($"Picked up: {State.Carried_Item}");
    }

    // --- PUN Sync ---
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(State.Current_Health);
            stream.SendNext(State.Is_Unconscious);
            stream.SendNext(State.Carried_Item);
        }
        else
        {
            // Network player: receive data
            State.Current_Health = (int)stream.ReceiveNext();
            State.Is_Unconscious = (bool)stream.ReceiveNext();
            State.Carried_Item = (CarriedItemType)stream.ReceiveNext();
            
            // TODO: Update visuals based on synced state
        }
    }
}