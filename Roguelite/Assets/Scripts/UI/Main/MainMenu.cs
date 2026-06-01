using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button continueButton;

    [Header("Popups")]
    public GameObject confirmPopup;
    public TextMeshProUGUI confirmText;
    

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
}