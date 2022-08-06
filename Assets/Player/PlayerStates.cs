using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStates
{
    #region ground-states

    public IdleState Idle { get; private set; }
    public WalkState Walk { get; private set; }
    public RunState Run { get; private set; }
    public ToSlideState ToSlide { get; private set; }
    public SlideState Slide { get; private set; }

    public SteepSlopeState SteepSlope { get; private set; }

    #region crouch-states

    public IdleCrouchState IdleCrouch { get; private set; }
        public WalkCrouchState WalkCrouch { get; private set; }

        #endregion

    #endregion

    #region air-states

    public AirborneState Airborne { get; private set; }

    #endregion

    public PlayerStates(Player player, CharacterController controller, PlayerStateMachine stateMachine,
        PlayerConstantMovementValues constValues, PlayerMovementData movementData, PlayerInputData inputData)
    {
        Idle = new IdleState(player, controller, this, stateMachine, constValues, movementData, inputData);
        Walk = new WalkState(player, controller, this, stateMachine, constValues, movementData, inputData);
        Run = new RunState(player, controller, this, stateMachine, constValues, movementData, inputData);
        ToSlide = new ToSlideState(player, controller, this, stateMachine, constValues, movementData, inputData);
        Slide = new SlideState(player, controller, this, stateMachine, constValues, movementData, inputData);
        SteepSlope = new SteepSlopeState(player, controller, this, stateMachine, constValues, movementData, inputData);

        IdleCrouch = new IdleCrouchState(player, controller, this, stateMachine, constValues, movementData, inputData);
        WalkCrouch = new WalkCrouchState(player, controller, this, stateMachine, constValues, movementData, inputData);

        Airborne = new AirborneState(player, controller, this, stateMachine, constValues, movementData, inputData);
    }
}
