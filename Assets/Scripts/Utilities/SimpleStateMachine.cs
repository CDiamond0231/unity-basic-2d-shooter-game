//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Simple State Machine
//             Author: Christopher A
//             Date Created: 24th June, 2025
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//      Simple State Machine with Entry, Update, and Exit functionality
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
#nullable enable

namespace BasicUnity2DShooter.Utilities
{
    /// <summary> Embeded member function state machine </summary> 
    public delegate void SimpleEnterState(SimpleStateMachine _stateMachine);

    /// <summary> Embeded member function state machine </summary> 
    public delegate void SimpleUpdateState(SimpleStateMachine _stateMachine);

    /// <summary> Embeded member function state machine </summary> 
    public delegate void SimpleExitState(SimpleStateMachine _stateMachine);


    public class SimpleState
    {
        public readonly SimpleEnterState? Enter;
        public readonly SimpleUpdateState? Update;
        public readonly SimpleExitState? Exit;

        public SimpleState(SimpleEnterState? _enter = null, SimpleUpdateState? _update = null, SimpleExitState? _exit = null)
        {
            this.Enter = _enter;
            this.Update = _update;
            this.Exit = _exit;
        }
    }


    public class SimpleStateMachine
    {
        public SimpleState? CurrentState { get; private set; }
        public SimpleState? PreviousState { get; private set; }
        public float TimeSpentInCurrentState { get; private set; }

        ///<summary> Constructor with an initial state set on construction </summary> 
        public SimpleStateMachine(SimpleState _initialState)
        {
            ChangeState(_initialState);
        }

        protected SimpleStateMachine()
        {
        }

        /// <summary> Calling Update on the StateMachine is essentially for calling the `update` function for the curren state.
        /// It is your responsibility to call Update from your host class. </summary>
        public void Update()
        {
            TimeSpentInCurrentState += UnityEngine.Time.deltaTime;

            if (CurrentState != null && CurrentState.Update != null)
                CurrentState.Update(this);
        }

        /// <summary> Simply changes to the next desired state.
        /// If next state is self, must set `can transition` value to true. This will alow it to go through exit & enter states again. </summary>
        public void ChangeState(SimpleState _state, bool _canTransitionToSelf = true)
        {
            if (CanTransitionToState(_state, _canTransitionToSelf))
            {
                PreviousState = CurrentState;
                CurrentState = _state;
                TimeSpentInCurrentState = 0.0f;

                if (PreviousState != null && PreviousState.Exit != null)
                    PreviousState.Exit(this);

                if (CurrentState != null && CurrentState.Enter != null)
                    CurrentState.Enter(this);
            }
        }

        protected bool CanTransitionToState(SimpleState _state, bool _canTransitionToSelf = true)
        {
            return CurrentState != _state || _canTransitionToSelf;
        }
    }
}