//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Enum Directed State Machine
//             Author: Christopher A
//             Date Created: 24th June, 2025
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//      State Machine that is directly controlled via an enum.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace BasicUnity2DShooter.Utilities
{
    public class EnumDirectedStateMachine<E> : SimpleStateMachine where E : System.Enum
    {
        public E CurrentEnumState { get; private set; }

        private System.Collections.Generic.Dictionary<E, SimpleState> enumValToState = null;

        public EnumDirectedStateMachine(E _initialState, System.Collections.Generic.Dictionary<E, SimpleState> _enumValToState) : base()
        {
            enumValToState = _enumValToState;
            CurrentEnumState = _initialState;
            ChangeState(_initialState, true);
        }

        public EnumDirectedStateMachine(System.Collections.Generic.Dictionary<E, SimpleState> _enumValToState) : base()
        {
            enumValToState = _enumValToState;
        }

        public void ChangeState(E _newState, bool _canTransitionToSelf = false)
        {
            if (CurrentEnumState.Equals(_newState) && _canTransitionToSelf == false)
                return;

            SimpleState nextState;
            if (enumValToState.TryGetValue(_newState, out nextState) == false)
            {
                UnityEngine.Debug.LogError($"Tried to switch to state [{_newState}]. But this state is undefined. Remaining in state [{CurrentEnumState}].");
                return;
            }

            CurrentEnumState = _newState;
            ChangeState(nextState, _canTransitionToSelf);
        }
    }
}