using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastFallState : AirParentState
{
    public FastFallState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
        : base(player, controller, states, stateMachine, constValues, movementData, inputData)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();

        // TODO:
        // Transition to Airborne when not crouching
    }

    public override void OnEnter(PlayerState previous) => base.OnEnter(previous);

    public override void OnExit(PlayerState next) { }

    public override void OnUpdate()
    {
        // TODO:
        // Horizontal and gravity

        ApplyVelocity();
    }
}
