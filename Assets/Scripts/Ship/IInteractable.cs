using UnityEngine;

/// <summary>
/// Interface for objects that players can interact with on the ship
/// Examples: Ship wheel, cannons, repair stations, etc.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Check if the player can interact with this object
    /// </summary>
    /// <param name="player">The player attempting to interact</param>
    /// <returns>True if interaction is allowed</returns>
    bool CanInteract(GameObject player);

    /// <summary>
    /// Execute the interaction
    /// </summary>
    /// <param name="player">The player performing the interaction</param>
    void Interact(GameObject player);

    /// <summary>
    /// Get the text to display when player is near this interactable
    /// </summary>
    /// <returns>Interaction text (e.g., "Press E to steer ship")</returns>
    string GetInteractionText();
}