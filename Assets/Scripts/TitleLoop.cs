//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Title Loop
//             Author: Christopher A
//             Date Created: 25th June, 2025
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//      Title Loop. State Machine which handles the title screen loop
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Title Screen Loop
/// </summary>
namespace BasicUnity2DShooter
{
	public class TitleLoop : MonoBehaviour
	{
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [SerializeField] private StageLoop m_stageLoop;
		[SerializeField] private SceneTransitionEffect m_sceneTransitionEffect;

		[Header("Layout")]
		[SerializeField] private GameObject m_titleUIHierarchy = null!;

        [Header("Spanwers")]
        [SerializeField] private EnemySpawner[] m_enemySpawners = System.Array.Empty<EnemySpawner>();

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Non-Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private List<int> m_enemySpawnerSelections = new List<int>();
        private int m_currentSpawnerIndex = -1;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Properties
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private EnemySpawner? CurrentEnemySpawner
        {
            get
            {
                if (m_currentSpawnerIndex == -1)
                    return null;

                return m_enemySpawners[ m_enemySpawnerSelections[m_currentSpawnerIndex] ];
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Unity Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        protected void OnEnable()
		{
            m_titleUIHierarchy.SetActive(true);
            m_currentSpawnerIndex = -1;

            m_enemySpawnerSelections.Clear();
            for (int i = 0; i < m_enemySpawners.Length; ++i)
            {
                m_enemySpawnerSelections.Add(i);
            }
        }

        protected void OnDisable()
        {
            m_titleUIHierarchy.SetActive(false);

            if (CurrentEnemySpawner != null)
            {
                CurrentEnemySpawner.StopSpawningEnemies();
                m_currentSpawnerIndex = -1;
            }
        }

        protected void Update()
        {
            // ~~~ Spawns in some enemies along their path to make the title screen a bit more interesting ~~~
            if (CurrentEnemySpawner == null || CurrentEnemySpawner.IsSpawningEnemies == false)
            {
                if (m_currentSpawnerIndex == -1 || m_currentSpawnerIndex + 1 >= m_enemySpawnerSelections.Count)
                {
                    m_currentSpawnerIndex = 0;
                    for (int i = m_enemySpawnerSelections.Count - 1; i > 0; --i)
                    {
                        // Swaps Array elements so we have randomness while not repeating the same spawner in a row
                        int r = UnityEngine.Random.Range(0, i);
                        (m_enemySpawnerSelections[i], m_enemySpawnerSelections[r]) = (m_enemySpawnerSelections[r], m_enemySpawnerSelections[i]);
                    }
                }
                else
                {
                    ++m_currentSpawnerIndex;
                }

                CurrentEnemySpawner!.StartSpawningEnemies(UnityEngine.Random.Range(5, 12), 5.0f, null);
            }


            // ~~~ Transitions to Gameplay ~~~
            if (Input.GetKeyDown(KeyCode.Space)
                && m_sceneTransitionEffect.CurrentTransitionState == SceneTransitionEffect.TransitionState.Idle)
            {
                m_sceneTransitionEffect.ShowRandomTransition(0.5f, _onFadeOutCompleted: () =>
                {
                    m_stageLoop.StartStageLoop();

                    // Switching off Title Screen Controller, which then switches off UI via OnDiable
                    this.enabled = false;
                });
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public void StartTitleLoop()
        {
            // Switching on Title Screen Controller, which then switches off on via OnEnable
            this.enabled = true;
        }
    }
}