using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image blackOverlay;
    [SerializeField] private DeathMessagesData messagesData;
    
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI hintText;
    
    [Header("Timings")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float textDelay = 0.8f;
    [SerializeField] private float textFadeDuration = 1.5f;
    [SerializeField] private float hintDelay = 2f;
    
    private bool waitingForInput;
    
    private void Awake()
    {
        blackOverlay.gameObject.SetActive(false);
        blackOverlay.color = new Color(0f, 0f, 0f, 0f);
        
        titleText.alpha = 0f;
        bodyText.alpha = 0f;
        hintText.alpha = 0f;
    }
    
    public IEnumerator FadeIn()
    {
        blackOverlay.gameObject.SetActive(true);
    
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            blackOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 1f, t));
            yield return null;
        }
    
        blackOverlay.color = Color.black; // полностью чёрный
    }
    
    public IEnumerator ShowText()
    {
        int lang = SettingsManager.Instance != null 
            ? SettingsManager.Instance.language 
            : 0;
        
        titleText.text = messagesData.GetTitle(lang);
        bodyText.text = messagesData.GetRandomMessage(lang);
        hintText.text = messagesData.GetHint(lang);
        
        yield return new WaitForSecondsRealtime(textDelay);
        
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / textFadeDuration;
            titleText.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        titleText.alpha = 1f;
        
        yield return new WaitForSecondsRealtime(textDelay);
        
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / textFadeDuration;
            bodyText.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        bodyText.alpha = 1f;
        
        yield return new WaitForSecondsRealtime(hintDelay);
        
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / textFadeDuration;
            hintText.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        hintText.alpha = 1f;
    }
    
    public void WaitForInput()
    {
        waitingForInput = true;
        Time.timeScale = 0f;
    }
    
    private void Update()
    {
        if (!waitingForInput) return;
        
        if (Keyboard.current != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                Restart();
            }
            
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ToMenu();
            }
        }
    }
    
    private void Restart()
    {
        // Монеты уже сохранены в OnPlayerDeath
        Time.timeScale = 1f;
        GameBootstrap.LoadSave = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ToMenu()
    {
        // Монеты уже сохранены в OnPlayerDeath
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}