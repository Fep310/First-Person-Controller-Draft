using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementData
{
    public float cameraXRotation;
    public float playerYRotation;
    public Vector3 groundNormal;
    public Vector3 ceilingNormal;
    public Vector3 wallNormal;
    public Vector3 worldPlayerDiretion;
    public Vector2 horizontalVel;
    public float verticalVel;
    public float appliedVerticalVel;
    public Vector3 finalVelocity;
    
    public float Gravity { get; private set; }
    public float JumpForce { get; private set; }
    public float SlideJumpForce { get; private set; }

    public PlayerMovementData(float jumpHeight, float jumpTime, float slideJumpHeight, float slideJumpTime)
    {
        CalculateVerticalForces(jumpHeight, jumpTime, slideJumpHeight, slideJumpTime);
        Reset();
    }

    public void Reset()
    {
        cameraXRotation = 0;
        playerYRotation = 0;

        var zeroVec = Vector3.zero;

        groundNormal = zeroVec;
        ceilingNormal = zeroVec;
        wallNormal = zeroVec;
        worldPlayerDiretion = zeroVec;
        finalVelocity = zeroVec;
    }

    private void CalculateVerticalForces(float jumpHeight, float jumpTime, float slideJumpHeight, float slideJumpTime)
    {
        // Debug.Log($"jumpHeight: {jumpHeight}, horSpeed: {horSpeed}, jumpTime: {jumpTime}");

        Gravity = (-2 * jumpHeight) / Mathf.Pow(jumpTime, 2);
        JumpForce = (2 * jumpHeight) / jumpTime;
        SlideJumpForce = (2 * slideJumpHeight) / slideJumpTime;

        Debug.Log($"Gravity: {Gravity}, JumpForce: {JumpForce}, SlideJumpForce: {SlideJumpForce}");

        // Gravity = -20;
        // JumpForce = 8;
    }
}
