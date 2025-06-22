using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingSystem.Core.StateMachine
{
    public class StateMachine<TStateType> where TStateType : Enum
    {
        private IState currentState;
        private Dictionary<TStateType, IState> states = new Dictionary<TStateType, IState>();
        private TStateType currentStateType;

        public TStateType CurrentStateType => currentStateType;
        public IState CurrentState => currentState;

        public void RegisterState(TStateType stateType, IState state)
        {
            if (states.ContainsKey(stateType))
            {
                Debug.LogWarning($"State {stateType} already registered. Overwriting.");
            }
            states[stateType] = state;
        }

        public void ChangeState(TStateType newStateType)
        {
            if (!states.ContainsKey(newStateType))
            {
                Debug.LogError($"State {newStateType} not registered!");
                return;
            }

            if (currentState != null)
            {
                currentState.Exit();
            }

            currentStateType = newStateType;
            currentState = states[newStateType];
            currentState.Enter();
        }

        public void Update()
        {
            currentState?.Update();
        }

        public void HandleInput()
        {
            currentState?.HandleInput();
        }
    }
}