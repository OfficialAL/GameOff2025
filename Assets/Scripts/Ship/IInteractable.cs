using PirateCoop; // For PlayerController

/// <summary>
/// Interface for all objects a player can interact with (Stations).
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when a player presses the Interact key.
    /// </summary>
    /// <param name="player">The PlayerController initiating the interaction.</param>
    void Interact(PlayerController player);

    /// <summary>
    /// Called when the player is no longer interacting.
    /// </summary>
    void StopInteract();

    /// <summary>
    /// Can the player currently interact with this object?
    /// </summary>
    bool CanInteract(PlayerController player);
    
    /// <summary>
    /// Gets a prompt message (e.g., "Press E to use Wheel").
    /// </summary>
    string GetInteractPrompt();
}