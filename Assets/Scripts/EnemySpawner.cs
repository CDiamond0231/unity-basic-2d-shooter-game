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
            ShowingPath,
            SpawningEnemy,
            SpawnCooldown,
            AwaitingPathingToComplete,
            RemovingPath,
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [Header("Pools")]
        [SerializeField] private EnemyPool m_enemyPool = null!;
        [SerializeField] private LineRenderer m_linePathRenderer = null!;

        [Header("Path")]
        [SerializeField] private Color m_gizmoPathColour = Color.red;
        [SerializeField] private Vector3[] m_movementPoints = System.Array.Empty<Vector3>();

        [Header("Parameter")]
        [SerializeField] private float m_spawnInterval = 0.5f;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Non-Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private EnumDirectedStateMachine<SpawnerState>? m_localStateMachine = null;

        private int m_numEnemiesSpawned = 0;
        private int m_numEnemiesToSpawn = 10;
        private float m_timeToTraversePathDuration = 5.0f;
        private int m_numEnemiesFinished = 0;
        private System.Action? m_whenAllEnemiesStoppedCallback = null;

        private readonly List<Enemy> m_ownedEnemyObjects = new List<Enemy>();
        private readonly List<Vector3> m_drawPathPoints = new List<Vector3>();

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Properties
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public bool IsSpawningEnemies => CurrentState != SpawnerState.Inactive;

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
            m_localStateMachine?.Update();
        }

        protected void OnDrawGizmos()
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
        public void StartSpawningEnemies(int _numEnemies, float _travelPathDuration, System.Action? _whenAllEnemiesStopped)
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

            CurrentState = SpawnerState.ShowingPath;
        }

        /// <summary> Stops Spawning Enemies </summary>
        public void StopSpawningEnemies()
        {
            foreach (Enemy enemy in m_ownedEnemyObjects)
            {
                enemy.DisableEnemy();
                m_enemyPool.ReleaseObj(enemy);
            }
            m_ownedEnemyObjects.Clear();

            m_linePathRenderer.gameObject.SetActive(false);

            CurrentState = SpawnerState.Inactive;
        }

        /// <summary> Builds the state machine </summary>
        private void BuildStateMachine(SpawnerState _initialState)
        {
            Dictionary<SpawnerState, SimpleState> states = new Dictionary<SpawnerState, SimpleState>
            {
                [SpawnerState.Inactive]                  = new SimpleState(null, null, null),
                [SpawnerState.ShowingPath]               = new SimpleState(ShowingPathState_OnEnter, ShowingPathState_Update, ShowingPathState_OnExit),
                [SpawnerState.SpawningEnemy]             = new SimpleState(SpawningEnemyState_OnEnter, SpawningEnemyState_Update, SpawningEnemyState_OnExit),
                [SpawnerState.SpawnCooldown]             = new SimpleState(SpawnCooldownState_OnEnter, SpawnCooldownState_Update, SpawnCooldownState_OnExit),
                [SpawnerState.AwaitingPathingToComplete] = new SimpleState(AwaitingPathingState_OnEnter, AwaitingPathingState_Update, AwaitingPathingState_OnExit),
                [SpawnerState.RemovingPath]              = new SimpleState(RemovingPathState_OnEnter, RemovingPathState_Update, RemovingPathState_OnExit)
            };

            m_localStateMachine = new EnumDirectedStateMachine<SpawnerState>(states);
            m_localStateMachine.ChangeState(_initialState);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          States
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private void ShowingPathState_OnEnter(SimpleStateMachine _stateMachine)
        {
            m_linePathRenderer.gameObject.SetActive(true);
            m_linePathRenderer.startColor = m_gizmoPathColour;
            m_linePathRenderer.endColor = m_gizmoPathColour / 2;
            m_linePathRenderer.SetPositions(new Vector3[] { this.transform.position, this.transform.position });
        }

        private void ShowingPathState_Update(SimpleStateMachine _stateMachine)
        {
            const float ProgressPerStep = 0.01f;
            m_drawPathPoints.Clear();

            float t = _stateMachine.TimeSpentInCurrentState / 2.0f;
            m_drawPathPoints.Add(m_movementPoints[0]);

            for (float step = ProgressPerStep; step <= 1.0f && step <= t; step += ProgressPerStep)
            {
                Vector3 nowPoint = BezierSpline.GetPointOnInterpolatedBezierSpline(m_movementPoints, step);
                m_drawPathPoints.Add(nowPoint);
            }

            m_linePathRenderer.positionCount = m_drawPathPoints.Count;
            m_linePathRenderer.SetPositions(m_drawPathPoints.ToArray());
            if (t >= 1f)
            {
                CurrentState = SpawnerState.RemovingPath;
            }
        }

        private void ShowingPathState_OnExit(SimpleStateMachine _stateMachine)
        {
        }

        private void RemovingPathState_OnEnter(SimpleStateMachine _stateMachine)
        {
            // Reversed now for the fade out effect
            m_linePathRenderer.startColor = m_gizmoPathColour / 2;
            m_linePathRenderer.endColor = m_gizmoPathColour;
        }

        private void RemovingPathState_Update(SimpleStateMachine _stateMachine)
        {
            const float ProgressPerStep = 0.01f;
            m_drawPathPoints.Clear();

            float t = _stateMachine.TimeSpentInCurrentState / 2.0f;
            m_drawPathPoints.Add(m_movementPoints[^1]);

            for (float step = 1.0f - ProgressPerStep; step >= 0.0f && step >= t; step -= ProgressPerStep)
            {
                Vector3 nowPoint = BezierSpline.GetPointOnInterpolatedBezierSpline(m_movementPoints, step);
                m_drawPathPoints.Add(nowPoint);
            }

            m_linePathRenderer.positionCount = m_drawPathPoints.Count;
            m_linePathRenderer.SetPositions(m_drawPathPoints.ToArray());
            if (_stateMachine.TimeSpentInCurrentState > 2.0f)
            {
                CurrentState = SpawnerState.SpawningEnemy;
            }
        }

        private void RemovingPathState_OnExit(SimpleStateMachine _stateMachine)
        {
            m_linePathRenderer.gameObject.SetActive(false);
        }


        private void SpawningEnemyState_OnEnter(SimpleStateMachine _stateMachine)
        {
            ++m_numEnemiesSpawned;
            Enemy spawnedEnemy = m_enemyPool.GetOrAddFreeObject();
            m_ownedEnemyObjects.Add(spawnedEnemy);

            MeshRenderer meshrenderer = spawnedEnemy.GetComponentInChildren<MeshRenderer>();
            meshrenderer.material = new Material(meshrenderer.material)
            {
                color = m_gizmoPathColour
            };

            spawnedEnemy.Initialise(m_movementPoints, m_timeToTraversePathDuration, (killedByPlayer) =>
            {
                ++m_numEnemiesFinished;
                if (StageLoop.Instance != null)
                {
                    StageLoop.Instance.OnEnemyRemoved();
                }
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
                foreach (Enemy enemy in m_ownedEnemyObjects)
                {
                    enemy.DisableEnemy();
                    m_enemyPool.ReleaseObj(enemy);
                }

                m_ownedEnemyObjects.Clear();
                CurrentState = SpawnerState.Inactive;
                m_whenAllEnemiesStoppedCallback?.Invoke();
            }
        }

        private void AwaitingPathingState_OnExit(SimpleStateMachine _stateMachine)
        {
        }
    }
}