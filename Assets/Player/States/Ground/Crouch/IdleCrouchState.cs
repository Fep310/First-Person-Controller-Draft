using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleCrouchState : CrouchParentState
{
    public IdleCrouchState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
        : base(player, controller, states, stateMachine, constValues, movementData, inputData)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();

        if (inputData.HorizontalMovementInput != Vector2.zero)
            stateMachine.ChangeState(states.WalkCrouch);
    }

    public override void OnEnter(PlayerState previous)
    {
        base.OnEnter(previous);
    }

    public override void OnExit(PlayerState next)
    {
        base.OnExit(next);
    }

    public override void OnUpdate()
    {
        DoChecks();

        // Deaccelerate if moving
        if (movementData.horizontalVel.sqrMagnitude >= float.Epsilon)
        {
            WorldPlayerDirectionOnSlopes();

            movementData.horizontalVel = Vector2.Lerp(
                movementData.horizontalVel,
                Vector2.zero,
                constValues.HorizontalDeacceleration * Time.deltaTime);

            movementData.finalVelocity = new Vector3(movementData.horizontalVel.x, movementData.appliedVerticalVel, movementData.horizontalVel.y);
            movementData.finalVelocity.Scale(movementData.worldPlayerDiretion);

            ApplyVelocity();
        }
    }
}
