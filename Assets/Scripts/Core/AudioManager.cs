using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton for managing all audio.
/// Plays SFX and music.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Clips")]
    [SerializeField] private AudioClip cannonFire;
    [SerializeField] private AudioClip cutlassSwing;
    [SerializeField] private AudioClip pistolShot;
    [SerializeField] private AudioClip blunderbussShot;
    [SerializeField] private AudioClip repairSound;
    [SerializeField] private AudioClip lootPickup;
    [SerializeField] private AudioClip uiClick;
    [SerializeField] private AudioClip sailingLoop;

    // TODO: Create a more robust system, e.g., Dictionary<string, AudioClip>
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        PlayMusic(sailingLoop);
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
    
    // Public helper methods
    public void PlayCannonFire() => PlaySFX(cannonFire);
    public void PlayUIClick() => PlaySFX(uiClick);

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }
}