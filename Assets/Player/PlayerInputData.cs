using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputData : MonoBehaviour
{
    public Vector2 HorizontalMovementInput { get; private set; }
    public Vector2 MouseDeltaInput { get; private set; }
    public bool IsPressingJump { get; private set; }
    public bool IsPressingSprint { get; private set; }
    public bool IsPressingCrouch { get; private set; }

    public void UpdateHorizontalMovementInput(InputAction.CallbackContext context)
    {
        HorizontalMovementInput = context.ReadValue<Vector2>();
    }

    public void UpdateMouseDeltaInput(InputAction.CallbackContext context)
    {
        MouseDeltaInput = context.ReadValue<Vector2>();
    }

    public void UpdateJumpInput(InputAction.CallbackContext context)
    {
        if (context.started)
            IsPressingJump = true;

        else if (context.canceled)
            IsPressingJump = false;
    }

    public void UpdateSprintInput(InputAction.CallbackContext context)
    {
        if (context.started)
            IsPressingSprint = true;

        else if (context.canceled)
            IsPressingSprint = false;
    }

    public void UpdateCrouchInput(InputAction.CallbackContext context)
    {
        if (context.started)
            IsPressingCrouch = true;

        else if (context.canceled)
            IsPressingCrouch = false;
    }
}
