using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODO:
// Extract jump and fall coroutines to update, just like other controllers do
// I want to do this bcs the jumps are higher on lower framerate and I think the transition between coroutines are the problem

public class FirstPersonPlayerController : MonoBehaviour
{
    #region components
    private Transform tr;
    [Header("Components")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CharacterController controller;
    [SerializeField] private FirstPersonPlayerInputData inputData;
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
    Vector3 groundNormal;
    #endregion

    #region movement-states
    private bool jumping;
    private bool falling;
    private bool sprinting;
    private bool sprintJump;
    private bool grounded;
    private bool colUp;
    // private bool onSteepSlope;
    #endregion

    #region coroutine-vars
    private Coroutine changeCamFovCoroutine;
    private Coroutine jumpFallCoroutine;
    #endregion

    #region misc-vars
    private float minFov;
    private float maxFov;
    #endregion

    #region movement-vars
    [Space]
    [Header("Horizontal Movement")]
    [SerializeField] [Range(.1f, 20f)] private float walkMoveSpeed;
    [SerializeField] [Range(.1f, 20f)] private float sprintMoveSpeed;
    [SerializeField] [Range(.0f, 50f)] private float horizontalStiffness;
    [SerializeField] [Range(.0f, 50f)] private float airHorizontalStiffness;

    [Space]
    [Header("Jump")]
    [SerializeField] [Range(1f, 20f)] private float jumpForce;
    [SerializeField] [Range(.1f, 1f)] private float maxJumpTimer;
    [SerializeField] private AnimationCurve jumpCurve;
    [Space]
    [SerializeField] [Range(0f, 100f)] private float jumpHeight;
    [SerializeField] [Range(0f, 2f)] private float maxJumpTime;
    // [SerializeField] [Range(0f, 2f)] private float gravity;

    [Space]
    [Header("Fall")]
    [SerializeField] [Range(1f, 20f)] private float maxFallSpeed;
    [SerializeField] [Range(.1f, 5f)] private float maxFallTimer;
    [SerializeField] private AnimationCurve fallCurve;

    [Space]
    [Header("Misc")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] [Range(.001f, 1f)] private float collisionDetectionRayLenght;
    [SerializeField] [Range(.001f, 1f)] private float mouseLookSensitivity;
    [SerializeField] private AnimationCurve sprintFovChangeCurve;
    #endregion

    private float gravity;
    private float initialJumpVel;
    private bool stopGravity;

    #region setup
    private void Awake()
    {
        Application.targetFrameRate = 30 /*Screen.currentResolution.refreshRate*/;
        QualitySettings.vSyncCount = 0;

        tr = transform;
        collisionNormals = new List<Vector3>();

        LockCursor();
        SetupCameraFov();

        float timeToJumpApex = maxJumpTime / 2;
        gravity = (-2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        initialJumpVel = (2 * jumpHeight) * timeToJumpApex;

        print($"gravity: {gravity}, jumpVel: {initialJumpVel}");
    }

    private void Start()
    {
        falling = true;
        jumpFallCoroutine = StartCoroutine(JumpFallCoroutine());
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void SetupCameraFov()
    {
        minFov = playerCamera.fieldOfView;
        maxFov = minFov + 8;
    }
    #endregion

    private void Update()
    {
        var horMoveInput = inputData.HorizontalMovementInput;

        worldPlayerDiretion = tr.TransformDirection(new Vector3(horMoveInput.x, 0, horMoveInput.y));

        HandleSprinting();
        UpdatePlayerRotation();
        CheckCollisions();

        if (grounded)
        {
            if (groundNormal != Vector3.up)
            {
                var projected = Vector3.ProjectOnPlane(worldPlayerDiretion, groundNormal);
                worldPlayerDiretion = new Vector3(worldPlayerDiretion.x, projected.y, worldPlayerDiretion.z);
            }
        }
        else
        {
            if (!stopGravity)
                yVelocity += gravity * Time.deltaTime;
        }

        // stopGravity = yVelocity / Time.deltaTime < -10;

        CalculateHorizontalMovement();
        CheckForJump();
        ApplyHorizontalCollision();

        velocity = new Vector3(xzVelocity.x, yVelocity, xzVelocity.y);

        controller.Move(velocity * Time.deltaTime);
    }

    private void CheckForJump()
    {
        if (grounded && !jumping && !colUp && inputData.IsPressingJump)
        {
            if (sprinting)
                sprintJump = true;

            if (jumpFallCoroutine != null)
                StopCoroutine(jumpFallCoroutine);

            grounded = false;
            jumping = true;
            jumpFallCoroutine = StartCoroutine(JumpFallCoroutine());
        }
    }

    private void CheckCollisions()
    {
        if (!jumping)
            CheckGroundCollision();
        
        if (yVelocity > 0)
            CheckCeilingCollision();

        if (velocity != Vector3.zero)
            CheckHorizontalCollision();
    }

    private void CheckGroundCollision()
    {
        Vector3 origin = tr.position + (Vector3.up * controller.radius);
        Vector3 direction = Vector3.down;

        if (Physics.SphereCast(origin, controller.radius, direction, out RaycastHit hitInfo, collisionDetectionRayLenght, collisionMask))
        {
            groundNormal = hitInfo.normal;
            
            grounded = true;

            if (groundNormal == Vector3.up)
                yVelocity = -.01f;

            if (falling)
            {
                falling = false;

                if (jumpFallCoroutine != null)
                    StopCoroutine(jumpFallCoroutine);
            }
            return;
        }

        grounded = false;

        // Start falling coroutine if not already startred
        if (!falling)
        {
            falling = true;
            jumpFallCoroutine = StartCoroutine(JumpFallCoroutine());
        }
    }

    private IEnumerator JumpFallCoroutine()
    {
        float endTime = (maxJumpTime * (falling ? .5f : 1)) + Time.time;

        if (jumping)
            yVelocity = initialJumpVel * 10;

        while (Time.time < endTime)
        {
            if (yVelocity < 0)
            {
                jumping = false;
                sprintJump = false;
                falling = true;
            }

            yield return null;
        }
    }

    private void CheckCeilingCollision()
    {
        Vector3 origin = tr.position + ((controller.skinWidth + controller.height - controller.radius) * Vector3.up);

        if (Physics.SphereCast(origin, controller.radius, Vector3.up, out RaycastHit hitInfo, collisionDetectionRayLenght, collisionMask))
        {
            colUp = true;
            yVelocity = Mathf.Min(yVelocity, 0);

            if (jumping)
            {
                jumping = false;
                sprintJump = false;
                falling = true;

                if (jumpFallCoroutine == null)
                    jumpFallCoroutine = StartCoroutine(JumpFallCoroutine());
            }
            return;
        }

        colUp = false;
    }

    private void CheckHorizontalCollision()
    {
        float skinWidth = controller.skinWidth;
        float radius = controller.radius;

        Vector3 direction = new Vector3(xzVelocity.x, 0, xzVelocity.y).normalized;
        Vector3 p0 = tr.position + ((skinWidth + radius) * Vector3.up);
        Vector3 p1 = tr.position + ((controller.height + skinWidth - radius) * Vector3.up);

        var hits = Physics.CapsuleCastAll(p0, p1, controller.radius, direction, collisionDetectionRayLenght, collisionMask);

        Debug.DrawRay(p0, direction, Color.blue);
        Debug.DrawRay(p1, direction, Color.red);

        if (hits.Length == 0)
        {
            collisionNormals.Clear();
            return;
        }

        if (hits.Any(h => h.distance == 0))
            return;

        collisionNormals.Clear();
        foreach (var hit in hits)
            collisionNormals.Add(hit.normal);
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
                var projectedVel = Vector3.ProjectOnPlane(new Vector3(xzVelocity.x, 0, xzVelocity.y), normal);
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
            /*float normalsDot = Vector2.Dot(new Vector2(collisionNormals[0].x, collisionNormals[0].z), new Vector2(collisionNormals[1].x, collisionNormals[1].z));
            if (normalsDot < 0)
            {
                var opposite = (collisionNormals[0] + collisionNormals[1]) * .004f;
                xzVelocity = new Vector2(opposite.x, opposite.z);
            }*/
        }
    }

    private void CalculateHorizontalMovement()
    {
        Vector2 worldPlayerDirVec2 = new Vector2(worldPlayerDiretion.x, worldPlayerDiretion.z);

        float speed;

        if (grounded)
            speed = sprinting ? sprintMoveSpeed : walkMoveSpeed;
        else
            speed = sprintJump ? Mathf.Lerp(walkMoveSpeed, sprintMoveSpeed, .82f) : walkMoveSpeed;

        /*if (onSteepSlope)
            worldPlayerDirVec2 = Vector2.Lerp(worldPlayerDirVec2, new Vector2(groundNormal.x, groundNormal.z), 1);*/

        xzVelocity = Vector2.Lerp(
            xzVelocity,
            speed * worldPlayerDirVec2,
            (grounded ? horizontalStiffness : airHorizontalStiffness) * Time.deltaTime);
    }

    private void UpdatePlayerRotation()
    {
        var mouseDelta = inputData.MouseDeltaInput;

        cameraXRotation -= mouseDelta.y * mouseLookSensitivity;
        cameraXRotation = Mathf.Clamp(cameraXRotation, -89, 89);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraXRotation, 0, 0);

        playerYRotation += mouseDelta.x * mouseLookSensitivity;
        tr.rotation = Quaternion.Euler(0, playerYRotation, 0);
    }

    private void HandleSprinting()
    {
        var wasSprinting = sprinting;
        sprinting = inputData.IsPressingSprint && Vector2.Dot(Vector2.up, inputData.HorizontalMovementInput) > .38f;

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
        float startFov = playerCamera.fieldOfView;
        float endFov = onOff ? maxFov : minFov;

        while (Time.time < endTime)
        {
            t = sprintFovChangeCurve.Evaluate(Mathf.InverseLerp(startTime, endTime, Time.time));
            playerCamera.fieldOfView = Mathf.Lerp(startFov, endFov, t);
            yield return null;
        }
        playerCamera.fieldOfView = endFov;
    }
    #endregion
}
