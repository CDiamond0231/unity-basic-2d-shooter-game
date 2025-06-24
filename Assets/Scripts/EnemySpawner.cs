//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Enemy Spawner
//             Author: Christopher A
//             Date Created: 24th June, 2025
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//      Spawner Class for enemies. Also handles their pathing.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
#nullable enable
using BasicUnity2DShooter.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary> Enemy SpawnPoint </summary>
namespace BasicUnity2DShooter
{
    public class EnemySpawner : MonoBehaviour
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Definitions
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private enum SpawnerState
        {
            Inactive,
            SpawningEnemy,
            SpawnCooldown,
            AwaitingPathingToComplete,
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [Header("Prefab")]
        [SerializeField] private EnemyPool m_enemyPool = null!;

        [Header("Path")]
        [SerializeField] private Color m_gizmoPathColour = Color.red;
        [SerializeField] private Vector3[] m_movementPoints = System.Array.Empty<Vector3>();

        [Header("Parameter")]
        private float m_spawnInterval = 0.5f;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Non-Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private EnumDirectedStateMachine<SpawnerState>? m_localStateMachine = null;

        private int m_numEnemiesSpawned = 0;
        private int m_numEnemiesToSpawn = 10;
        private float m_timeToTraversePathDuration = 5.0f;
        private int m_numEnemiesFinished = 0;
        private System.Action? m_whenAllEnemiesStoppedCallback = null;
        private List<Enemy> m_ownedEnemyObjects = new List<Enemy>();

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Properties
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private SpawnerState CurrentState
        {
            get
            {
                if (m_localStateMachine == null)
                    return SpawnerState.Inactive;
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
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = m_gizmoPathColour;
            Gizmos.DrawWireSphere(transform.position, 1.0f);

            const float ProgressPerStep = 0.01f;
            for (float step = ProgressPerStep; step <= 1.0f; step += ProgressPerStep)
            {
                Vector3 priorPoint = BezierSpline.GetPointOnInterpolatedBezierSpline(m_movementPoints, step - ProgressPerStep);
                Vector3 nowPoint = BezierSpline.GetPointOnInterpolatedBezierSpline(m_movementPoints, step);
                Gizmos.DrawLine(priorPoint, nowPoint);
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary> Starts spawning enemies along the path </summary>
        /// <param name="_numEnemies"> Total number of enemies to chuck onto the path. </param>
        /// <param name="_whenAllEnemiesStopped"> Callback when all enemies have either left the path or have been destroyed. </param>
        public void StartSpawningEnemies(int _numEnemies, float _travelPathDuration, System.Action _whenAllEnemiesStopped)
        {
            m_numEnemiesSpawned = 0;
            m_numEnemiesFinished = 0;

            m_numEnemiesToSpawn = Mathf.Max(1, _numEnemies);
            m_timeToTraversePathDuration = _travelPathDuration;
            m_whenAllEnemiesStoppedCallback = _whenAllEnemiesStopped;

            if (m_ownedEnemyObjects.Capacity < _numEnemies)
            {
                m_ownedEnemyObjects.Capacity = _numEnemies;
            }

            CurrentState = SpawnerState.SpawningEnemy;
        }

        private void BuildStateMachine(SpawnerState _initialState)
        {
            Dictionary<SpawnerState, SimpleState> states = new Dictionary<SpawnerState, SimpleState>();
            states[SpawnerState.Inactive] = new SimpleState(null, null, null);
            states[SpawnerState.SpawningEnemy] = new SimpleState(SpawningEnemyState_OnEnter, SpawningEnemyState_Update, SpawningEnemyState_OnExit);
            states[SpawnerState.SpawnCooldown] = new SimpleState(SpawnCooldownState_OnEnter, SpawnCooldownState_Update, SpawnCooldownState_OnExit);
            states[SpawnerState.AwaitingPathingToComplete] = new SimpleState(AwaitingPathingState_OnEnter, AwaitingPathingState_Update, AwaitingPathingState_OnExit);

            m_localStateMachine = new EnumDirectedStateMachine<SpawnerState>(states);
            m_localStateMachine.ChangeState(_initialState);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          States
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private void SpawningEnemyState_OnEnter(SimpleStateMachine _stateMachine)
        {
            ++m_numEnemiesSpawned;
            Enemy spawnedEnemy = m_enemyPool.GetOrAddFreeObject();
            m_ownedEnemyObjects.Add(spawnedEnemy);

            spawnedEnemy.Initialise(m_movementPoints, m_timeToTraversePathDuration, (killedByPlayer) =>
            {
                ++m_numEnemiesFinished;
            });

            if (m_numEnemiesSpawned >= m_numEnemiesToSpawn)
                CurrentState = SpawnerState.AwaitingPathingToComplete;
            else
                CurrentState = SpawnerState.SpawnCooldown;
        }

        private void SpawningEnemyState_Update(SimpleStateMachine _stateMachine)
        {
        }

        private void SpawningEnemyState_OnExit(SimpleStateMachine _stateMachine)
        {
        }



        private void SpawnCooldownState_OnEnter(SimpleStateMachine _stateMachine)
        {
        }

        private void SpawnCooldownState_Update(SimpleStateMachine _stateMachine)
        {
            if (_stateMachine.TimeSpentInCurrentState > m_spawnInterval)
                CurrentState = SpawnerState.SpawningEnemy;
        }

        private void SpawnCooldownState_OnExit(SimpleStateMachine _stateMachine)
        {
        }



        private void AwaitingPathingState_OnEnter(SimpleStateMachine _stateMachine)
        {
        }

        private void AwaitingPathingState_Update(SimpleStateMachine _stateMachine)
        {
            if (m_numEnemiesFinished >= m_numEnemiesToSpawn)
            {
                CurrentState = SpawnerState.Inactive;
                m_whenAllEnemiesStoppedCallback?.Invoke();
            }
        }

        private void AwaitingPathingState_OnExit(SimpleStateMachine _stateMachine)
        {
        }
    }
}