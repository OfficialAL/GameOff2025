using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Handles the Loading Scene (Screen 1).
/// Displays logos and transitions to the Main Menu after a delay. [cite: 17, 18]
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Tooltip("The name of the main menu scene to load.")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    [Tooltip("Delay in seconds before transitioning.")]
    [SerializeField] private float transitionDelay = 3.0f;

    void Start()
    {
        // Start the coroutine to auto-transition after the delay 
        StartCoroutine(LoadMainMenu());
    }

    private IEnumerator LoadMainMenu()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(transitionDelay);

        // Load the Main Menu Scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}