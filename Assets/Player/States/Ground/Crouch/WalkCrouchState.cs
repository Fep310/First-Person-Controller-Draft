using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkCrouchState : CrouchParentState
{
    public WalkCrouchState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
        : base(player, controller, states, stateMachine, constValues, movementData, inputData)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();

        if (inputData.HorizontalMovementInput == Vector2.zero)
            stateMachine.ChangeState(states.IdleCrouch);
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

        WorldPlayerDirectionOnSlopes();

        // Get to walk speed
        var worldPlayerDirVec2 = new Vector2(movementData.worldPlayerDiretion.x, movementData.worldPlayerDiretion.z);

        movementData.horizontalVel = Vector2.Lerp(
            movementData.horizontalVel,
            worldPlayerDirVec2 * constValues.CrouchMoveSpeed,
            constValues.HorizontalAcceleration * Time.deltaTime);

        movementData.finalVelocity = new Vector3(movementData.horizontalVel.x, movementData.appliedVerticalVel, movementData.horizontalVel.y);

        if (movementData.groundNormal != Vector3.up && Vector3.Angle(Vector3.up, movementData.groundNormal) < 45)
            movementData.finalVelocity = Vector3.ProjectOnPlane(movementData.finalVelocity, movementData.groundNormal);

        ApplyVelocity();
    }
}
