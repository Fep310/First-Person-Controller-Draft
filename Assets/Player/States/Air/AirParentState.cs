using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AirParentState : PlayerState
{
    public AirParentState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
        : base(player, controller, states, stateMachine, constValues, movementData, inputData)
    {
    }

    protected bool decreasedBoolAlready;

    public override void DoChecks()
    {
        if (CheckCeiling())
            movementData.verticalVel = movementData.appliedVerticalVel = -.2f;

        if (!decreasedBoolAlready && !inputData.IsPressingSprint)
        {
            decreasedBoolAlready = true;
            player.FovController.DecreaseFov();
        }

        if (!jumping && CheckGround())
        {
            if (inputData.HorizontalMovementInput == Vector2.zero)
            {
                stateMachine.ChangeState(states.Idle);
                return;
            }

            stateMachine.ChangeState(inputData.IsPressingSprint ? states.Run : states.Walk);
            return;
        }
    }

    public override void OnEnter(PlayerState previous) { decreasedBoolAlready = false; }

    public override void OnExit(PlayerState next) { }

    // Both horizontal and vertical movements should differ on Airborne and FastFall states
    public override void OnUpdate() { }
}
