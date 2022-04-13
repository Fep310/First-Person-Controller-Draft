using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CrouchParentState : PlayerState
{
    public CrouchParentState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
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

        /*if (inputData.IsPressingJump)
        {
            jumping = true;
            ApplyJumpForce();
            stateMachine.ChangeState(states.Airborne);
            return;
        }*/

        movementData.verticalVel = movementData.appliedVerticalVel = 0;

        if (!inputData.IsPressingCrouch)
        {
            if (!Physics.SphereCast(player.Transform.position, controller.radius, Vector3.up, out _, player.CrouchAnimator.OriginalHeight, constValues.CollisionMask))
            {
                if (inputData.HorizontalMovementInput == Vector2.zero)
                    stateMachine.ChangeState(states.Idle);
                else
                    stateMachine.ChangeState(inputData.IsPressingSprint ? states.Run : states.Walk);
            }
        }
    }

    public override void OnEnter(PlayerState previous)
    {
        base.OnEnter(previous);

        if (previous is not CrouchParentState && !inputData.IsPressingJump)
            player.CrouchAnimator.Crouch();
    }

    public override void OnExit(PlayerState next)
    {
        if (next is not CrouchParentState)
            player.CrouchAnimator.StandUp();
    }

    public override void OnUpdate() { }
}
