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
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Non-Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private readonly System.Collections.Generic.Dictionary<E, SimpleState> _enumValToState = null!;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Properties
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public E CurrentEnumState { get; private set; }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public EnumDirectedStateMachine(E _initialState, System.Collections.Generic.Dictionary<E, SimpleState> _enumValToState) : base()
        {
            this._enumValToState = _enumValToState;
            CurrentEnumState = _initialState;
            ChangeState(_initialState, true);
        }

        public EnumDirectedStateMachine(System.Collections.Generic.Dictionary<E, SimpleState> _enumValToState) : base()
        {
            this._enumValToState = _enumValToState;
        }

        /// <summary> Simply changes to the next desired state.
        /// If next state is self, must set `can transition` value to true. This will alow it to go through exit & enter states again. </summary>
        public void ChangeState(E _newState, bool _canTransitionToSelf = false)
        {
            if (CurrentEnumState.Equals(_newState) && _canTransitionToSelf == false)
                return;

            if (_enumValToState.TryGetValue(_newState, out SimpleState nextState) == false)
            {
                UnityEngine.Debug.LogError($"Tried to switch to state [{_newState}]. But this state is undefined. Remaining in state [{CurrentEnumState}].");
                return;
            }

            CurrentEnumState = _newState;
            ChangeState(nextState, _canTransitionToSelf);
        }
    }
}