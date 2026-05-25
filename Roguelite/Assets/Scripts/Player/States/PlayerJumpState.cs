using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerStateMachine sm, PlayerController pc) : base(sm, pc) { }

    public override void Enter()
    {
        controller.Jump();
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