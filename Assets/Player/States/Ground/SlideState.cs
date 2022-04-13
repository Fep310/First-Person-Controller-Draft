using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideState : GroundParentState
{
    public SlideState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
        : base(player, controller, states, stateMachine, constValues, movementData, inputData)
    {
    }

    private Vector2 slideVector;
    private Vector2 slideDirection;
    private Vector2 maxVelocity;
    private Vector2 slopeDirection;
    private float inter;
    private float endTime;
    private float extraTime;
    private bool keepSliding;

    public override void DoChecks()
    {
        base.DoChecks();

        if (inter >= 1)
        {
            if (inputData.IsPressingCrouch)
            {
                stateMachine.ChangeState(inputData.HorizontalMovementInput == Vector2.zero ? states.IdleCrouch : states.WalkCrouch);
                return;
            }
            else
            {
                if (inputData.HorizontalMovementInput == Vector2.zero)
                {
                    stateMachine.ChangeState(states.Idle);
                    return;
                }
                else
                {
                    stateMachine.ChangeState(
                        inputData.IsPressingSprint && Vector2.Dot(Vector2.up, inputData.HorizontalMovementInput) > .38f ?
                        states.Run : states.Walk);
                    return;
                }
            }
        }

        if (!inputData.IsPressingCrouch)
        {
            stateMachine.ChangeState(
                inputData.IsPressingSprint && Vector2.Dot(Vector2.up, inputData.HorizontalMovementInput) > .38f ?
                states.Run : states.Walk);
        }
    }

    public override void OnEnter(PlayerState previous)
    {
        startTime = Time.time;
        endTime = startTime + constValues.MinSlideTime;
        inter = 0;
        extraTime = 0;
        keepSliding = false;

        WorldPlayerDirectionOnSlopes();
        slideVector = new Vector2(movementData.worldPlayerDiretion.x, movementData.worldPlayerDiretion.z);
        slideDirection = slideVector.normalized;
        maxVelocity = slideDirection * constValues.SlideMaxMoveSpeed;

        UpdateExtraSlideTime();

        player.CrouchAnimator.Crouch();
    }

    public override void OnUpdate()
    {
        DoChecks();

        UpdateExtraSlideTime();

        if (!keepSliding)
        {
            inter = Mathf.InverseLerp(startTime, endTime + extraTime, Time.time);
            inter = constValues.SlideSpeedCurve.Evaluate(inter);
        }

        movementData.horizontalVel = Vector2.Lerp(maxVelocity, slideDirection * constValues.CrouchMoveSpeed, inter);

        movementData.finalVelocity = new Vector3(movementData.horizontalVel.x, movementData.appliedVerticalVel, movementData.horizontalVel.y);

        if (movementData.groundNormal != Vector3.up && Vector3.Angle(Vector3.up, movementData.groundNormal) < 45)
            movementData.finalVelocity = Vector3.ProjectOnPlane(movementData.finalVelocity, movementData.groundNormal);

        ApplyVelocity();
    }

    public override void OnExit(PlayerState next)
    {
        if (next is not CrouchParentState)
            player.CrouchAnimator.StandUp();

        if (next is not RunState || next is not AirborneState)
            player.FovController.DecreaseFov();
    }

    private void UpdateExtraSlideTime()
    {
        var groundNormalXZ = new Vector2(movementData.groundNormal.x, movementData.groundNormal.z);
        extraTime = Mathf.Clamp(Vector2.Dot(slideVector, groundNormalXZ) * 2f, -constValues.MinSlideTime / 2, 1);
        

        if (extraTime >= .5f)
        {
            startTime = Time.time;
            endTime = startTime + constValues.MinSlideTime;
            inter = 0;
            keepSliding = true;
        }
        else
            keepSliding = false;

        player.debug.SetDebugText(
            $"Dot({slideVector}, {groundNormalXZ}) * 2\n" +
            $"extraTime: {extraTime}\n" +
            $"keepSliding: {keepSliding}");
    }

    protected override void ApplyJumpForce()
    {
        movementData.verticalVel = movementData.SlideJumpForce;
        movementData.appliedVerticalVel = movementData.SlideJumpForce;

        var horizontalBoost = (constValues.SlideJumpBoost * Time.deltaTime) * movementData.worldPlayerDiretion;

        movementData.finalVelocity = new Vector3(
            movementData.horizontalVel.x + horizontalBoost.x,
            movementData.appliedVerticalVel,
            movementData.horizontalVel.y + horizontalBoost.y);

        ApplyVelocity();
    }
}
