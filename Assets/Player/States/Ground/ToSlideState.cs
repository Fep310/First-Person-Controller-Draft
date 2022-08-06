using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToSlideState : GroundParentState
{
    public ToSlideState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
        : base(player, controller, states, stateMachine, constValues, movementData, inputData)
    {
    }

    private float endTime;
    private Vector2 startVector;
    private Vector2 slideVector;
    private Vector2 aimingVector;
    private float t;

    public override void DoChecks()
    {
        base.DoChecks();

        if (Time.time - startTime > constValues.SlideTransitionTime)
        {
            stateMachine.ChangeState(states.Slide);
            return;
        }
    }

    public override void OnEnter(PlayerState previous)
    {
        base.OnEnter(previous);

        WorldPlayerDirectionOnSlopes();

        t = 0;
        startVector = movementData.horizontalVel;
        UpdateAimingVector();
        slideVector = Vector2.Lerp(startVector, aimingVector, .8f);
        endTime = startTime + constValues.SlideTransitionTime;

        crouching = true;
        player.CrouchAnimator.Crouch();

        player.CameraAnimations.UpdateSway(0);
    }

    public override void OnExit(PlayerState next)
    {
        base.OnExit(next);

        if (next is not SlideState)
            TryToStandup();
    }

    public override void OnUpdate()
    {
        DoChecks();

        t = Mathf.InverseLerp(startTime, endTime, Time.time);

        UpdateAimingVector();
        slideVector = Vector2.Lerp(startVector, aimingVector, .8f);
        movementData.horizontalVel = Vector2.Lerp(startVector, slideVector, t);

        movementData.finalVelocity = new Vector3(movementData.horizontalVel.x, movementData.appliedVerticalVel, movementData.horizontalVel.y);

        if (Vector3.Angle(Vector3.up, movementData.groundNormal) < 45)
            movementData.finalVelocity = Vector3.ProjectOnPlane(movementData.finalVelocity, movementData.groundNormal);

        ApplyVelocity();
    }

    public override void ApplyJumpForce() => base.ApplyJumpForce();

    private void UpdateAimingVector()
    {
        aimingVector = new Vector2(movementData.worldPlayerDiretion.x, movementData.worldPlayerDiretion.z).normalized * constValues.SprintMoveSpeed;
    }
}
