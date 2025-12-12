using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private IControllable _controllable;
    private GameInput _gameInput;

    private void Awake()
    {
        InputSystem.settings.maxEventBytesPerUpdate = 0;
        
        _gameInput = new GameInput();
        _gameInput.Enable();
        
        _controllable = GetComponent<IControllable>();

        if (_controllable == null)
        {
            throw new Exception($"There is no IControllable component on the object {gameObject.name}");
        }
    }

    private void OnEnable()
    {
        _gameInput.Gameplay.Jump.performed += OnJumpPerformed;
    }

    private void OnDestroy()
    {
        _gameInput.Gameplay.Jump.performed -= OnJumpPerformed;
    }
    
    private void OnJumpPerformed(InputAction.CallbackContext obj)
    {
        _controllable.Jump();
    }

    private void Update()
    {
        ReadMovement();
    }

    private void ReadMovement()
    {
        var imputDirection = _gameInput.Gameplay.Movement.ReadValue<Vector2>();
        var direction = new Vector3(imputDirection.x, imputDirection.y, 0f);
        _controllable.Move(direction);
    }
}

