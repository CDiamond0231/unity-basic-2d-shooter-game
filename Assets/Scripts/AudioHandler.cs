//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Audio Handler
//             Author: Christopher A
//             Date Created: 24th June, 2025
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//      Static Class to interact with OneShot Audio
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicUnity2DShooter
{
    public class AudioHandler : MonoBehaviour
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Statics
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public static AudioHandler Instance { get; private set; } = null!;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [SerializeField] private AudioSource[] m_audioSources = System.Array.Empty<AudioSource>();


        protected void Awake()
        {
            Instance = this;
        }

        public void PlayOneShot(AudioClip _clip, float _volume = 1.0f)
        {
            foreach (AudioSource audSource in m_audioSources)
            {
                if (audSource.isPlaying == false)
                {
                    audSource.PlayOneShot(_clip, _volume);
                    return;
                }
            }
        }
    }
}
