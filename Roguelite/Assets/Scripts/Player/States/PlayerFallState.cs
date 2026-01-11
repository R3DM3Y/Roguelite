using UnityEngine;

public class PlayerFallState : PlayerState
{
    public PlayerFallState(PlayerStateMachine sm, PlayerController pc) : base(sm, pc) { }

    public override void Update()
    {
        controller.Move(controller.InputX);

        if (controller.IsGrounded())
        {
            if (controller.InputX != 0)
                stateMachine.ChangeState(new PlayerRunState(stateMachine, controller));
            else
                stateMachine.ChangeState(new PlayerIdleState(stateMachine, controller));
        }
    }

}