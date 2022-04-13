using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GroundParentState : PlayerState
{
    protected GroundParentState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
        : base(player, controller, states, stateMachine, constValues, movementData, inputData)
    {
    }

    public override void DoChecks()
    {
        if (!CheckGround())
        {
            stateMachine.ChangeState(states.Airborne);
            return;
        }

        if (inputData.IsPressingJump)
        {
            jumping = true;
            ApplyJumpForce();
            stateMachine.ChangeState(states.Airborne);
            return;
        }

        movementData.verticalVel = movementData.appliedVerticalVel = 0;
    }

    // virtual -> different jumps
    protected virtual void ApplyJumpForce()
    {
        movementData.verticalVel = movementData.JumpForce;
        movementData.appliedVerticalVel = movementData.JumpForce;

        movementData.finalVelocity.y = movementData.appliedVerticalVel;

        ApplyVelocity();
    }

    

    public override void OnEnter(PlayerState previous)
    {
        base.OnEnter(previous);
    }

    public override void OnExit(PlayerState next) { }

    public override void OnUpdate() { }
}
