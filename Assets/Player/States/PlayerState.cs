using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState
{
    protected Player player;
    protected CharacterController controller;
    protected PlayerStates states;
    protected PlayerStateMachine stateMachine;
    protected PlayerConstantMovementValues constValues;
    protected PlayerMovementData movementData;
    protected PlayerInputData inputData;

    protected float startTime;
    protected bool jumping;
    protected bool probablyOnCorner;

    public PlayerState(Player player, CharacterController controller, PlayerStates states, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
    {
        this.player = player;
        this.controller = controller;
        this.states = states;
        this.stateMachine = stateMachine;
        this.constValues = constValues;
        this.movementData = movementData;
        this.inputData = inputData;
    }

    public virtual void OnEnter(PlayerState previous)
    {
        DoChecks();
        startTime = Time.time;
    }

    public virtual void OnUpdate() { }

    public virtual void OnExit(PlayerState next) { }

    public virtual void DoChecks() { }

    protected void ApplyVelocity() => controller.Move(movementData.finalVelocity * Time.deltaTime);

    protected bool CheckGround()
    {
        Vector3 origin = player.Transform.position + (Vector3.up * controller.radius);
        Vector3 direction = Vector3.down;

        if (Physics.SphereCast(origin, controller.radius, direction, out RaycastHit hitInfo, constValues.CollisionRayLenght, constValues.CollisionMask))
        {
            if (Physics.Raycast(
                origin + (direction * controller.radius),
                direction,
                out RaycastHit _hitInfo,
                controller.radius,
                constValues.CollisionMask))
            {
                probablyOnCorner = false;
                movementData.groundNormal = hitInfo.normal;
            }
            else
            {
                probablyOnCorner = true;
                movementData.groundNormal = Vector3.up;
            }

            return true;
        }

        probablyOnCorner = false;

        return false;
    }

    protected bool CheckCeiling()
    {
        Vector3 origin = player.Transform.position + ((controller.skinWidth + controller.height - controller.radius) * Vector3.up);
        Vector3 direction = Vector3.up;

        if (Physics.SphereCast(origin, controller.radius, direction, out RaycastHit hitInfo, constValues.CollisionRayLenght, constValues.CollisionMask))
        {
            movementData.ceilingNormal = hitInfo.normal;

            /*if (hitInfo.normal == Vector3.up)
                movementData.finalVelocity.y = 0f;*/

            return true;
        }

        return false;
    }

    protected void WorldPlayerDirectionOnSlopes()
    {
        Vector3 projected = Vector3.ProjectOnPlane(movementData.worldPlayerDiretion, movementData.groundNormal);
        movementData.worldPlayerDiretion.y = projected.y;
    }
}
