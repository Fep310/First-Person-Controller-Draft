using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform Transform { get; private set; }

    [SerializeField] private new Camera camera;
    [SerializeField] private Transform yRotTf;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private CameraFovController fovController;
    [SerializeField] private CameraAnimations cameraAnimations;
    [SerializeField] private PlayerCrouchAnimator playerCrouchAnimator;
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerConstantMovementValues constMovementValues;
    [SerializeField] private PlayerInputData inputData;
    [SerializeField] private SwordBehaviour swordBehaviour;
    public PlayerDebugUI debug;

    public Camera Camera
    { get => camera; private set => camera = value; }

    public Transform CameraHolder
    { get => cameraHolder; private set => cameraHolder = value; }

    public CameraFovController FovController
    { get => fovController; private set => fovController = value; }

    public CameraAnimations CameraAnimations
    { get => cameraAnimations; private set => cameraAnimations = value; }

    public PlayerCrouchAnimator CrouchAnimator
    { get => playerCrouchAnimator; private set => playerCrouchAnimator = value; }

    public CharacterController Controller
    { get => controller; private set => controller = value; }

    public PlayerConstantMovementValues ConstMovementValues
    { get => constMovementValues; private set => constMovementValues = value; }

    public PlayerInputData InputData
    { get => inputData; private set => inputData = value; }

    public PlayerMovementData MovementData { get; private set; }
    public PlayerStates States { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }

    private bool canStart;

    private void Awake()
    {
        if (!IsReady())
        {
            gameObject.SetActive(false);
            canStart = false;
            return;
        }

        canStart = true;

        Transform = transform;

        MovementData = new PlayerMovementData(
            ConstMovementValues.JumpHeight, ConstMovementValues.JumpTime, 
            ConstMovementValues.SlideJumpHeight, ConstMovementValues.SlideJumpTime);
        StateMachine = new PlayerStateMachine();
        States = new PlayerStates(this, Controller, StateMachine, ConstMovementValues, MovementData, InputData);

        Controller.enableOverlapRecovery = true;

        Application.targetFrameRate = 144;
    }

    private bool IsReady()
    {
        if (camera == null)
        {
            Debug.LogError("Player's \"camera\" is null.");
            return false;
        }

        if (fovController == null)
        {
            Debug.LogError("Player's \"fovController\" is null.");
            return false;
        }

        if (controller == null)
        {
            Debug.LogError("Player's \"controller\" is null.");
            return false;
        }

        if (constMovementValues == null)
        {
            Debug.LogError("Player's \"constMovementValues\" is null.");
            return false;
        }

        if (inputData == null)
        {
            Debug.LogError("Player's \"inputData\" is null.");
            return false;
        }

        return true;
    }

    private void Start()
    {
        if (canStart)
        {
            LockCursor();
            StateMachine.Init(States.Idle);
        }
    }

    private void Update()
    {
        HandleRotation();
        CalculateWorldPlayerDir();

        StateMachine.CurrentState.OnUpdate();
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void HandleRotation()
    {
        var mouseDelta = InputData.MouseDeltaInput;
        var mouseSens = constMovementValues.MouseLookSensitivity * (swordBehaviour.Charging ? swordBehaviour.MouseSensetivityMultiplier : 1);

        MovementData.cameraXRotation -= mouseDelta.y * mouseSens;
        MovementData.cameraXRotation = Mathf.Clamp(MovementData.cameraXRotation, -89, 89);
        Camera.transform.localRotation = Quaternion.Euler(MovementData.cameraXRotation, 0, 0);

        MovementData.playerYRotation += mouseDelta.x * mouseSens;
        yRotTf.rotation = Quaternion.Euler(0, MovementData.playerYRotation, 0);
    }

    private void CalculateWorldPlayerDir()
    {
        var horMovInput = InputData.HorizontalMovementInput;
        MovementData.worldPlayerDiretion = yRotTf.TransformDirection(new Vector3(horMovInput.x, 0, horMovInput.y));
    }
}
