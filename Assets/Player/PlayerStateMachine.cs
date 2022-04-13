using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine
{
    public PlayerState CurrentState { get; private set; }

    public void Init(PlayerState initState)
    {
        CurrentState = initState;
        CurrentState.OnEnter(initState);
    }

    public void ChangeState(PlayerState newState)
    {
        CurrentState.OnExit(newState);
        var previousState = CurrentState;

        CurrentState = newState;
        CurrentState.OnEnter(previousState);

        /*
        Debug.Log("- - - CHANGING STATE - - -");
        Debug.Log($"Previous Type: {previousState}");
        Debug.Log($"Next Type: {newState}");
        Debug.Log("");
        */
    }
}
