using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    protected bool probablyOnCorner;
    protected bool onSteepSlope;
    protected bool crouching;

    protected bool CanStandup => !Physics.SphereCast(player.Transform.position, controller.radius * 1.5f, Vector3.up, out _, player.CrouchAnimator.OriginalHeight, constValues.CollisionMask);

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
        // DoChecks();
        startTime = Time.time;
    }

    public virtual void OnUpdate()
    {

    }

    public virtual void OnExit(PlayerState next) { }

    public virtual void DoChecks() { }

    protected void ApplyVelocity()
    {
        // player.debug.SetLine(6, movementData.finalVelocity.ToString());
        controller.Move(movementData.finalVelocity * Time.deltaTime);
    } 

    public virtual void ApplyJumpForce()
    {
        movementData.jumping = true;

        movementData.verticalVel = movementData.JumpForce;
        movementData.appliedVerticalVel = movementData.JumpForce;

        movementData.finalVelocity.y = movementData.appliedVerticalVel;

        ApplyVelocity();

        player.CameraAnimations.JumpBob(1);
    }

    protected bool CheckGround()
    {
        Vector3 origin = player.Transform.position + (Vector3.up * controller.radius);
        Vector3 direction = Vector3.down;

        if (Physics.SphereCast(origin, controller.radius, direction, out RaycastHit hitInfo, constValues.CollisionRayLenght, constValues.CollisionMask))
        {
            bool onCorner = false;

            var hits = new List<RaycastHit>();

            Vector3[] positions = new Vector3[4];
            positions[0] = new Vector3(origin.x, origin.y, origin.z + controller.radius);
            positions[1] = new Vector3(origin.x + controller.radius, origin.y, origin.z);
            positions[2] = new Vector3(origin.x, origin.y, origin.z - controller.radius);
            positions[3] = new Vector3(origin.x - controller.radius, origin.y, origin.z);

            Vector3 toPointVector = hitInfo.point - origin;
            Vector3 newDirection = toPointVector.normalized;
            float newDistance = toPointVector.magnitude;

            for (int i = 0; i < positions.Length; i++)
            {
                if (Physics.Raycast(positions[i], newDirection, out RaycastHit hit, newDistance, constValues.CollisionMask))
                    hits.Add(hit);
            }

            if (hits.Count > 0)
            {
                onCorner = hits.Any(h => h.normal != hitInfo.normal);
            }

            // movementData.groundNormal = onCorner ? Vector3.up : hitInfo.normal;
            movementData.groundNormal = hitInfo.normal;

            float angle = Vector3.Angle(Vector3.up, movementData.groundNormal);

            player.debug.Clear();

            int hitIndex = 2;
            // var normalVsNormalAngles = new List<float>();
            float normalVsNormalAnglesSum = 0;

            foreach (var hit in hits)
            {
                float normalVsNormalAngle = Vector3.Angle(hitInfo.normal, hit.normal);
                normalVsNormalAnglesSum += normalVsNormalAngle;
                // float normalVsUpAngle = Vector3.Angle(Vector3.up, hit.normal);
                // normalVsNormalAngles.Add(normalVsNormalAngle);
                //player.debug.SetLine(hitIndex, $"[{hitIndex - 2} HIT] VsNormalAngle: {normalVsNormalAngle}, VsUpAngle {normalVsUpAngle}");
                hitIndex++;
            }

            onSteepSlope = angle > 45 && normalVsNormalAnglesSum == 0;

            player.debug.SetLine(0, $"ground angle: {angle}");
            player.debug.SetLine(1, $"onCorner: {onCorner}");
            player.debug.SetLine(2, $"onSteepSlope: {onSteepSlope}");

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

    protected bool CheckWall()
    {
        /*Vector3 origin = ;
        Vector3 direction = ;

        if (Physics.CapsuleCast())
        {
            return true;
        }*/

        return false;
    }

    protected void WorldPlayerDirectionOnSlopes()
    {
        Vector3 projected = Vector3.ProjectOnPlane(movementData.worldPlayerDiretion, movementData.groundNormal);
        movementData.worldPlayerDiretion.y = projected.y;
    }
}
