using UnityEngine;
using Photon.Pun;

/// <summary>
/// Basic networked player controller for WebGL + PUN2
/// Handles movement, input, and basic networking synchronization
/// </summary>
public class PlayerController : MonoBehaviourPunPV, IPunObservable
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float smoothing = 10f;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color[] playerColors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow };
    
    private Vector2 networkPosition;
    private Vector2 networkVelocity;
    private Vector2 inputMovement;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Set player color based on network ID
        if (spriteRenderer != null && photonView.IsMine)
        {
            int colorIndex = photonView.owner.ActorNumber % playerColors.Length;
            spriteRenderer.color = playerColors[colorIndex];
        }
        
        // Set player name
        if (photonView.IsMine)
        {
            name = $"Player_{PhotonNetwork.LocalPlayer.NickName}";
        }
        else
        {
            name = $"Player_{photonView.owner.NickName}";
        }
    }

    void Update()
    {
        // Only handle input for local player
        if (photonView.IsMine)
        {
            HandleInput();
        }
        else
        {
            // Smooth network position for remote players
            transform.position = Vector2.Lerp(transform.position, networkPosition, Time.deltaTime * smoothing);
        }
    }

    void FixedUpdate()
    {
        // Only move local player
        if (photonView.IsMine && rb != null)
        {
            rb.velocity = inputMovement * moveSpeed;
        }
    }

    void HandleInput()
    {
        // Get input (works for both keyboard and touch/mobile)
        float horizontal = 0f;
        float vertical = 0f;

        // Keyboard input
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontal = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontal = 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            vertical = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            vertical = -1f;

        // Touch/mouse input for WebGL
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = ((Vector2)mousePos - (Vector2)transform.position).normalized;
            horizontal = direction.x;
            vertical = direction.y;
        }

        inputMovement = new Vector2(horizontal, vertical).normalized;
    }

    // Synchronize position and movement across network
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send our data to other players
            stream.SendNext(transform.position);
            stream.SendNext(rb != null ? rb.velocity : Vector2.zero);
        }
        else
        {
            // Receive data from other players
            networkPosition = (Vector2)stream.ReceiveNext();
            networkVelocity = (Vector2)stream.ReceiveNext();
            
            // Lag compensation
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            networkPosition += networkVelocity * lag;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw movement direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        if (inputMovement != Vector2.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, inputMovement);
        }
    }
}