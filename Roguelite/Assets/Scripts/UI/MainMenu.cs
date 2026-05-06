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
        bool hasSave = PlayerPrefs.HasKey("SaveExists");

        continueButton.interactable = hasSave;
    }

    public void OnContinue()
    {
        Debug.Log("LOAD GAME");

        SceneManager.LoadScene("Game");
        // TODO: загрузка сцены
    }

    // ======================================================
    // START GAME
    // ======================================================

    public void OnStart()
    {
        OpenPopup("Start new game?");
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
        Debug.Log("NEW GAME STARTED");

        PlayerPrefs.SetInt("SaveExists", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Game");

        // TODO: загрузка сцены
    }

    void QuitGame()
    {
        Debug.Log("QUIT");

        Application.Quit();
    }
}