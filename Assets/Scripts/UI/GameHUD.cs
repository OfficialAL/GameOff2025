using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PirateCoop; // For PlayerState

/// <summary>
/// Manages the Game Session HUD (Screen 7).
/// Displays local player health, shared resources, etc.
/// </summary>
public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    [Header("Player HUD")]
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TextMeshProUGUI playerHealthText;

    private int maxPlayerHealth = 100; // Default, will be set by PlayerState

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Called by the local PlayerState to set the max health.
    /// </summary>
    public void SetMaxHealth(int health)
    {
        maxPlayerHealth = health;
        playerHealthSlider.maxValue = health;
        playerHealthSlider.value = health;
        UpdateHealthText(health, health);
    }

    /// <summary>
    /// Called by the local PlayerState whenever health changes.
    /// </summary>
    public void UpdateHealth(int currentHealth)
    {
        playerHealthSlider.value = currentHealth;
        UpdateHealthText(currentHealth, maxPlayerHealth);
    }
    
    private void UpdateHealthText(int current, int max)
    {
        if(playerHealthText)
        {
            playerHealthText.text = $"{current} / {max}";
        }
    }
}