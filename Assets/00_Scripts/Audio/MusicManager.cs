using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }
    
    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }
    
    private void PlayMusic(AudioClip clip)
    {
        AudioManager.PlayMusic(clip);
    }
}
