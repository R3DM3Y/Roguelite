using UnityEngine;

public class PlayerJumpState : PlayerState
{
    private bool hasJumped = false;

    public PlayerJumpState(PlayerStateMachine sm, PlayerController pc) : base(sm, pc) { }

    public override void Enter()
    {
        if (controller.IsGrounded())
        {
            controller.Jump();
            hasJumped = true;
        }
    }

    public override void Update()
    {
        controller.Move(controller.InputX);

        if (controller.rb.linearVelocity.y < 0)
        {
            stateMachine.ChangeState(new PlayerFallState(stateMachine, controller));
        }
    }
}