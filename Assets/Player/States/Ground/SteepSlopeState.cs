using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteepSlopeState : GroundParentState
{
    public SteepSlopeState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
        : base(player, controller, states, stateMachine, constValues, movementData, inputData)
    {
    }

    private Vector3 slopeNormal;
    private Vector3 slopeDirection;

    public override void DoChecks()
    {
        if (!CheckGround())
        {
            stateMachine.ChangeState(states.Airborne);
            return;
        }

        if (!onSteepSlope)
        {
            if (inputData.HorizontalMovementInput == Vector2.zero)
                stateMachine.ChangeState(states.Idle);
            else
                stateMachine.ChangeState(inputData.IsPressingSprint ? states.Run : states.Walk);

            return;
        }

        if (inputData.IsPressingJump)
        {
            movementData.jumping = true;
            ApplyJumpForce();
            stateMachine.ChangeState(states.Airborne);
            return;
        }
    }

    public override void OnEnter(PlayerState previous)
    {
        slopeNormal = movementData.groundNormal;
        slopeDirection = Vector3.Cross(Vector3.Cross(slopeNormal, Vector3.down), slopeNormal);

        movementData.horizontalVel = Vector2.Lerp(
            movementData.horizontalVel,
            new Vector2(slopeDirection.x * constValues.WalkMoveSpeed, slopeDirection.z * constValues.WalkMoveSpeed),
            .5f);
        movementData.finalVelocity *= 0;

        player.CameraAnimations.UpdateSway(0);
    }

    public override void OnExit(PlayerState next) { }

    public override void OnUpdate()
    {
        player.debug.SetLine(3, $"slopeNormal: {slopeNormal}");
        player.debug.SetLine(4, $"slopeDirection: {slopeDirection}");

        DoChecks();

        /*
        movementData.finalVelocity =
            Vector3.Lerp(
                new Vector3(
                    movementData.horizontalVel.x,
                    movementData.verticalVel,
                    movementData.horizontalVel.y),
                slopeDirection,
                1);
        */

        movementData.finalVelocity = Vector3.Lerp(
            movementData.finalVelocity,
            slopeDirection * constValues.MaxSteepSlopeSpeed,
            constValues.AirHorizontalStiffness * Time.deltaTime);

        ApplyVelocity();
    }

    public override void ApplyJumpForce()
    {
        movementData.verticalVel = movementData.JumpForce * slopeNormal.y;
        movementData.appliedVerticalVel = movementData.JumpForce * slopeNormal.y;

        movementData.horizontalVel = new Vector2(slopeNormal.x * movementData.JumpForce, slopeNormal.z * movementData.JumpForce);
        movementData.finalVelocity = slopeNormal * movementData.JumpForce;

        ApplyVelocity();
    }
}
