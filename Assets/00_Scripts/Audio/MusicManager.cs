using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip level1Music;
    [SerializeField] private AudioClip level2Music;
    [SerializeField] private AudioClip level3Music;
    [SerializeField] private AudioClip level4Music;
    [SerializeField] private AudioClip level5Music;
    [SerializeField] private AudioClip level6Music;
    [SerializeField] private AudioClip level7Music;
    [SerializeField] private AudioClip level8Music;
    [SerializeField] private AudioClip level9Music;

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
    
    public void PlayGameplayMusic(int levelId)
    {
        switch (levelId)
        {
            case 1:
                PlayMusic(level1Music);
                break;
            case 2:
                PlayMusic(level2Music);
                break;
            case 3:
                PlayMusic(level3Music);
                break;
            case 4:
                PlayMusic(level4Music);
                break;
            case 5:
                PlayMusic(level5Music);
                break;
            case 6:
                PlayMusic(level6Music);
                break;
            case 7:
                PlayMusic(level7Music);
                break;
            case 8:
                PlayMusic(level8Music);
                break;
            case 9:
                PlayMusic(level9Music);
                break;
            default:
                Debug.LogWarning("No music defined for this level.");
                break;
        }
    }
    
    private void PlayMusic(AudioClip clip)
    {
        AudioManager.PlayMusic(clip);
    }
}
