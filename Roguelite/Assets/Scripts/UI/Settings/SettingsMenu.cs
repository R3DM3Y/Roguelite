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
    [Header("Optional")]
    public GameObject mainMenu;
    public GameObject glowA;
    public GameObject glowB;

    [Header("UI")]
    public GameObject settingsPanel;
    public GameObject confirmPopup;

    public RectTransform selector;
    public SettingsRow[] rows;

    public float rowHeight = 80f;

    private GameInput input;

    private enum MenuState
    {
        Closed,
        Settings,
        Confirm
    }

    private MenuState state;

    public bool IsOpen => state != MenuState.Closed;

    public System.Action OnClosed;

    private int index;

    private int music;
    private int sfx;
    private int res;
    private int quality;
    private bool fullscreen;
    private int language;

    private int savedMusic;
    private int savedSfx;
    private int savedRes;
    private int savedQuality;
    private bool savedFullscreen;
    private int savedLanguage;

    private readonly string[] resolutions =
    {
        "1280x720",
        "1600x900",
        "1920x1080"
    };

    private readonly string[] qualities =
    {
        "Low",
        "Normal",
        "High"
    };

    private readonly string[] languages =
    {
        "English",
        "Русский"
    };

    private void Awake()
    {
        input = InputManager.Instance.Input;
    }

    private void OnEnable()
    {
        input.UI.Enable();

        input.UI.Navigate.performed += OnNavigate;
        input.UI.Submit.performed += OnSubmit;
        input.UI.Cancel.performed += OnCancel;
    }

    private void OnDisable()
    {
        if (input == null) return;

        input.UI.Disable();
        input.Gameplay.Disable();
        
        input.UI.Navigate.performed -= OnNavigate;
        input.UI.Submit.performed -= OnSubmit;
        input.UI.Cancel.performed -= OnCancel;
    }

    public void Open()
    {
        gameObject.SetActive(true);

        if (mainMenu != null)
            mainMenu.SetActive(false);

        if (glowA != null)
            glowA.SetActive(false);

        if (glowB != null)
            glowB.SetActive(false);

        settingsPanel.SetActive(true);
        confirmPopup.SetActive(false);

        state = MenuState.Settings;

        index = 0;

        LoadFromManager();

        SaveSnapshot();

        RefreshUI();
    }

    public void Close()
    {
        state = MenuState.Closed;

        if (mainMenu != null)
            mainMenu.SetActive(true);

        if (glowA != null)
            glowA.SetActive(true);

        if (glowB != null)
            glowB.SetActive(true);

        gameObject.SetActive(false);

        OnClosed?.Invoke();
    }

    private void LoadFromManager()
    {
        SettingsManager s = SettingsManager.Instance;

        music = s.music;
        sfx = s.sfx;
        res = s.res;
        quality = s.quality;
        fullscreen = s.fullscreen;
        language = s.language;
    }

    private void SaveSnapshot()
    {
        savedMusic = music;
        savedSfx = sfx;
        savedRes = res;
        savedQuality = quality;
        savedFullscreen = fullscreen;
        savedLanguage = language;
    }

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
            confirmPopup.SetActive(false);
            state = MenuState.Settings;
        }
    }

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

    private void ChangeValue(int dir)
    {
        switch (index)
        {
            case 0:
                music = Mathf.Clamp(music + dir * 5, 0, 100);
                break;

            case 1:
                sfx = Mathf.Clamp(sfx + dir * 5, 0, 100);
                break;

            case 2:
                res = Wrap(res + dir, resolutions.Length);
                break;

            case 3:
                quality = Wrap(quality + dir, qualities.Length);
                break;

            case 4:
                fullscreen = !fullscreen;
                break;

            case 5:
                language = Wrap(language + dir, languages.Length);
                break;
        }
    }

    private int Wrap(int value, int max)
    {
        if (value < 0) return max - 1;
        if (value >= max) return 0;
        return value;
    }

    public void SaveAndClose()
    {
        SettingsManager s = SettingsManager.Instance;

        s.music = music;
        s.sfx = sfx;
        s.res = res;
        s.quality = quality;
        s.fullscreen = fullscreen;
        s.language = language;

        s.ApplyAll();
        s.Save();

        confirmPopup.SetActive(false);

        Close();
    }

    public void DiscardAndClose()
    {
        confirmPopup.SetActive(false);

        Close();
    }

    private bool HasChanges()
    {
        return
            music != savedMusic ||
            sfx != savedSfx ||
            res != savedRes ||
            quality != savedQuality ||
            fullscreen != savedFullscreen ||
            language != savedLanguage;
    }

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

        rows[5].label.text = "Language";
        rows[5].value.text = languages[language];

        selector.anchoredPosition = new Vector2(
            selector.anchoredPosition.x,
            -index * rowHeight
        );
    }
}