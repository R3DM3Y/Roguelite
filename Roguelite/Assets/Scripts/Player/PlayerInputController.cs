using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private GameInput _gameInput;
    private PlayerController _player;

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

        _gameInput.Gameplay.Jump.performed += ctx => _player.Jump();
        
        _gameInput.Gameplay.Attack.performed += ctx => _player.StartAttack();
        
        _gameInput.Gameplay.AirAttackDown.performed += ctx => _player.StartAirAttackDown();

    }

    private void Update()
    {
        ReadMovement();
        HandleDropDown();   
    }

    private void ReadMovement()
    {
        Vector2 move = _gameInput.Gameplay.Movement.ReadValue<Vector2>();
        _player.InputX = move.x;
        _player.InputY = move.y;

        _player.sHeld = move.y < -0.5f;
    }

    private void HandleDropDown()
    {
        float inputY = _player.InputY;
        if (inputY < -0.5f)
        {
            if (!sPressed)
            {
                sPressed = true;
                _player.DropDown();
            }
        }
        else
        {
            sPressed = false;
        }
    }
}