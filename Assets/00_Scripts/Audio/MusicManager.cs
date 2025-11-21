using System;
using _00_Scripts.Save;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip[] levelMusic;
    
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

    private void Start()
    {
        PlayGameplayMusic(SaveManager.CurrentSaveData.LastLevelCompleted);
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }
    
    public void PlayGameplayMusic(int levelId)
    {
        if (levelId >= 0 && levelId < levelMusic.Length)
        {
            if (levelId == 0 || levelMusic[levelId] != levelMusic[levelId - 1])
                PlayMusic(levelMusic[levelId]);
        }
        else
        {
            Debug.LogWarning($"No music found for level ID {levelId}. Playing default music.");
            PlayMusic(mainMenuMusic);
        }
    }
    
    private void PlayMusic(AudioClip clip)
    {
        AudioManager.PlayMusic(clip);
    }
}
