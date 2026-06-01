using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseUI;
    public GameObject confirmExitPopup;

    [Header("Settings")]
    public SettingsMenu settingsMenu;

    private bool paused;

    private void Update()
    {
        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        // Если игрок мёртв — не открываем паузу
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null && player.IsDead)
            return;

        // SETTINGS OPEN
        if (settingsMenu != null && settingsMenu.IsOpen)
        {
            settingsMenu.Close();
            pauseUI.SetActive(true);
            return;
        }

        // EXIT POPUP OPEN
        if (confirmExitPopup.activeSelf)
        {
            confirmExitPopup.SetActive(false);
            return;
        }

        // NORMAL PAUSE
        if (paused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        paused = true;

        pauseUI.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        paused = false;

        pauseUI.SetActive(false);
        confirmExitPopup.SetActive(false);

        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        pauseUI.SetActive(false);

        settingsMenu.Open();

        settingsMenu.OnClosed = () =>
        {
            if (paused)
                pauseUI.SetActive(true);
        };
    }

    public void OpenExitPopup()
    {
        confirmExitPopup.SetActive(true);
    }

    public void CloseExitPopup()
    {
        confirmExitPopup.SetActive(false);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("Menu");
    }
}