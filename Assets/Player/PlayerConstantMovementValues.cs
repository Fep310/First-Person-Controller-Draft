using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerConstantMovementValues : ScriptableObject
{
    [Space]
    //[Header("Horizontal Movement")]
    [SerializeField] [Range(.1f, 15f)] private float walkMoveSpeed;
    [SerializeField] [Range(.1f, 15f)] private float sprintMoveSpeed;
    [SerializeField] [Range(.1f, 15f)] private float crouchMoveSpeed;
    [SerializeField] [Range(.0f, 30)] private float horizontalAcceleration;
    [SerializeField] [Range(.0f, 30)] private float horizontalDeacceleration;
    [SerializeField] [Range(.0f, 30f)] private float airHorizontalStiffness;

    [Space]
    [Header("Jump")]
    [SerializeField] [Range(0f, 5f)] private float jumpHeight;
    [SerializeField] [Range(0f, 2f)] private float jumpTime;
    [SerializeField] [Range(1f, 5f)] private float runJumpBoost;
    [SerializeField] [Range(0f, 5f)] private float slideJumpHeight;
    [SerializeField] [Range(0f, 2f)] private float slideJumpTime;
    [SerializeField] [Range(1f, 5f)] private float slideJumpBoost;
    [SerializeField][Range(0f, 1f)] private float coyoteTime;

    [Space]
    [Header("Slide")]
    [SerializeField] [Range(.1f, 15f)] private float slideMaxMoveSpeed;
    [SerializeField][Range(0f, 1f)] private float slideTransitionTime;
    [SerializeField] [Range(0f, 3f)] private float minSlideTime;
    [SerializeField] private AnimationCurve slideSpeedCurve;
    [SerializeField] [Range(.1f, 30f)] private float maxSteepSlopeSpeed;

    [Space]
    [Header("Misc")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] [Range(0f, .25f)] private float collisionRayLenght;
    [SerializeField] [Range(0f, 1f)] private float mouseLookSensitivity;

    public float WalkMoveSpeed { get => walkMoveSpeed; private set => walkMoveSpeed = value; }
    public float SprintMoveSpeed { get => sprintMoveSpeed; private set => sprintMoveSpeed = value; }
    public float AirMoveSpeed => (WalkMoveSpeed + SprintMoveSpeed) / 2;
    public float CrouchMoveSpeed { get => crouchMoveSpeed; private set => crouchMoveSpeed = value; }
    public float HorizontalAcceleration { get => horizontalAcceleration; private set => horizontalAcceleration = value; }
    public float HorizontalDeacceleration { get => horizontalDeacceleration; private set => horizontalDeacceleration = value; }
    public float AirHorizontalStiffness { get => airHorizontalStiffness; private set => airHorizontalStiffness = value; }

    public float JumpHeight { get => jumpHeight; private set => jumpHeight = value; }
    public float JumpTime { get => jumpTime; private set => jumpTime = value; }
    public float RunJumpBoost { get => runJumpBoost; private set => runJumpBoost = value; }
    public float SlideJumpHeight { get => slideJumpHeight; private set => slideJumpHeight = value; }
    public float SlideJumpTime { get => slideJumpTime; private set => slideJumpTime = value; }
    public float SlideJumpBoost { get => slideJumpBoost; private set => slideJumpBoost = value; }
    public float CoyoteTime { get => coyoteTime; private set => coyoteTime = value; }

    public float SlideMaxMoveSpeed { get => slideMaxMoveSpeed; private set => slideMaxMoveSpeed = value; }
    public float SlideTransitionTime { get => slideTransitionTime; private set => slideTransitionTime = value; }
    public float MinSlideTime { get => minSlideTime; private set => minSlideTime = value; }
    public AnimationCurve SlideSpeedCurve { get => slideSpeedCurve; private set => slideSpeedCurve = value; }
    public float MaxSteepSlopeSpeed { get => maxSteepSlopeSpeed; private set => maxSteepSlopeSpeed = value; }

    public LayerMask CollisionMask { get => collisionMask; private set => collisionMask = value; }
    public float CollisionRayLenght { get => collisionRayLenght; private set => collisionRayLenght = value; }
    public float MouseLookSensitivity { get => mouseLookSensitivity; private set => mouseLookSensitivity = value; }
}
