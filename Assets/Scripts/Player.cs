//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Player
//             Author: Christopher A
//             Date Created: 24th June, 2025
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//      Player Class. Handles movement, Shooting collision detection.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Player Character </summary>
namespace BasicUnity2DShooter
{
	public class Player : MonoBehaviour
	{
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Definitions 
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		public enum PlayerStates
		{
			Active,
			Dead,
		}

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [Header("Pools")]
		[SerializeField] private BulletPool m_bulletPool = null!;

		[Header("Parameter")]
		[SerializeField] private float m_moveSpeed = 5f;
		[SerializeField] private float m_smoothing = 0.8f;

        [Header("Audio")]
        [SerializeField] private AudioClip[] m_shootSFX = System.Array.Empty<AudioClip>();
        [SerializeField] private AudioClip[] m_deathSFX = System.Array.Empty<AudioClip>();

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Non-Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private Vector3 m_targetVelocity = Vector3.zero;
		private Vector3 m_currentVelocity = Vector3.zero;

        private LinkedList<PlayerBullet> m_ownedBullets = new LinkedList<PlayerBullet>();

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		//          Properties
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		public PlayerStates PlayerState { get; private set; } = PlayerStates.Active;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Unity Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        protected void Update()
        {
            // Movement
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            Vector3 targetDirection = new Vector3(moveHorizontal, moveVertical, 0);
            targetDirection.Normalize();
            m_targetVelocity = targetDirection * m_moveSpeed;

            m_currentVelocity = Vector3.Lerp(m_currentVelocity, m_targetVelocity, m_smoothing);
            transform.position += m_currentVelocity * Time.deltaTime;

            // Shooting
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
            {
                PlayerBullet bullet = m_bulletPool.GetOrAddFreeObject();
                m_ownedBullets.AddLast(bullet);

                bullet.Initialise(OnPlayerBulletRemoved);
                bullet.transform.position = transform.position;

                int r = UnityEngine.Random.Range(0, m_shootSFX.Length);
                AudioHandler.Instance.PlayOneShot(m_shootSFX[r], 0.35f);
            }
        }

        protected void OnCollisionEnter(Collision _collision)
        {
            Enemy enemy = _collision.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                PlayerState = PlayerStates.Dead;

                int r = UnityEngine.Random.Range(0, m_deathSFX.Length);
                AudioHandler.Instance.PlayOneShot(m_deathSFX[r]);

                Disable();
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public void Initialise()
		{
			PlayerState = PlayerStates.Active;
			gameObject.SetActive(true);
        }

        public void Disable()
        {
            // Clearing any lingering bullets for pool.
            foreach (PlayerBullet bullet in m_ownedBullets)
            {
                m_bulletPool.ReleaseObj(bullet);
            }

            m_ownedBullets.Clear();
            gameObject.SetActive(false);
        }

        private void OnPlayerBulletRemoved(PlayerBullet _bullet)
        {
            if (m_ownedBullets.Remove(_bullet))
            {
                m_bulletPool.ReleaseObj(_bullet);
            }
        }
    }
}