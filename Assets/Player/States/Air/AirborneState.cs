using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirborneState : AirParentState
{
    public AirborneState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
        : base(player, controller, states, stateMachine, constValues, movementData, inputData)
    {
    }

    private float slideBoost;
    private float endSlideBoostTime;
    private float slideBoostInter;
    private bool doSlideBoost;

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void OnEnter(PlayerState previous)
    {
        base.OnEnter(previous);
        startTime = Time.time;
        slideBoost = 0;
        doSlideBoost = false;

        if (previous is SlideState)
        {
            slideBoost = 2;
            endSlideBoostTime = startTime + .5f;
            slideBoostInter = 0;
            doSlideBoost = true;
        }
    }

    public override void OnExit(PlayerState next) { }

    public override void OnUpdate()
    {
        DoChecks();

        if (doSlideBoost)
        {
            slideBoostInter = Mathf.InverseLerp(startTime, endSlideBoostTime, Time.time);
            slideBoost = Mathf.Lerp(2, 0, slideBoostInter);
            if (slideBoostInter == 1)
                doSlideBoost = false;
        }

        // Horizontal movement
        var worldPlayerDirVec2 = new Vector2(movementData.worldPlayerDiretion.x, movementData.worldPlayerDiretion.z);

        movementData.horizontalVel = Vector2.Lerp(
            movementData.horizontalVel,
            worldPlayerDirVec2 * (constValues.AirMoveSpeed + slideBoost),
            constValues.AirHorizontalStiffness * Time.deltaTime);

        // Gravity
        float oldVerticalVel = movementData.verticalVel;

        movementData.verticalVel += ((movementData.verticalVel < 0 || !inputData.IsPressingJump ? movementData.Gravity * 1.5f : movementData.Gravity) * Time.deltaTime);

        movementData.appliedVerticalVel = (oldVerticalVel + movementData.verticalVel) * .5f;

        if (movementData.verticalVel <= 0)
            jumping = false;

        // Construct finalVelocity
        movementData.finalVelocity = new Vector3(movementData.horizontalVel.x, movementData.appliedVerticalVel, movementData.horizontalVel.y);

        ApplyVelocity();
    }
}
