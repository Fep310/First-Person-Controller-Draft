using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GroundParentState : PlayerState
{
    // TODO: Check wall and influence the player's velocity
    // BUG: Landing after coyote jumping is broken

    protected GroundParentState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
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

        if (onSteepSlope)
        {
            stateMachine.ChangeState(states.SteepSlope);
            return;
        }

        if (inputData.IsPressingJump)
        {
            ApplyJumpForce();
            stateMachine.ChangeState(states.Airborne);
            return;
        }

        movementData.verticalVel = movementData.appliedVerticalVel = 0;
    }

    public override void ApplyJumpForce() => base.ApplyJumpForce();

    public override void OnEnter(PlayerState previous)
    {
        base.OnEnter(previous);

        if (previous is AirParentState)
        {
            float yDifference = movementData.lastGroundY - player.Transform.position.y;
            float landMultiplier = Mathf.Clamp(yDifference * .2f, 0, 1.5f);

            if (landMultiplier < .35f)
                return;

            player.CameraAnimations.LandBob(landMultiplier);
        }

        movementData.hitCeiling = false;
    }

    public override void OnExit(PlayerState next) { }

    public override void OnUpdate() { }

    protected void TryToStandup()
    {
        player.StartCoroutine(TryToStandUpCoroutine());

        IEnumerator TryToStandUpCoroutine()
        {
            while (!CanStandup) yield return null;

            player.CrouchAnimator.StandUp();
            crouching = false;
        }
    }
}
