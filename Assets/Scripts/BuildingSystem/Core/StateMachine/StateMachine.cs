using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingSystem.Core.StateMachine
{
    public class StateMachine<TStateType> where TStateType : Enum
    {
        private readonly Dictionary<TStateType, IState> states = new Dictionary<TStateType, IState>();
        private IState currentState;
        private TStateType currentStateType;

        public TStateType CurrentStateType => currentStateType;
        public IState CurrentState => currentState;

        public void RegisterState(TStateType stateType, IState state)
        {
            if (states.ContainsKey(stateType))
            {
                Debug.LogWarning($"State {stateType} is already registered!");
                return;
            }

            states[stateType] = state;
        }

        public void ChangeState(TStateType newStateType)
        {
            if (!states.TryGetValue(newStateType, out var newState))
            {
                Debug.LogError($"State {newStateType} is not registered!");
                return;
            }

            // Exit current state
            currentState?.Exit();

            // Change state
            currentStateType = newStateType;
            currentState = newState;

            // Enter new state
            currentState?.Enter();
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

    // IState.cs
    public interface IState
    {
        void Enter();
        void Exit();
        void Update();
        void HandleInput();
    }

    // BuildingSystemStateBase.cs
    public abstract class BuildingSystemStateBase : IState
    {
        protected readonly BuildingSystemContext context;

        protected BuildingSystemStateBase(BuildingSystemContext context)
        {
            this.context = context;
        }

        public abstract void Enter();
        public abstract void Exit();
        public abstract void Update();
        public abstract void HandleInput();
    }

    // BuildingSystemState.cs
    public enum BuildingSystemState
    {
        Idle,
        PlacingBuilding,
        ConstructingBuilding,
        DemolishingBuilding
    }
}