using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ConfirmPopup : MonoBehaviour
{
    public SettingsMenu menu;

    public void OnYes()
    {
        menu.SaveAndClose();
    }

    public void OnNo()
    {
        menu.DiscardAndClose();
    }
}