using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    private static AudioSource _audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        
        _audioSource = TryGetComponent(out AudioSource audioSource) ? audioSource : gameObject.AddComponent<AudioSource>();
    }

    public static void PlaySound(AudioClip clip)
    {
        if (_audioSource == null)
        {
            Debug.LogWarning("AudioSource component is missing.");
            return;
        }
        _audioSource.PlayOneShot(clip);
    }
}
