//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Scene Transition Effect
//             Author: Christopher A
//             Date Created: 24th June, 2025
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//        Handles the Scene Transition visuals for the game. Basically just
//      a screen mask that fades out and then back in.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
#nullable enable


using BasicUnity2DShooter.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BasicUnity2DShooter
{
    public class SceneTransitionEffect : MonoBehaviour
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Definitions
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public enum TransitionState
        {
            Idle,
            FadeOut,
            Hang,
            FadeIn,
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Consts
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private static readonly int TransitionTextureID = Shader.PropertyToID("_TransitionTexture");
        private static readonly int CutoffID = Shader.PropertyToID("_Cutoff");

        private const float MinCutoffValue = 0f;
        private const float MaxCutoffValue = 1f;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [SerializeField] private float m_defaultFadeDuration = 0.5f;
        [SerializeField] private Texture2D[] m_transitionEffectImages = new Texture2D[0];

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Non-Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private float m_fadeOutTime = 1.0f;
        private float m_hangDuration = 0.25f;
        private float m_fadeInTime = 1.0f;

        private Image[]? images = null;
        private EnumDirectedStateMachine<TransitionState>? m_localStateMachine = null;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Properties
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private System.Action? OnFadeOutCompleted { get; set; } = null;
        private System.Action? OnFadeInCompleted { get; set; } = null;

        public TransitionState CurrentTransitionState
        {
            get
            {
                if (m_localStateMachine == null)
                    return TransitionState.Idle;
                return m_localStateMachine.CurrentEnumState;
            }
            private set
            {
                if (m_localStateMachine == null)
                    BuildStateMachine(value);
                else
                    m_localStateMachine.ChangeState(value);
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Unity Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        protected void Update()
        {
            m_localStateMachine?.Update();
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary> Begins a Scene Transition Wipe Effect. </summary>
        /// <param name="_transitionImg"> The Transition Image you wish to show. </param>
        /// <param name="_fadeOutTime"> The length of time (seconds) it will take for the screen to fully fade out. </param>
        /// <param name="_hangDuration"> The length of time (seconds) it will remain in the faded out state before transitioning out (fade in). </param>
        /// <param name="_fadeInTime"> The length of time (seconds) it will take to fade back in. If null, will reuse the FadeOutTime. </param>
        /// <param name="_onFadeOutCompleted"> Callback when the Fadeout has fully occurred. Use this callback to set up your Scene. </param>
        /// <param name="_onFadeInCompleted"> Callback when the Fade in has completed and the transition effect has ended. Use this to enable player input for your scene. </param>
        public void BeginTransition(Texture2D _transitionImg, float? _fadeOutTime, float _hangDuration = 0.25f, float? _fadeInTime = null, System.Action? _onFadeOutCompleted = null, System.Action? _onFadeInCompleted = null)
        {
            m_fadeOutTime = _fadeOutTime ?? m_defaultFadeDuration;
            m_hangDuration = _hangDuration;
            m_fadeInTime = _fadeInTime ?? m_fadeOutTime;
            OnFadeOutCompleted = _onFadeOutCompleted;
            OnFadeInCompleted = _onFadeInCompleted;


            if (CurrentTransitionState == TransitionState.Idle || CurrentTransitionState == TransitionState.FadeIn)
            {
                images ??= GetComponentsInChildren<Image>(true);

                foreach (var image in images)
                {
                    image.material = Instantiate(image.material);
                    image.material.SetTexture(TransitionTextureID, _transitionImg);
                }

                CurrentTransitionState = TransitionState.FadeOut;
            }
            else
            {
                _onFadeOutCompleted?.Invoke();
            }
        }

        /// <summary> Begins a Random Scene Transition Wipe Effect. </summary>
        /// <param name="_fadeOutTime"> The length of time (seconds) it will take for the screen to fully fade out. </param>
        /// <param name="_hangDuration"> The length of time (seconds) it will remain in the faded out state before transitioning out (fade in). </param>
        /// <param name="_fadeInTime"> The length of time (seconds) it will take to fade back in. If null, will reuse the FadeOutTime. </param>
        /// <param name="_onFadeOutCompleted"> Callback when the Fadeout has fully occurred. Use this callback to set up your Scene. </param>
        /// <param name="_onFadeInCompleted"> Callback when the Fade in has completed and the transition effect has ended. Use this to enable player input for your scene. </param>
        public void ShowRandomTransition(float? _fadeOutTime, float _hangDuration = 0.25f, float? _fadeInTime = null, System.Action? _onFadeOutCompleted = null, System.Action? _onFadeInCompleted = null)
        {
            int r = UnityEngine.Random.Range(0, m_transitionEffectImages.Length);
            Texture2D transitionEffect = m_transitionEffectImages[r];
            BeginTransition(transitionEffect, _fadeOutTime, _hangDuration, _fadeInTime, _onFadeOutCompleted, _onFadeInCompleted);
        }

        private void BuildStateMachine(TransitionState _initialState)
        {
            Dictionary<TransitionState, SimpleState> states = new Dictionary<TransitionState, SimpleState>
            {
                [TransitionState.Idle]    = new SimpleState(null, null, null),
                [TransitionState.FadeOut] = new SimpleState(FadeOutState_OnEnter, FadeOutState_Update, FadeOutState_OnExit),
                [TransitionState.Hang]    = new SimpleState(HangState_OnEnter, HangState_Update, HangState_OnExit),
                [TransitionState.FadeIn]  = new SimpleState(FadeInState_OnEnter, FadeInState_Update, FadeInState_OnExit)
            };

            m_localStateMachine = new EnumDirectedStateMachine<TransitionState>(states);
            m_localStateMachine.ChangeState(_initialState);
        }

        private void OnDisable()
        {
            CurrentTransitionState = TransitionState.Idle;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          States
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private void FadeOutState_OnEnter(SimpleStateMachine _stateMachine)
        {
        }

        private void FadeOutState_Update(SimpleStateMachine _stateMachine)
        {
            if (_stateMachine.TimeSpentInCurrentState >= m_fadeOutTime)
            {
                CurrentTransitionState = TransitionState.Hang;
            }
            else
            {
                float progress = _stateMachine.TimeSpentInCurrentState / m_fadeOutTime;
                float f = Mathf.Lerp(MinCutoffValue, MaxCutoffValue, progress);
                foreach (var image in images!)
                    image.material.SetFloat(CutoffID, f);
            }
        }

        private void FadeOutState_OnExit(SimpleStateMachine _stateMachine)
        {
            foreach (var image in images!)
            {
                image.material.SetFloat(CutoffID, MaxCutoffValue);
            }

            OnFadeOutCompleted?.Invoke();
        }



        private void HangState_OnEnter(SimpleStateMachine _stateMachine)
        {
        }

        private void HangState_Update(SimpleStateMachine _stateMachine)
        {
            if (_stateMachine.TimeSpentInCurrentState >= m_hangDuration)
            {
                CurrentTransitionState = TransitionState.FadeIn;
            }
        }

        private void HangState_OnExit(SimpleStateMachine _stateMachine)
        {
        }



        private void FadeInState_OnEnter(SimpleStateMachine _stateMachine)
        {
        }

        private void FadeInState_Update(SimpleStateMachine _stateMachine)
        {
            if (_stateMachine.TimeSpentInCurrentState >= m_fadeInTime)
            {
                CurrentTransitionState = TransitionState.Idle;
            }
            else
            {
                float progress = _stateMachine.TimeSpentInCurrentState / m_fadeInTime;
                float f = Mathf.Lerp(MaxCutoffValue, MinCutoffValue, progress);
                foreach (var image in images!)
                {
                    image.material.SetFloat(CutoffID, f);
                }
            }
        }

        private void FadeInState_OnExit(SimpleStateMachine _stateMachine)
        {
            foreach (var image in images!)
            {
                image.material.SetFloat(CutoffID, MinCutoffValue);
            }

            OnFadeInCompleted?.Invoke();
        }
    }
}