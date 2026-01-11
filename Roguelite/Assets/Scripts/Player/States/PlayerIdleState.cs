using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerStateMachine sm, PlayerController pc) : base(sm, pc) { }

    public override void Update()
    {
        if (controller.InputX != 0)
        {
            stateMachine.ChangeState(new PlayerRunState(stateMachine, controller));
        }
        else if (controller.JumpPressed)
        {
            stateMachine.ChangeState(new PlayerJumpState(stateMachine, controller));
        }
        else if (!controller.IsGrounded())
        {
            stateMachine.ChangeState(new PlayerFallState(stateMachine, controller));
        }
    }
}