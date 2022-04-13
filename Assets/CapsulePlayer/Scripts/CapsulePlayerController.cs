using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class CapsulePlayerController : MonoBehaviour
{
    #region components
    private Transform tr;
    [SerializeField] private Camera playerCam;
    [SerializeField] private CapsuleCollider capsuleCol;

    #endregion

    #region input-values
    private Vector2 horizontalMovementInput;
    private Vector2 mouseDeltaInput;
    private bool pressingJump;
    private bool pressingSprint;
    #endregion

    #region movement-values
    private float cameraXRotation;
    private float playerYRotation;
    private List<Vector3> collisionNormals;
    // private Vector3 collisionNormal;
    private Vector3 worldPlayerDiretion;
    private Vector2 xzVelocity;
    private float yVelocity;
    private Vector3 velocity;
    #endregion

    #region movement-states
    private bool jumping;
    private bool falling;
    private bool sprinting;
    private bool sprintJump;
    private bool colDown;
    private bool colUp;
    #endregion

    #region coroutine-vars
    private Coroutine changeCamFovCoroutine;
    private Coroutine jumpCoroutine;
    private Coroutine fallCoroutine;
    #endregion

    #region misc-vars
    private float minFov;
    private float maxFov;
    Vector3 point0, point1, point3, normal0, normal1;
    private bool wait;
    #endregion

    #region movement-vars
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] [Range(.001f, 1f)] private float mouseLookSensitivity;
    [SerializeField] [Range(.1f, 20f)] private float walkMoveSpeed;
    [SerializeField] [Range(.1f, 20f)] private float sprintMoveSpeed;
    [SerializeField] [Range(1f, 20f)] private float jumpForce;
    [SerializeField] [Range(.1f, 1f)] private float maxJumpTimer;
    [SerializeField] [Range(1f, 20f)] private float maxFallSpeed;
    [SerializeField] [Range(.1f, 5f)] private float maxFallTimer;
    [SerializeField] [Range(.0f, 50f)] private float horizontalStiffness;
    [SerializeField] [Range(.0f, 50f)] private float airHorizontalStiffness;
    [SerializeField] private AnimationCurve jumpCurve;
    [SerializeField] private AnimationCurve fallCurve;
    [SerializeField] private AnimationCurve sprintFovChangeCurve;
    #endregion

    #region setup
    private void Awake()
    {
        Application.targetFrameRate = 144 /*Screen.currentResolution.refreshRate*/;
        QualitySettings.vSyncCount = 0;

        tr = transform;
        collisionNormals = new List<Vector3>();

        SetupCursor();
        SetupCameraFov();
    }

    private void Start()
    {
        wait = true;
        StartCoroutine(WaitAndFallCoroutine());
    }

    private IEnumerator WaitAndFallCoroutine()
    {
        yield return new WaitForSeconds(1);

        falling = true;
        fallCoroutine = StartCoroutine(FallCoroutine());
        wait = false;
    }

    private void SetupCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void SetupCameraFov()
    {
        minFov = playerCam.fieldOfView;
        maxFov = minFov + 8;
    }
    #endregion

    private void Update()
    {
        if (wait)
            return;

        capsuleCol.transform.position = tr.position;

        worldPlayerDiretion = tr.TransformDirection(new Vector3(horizontalMovementInput.x, 0, horizontalMovementInput.y));

        HandleSprinting();
        UpdatePlayerRotation();
        CalculateHorizontalMovement();

        CheckCollisions();
        CheckForJump();
        ApplyHorizontalCollision();

        velocity = new Vector3(xzVelocity.x, yVelocity, xzVelocity.y);
        tr.position += velocity;
    }

    private void CheckForJump()
    {
        if (colDown && !jumping && !colUp && pressingJump)
        {
            if (sprinting)
                sprintJump = true;

            if (jumpCoroutine != null)
                StopCoroutine(jumpCoroutine);

            colDown = false;
            jumping = true;
            jumpCoroutine = StartCoroutine(JumpCoroutine());
        }
    }

    private void CheckCollisions()
    {
        float height = capsuleCol.height;
        float radius = capsuleCol.radius;
        float realHeight = height - (radius * 2);

        if (!jumping)
            CheckGroundCollision(height);

        // TODO
        // if (!falling)
        //     CheckCeilingCollision();

        if (velocity != Vector3.zero)
            CheckHorizontalCollision(realHeight, radius);
    }

    private void CheckGroundCollision(float height)
    {
        float deltaTimeDistance = Mathf.Abs(yVelocity) * 2;

        Vector3 origin;

        for (int i = 0; i < 8; i++)
        {
            float t = i / 8f;
            float x = Mathf.Sin(Mathf.PI * 2 * t) * (capsuleCol.radius - .05f);
            float z = Mathf.Cos(Mathf.PI * 2 * t) * (capsuleCol.radius - .05f);

            origin = new Vector3(
                tr.position.x + x,
                tr.position.y - (height / 2) + (deltaTimeDistance / 2),
                tr.position.z + z);

            // If any of these raycasts hit, the player should be grounded.
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, deltaTimeDistance, collisionMask))
            {
                colDown = true;
                yVelocity = 0;

                if (falling)
                {
                    falling = false;
                    sprintJump = false;

                    if (fallCoroutine != null)
                        StopCoroutine(fallCoroutine);

                    // Snap the player if they were falling
                    // This is not smooth.
                    tr.position = new Vector3(tr.position.x, hitInfo.point.y + (height / 2), tr.position.z);
                }
                return;
            }
        }

        colDown = false;

        // Start falling coroutine if not already startred
        if (!falling)
        {
            falling = true;
            fallCoroutine = StartCoroutine(FallCoroutine());
        }
    }

    private IEnumerator JumpCoroutine()
    {
        float startTime = Time.time;
        float endTime = Time.time + maxJumpTimer;
        float t;

        while (Time.time < endTime)
        {
            t = Mathf.InverseLerp(startTime, endTime, Time.time);
            yVelocity = jumpCurve.Evaluate(t) * jumpForce * Time.deltaTime;

            yield return null;
        }

        jumping = false;
    }

    private IEnumerator FallCoroutine()
    {
        float startTime = Time.time;
        float maxTime = Time.time + maxFallTimer;
        float t;

        while (true)
        {
            if (Time.time < maxTime)
            {
                t = Mathf.InverseLerp(startTime, maxTime, Time.time);
                yVelocity = fallCurve.Evaluate(t) * -maxFallSpeed * Time.deltaTime;
            }
            else
                yVelocity = -maxFallSpeed * Time.deltaTime;

            yield return null;
        }
    }

    private void CheckHorizontalCollision(float realHeight, float radius)
    {
        float deltaTimeDistance = /*Time.deltaTime * 8 + */xzVelocity.magnitude;

        Vector3 direction = new Vector3(xzVelocity.x, 0, xzVelocity.y).normalized;
        Vector3 p0 = tr.position - (Vector3.up * ((realHeight * .5f) - deltaTimeDistance));
        Vector3 p1 = p0 + (Vector3.up * realHeight);

        // The capsulecast has to have some inset in order to keep colliding to a wall
        p0 -= direction * (deltaTimeDistance * 0.5f);
        p1 -= direction * (deltaTimeDistance * 0.5f);

        var hits = Physics.CapsuleCastAll(p0, p1, radius, direction, deltaTimeDistance, collisionMask);

        if (hits.Length == 0)
        {
            collisionNormals.Clear();
            return;
        }

        if (hits.Any(h => h.distance == 0))
            return;

        var maxDist = hits.Max(h => h.distance);

        collisionNormals.Clear();
        foreach (var hit in hits)
            collisionNormals.Add(hit.normal);

        tr.position = new Vector3(p0.x, tr.position.y, p0.z) + (direction * maxDist);
    }

    private void ApplyHorizontalCollision()
    {
        if (collisionNormals.Count == 0)
            return;

        if (collisionNormals.Count == 1)
        {
            var normal = collisionNormals[0];
            if (Vector2.Dot(xzVelocity, new Vector2(normal.x, normal.z)) < 0)
            {
                var projectedVel = Vector3.ProjectOnPlane(new Vector3(xzVelocity.x, 0 , xzVelocity.y), normal);
                xzVelocity = new Vector2(projectedVel.x, projectedVel.z);
            }
        }
        else
        {
            foreach (var normal in collisionNormals)
            {
                if (Vector2.Dot(new Vector2(velocity.x, velocity.z), new Vector2(normal.x, normal.z)) < 0)
                {
                    var projectedVel = Vector3.ProjectOnPlane(new Vector3(xzVelocity.x, 0, xzVelocity.y), normal);
                    xzVelocity = new Vector2(projectedVel.x, projectedVel.z);
                }
            }

            // This is a very bad and lazy fix to steep corners collision not working
            float normalsDot = Vector2.Dot(new Vector2(collisionNormals[0].x, collisionNormals[0].z), new Vector2(collisionNormals[1].x, collisionNormals[1].z));
            if (normalsDot < 0)
            {
                var opposite = (collisionNormals[0] + collisionNormals[1]) * .004f;
                xzVelocity = new Vector2(opposite.x, opposite.z);
            }
        }
    }

    private void CalculateHorizontalMovement()
    {
        Vector2 worldPlayerDirVec2 = new Vector2(worldPlayerDiretion.x, worldPlayerDiretion.z);

        float speed;

        if (colDown)
            speed = sprinting ? sprintMoveSpeed : walkMoveSpeed;
        else
            speed = sprintJump ? Mathf.Lerp(walkMoveSpeed, sprintMoveSpeed, .82f) : walkMoveSpeed;

        xzVelocity = Vector2.Lerp(
            xzVelocity,
            worldPlayerDirVec2 * speed * Time.deltaTime,
            (colDown ? horizontalStiffness : airHorizontalStiffness) * Time.deltaTime);
    }

    private void UpdatePlayerRotation()
    {
        cameraXRotation -= mouseDeltaInput.y * mouseLookSensitivity;
        cameraXRotation = Mathf.Clamp(cameraXRotation, -89, 89);
        playerCam.transform.localRotation = Quaternion.Euler(cameraXRotation, 0, 0);

        playerYRotation += mouseDeltaInput.x * mouseLookSensitivity;
        tr.rotation = Quaternion.Euler(0, playerYRotation, 0);
    }

    private void HandleSprinting()
    {
        var wasSprinting = sprinting;
        sprinting = pressingSprint && Vector2.Dot(Vector2.up, horizontalMovementInput) > .38f;

        if (sprinting != wasSprinting)
            UpdateCamFov(sprinting);
    }

    #region camera-fov
    private void UpdateCamFov(bool onOff)
    {
        if (changeCamFovCoroutine != null)
            StopCoroutine(changeCamFovCoroutine);
        changeCamFovCoroutine = StartCoroutine(ChangeCamFov(onOff));
    }

    private IEnumerator ChangeCamFov(bool onOff)
    {
        float startTime = Time.time;
        float endTime = Time.time + .12f;
        float t;
        float startFov = playerCam.fieldOfView;
        float endFov = onOff ? maxFov : minFov;

        while (Time.time < endTime)
        {
            t = sprintFovChangeCurve.Evaluate(Mathf.InverseLerp(startTime, endTime, Time.time));
            playerCam.fieldOfView = Mathf.Lerp(startFov, endFov, t);
            yield return null;
        }
        playerCam.fieldOfView = endFov;
    }
    #endregion

    #region read-update-input
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
            pressingSprint = true;


        else if (context.canceled)
            pressingSprint = false;
    }
    #endregion
}
