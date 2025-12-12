using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private GameInput _gameInput;
    private PlayerController _player;

    // Флаг для отслеживания нового нажатия S
    private bool sPressed = false;

    private void Awake()
    {
        _gameInput = new GameInput();
        _gameInput.Enable();

        _player = GetComponent<PlayerController>();
        if (_player == null)
        {
            Debug.LogError("PlayerController не найден на объекте!");
        }

        // Подключаем прыжок
        _gameInput.Gameplay.Jump.performed += ctx => _player.Jump();
    }

    private void Update()
    {
        ReadMovement();
        HandleDropDown();
    }

    private void ReadMovement()
    {
        Vector2 move = _gameInput.Gameplay.Movement.ReadValue<Vector2>();
        _player.Move(move.x);
    }

    private void HandleDropDown()
    {
        // Читаем вертикальное направление
        float inputY = _gameInput.Gameplay.Movement.ReadValue<Vector2>().y;

        if (inputY < -0.5f)
        {
            // Спуск срабатывает только при новом нажатии S
            if (!sPressed)
            {
                sPressed = true;
                _player.DropDown();
            }
        }
        else
        {
            // Сбрасываем флаг после отпускания S
            sPressed = false;
        }
    }
}