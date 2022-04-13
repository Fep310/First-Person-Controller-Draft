using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkState : GroundParentState
{
    public WalkState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
        : base(player, controller, states, stateMachine, constValues, movementData, inputData)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();

        if (inputData.IsPressingCrouch)
        {
            stateMachine.ChangeState(states.WalkCrouch);
            return;
        }

        if (inputData.IsPressingSprint && Vector2.Dot(Vector2.up, inputData.HorizontalMovementInput) > .38f)
        {
            stateMachine.ChangeState(states.Run);
            return;
        }

        if (inputData.HorizontalMovementInput == Vector2.zero)
        {
            stateMachine.ChangeState(states.Idle);
            return;
        }
    }

    public override void OnEnter(PlayerState previous) => base.OnEnter(previous);

    public override void OnExit(PlayerState next) { }

    public override void OnUpdate()
    {
        DoChecks();

        WorldPlayerDirectionOnSlopes();

        // Get to walk speed
        var worldPlayerDirVec2 = new Vector2(movementData.worldPlayerDiretion.x, movementData.worldPlayerDiretion.z);

        movementData.horizontalVel = Vector2.Lerp(
            movementData.horizontalVel,
            worldPlayerDirVec2 * constValues.WalkMoveSpeed,
            constValues.HorizontalAcceleration * Time.deltaTime);

        movementData.finalVelocity = new Vector3(movementData.horizontalVel.x, movementData.appliedVerticalVel, movementData.horizontalVel.y);

        if (movementData.groundNormal != Vector3.up && Vector3.Angle(Vector3.up, movementData.groundNormal) < 45)
            movementData.finalVelocity = Vector3.ProjectOnPlane(movementData.finalVelocity, movementData.groundNormal);

        ApplyVelocity();
    }

    protected override void ApplyJumpForce() => base.ApplyJumpForce();
}
