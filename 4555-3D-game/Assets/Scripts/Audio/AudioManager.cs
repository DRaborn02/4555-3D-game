using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;
    
    [Header("Background Music")]
    [SerializeField] AudioClip backgroundMusic; // Assign in Inspector from Assets/Audio/Music/
    [SerializeField] float musicVolume = 0.5f; // Background music volume (typically lower than SFX)
    [SerializeField] bool playMusicOnStart = true; // Auto-play music when game starts

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Auto-assign AudioSource components if not assigned in Inspector
        AudioSource[] audioSources = GetComponents<AudioSource>();
        
        if (musicSource == null)
        {
            if (audioSources.Length > 0)
            {
                musicSource = audioSources[0];
            }
            else
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }
        }
        
        if (sfxSource == null)
        {
            if (audioSources.Length > 1)
            {
                sfxSource = audioSources[1];
            }
            else if (audioSources.Length == 1 && musicSource != audioSources[0])
            {
                sfxSource = audioSources[0];
            }
            else
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
            }
        }
        
        // Ensure we have two separate AudioSources
        if (musicSource == sfxSource && audioSources.Length < 2)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
    }

    void Start()
    {
        // Auto-play background music if enabled
        if (playMusicOnStart && backgroundMusic != null)
        {
            PlayMusic(backgroundMusic, musicVolume, true);
        }
    }

    public void PlayMusic(AudioClip clip, float volume = 1f, bool loop = true)
    {
        if (clip == null || musicSource == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }
    
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}

