using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip menuMusic;
    public AudioClip clickSound;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        PlayMusic(menuMusic);
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
    
    public void PlayClick()
    {
        PlaySFX(clickSound);
    }
    
    public void SetMusicVolume(float value)
    {
        musicSource.volume = Mathf.Clamp(value, 0.0001f, 1f);
    }

    public void SetSFXVolume(float value)
    {
        sfxSource.volume = Mathf.Clamp(value, 0.0001f, 1f);
    }
}