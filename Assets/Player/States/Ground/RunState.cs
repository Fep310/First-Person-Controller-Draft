using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunState : GroundParentState
{
    public RunState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
        : base(player, controller, states, stateMachine, constValues, movementData, inputData)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();

        if (inputData.IsPressingCrouch)
        {
            stateMachine.ChangeState(states.ToSlide);
            return;
        }

        if (!inputData.IsPressingSprint || Vector2.Dot(Vector2.up, inputData.HorizontalMovementInput) < .38f)
        {
            player.FovController.DecreaseFov();
            stateMachine.ChangeState(states.Walk);
            return;
        }

        if (inputData.HorizontalMovementInput == Vector2.zero)
        {
            player.FovController.DecreaseFov();
            stateMachine.ChangeState(states.Idle);
        }
    }

    public override void OnEnter(PlayerState previous)
    {
        base.OnEnter(previous);

        player.FovController.IncreaseFov();
    }

    public override void OnExit(PlayerState next) => base.OnExit(next);

    public override void OnUpdate()
    {
        DoChecks();

        player.CameraAnimations.UpdateSway(inputData.HorizontalMovementInput.x);

        WorldPlayerDirectionOnSlopes();

        // Get to run speed
        var worldPlayerDirVec2 = new Vector2(movementData.worldPlayerDiretion.x, movementData.worldPlayerDiretion.z);

        movementData.horizontalVel = Vector2.Lerp(
            movementData.horizontalVel,
            worldPlayerDirVec2 * constValues.SprintMoveSpeed,
            constValues.HorizontalAcceleration * Time.deltaTime);

        movementData.finalVelocity = new Vector3(movementData.horizontalVel.x, movementData.appliedVerticalVel, movementData.horizontalVel.y);

        if (movementData.groundNormal != Vector3.up && Vector3.Angle(Vector3.up, movementData.groundNormal) < 45)
            movementData.finalVelocity = Vector3.ProjectOnPlane(movementData.finalVelocity, movementData.groundNormal);

        ApplyVelocity();

        if (crouching)
            TryToStandup();
    }

    public override void ApplyJumpForce()
    {
        movementData.jumping = true;

        movementData.verticalVel = movementData.JumpForce;
        movementData.appliedVerticalVel = movementData.JumpForce;

        var horizontalBoost = (constValues.RunJumpBoost * Time.deltaTime) * movementData.worldPlayerDiretion;

        movementData.finalVelocity = new Vector3(
            movementData.horizontalVel.x + horizontalBoost.x,
            movementData.appliedVerticalVel,
            movementData.horizontalVel.y + horizontalBoost.y);
        
        ApplyVelocity();

        player.CameraAnimations.JumpBob(1.2f);
    }
}
