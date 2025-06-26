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
using System.Collections.Generic;
using System.Linq;
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
            SettingUp,       // Triggered when game is entered from Title Screen.
            Paused,          // This state means we are leaving the game to return to title screen. So clear the enemies, turn off player, etc.
            ShowingWaveInfo, // Showing `Wave 01`, etc. for a few seconds.
            PlayingGame,     // Awaits all enemies to finish their paths (or die); or for the player to die.
            WinState,        // After completing all waves successfully, this state plays a cheer and tells the player they won
            LoseState,       // Occurs when player fails to kill required enemies, or is killed by an enemy.
        }

        [System.Serializable]
		private class EnemySetData
		{
			public EnemySpawner SpawnerToKickOff = null!;
			public int NumEnemiesForSet = 10;
			public float TimeOfSetStart = 0.0f;
			public float SetDuration = 5.0f; // How long till enemies reach end of path.
		}

        [System.Serializable]
        private class EnemyWaveData
        {
            public int RequiredEnemyKills = 10; // Need x kills to progress to the next stage.
            public EnemySetData[] EnemySets = System.Array.Empty<EnemySetData>();
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Statics
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public static StageLoop? Instance { get; private set; }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [SerializeField] private TitleLoop m_titleLoopRef = null!;
        [SerializeField] private SceneTransitionEffect m_sceneTransitionEffect = null!;
        [SerializeField] private Player m_player = null!;

        [Header("Layout")]
        [SerializeField] private GameObject m_stageUIHierarchy = null!;
        [SerializeField] private Text m_enemiesRemainingCountText = null!;
        [SerializeField] private Text m_killsCountText = null!;
        [SerializeField] private Text m_killsNeededCountText = null!;
        [SerializeField] private Text m_centerScreenText = null!;
        [SerializeField] private Text m_centerScreenSubtitleText = null!;

        [Header("Audio")]
        [SerializeField] private AudioClip[] m_newWaveAudioSFX = System.Array.Empty<AudioClip>();
        [SerializeField] private AudioClip[] m_loseAudioSFX = System.Array.Empty<AudioClip>();
        [SerializeField] private AudioClip m_winAudioSFX = null!;

        [Header("Enemy Waves - Defines Enemy Spawns")]
        [SerializeField] private EnemyWaveData[] m_enemyWavesData = System.Array.Empty<EnemyWaveData>();

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Non-Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		private int m_currentWaveIndex = 0;
        private int m_numEnemiesRemaining = 0;
        private int m_numKillsThisWave = 0;
        private int m_numSetsSpawned = 0;
        private int m_numCompletedSets = 0;

        private EnumDirectedStateMachine<StageState>? m_localStateMachine = null;

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
        protected void OnEnable()
        {
            m_stageUIHierarchy.SetActive(true);
        }

        protected void OnDisable()
        {
            m_stageUIHierarchy.SetActive(false);
        }

        protected void Update()
        {
            m_localStateMachine?.Update();

            // Goes back to the title screen when `Esc` is pressed
            if (Input.GetKeyDown(KeyCode.Escape)
                    && m_sceneTransitionEffect.CurrentTransitionState == SceneTransitionEffect.TransitionState.Idle)
            {
                CurrentState = StageState.Paused;
                m_sceneTransitionEffect.ShowRandomTransition(0.5f, _onFadeOutCompleted: () =>
                {
                    // Switching off Game Screen Controller, which then switches off UI via OnDiable
                    this.enabled = false;

                    Instance = null;
                    m_titleLoopRef.StartTitleLoop();
                });
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary> Invoked when entering the Game from the Title screen. </summary>
        public void StartStageLoop()
		{
            // Switching on Game Screen Controller, which then switches on UI via OnEnable
            Instance = this;
            this.enabled = true;

            m_numKillsThisWave = 0;
            m_currentWaveIndex = -1;
            CurrentState = StageState.SettingUp;
        }

        /// <summary> Invoked when an enemy is killed by player. </summary>
        public void AddKill()
        {
            ++m_numKillsThisWave;
            m_killsCountText.text = $"Kills: {m_numKillsThisWave:00}";
        }

        /// <summary> Invoked either when an enemy is killed by player or enemy finishes moving. </summary>
        public void OnEnemyRemoved()
        {
            m_numEnemiesRemaining = Mathf.Max(m_numEnemiesRemaining - 1, 0);
            m_enemiesRemainingCountText.text = $"Enemies Remaining: {m_numEnemiesRemaining:00}";
        }

        /// <summary> Builds the State Machine </summary>
        private void BuildStateMachine(StageState _initialState)
        {
            Dictionary<StageState, SimpleState> states = new Dictionary<StageState, SimpleState>
            {
                [StageState.SettingUp]       = new SimpleState(SettingUpState_OnEnter, SettingUpState_Update, SettingUpState_OnExit),
                [StageState.Paused]          = new SimpleState(PausedState_OnEnter, PausedState_Update, PausedState_OnExit),
                [StageState.ShowingWaveInfo] = new SimpleState(ShowingWaveInfoState_OnEnter, ShowingWaveInfoState_Update, ShowingWaveInfoState_OnExit),
                [StageState.PlayingGame]     = new SimpleState(PlayingGameState_OnEnter, PlayingGameState_Update, PlayingGameState_OnExit),
                [StageState.WinState]        = new SimpleState(WinGameState_OnEnter, WinGameState_Update, WinGameState_OnExit),
                [StageState.LoseState]       = new SimpleState(LoseState_OnEnter, LoseGameState_Update, LoseGameState_OnExit)
            };

            m_localStateMachine = new EnumDirectedStateMachine<StageState>(states);
            m_localStateMachine.ChangeState(_initialState, true);
        }


        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          States
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // --- Setting Up State - Triggered when game is entered from Title Screen.
        private void SettingUpState_OnEnter(SimpleStateMachine _stateMachine)
        {
            m_enemiesRemainingCountText.text = $"Enemies Remaining: 00";
            m_killsCountText.text = $"Kills: {m_numKillsThisWave:00}";
            m_numSetsSpawned = 0;

            m_player.Initialise();
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


        // --- Paused State - This state means we are leaving the game to return to title screen. So clear the enemies, turn off player, etc.
        private void PausedState_OnEnter(SimpleStateMachine _stateMachine)
        {
            if (m_currentWaveIndex > -1 && m_currentWaveIndex < m_enemyWavesData.Length)
            {
                EnemySetData[] waveEnemySets = m_enemyWavesData[m_currentWaveIndex].EnemySets;
                for (int i = m_numSetsSpawned - 1; i > -1; --i)
                {
                    waveEnemySets[i].SpawnerToKickOff.StopSpawningEnemies();
                }
            }

            m_player.Disable();

            m_centerScreenText.text = string.Empty;
            m_centerScreenSubtitleText.text = string.Empty;
        }

        private void PausedState_Update(SimpleStateMachine _stateMachine)
        {
        }

        private void PausedState_OnExit(SimpleStateMachine _stateMachine)
        {
        }


        // --- Showing Wave Info State - Showing `Wave 01`, etc. for a few seconds.
        private void ShowingWaveInfoState_OnEnter(SimpleStateMachine _stateMachine)
        {
            ++m_currentWaveIndex;
            m_numKillsThisWave = 0;
            m_numEnemiesRemaining = m_enemyWavesData[m_currentWaveIndex].EnemySets.Sum(v => v.NumEnemiesForSet);

            m_centerScreenText.text = $"Wave {m_currentWaveIndex + 1:00}";
            m_killsCountText.text = $"Kills: {m_numKillsThisWave:00}";
            m_killsNeededCountText.text = $"Required: {m_enemyWavesData[m_currentWaveIndex].RequiredEnemyKills:00}";
            m_enemiesRemainingCountText.text = $"Enemies Remaining: {m_numEnemiesRemaining:00}";

            int r = UnityEngine.Random.Range(0, m_newWaveAudioSFX.Length);
            AudioHandler.Instance.PlayOneShot(m_newWaveAudioSFX[r]);
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
            m_centerScreenText.text = string.Empty;
            m_centerScreenSubtitleText.text = string.Empty;
        }


        // --- Playing Game State - Awaits all enemies to finish their paths (or die); or for the player to die.
        private void PlayingGameState_OnEnter(SimpleStateMachine _stateMachine)
        {
            m_numSetsSpawned = 0;
            m_numCompletedSets = 0;
        }

        private void PlayingGameState_Update(SimpleStateMachine _stateMachine)
        {
            if (m_player.PlayerState == Player.PlayerStates.Dead)
            {
                CurrentState = StageState.LoseState;
                return;
            }

            EnemySetData[] waveEnemySets = m_enemyWavesData[m_currentWaveIndex].EnemySets;
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
                            if (m_player.PlayerState != Player.PlayerStates.Dead
                                && m_numKillsThisWave >= m_enemyWavesData[m_currentWaveIndex].RequiredEnemyKills)
                            {
                                // Moves on to the next Wave or finish state
                                if (m_currentWaveIndex + 1 >= m_enemyWavesData.Length)
                                {
                                    CurrentState = StageState.WinState;
                                }
                                else
                                {
                                    CurrentState = StageState.ShowingWaveInfo;
                                }
                            }
                            else
                            {
                                int r = UnityEngine.Random.Range(0, m_loseAudioSFX.Length);
                                AudioHandler.Instance.PlayOneShot(m_loseAudioSFX[r]);
                                CurrentState = StageState.LoseState;
                            }
                        }
                    }
                );

                ++m_numSetsSpawned;
            }
        }

        private void PlayingGameState_OnExit(SimpleStateMachine _stateMachine)
        {
        }


        // --- Win Game State - After completing all waves successfully, this state plays a cheer and tells the player they won
        private void WinGameState_OnEnter(SimpleStateMachine _stateMachine)
        {
            AudioHandler.Instance.PlayOneShot(m_winAudioSFX);

            m_centerScreenText.text = $"You Won!";
            m_centerScreenSubtitleText.text = "Press Esc to exit";
        }

        private void WinGameState_Update(SimpleStateMachine _stateMachine)
        {
        }

        private void WinGameState_OnExit(SimpleStateMachine _stateMachine)
        {
        }


        // --- Lose State - Occurs when player fails to kill required enemies, or is killed by an enemy.
        private void LoseState_OnEnter(SimpleStateMachine _stateMachine)
        {
            EnemySetData[] waveEnemySets = m_enemyWavesData[m_currentWaveIndex].EnemySets;
            for (int i = m_numSetsSpawned - 1; i > -1; --i)
            {
                waveEnemySets[i].SpawnerToKickOff.StopSpawningEnemies();
            }

            if (m_player.PlayerState == Player.PlayerStates.Dead)
            {
                m_centerScreenText.text = $"You Lose. You did not survive the wave.";
            }
            else
            {
                m_centerScreenText.text = $"You Lose. You only killed {m_numKillsThisWave:00} / {m_enemyWavesData[m_currentWaveIndex].RequiredEnemyKills:00}.";
            }

            m_centerScreenSubtitleText.text = "Press Esc to exit";
        }

        private void LoseGameState_Update(SimpleStateMachine _stateMachine)
        {
        }

        private void LoseGameState_OnExit(SimpleStateMachine _stateMachine)
        {
        }
	}
}