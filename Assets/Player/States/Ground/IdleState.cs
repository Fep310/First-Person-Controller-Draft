using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : GroundParentState
{
    public IdleState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
        : base(player, controller, states, stateMachine, constValues, movementData, inputData)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();

        if (inputData.HorizontalMovementInput != Vector2.zero)
        {
            stateMachine.ChangeState(
                inputData.IsPressingSprint && Vector2.Dot(Vector2.up, inputData.HorizontalMovementInput) > .38f ?
                states.Run : states.Walk);
            return;
        }

        if (inputData.IsPressingCrouch)
            stateMachine.ChangeState(states.IdleCrouch);
    }

    public override void OnEnter(PlayerState previous) => base.OnEnter(previous);

    public override void OnExit(PlayerState next) { /*Debug.Log("Exiting Idle");*/ }

    public override void OnUpdate()
    {
        DoChecks();

        // Deaccelerate if moving
        if (movementData.horizontalVel.sqrMagnitude >= float.Epsilon)
        {
            WorldPlayerDirectionOnSlopes();

            movementData.horizontalVel = Vector2.Lerp(
                movementData.horizontalVel,
                Vector2.zero,
                constValues.HorizontalDeacceleration * Time.deltaTime);

            movementData.finalVelocity = new Vector3(movementData.horizontalVel.x, movementData.appliedVerticalVel, movementData.horizontalVel.y);
            movementData.finalVelocity.Scale(movementData.worldPlayerDiretion);

            ApplyVelocity();
        }
    }

    protected override void ApplyJumpForce() => base.ApplyJumpForce();
}
