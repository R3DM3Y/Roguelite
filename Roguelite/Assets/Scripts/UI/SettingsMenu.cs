using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[System.Serializable]
public class SettingsRow
{
    public TextMeshProUGUI label;
    public TextMeshProUGUI value;
}

public class SettingsMenu : MonoBehaviour
{
    [Header("Glow Effects")]
    public GameObject glowA;
    public GameObject glowB;
    
    [Header("UI")]
    public GameObject mainMenu;
    public GameObject settingsPanel;
    public GameObject confirmPopup;

    public RectTransform selector;
    public SettingsRow[] rows;

    [Header("Move Selector")]
    public float rowHeight = 80f;

    private GameInput input;

    private enum MenuState
    {
        Closed,
        Settings,
        Confirm
    }

    private MenuState state = MenuState.Closed;

    private int index = 0;

    // CURRENT VALUES
    private int music;
    private int sfx;
    private int res;
    private int quality;
    private bool fullscreen;
    private int language;

    // SAVED VALUES
    private int savedMusic;
    private int savedSfx;
    private int savedRes;
    private int savedQuality;
    private bool savedFullscreen;
    private int savedLanguage;

    private readonly string[] resolutions = { "1280x720", "1600x900", "1920x1080" };
    private readonly string[] qualities = { "Low", "Normal", "High" };
    private readonly string[] languages = { "English", "Русский" };
    

    // ======================================================
    // START
    // ======================================================

    private void Awake()
    {
        input = InputManager.Instance.Input;
    }
    
    private void SetGlow(CanvasGroup g, bool active)
    {
        if (g == null) return;

        g.alpha = active ? 1f : 0f;
        g.interactable = active;
        g.blocksRaycasts = active;
    }

    private bool subscribed = false;

    private void OnEnable()
    {
        if (subscribed) return;

        input.UI.Enable();
        input.UI.Navigate.performed += OnNavigate;
        input.UI.Submit.performed += OnSubmit;
        input.UI.Cancel.performed += OnCancel;

        subscribed = true;
    }

    private void OnDisable()
    {
        if (!subscribed) return;

        input.UI.Navigate.performed -= OnNavigate;
        input.UI.Submit.performed -= OnSubmit;
        input.UI.Cancel.performed -= OnCancel;

        subscribed = false;
    }

    // ======================================================
    // OPEN / CLOSE
    // ======================================================

    public void Open()
    {
        gameObject.SetActive(true);

        mainMenu.SetActive(false);
        settingsPanel.SetActive(true);
        confirmPopup.SetActive(false);
        
        glowA.SetActive(false);
        glowB.SetActive(false);

        state = MenuState.Settings;
        index = 0;

        LoadSettings();
        RefreshUI();
    }
    

    public void Close()
    {
        state = MenuState.Closed;

        mainMenu.SetActive(true);

        glowA.SetActive(true);
        glowB.SetActive(true);

        gameObject.SetActive(false);
    }

    // ======================================================
    // INPUT
    // ======================================================

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        if (state != MenuState.Settings)
            return;

        Vector2 v = ctx.ReadValue<Vector2>();

        if (v.y > 0.5f)
            index--;

        if (v.y < -0.5f)
            index++;

        if (index < 0)
            index = rows.Length - 1;

        if (index >= rows.Length)
            index = 0;

        if (v.x > 0.5f)
            ChangeValue(1);

        if (v.x < -0.5f)
            ChangeValue(-1);

