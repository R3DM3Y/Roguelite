using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button continueButton;

    [Header("Popups")]
    public GameObject confirmPopup;
    public TextMeshProUGUI confirmText;
    
    [Header("Credits")]
    public GameObject creditsPanel;
    public CreditsData creditsData;
    public float creditsFadeDuration = 1.5f;
    
    [Header("Glow")]
    public GameObject glowA;
    public GameObject glowB;
    
    [Header("Credits Text")]
    public TextMeshProUGUI creditsTitleText;
    public TextMeshProUGUI creditsAuthorText;
    public TextMeshProUGUI creditsHintText;
    

    private enum ActionType
    {
        None,
        NewGame,
        Quit
    }

    private ActionType pendingAction = ActionType.None;

    // ======================================================
    // INIT
    // ======================================================

    void Start()
    {
        UpdateContinueButton();
        confirmPopup.SetActive(false);
    }

    // ======================================================
    // CONTINUE
    // ======================================================

    void UpdateContinueButton()
    {
        SaveData save = SaveSystem.Load();

        // Continue доступен всегда, если есть любой save
        continueButton.interactable = save != null && save.hasRun;
    }

    public void OnContinue()
    {
        SaveData save = SaveSystem.Load();
    
        if (save != null && save.playerIsDead)
        {
            // Игрок умер — новый забег, но с мета-монетами
            GameBootstrap.LoadSave = false;
        }
        else
        {
            // Игрок вышел живым — продолжаем старый забег
            GameBootstrap.LoadSave = true;
        }

        SceneManager.LoadScene("Game");
    }

    // ======================================================
    // START GAME
    // ======================================================

    public void OnStart()
    {
        OpenPopup("Start new game? All progress will be lost.");
        pendingAction = ActionType.NewGame;    
    }

    // ======================================================
    // QUIT
    // ======================================================

    public void OnQuit()
    {
        OpenPopup("Quit game?");
        pendingAction = ActionType.Quit;
    }

    // ======================================================
    // POPUP
    // ======================================================

    void OpenPopup(string text)
    {
        confirmPopup.SetActive(true);
        confirmText.text = text;
    }

    public void OnConfirmYes()
    {
        confirmPopup.SetActive(false);

        switch (pendingAction)
        {
            case ActionType.NewGame:
                StartNewGame();
                break;

            case ActionType.Quit:
                QuitGame();
                break;
        }

        pendingAction = ActionType.None;
    }

    public void OnConfirmNo()
    {
        confirmPopup.SetActive(false);
        pendingAction = ActionType.None;
    }

    // ======================================================
    // ACTIONS
    // ======================================================

    void StartNewGame()
    {
        // Полный сброс
        SaveSystem.DeleteRun();
        PlayerPrefs.DeleteAll(); // Удаляет все мета-монеты и улучшения
    
        GameBootstrap.LoadSave = false;
        SceneManager.LoadScene("Game");
    }

    void QuitGame()
    {
        Application.Quit();
    }
    
    public void OnCredits()
    {
        if (glowA != null) glowA.SetActive(false);
        if (glowB != null) glowB.SetActive(false);
    
        StartCoroutine(ShowCredits());
    }

    private IEnumerator ShowCredits()
    {
        int lang = SettingsManager.Instance != null ? SettingsManager.Instance.language : 0;

        creditsPanel.SetActive(true);
        CanvasGroup cg = creditsPanel.GetComponent<CanvasGroup>();

        creditsTitleText.text = creditsData.GetTitle(lang);
        creditsAuthorText.text = creditsData.GetText(lang);
        creditsHintText.text = creditsData.GetHint(lang);

        cg.alpha = 0f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / creditsFadeDuration;
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                break;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                break;
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / creditsFadeDuration;
            cg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        creditsPanel.SetActive(false);

        if (glowA != null) glowA.SetActive(true);
        if (glowB != null) glowB.SetActive(true);
    }
}