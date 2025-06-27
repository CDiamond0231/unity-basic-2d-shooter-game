//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             PFX Handler
//             Author: Christopher A
//             Date Created: 26th June, 2025
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//      Static Class to interact with PFX
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicUnity2DShooter
{
    public class PFXHandler : MonoBehaviour
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Statics
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public static PFXHandler Instance { get; private set; } = null!;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [SerializeField] private ParticleSystem m_playerDeathPFX = null!;
        [SerializeField] private ParticleSystem[] m_enemyDeathPFX = System.Array.Empty<ParticleSystem>();

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Non-Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private int m_enemyDeathPfxIndex = 0;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Unity Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        protected void Awake()
        {
            Instance = this;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary> Plays a player PFX on demand. </summary>
        public void PlayPlayerDeathPFX(Vector3 _worldPosition)
        {
            m_playerDeathPFX.transform.position = _worldPosition;
            m_playerDeathPFX.Play();
        }

        /// <summary> Plays an enemy PFX on demand. </summary>
        public void PlayEnemyDeathPFX(Vector3 _worldPosition)
        {
            m_enemyDeathPfxIndex = (m_enemyDeathPfxIndex + 1) % m_enemyDeathPFX.Length;
            m_enemyDeathPFX[m_enemyDeathPfxIndex].transform.position = _worldPosition;
            m_enemyDeathPFX[m_enemyDeathPfxIndex].Play();
        }
    }
}
