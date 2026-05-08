using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private GameInput input;

    public GameInput Input => input;

    public enum InputMode
    {
        Gameplay,
        UI
    }

    private void Awake()
    {
        Instance = this;

        input = new GameInput();
        input.Enable();

        SetMode(InputMode.Gameplay);
        
    }

    public void SetMode(InputMode mode)
    {
        input.Gameplay.Disable();
        input.UI.Disable();

        if (mode == InputMode.Gameplay)
            input.Gameplay.Enable();
        else
            input.UI.Enable();
    }
}