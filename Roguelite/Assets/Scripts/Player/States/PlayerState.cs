using UnityEngine;

public abstract class PlayerState
{
    protected PlayerStateMachine stateMachine;
    protected PlayerController controller;

    protected PlayerState(PlayerStateMachine sm, PlayerController pc)
    {
        stateMachine = sm;
        controller = pc;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}