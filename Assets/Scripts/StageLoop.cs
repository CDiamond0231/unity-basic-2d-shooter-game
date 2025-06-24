//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Stage Loop
//             Author: Christopher A
//             Date Created: 24th June, 2025
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//      Stage Loop. State Machine which handles the gameplay loop (stage loop)
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
#nullable enable
using BasicUnity2DShooter.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Stage main loop
/// </summary>
namespace BasicUnity2DShooter
{
	public class StageLoop : MonoBehaviour
	{
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Definitions
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private enum StageState
        {
            Paused,
            SettingUp,
            ShowingWaveInfo,
            PlayingGame,
        }

        [System.Serializable]
		private class EnemySetData
		{
			public EnemySpawner SpawnerToKickOff;
			public int NumEnemiesForSet = 10;
			public float TimeOfSetStart = 0.0f;
			public float SetDuration = 5.0f; // How long till enemies reach end of path.
		}

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Statics
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public static StageLoop Instance { get; private set; }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [SerializeField] private TitleLoop m_title_loop;
        [SerializeField] private SceneTransitionEffect m_sceneTransitionEffect;

        [Header("Layout")]
        [SerializeField] private Transform m_stage_transform;
        [SerializeField] private Text m_stageScoreText;
        [SerializeField] private Text m_waveInfoText;

        [Header("Prefab")]
        [SerializeField] private Player m_prefab_player;

		[Header("Waves")]
		[SerializeField] private EnemySetData[] m_wave1EnemySets = System.Array.Empty<EnemySetData>();
        [SerializeField] private EnemySetData[] m_wave2EnemySets = System.Array.Empty<EnemySetData>();
        [SerializeField] private EnemySetData[] m_wave3EnemySets = System.Array.Empty<EnemySetData>();

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Non-Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private int m_game_score = 0;
		private int m_currentWave = 0;
        private int m_numSetsSpawned = 0;
        private int m_numCompletedSets = 0;

        private EnumDirectedStateMachine<StageState>? m_localStateMachine = null;

        private EnemySetData[] CurrentWaveEnemySets
        {
            get
            {
                return  m_currentWave == 0 ? m_wave1EnemySets :
                        m_currentWave == 1 ? m_wave2EnemySets :
                                             m_wave3EnemySets;
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Properties
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private StageState CurrentState
        {
            get
            {
                if (m_localStateMachine == null)
                    return StageState.Paused;
                return m_localStateMachine.CurrentEnumState;
            }
            set
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
            if (m_localStateMachine != null)
            {
                m_localStateMachine.Update();
            }

            if (Input.GetKeyDown(KeyCode.Escape)
                    && m_sceneTransitionEffect.CurrentTransitionState == SceneTransitionEffect.TransitionState.Idle)
            {
                CurrentState = StageState.Paused;
                m_sceneTransitionEffect.ShowRandomTransition(0.5f, _onFadeOutCompleted: () =>
                {
                    //exit stage
                    CleanupStage();
                    m_title_loop.StartTitleLoop();
                });
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public void StartStageLoop()
		{
            Instance = this;

            m_game_score = 0;
            m_currentWave = -1;
            CurrentState = StageState.SettingUp;
        }

        private void BuildStateMachine(StageState _initialState)
        {
            Dictionary<StageState, SimpleState> states = new Dictionary<StageState, SimpleState>();
            states[StageState.SettingUp] = new SimpleState(SettingUpState_OnEnter, SettingUpState_Update, SettingUpState_OnExit);
            states[StageState.Paused] = new SimpleState(PausedState_OnEnter, PausedState_Update, PausedState_OnExit);
            states[StageState.ShowingWaveInfo] = new SimpleState(ShowingWaveInfoState_OnEnter, ShowingWaveInfoState_Update, ShowingWaveInfoState_OnExit);
            states[StageState.PlayingGame] = new SimpleState(PlayingGameState_OnEnter, PlayingGameState_Update, PlayingGameState_OnExit);

            m_localStateMachine = new EnumDirectedStateMachine<StageState>(states);
            m_localStateMachine.ChangeState(_initialState);
        }

        private void SettingUpState_OnEnter(SimpleStateMachine _stateMachine)
        {
            RefreshScore();

            //create player
            {
                Player player = Instantiate(m_prefab_player, m_stage_transform);
                if (player)
                {
                    player.transform.position = new Vector3(0, -4, 0);
                    player.StartRunning();
                }
            }
        }

        private void SettingUpState_Update(SimpleStateMachine _stateMachine)
        {
            // 1 second timer for the Transition effect to wear out
            if (_stateMachine.TimeSpentInCurrentState > 1f)
            {
                CurrentState = StageState.ShowingWaveInfo;
            }
        }

        private void SettingUpState_OnExit(SimpleStateMachine _stateMachine)
        {
        }

        private void PausedState_OnEnter(SimpleStateMachine _stateMachine)
        {
        }

        private void PausedState_Update(SimpleStateMachine _stateMachine)
        {
        }

        private void PausedState_OnExit(SimpleStateMachine _stateMachine)
        {
        }

        private void ShowingWaveInfoState_OnEnter(SimpleStateMachine _stateMachine)
        {
            ++m_currentWave;
            m_waveInfoText.text = $"Wave {m_currentWave + 1:00}";
        }

        private void ShowingWaveInfoState_Update(SimpleStateMachine _stateMachine)
        {
            if (_stateMachine.TimeSpentInCurrentState > 3f)
            {
                CurrentState = StageState.PlayingGame;
            }
        }

        private void ShowingWaveInfoState_OnExit(SimpleStateMachine _stateMachine)
        {
            m_waveInfoText.text = string.Empty;
        }

        private void PlayingGameState_OnEnter(SimpleStateMachine _stateMachine)
        {
            m_numSetsSpawned = 0;
            m_numCompletedSets = 0;
        }

        private void PlayingGameState_Update(SimpleStateMachine _stateMachine)
        {
            EnemySetData[] waveEnemySets = CurrentWaveEnemySets;
            if (m_numSetsSpawned >= waveEnemySets.Length)
            {
                // All Enemies spawned. Just waiting for them to either finish moving or perish
                return;
            }

            if (_stateMachine.TimeSpentInCurrentState > waveEnemySets[m_numSetsSpawned].TimeOfSetStart)
            {
                waveEnemySets[m_numSetsSpawned].SpawnerToKickOff.StartSpawningEnemies(
                    _numEnemies: waveEnemySets[m_numSetsSpawned].NumEnemiesForSet,
                    _travelPathDuration: waveEnemySets[m_numSetsSpawned].SetDuration,
                    _whenAllEnemiesStopped: () =>
                    {
                        ++m_numCompletedSets;
                        if (m_numCompletedSets >= waveEnemySets.Length)
                        {
                            // Moves on to the next Wave
                            CurrentState = StageState.ShowingWaveInfo;
                        }
                    }
                );

                ++m_numSetsSpawned;
            }
        }

        private void PlayingGameState_OnExit(SimpleStateMachine _stateMachine)
        {
        }
			

		void CleanupStage()
		{
			//delete all object in Stage
			{
				for (var n = 0; n < m_stage_transform.childCount; ++n)
				{
					Transform temp = m_stage_transform.GetChild(n);
					GameObject.Destroy(temp.gameObject);
				}
			}

			Instance = null;
		}

		//------------------------------------------------------------------------------

		public void AddScore(int a_value)
		{
			m_game_score += a_value;
			RefreshScore();
		}

		void RefreshScore()
		{
			if (m_stageScoreText)
			{
				m_stageScoreText.text = $"Score {m_game_score:00000}";
			}
		}

	}
}