using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoxPlayerController : MonoBehaviour
{
    private Vector2 horizontalMovementInput;
    private Vector2 mouseDeltaInput;
    private float cameraXRotation;
    private float playerYRotation;
    private bool colUp, colDown, colLeft, colRight, colForawrd, colBackward;
    private bool jumping;
    private Transform tr;
    private bool pressingJump;
    private bool pressingSprint;
    private bool sprinting;
    private bool sprintJump;

    private float jumpTimer;
    private float jumpInter;
    private float fallTimer;
    private float fallInter;

    private Vector2 xzVelocity;
    private float yVelocity;
    private Vector3 velocity;
    private Coroutine changeCamFov;
    private float minFov;
    private float maxFov;
    private readonly Vector3 halfExtends = new Vector3(.3f, .9f, .3f);

    [SerializeField] private Camera cam;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] [Range(.001f, 1f)] private float mouseLookSensitivity;
    [SerializeField] [Range(.1f, 20f)] private float walkMoveSpeed;
    [SerializeField] [Range(.1f, 20f)] private float sprintMoveSpeed;
    [SerializeField] [Range(1f, 20f)] private float jumpForce;
    [SerializeField] [Range(.1f, 1f)] private float maxJumpTimer;
    [SerializeField] [Range(1f, 20f)] private float gravity;
    [SerializeField] [Range(.1f, 1f)] private float maxFallTimer;
    [SerializeField] [Range(.0f, 50f)] private float horizontalStiffness;
    [SerializeField] [Range(.0f, 50f)] private float airHorizontalStiffness;
    [SerializeField] private AnimationCurve jumpCurve;
    [SerializeField] private AnimationCurve fallCurve;
    [SerializeField] private AnimationCurve sprintFovChangeCurve;

    public event Action<float> OnUnitsPerSecondChange;
    public event Action<bool> OnColDownChange;
    public event Action<float> OnJumpInterChange;
    public event Action<Vector3> OnVelocityChange;

    private void Awake()
    {
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        QualitySettings.vSyncCount = 0;

        tr = transform;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        minFov = cam.fieldOfView;
        maxFov = minFov + 8;
    }

    private void Start()
    {
        StartCoroutine(VelocityMagnitudeUpdate());
    }

    private IEnumerator VelocityMagnitudeUpdate()
    {
        // Vector3 pos0, pos1;
        float endTime;

        while (true)
        {
            /*pos0 = tr.position;*/
            endTime = Time.time + .05f;
            while (Time.time < endTime)
            {
                yield return null;
            }
            /*pos1 = tr.position;
            OnVelocityChange?.Invoke((pos0 - pos1).magnitude * 10);*/
            OnUnitsPerSecondChange?.Invoke(velocity.magnitude / Time.deltaTime);
            yield return null;
        }
    }

    private void Update()
    {
        RunCollisionChecks();
        HandleSprinting();

        UpdatePlayerRotation();
        UpdatePlayerMovement();

        OnColDownChange?.Invoke(colDown);
    }

    private void RunCollisionChecks()
    {
        if (jumping)
        {
            colDown = false;
        }
        else
        {
            if (Physics.BoxCast(
                tr.position + (Vector3.up * (Time.deltaTime * 4)),
                halfExtends,
                Vector3.down,
                out RaycastHit hitInfo,
                Quaternion.identity,
                Mathf.Abs(Mathf.Min(velocity.y * 2f, -Time.deltaTime * 8)),
                collisionMask))
            {
                colDown = true;

                var hitPoint = hitInfo.point;
                tr.position = new Vector3(tr.position.x, hitPoint.y + halfExtends.y, tr.position.z);
            }
            else
            {
                colDown = false;
            }
        }

        colUp = Physics.BoxCast(tr.position, halfExtends, Vector3.up, Quaternion.identity, .2f /*Time.deltaTime * 10*/, collisionMask);

        colLeft = Physics.BoxCast(tr.position, halfExtends, -Vector3.right, Quaternion.identity, .2f /*Time.deltaTime * 10*/, collisionMask);
        colRight = Physics.BoxCast(tr.position, halfExtends, Vector3.right, Quaternion.identity, .2f /*Time.deltaTime * 10*/, collisionMask);

        colForawrd = Physics.BoxCast(tr.position, halfExtends, Vector3.forward, Quaternion.identity, .2f /*Time.deltaTime * 10*/, collisionMask);
        colBackward = Physics.BoxCast(tr.position, halfExtends, -Vector3.forward, Quaternion.identity, .2f /*Time.deltaTime * 10*/, collisionMask);

        // print($"colDown: {colDown}, colForward: {colForawrd}, colBackward: {colBackward}, colLeft: {colLeft}, colRight: {colRight}");
    }

    private void HandleSprinting()
    {
        var wasSprinting = sprinting;
        sprinting = pressingSprint && Vector2.Dot(Vector2.up, horizontalMovementInput) > .38f;
        if (sprinting != wasSprinting)
            UpdateCamFov(sprinting);
    }

    private void UpdatePlayerRotation()
    {
        cameraXRotation -= mouseDeltaInput.y * mouseLookSensitivity;
        cameraXRotation = Mathf.Clamp(cameraXRotation, -89, 89);
        cam.transform.localRotation = Quaternion.Euler(cameraXRotation, 0, 0);

        playerYRotation += mouseDeltaInput.x * mouseLookSensitivity;
        tr.rotation = Quaternion.Euler(0, playerYRotation, 0);
    }

    private void UpdatePlayerMovement()
    {
        CalculateHorizontalMovement();
        CalculateVerticalMovement();

        ApplyCollisions();

        velocity = new Vector3(xzVelocity.x, yVelocity, xzVelocity.y);
        OnVelocityChange?.Invoke(velocity);

        tr.position += velocity;
    }

    private void CalculateHorizontalMovement()
    {
        Vector3 worldDirVec3 = tr.TransformDirection(new Vector3(horizontalMovementInput.x, 0, horizontalMovementInput.y));
        Vector2 worldDir = new Vector2(worldDirVec3.x, worldDirVec3.z);

        float speed;

        if (colDown)
            speed = sprinting ? sprintMoveSpeed : walkMoveSpeed;
        else
            speed = sprintJump ? Mathf.Lerp(walkMoveSpeed, sprintMoveSpeed, .75f) : walkMoveSpeed;

        xzVelocity = Vector2.Lerp(
            xzVelocity,
            worldDir * speed * Time.deltaTime,
            (colDown ? horizontalStiffness : airHorizontalStiffness) * Time.deltaTime);
    }

    private void CalculateVerticalMovement()
    {
        if (colDown)
        {
            sprintJump = false;
            jumpTimer = maxJumpTimer;
            fallTimer = 0;
            fallInter = 1;

            if (pressingJump && yVelocity <= 0)
            {
                if (sprinting)
                    sprintJump = true;
                jumpTimer = 0;

                jumping = true;
            }
        }
        else
        {
            if (jumpTimer < maxJumpTimer /*|| (jumpTimer > 0 && jumpTimer < maxJumpTimer / 2) // Min jump time*/ )
            {
                if (colUp)
                {
                    jumpTimer = maxJumpTimer;
                    fallInter = 0;
                    jumping = false;
                    return;
                }

                jumpTimer += Time.deltaTime;
                jumpInter = jumpCurve.Evaluate(Mathf.InverseLerp(0, maxJumpTimer, jumpTimer));
                yVelocity = Time.deltaTime * (jumpForce * jumpInter);

                OnJumpInterChange?.Invoke(jumpInter);
            }
            else
            {
                jumping = false;

                fallTimer += Time.deltaTime;
                fallInter = fallCurve.Evaluate(Mathf.InverseLerp(0, maxFallTimer, fallTimer));
                yVelocity = Time.deltaTime * -(gravity * fallInter);
            }
        }
    }

    private void ApplyCollisions()
    {
        xzVelocity = new Vector2(
            Mathf.Clamp(xzVelocity.x, colLeft ? 0 : -1, colRight ? 0 : 1),
            Mathf.Clamp(xzVelocity.y, colBackward ? 0 : -1, colForawrd ? 0 : 1));

        yVelocity = Mathf.Clamp(yVelocity, colDown ? 0 : -1, colUp ? 0 : 1);
    }

    #region read-and-update-input

    public void UpdateHorizontalMovementInput(InputAction.CallbackContext context)
    {
        horizontalMovementInput = context.ReadValue<Vector2>();
    }

    public void UpdateMouseDeltaInput(InputAction.CallbackContext context)
    {
        mouseDeltaInput = context.ReadValue<Vector2>();
    }

    public void UpdateJumpInput(InputAction.CallbackContext context)
    {
        if (context.started)
            pressingJump = true;

        else if (context.canceled)
            pressingJump = false;
    }

    public void UpdateSprintInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            pressingSprint = true;
        }

        else if (context.canceled)
        {
            pressingSprint = false;
        }
    }

    #endregion

    private void UpdateCamFov(bool onOff)
    {
        if (changeCamFov != null)
            StopCoroutine(changeCamFov);
        changeCamFov = StartCoroutine(ChangeCamFov(onOff));
    }

    private IEnumerator ChangeCamFov(bool onOff)
    {
        float startTime = Time.time;
        float endTime = Time.time + .12f;
        float t;
        float startFov = cam.fieldOfView;
        float endFov = onOff ? maxFov : minFov;

        while (Time.time < endTime)
        {
            t = sprintFovChangeCurve.Evaluate(Mathf.InverseLerp(startTime, endTime, Time.time));
            cam.fieldOfView = Mathf.Lerp(startFov, endFov, t);
            yield return null;
        }
        cam.fieldOfView = endFov;
    }

    private void OnDrawGizmos()
    {
        /*var _tr = transform;

        const float colliderWidth = 0.8f;
        const float colliderHeight = 1.8f;
        const float colliderHalfWidth = colliderWidth / 2;
        const float colliderHalfHeight = colliderHeight / 2;

        const int sideRaysPerColumn = 5;
        const int sideRayRows = 3;

        const int polarRaysPerColumn = 4;
        const int polarRayRows = 4;

        Matrix4x4 localToWorld = _tr.localToWorldMatrix;

        // Back and forward points
        Gizmos.color = Color.cyan;
        for (int i = 0; i < sideRayRows; i++)
        {
            float it = (float)i / (sideRayRows - 1);

            for (int n = 0; n < sideRaysPerColumn; n++)
            {
                float nt = (float)n / (sideRaysPerColumn - 1);
                
                var backPointLocal = new Vector3(colliderWidth * (it - .5f), colliderHeight * (nt - .5f), -colliderHalfWidth);
                var backPointWorld = localToWorld.MultiplyPoint(backPointLocal);

                var frontPointLocal = new Vector3(colliderWidth * (it - .5f), colliderHeight * (nt - .5f), colliderHalfWidth);
                var frontPointWorld = localToWorld.MultiplyPoint(frontPointLocal);

                Gizmos.DrawWireSphere(backPointWorld, .05f);
                Gizmos.DrawWireSphere(frontPointWorld, .05f);
            }
        }

        // Left and right points
        Gizmos.color = Color.red;
        for (int i = 0; i < sideRayRows; i++)
        {
            float it = (float)i / (sideRayRows - 1);

            for (int n = 0; n < sideRaysPerColumn; n++)
            {
                float nt = (float)n / (sideRaysPerColumn - 1);

                var leftPointLocal = new Vector3(-colliderHalfWidth, colliderHeight * (nt - .5f), colliderWidth * (it - .5f));
                var leftPointWorld = localToWorld.MultiplyPoint(leftPointLocal);

                var rightPointLocal = new Vector3(colliderHalfWidth, colliderHeight * (nt - .5f), colliderWidth * (it - .5f));
                var rightPointWorld = localToWorld.MultiplyPoint(rightPointLocal);

                Gizmos.DrawWireSphere(leftPointWorld, .04f);
                Gizmos.DrawWireSphere(rightPointWorld, .04f);
            }
        }

        // Bottom and top points
        Gizmos.color = Color.green;
        for (int i = 0; i < polarRayRows; i++)
        {
            float it = (float)i / (polarRayRows - 1);

            for (int n = 0; n < polarRaysPerColumn; n++)
            {
                float nt = (float)n / (polarRaysPerColumn - 1);

                var bottomPointLocal = new Vector3(colliderWidth * (it - .5f), -colliderHalfHeight, colliderWidth * (nt - .5f));
                var bottomPointWorld = bottomPointLocal + _tr.position;

                var topPointLocal = new Vector3(colliderWidth * (it - .5f), colliderHalfHeight, colliderWidth * (nt - .5f));
                var topPointWorld = topPointLocal + _tr.position;

                Gizmos.DrawWireSphere(bottomPointWorld, .03f);
                Gizmos.DrawWireSphere(topPointWorld, .03f);
            }
        }*/
    }
}
