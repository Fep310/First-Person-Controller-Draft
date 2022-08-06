using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : GroundParentState
{
    public IdleState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
        : base(player, controller, states, stateMachine, constValues, movementData, inputData)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();

        if (inputData.HorizontalMovementInput != Vector2.zero)
        {
            stateMachine.ChangeState(
                inputData.IsPressingSprint && Vector2.Dot(Vector2.up, inputData.HorizontalMovementInput) > .38f ?
                states.Run : states.Walk);
            return;
        }

        if (inputData.IsPressingCrouch)
            stateMachine.ChangeState(states.IdleCrouch);
    }

    public override void OnEnter(PlayerState previous)
    {
        base.OnEnter(previous);

        player.CameraAnimations.UpdateSway(0);
    }

    public override void OnExit(PlayerState next) => base.OnExit(next);
    public override void OnUpdate()
    {
        DoChecks();

        WorldPlayerDirectionOnSlopes();

        player.debug.SetLine(6, movementData.horizontalVel.ToString());

        movementData.horizontalVel = Vector2.Lerp(
            movementData.horizontalVel,
            Vector2.zero,
            constValues.HorizontalDeacceleration * Time.deltaTime);

        player.debug.SetLine(7, movementData.horizontalVel.ToString());

        movementData.finalVelocity = new Vector3(movementData.horizontalVel.x, movementData.appliedVerticalVel, movementData.horizontalVel.y);
        //movementData.finalVelocity.Scale(movementData.worldPlayerDiretion);

        ApplyVelocity();

        // Deaccelerate if moving
        /*if (movementData.horizontalVel.sqrMagnitude >= float.Epsilon)
        {
            
        }*/

        if (crouching)
            TryToStandup();
    }

    public override void ApplyJumpForce() => base.ApplyJumpForce();
}
