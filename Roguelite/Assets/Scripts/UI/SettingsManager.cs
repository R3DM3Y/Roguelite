using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Current Settings")]
    public int music;
    public int sfx;
    public int res;
    public int quality;
    public bool fullscreen;
    public int language;

    private readonly Vector2Int[] resolutions =
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080)
    };

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
        ApplyAll(); 
    }

    public void Load()
    {
        music = PlayerPrefs.GetInt("Music", 80);
        sfx = PlayerPrefs.GetInt("SFX", 80);
        res = PlayerPrefs.GetInt("Res", 2);
        quality = PlayerPrefs.GetInt("Quality", 2);
        fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        language = PlayerPrefs.GetInt("Language", 0);
    }

    public void Save()
    {
        PlayerPrefs.SetInt("Music", music);
        PlayerPrefs.SetInt("SFX", sfx);
        PlayerPrefs.SetInt("Res", res);
        PlayerPrefs.SetInt("Quality", quality);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("Language", language);

        PlayerPrefs.Save();
    }

    public void ApplyAll()
    {
        // AUDIO
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(music / 100f);
            AudioManager.Instance.SetSFXVolume(sfx / 100f);
        }

        // QUALITY
        QualitySettings.SetQualityLevel(quality);

        // FULLSCREEN
        Screen.fullScreen = fullscreen;

        // RESOLUTION
        Vector2Int r = resolutions[res];
        Screen.SetResolution(r.x, r.y, fullscreen);
    }
}