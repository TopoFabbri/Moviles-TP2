using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tools.FSM
{
    /// <summary>
    /// A generic Finite State Machine that allows for creating and managing states and transitions.
    /// </summary>
    /// <typeparam name="TStateType">An enum representing the different states of the FSM.</typeparam>
    /// <typeparam name="TFlagType">An enum representing the flags that can trigger transitions.</typeparam>
    internal class FSM<TStateType, TFlagType> where TStateType : Enum where TFlagType : Enum
    {
        private const int UnassignedTransition = -1;
        private readonly Dictionary<int, Func<object[]>> behaviourOnEnterParameters;
        private readonly Dictionary<int, Func<object[]>> behaviourOnExitParameters;
        private readonly Dictionary<int, Func<object[]>> behaviourOnTickParameters;
        
        private readonly ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 32 };
        private readonly Dictionary<int, State> states;

        private readonly (int destinationState, Action onTrnasition)[,] transitions;
        public int CurrentState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the FSM class with a default state.
        /// </summary>
        /// <param name="defaultState">The initial state of the FSM.</param>
        public FSM(TStateType defaultState)
        {
            states = new Dictionary<int, State>();
            transitions = new (int, Action)[Enum.GetValues(typeof(TStateType)).Length, Enum.GetValues(typeof(TFlagType)).Length];
            for (int i = 0; i < transitions.GetLength(0); i++)
            for (int j = 0; j < transitions.GetLength(1); j++)
                transitions[i, j] = (UnassignedTransition, null);

            behaviourOnTickParameters = new Dictionary<int, Func<object[]>>();
            behaviourOnEnterParameters = new Dictionary<int, Func<object[]>>();
            behaviourOnExitParameters = new Dictionary<int, Func<object[]>>();
            ForceState(defaultState);
        }

        /// <summary>
        /// Gets the behaviour actions to be executed on each tick for the current state.
        /// </summary>
        private BehaviourActions GetCurrentTickBehaviour => states[CurrentState].GetOnTickBehaviours(behaviourOnTickParameters[CurrentState]?.Invoke());

        /// <summary>
        /// Gets the behaviour actions to be executed upon entering the current state.
        /// </summary>
        private BehaviourActions GetCurrentOnEnterBehaviour => states[CurrentState].GetOnEnterBehaviours(behaviourOnEnterParameters[CurrentState]?.Invoke());

        /// <summary>
        /// Gets the behaviour actions to be executed upon exiting the current state.
        /// </summary>
        private BehaviourActions GetCurrentOnExitBehaviour => states[CurrentState].GetOnExitBehaviour(behaviourOnExitParameters[CurrentState]?.Invoke());

        /// <summary>
        /// Adds a new state to the FSM.
        /// </summary>
        /// <typeparam name="TState">The type of the state to add, must inherit from State.</typeparam>
        /// <param name="stateIndex">The enum value representing this state.</param>
        /// <param name="onTickParameters">onTickParameters: () => new object[] { }</param>
        /// <param name="onEnterParameters">onEnterParameters: () => new object[] { }.</param>
        /// <param name="onExitParameters">onExitParameters: () => new object[] { }</param>
        public void AddState<TState>(TStateType stateIndex, Func<object[]> onTickParameters = null, Func<object[]> onEnterParameters = null, Func<object[]> onExitParameters = null)
            where TState : State, new()
        {
            if (states.ContainsKey(Convert.ToInt32(stateIndex))) return;
            TState state = new();

            ValidateParams(state.OnEnterParamTypes, onEnterParameters);
            ValidateParams(state.OnTickParamTypes, onTickParameters);
            ValidateParams(state.OnExitParamTypes, onExitParameters);

            state.flag += Transition;
            states.Add(Convert.ToInt32(stateIndex), state);
            behaviourOnTickParameters.Add(Convert.ToInt32(stateIndex), onTickParameters);
            behaviourOnEnterParameters.Add(Convert.ToInt32(stateIndex), onEnterParameters);
            behaviourOnExitParameters.Add(Convert.ToInt32(stateIndex), onExitParameters);
        }

        /// <summary>
        /// Forces the FSM to a specific state without executing exit or enter behaviours.
        /// </summary>
        /// <param name="state">The state to force the FSM into.</param>
        private void ForceState(TStateType state)
        {
            CurrentState = Convert.ToInt32(state);
        }

        /// <summary>
        /// Defines a transition between two states.
        /// </summary>
        /// <param name="originalState">The state from which the transition originates.</param>
        /// <param name="flag">The flag that triggers the transition.</param>
        /// <param name="destinationState">The state to which the FSM will transition.</param>
        /// <param name="onTransition">An optional action to execute when the transition occurs.</param>
        public void SetTransition(TStateType originalState, TFlagType flag, TStateType destinationState, Action onTransition = null)
        {
            transitions[Convert.ToInt32(originalState), Convert.ToInt32(flag)] = (Convert.ToInt32(destinationState), onTransition);
        }

        /// <summary>
        /// Triggers a transition based on a given flag.
        /// </summary>
        /// <param name="flag">The flag to trigger a transition.</param>
        public void Transition(Enum flag)
        {
            if (states.ContainsKey(CurrentState)) ExecuteBehaviour(GetCurrentOnExitBehaviour);

            if (transitions[Convert.ToInt32(CurrentState), Convert.ToInt32(flag)].destinationState == UnassignedTransition) return;

            transitions[CurrentState, Convert.ToInt32(flag)].onTrnasition?.Invoke();
            CurrentState = transitions[Convert.ToInt32(CurrentState), Convert.ToInt32(flag)].destinationState;
            ExecuteBehaviour(GetCurrentOnEnterBehaviour);
        }

        /// <summary>
        /// Executes the OnTick behaviours for the current state.
        /// </summary>
        public void Tick()
        {
            if (states.ContainsKey(CurrentState)) ExecuteBehaviour(GetCurrentTickBehaviour);
        }

        /// <summary>
        /// Executes a set of behaviour actions, handling both main-thread and multi-threaded actions.
        /// </summary>
        /// <param name="behaviourActions">The behaviour actions to execute.</param>
        private void ExecuteBehaviour(BehaviourActions behaviourActions)
        {
            if (behaviourActions == null)
                return;

            int executionOrder = 0;

            while ((behaviourActions.MainThreadBehaviours != null && behaviourActions.MainThreadBehaviours.Count > 0) ||
                   (behaviourActions.MultiThreadableBehaviours != null && behaviourActions.MultiThreadableBehaviours.Count > 0))
            {
                Task multithradableBehaviour = new(() =>
                {
                    if (behaviourActions.MultiThreadableBehaviours == null) return;
                    if (!behaviourActions.MultiThreadableBehaviours.TryGetValue(executionOrder, out ConcurrentBag<Action> currentBehaviour)) return;

                    Parallel.ForEach(currentBehaviour, parallelOptions, behaviour => { behaviour?.Invoke(); });
                    behaviourActions.MultiThreadableBehaviours.TryRemove(executionOrder, out _);
                });

                multithradableBehaviour.Start();

                if (behaviourActions.MainThreadBehaviours != null)
                    if (behaviourActions.MainThreadBehaviours.ContainsKey(executionOrder))
                    {
                        foreach (Action behaviour in behaviourActions.MainThreadBehaviours[executionOrder]) behaviour?.Invoke();
                        behaviourActions.MainThreadBehaviours.Remove(executionOrder);
                    }

                multithradableBehaviour?.Wait();
                executionOrder++;
            }

            behaviourActions.TransitionBehaviour?.Invoke();
            State.Pool.Release(behaviourActions);
        }

        /// <summary>
        /// Validates that the types of the provided parameters match the expected types.
        /// </summary>
        /// <param name="expectedParams">An array of expected parameter types.</param>
        /// <param name="receivedParams">A function that returns an array of the received parameters.</param>
        /// <exception cref="ArgumentException">Thrown when the number of parameters is incorrect.</exception>
        /// <exception cref="InvalidCastException">Thrown when a parameter has an incorrect type.</exception>
        private void ValidateParams(Type[] expectedParams, Func<object[]> receivedParams)
        {
            if (expectedParams.Length == 0 && receivedParams == null) return;

            List<Type> receivedParamTypes = new();

            foreach (object param in receivedParams.Invoke())
                receivedParamTypes.Add(param.GetType());

            if (expectedParams.Length != receivedParamTypes.Count)
                throw new ArgumentException("Wrong number of parameters");

            for (int i = 0; i < expectedParams.Length; i++)
                if (!expectedParams[i].IsAssignableFrom(receivedParamTypes[i]))
                    throw new InvalidCastException("Param " + i + ". Type: " + receivedParamTypes[i].Name + ". Expected type: " + expectedParams[i].Name);
        }
    }
}