        RefreshUI();
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (state == MenuState.Confirm)
        {
            SaveAndClose();
        }
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (state == MenuState.Settings)
        {
            if (HasChanges())
                OpenConfirm();
            else
                Close();
        }
        else if (state == MenuState.Confirm)
        {
            CloseConfirm();
        }
    }

    // ======================================================
    // CONFIRM POPUP
    // ======================================================

    private void OpenConfirm()
    {
        state = MenuState.Confirm;
        confirmPopup.SetActive(true);
    }

    private void CloseConfirm()
    {
        state = MenuState.Settings;
        confirmPopup.SetActive(false);
    }

    public void ConfirmYes()
    {
        SaveAndClose();
    }

    public void ConfirmNo()
    {
        DiscardAndClose();
    }

    // ======================================================
    // SETTINGS LOGIC
    // ======================================================

    private void ChangeValue(int dir)
    {
        switch (index)
        {
            case 0:
                music = Mathf.Clamp(music + dir * 5, 0, 100);
                AudioManager.Instance.SetMusicVolume(music / 100f);
                break;

            case 1:
                sfx = Mathf.Clamp(sfx + dir * 5, 0, 100);
                AudioManager.Instance.SetSFXVolume(sfx / 100f);
                break;

            case 2:
                res = Wrap(res + dir, resolutions.Length);
                ApplyResolution();
                break;

            case 3:
                quality = Wrap(quality + dir, qualities.Length);
                QualitySettings.SetQualityLevel(quality);
                Debug.Log("QUALITY → " + QualitySettings.names[quality]);
                break;

            case 4:
                fullscreen = !fullscreen;
                Screen.fullScreen = fullscreen;
                Debug.Log("FULLSCREEN → " + Screen.fullScreen);
                break;

            case 6:
                language = Wrap(language + dir, languages.Length);
                break;
        }
    }
    
    void ApplyResolution()
    {
        int width = 0;
        int height = 0;

        switch (res)
        {
            case 0: width = 1280; height = 720; break;
            case 1: width = 1600; height = 900; break;
            case 2: width = 1920; height = 1080; break;
        }

        Screen.SetResolution(width, height, fullscreen);

        Debug.Log($"RESOLUTION SET → {width}x{height} | Fullscreen: {fullscreen}");
        Debug.Log($"CURRENT → {Screen.width}x{Screen.height} | Fullscreen: {Screen.fullScreen}");
    }

    private int Wrap(int value, int max)
    {
        if (value < 0) return max - 1;
        if (value >= max) return 0;
        return value;
    }

    // ======================================================
    // SAVE / LOAD
    // ======================================================

    private void LoadSettings()
    {
        music = PlayerPrefs.GetInt("Music", 80);
        sfx = PlayerPrefs.GetInt("SFX", 80);
        res = PlayerPrefs.GetInt("Res", 2);
        quality = PlayerPrefs.GetInt("Quality", 2);
        fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        language = PlayerPrefs.GetInt("Language", 0);

        savedMusic = music;
        savedSfx = sfx;
        savedRes = res;
        savedQuality = quality;
        savedFullscreen = fullscreen;
        savedLanguage = language;
        
        AudioManager.Instance.SetMusicVolume(music / 100f);
        AudioManager.Instance.SetSFXVolume(sfx / 100f);
        ApplyResolution();
        QualitySettings.SetQualityLevel(quality);
        Screen.fullScreen = fullscreen;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("Music", music);
        PlayerPrefs.SetInt("SFX", sfx);
        PlayerPrefs.SetInt("Res", res);
        PlayerPrefs.SetInt("Quality", quality);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("Language", language);

        PlayerPrefs.Save();
    }

    public void SaveAndClose()
    {
        SaveSettings();
        confirmPopup.SetActive(false);
        Close();
    }

    public void DiscardAndClose()
    {
        LoadSettings();
        confirmPopup.SetActive(false);
        Close();
    }

    private bool HasChanges()
    {
        return music != savedMusic ||
               sfx != savedSfx ||
               res != savedRes ||
               quality != savedQuality ||
               fullscreen != savedFullscreen ||
               language != savedLanguage;
    }

    // ======================================================
    // UI
    // ======================================================

    private void RefreshUI()
    {
        rows[0].label.text = "Music";
        rows[0].value.text = music.ToString();

        rows[1].label.text = "SFX";
        rows[1].value.text = sfx.ToString();

        rows[2].label.text = "Resolution";
        rows[2].value.text = resolutions[res];

        rows[3].label.text = "Quality";
        rows[3].value.text = qualities[quality];

        rows[4].label.text = "Fullscreen";
        rows[4].value.text = fullscreen ? "ON" : "OFF";

        rows[5].label.text = "Controls";
        rows[5].value.text = "";

        rows[6].label.text = "Language";
        rows[6].value.text = languages[language];

        selector.anchoredPosition = new Vector2(
            selector.anchoredPosition.x,
            -index * rowHeight
        );
    }
}