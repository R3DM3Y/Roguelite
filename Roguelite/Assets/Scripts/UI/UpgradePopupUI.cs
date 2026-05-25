using System.Collections;
using TMPro;
using UnityEngine;

public class UpgradePopupUI : MonoBehaviour
{
    public static UpgradePopupUI Instance;

    public TextMeshProUGUI text;

    private CanvasGroup canvasGroup;

    private Coroutine routine;

    private void Awake()
    {
        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show(string message)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        text.text = message;

        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(1.5f);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f;

            canvasGroup.alpha =
                Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}