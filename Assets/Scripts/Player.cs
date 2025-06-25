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

        [Header("Camera")]
        [SerializeField] private Camera m_mainCamera = null!;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Non-Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private Vector3 m_targetVelocity = Vector3.zero;
		private Vector3 m_currentVelocity = Vector3.zero;

        private float m_leftBound = 0f;
        private float m_rightBound = 0f;
        private float m_topBound = 0f;
        private float m_bottomBound = 0f;

        private readonly LinkedList<PlayerBullet> m_ownedBullets = new LinkedList<PlayerBullet>();

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		//          Properties
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		public PlayerStates PlayerState { get; private set; } = PlayerStates.Active;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Unity Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        protected void Awake()
        {
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            float spriteWidth = meshRenderer.bounds.extents.x;
            float spriteHeight = meshRenderer.bounds.extents.y;

            Vector2 screenBounds = m_mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, m_mainCamera.transform.position.z));
            m_leftBound = -screenBounds.x + spriteWidth;
            m_rightBound = screenBounds.x - spriteWidth;
            m_bottomBound = -screenBounds.y + spriteHeight;

            m_topBound = 0; // Halfway up the screen. You can't cheese the enemies by going to the top of the screen
        }

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

                bullet.transform.position = transform.position;
                bullet.Initialise(OnPlayerBulletRemoved);

                int r = UnityEngine.Random.Range(0, m_shootSFX.Length);
                AudioHandler.Instance.PlayOneShot(m_shootSFX[r], 0.35f);
            }
        }

        protected void LateUpdate() 
        {
            // Using LateUpdate for clamping to avoid jitter
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, m_leftBound, m_rightBound);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, m_bottomBound, m_topBound);
            transform.position = clampedPosition;
        }

        protected void OnCollisionEnter(Collision _collision)
        {
            if (_collision.transform.TryGetComponent<Enemy>(out _))
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
        /// <summary> Invoked when entering the GameState to kick off player input </summary>
        public void Initialise()
		{
			PlayerState = PlayerStates.Active;
            transform.position = new Vector3(0, -4, 0);
            gameObject.SetActive(true);
        }

        /// <summary> Invoked either when player dies or game state is exitted via `Esc` key. </summary>
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

        /// <summary> Callback for when a player bullet is removed (either from timeout or from hitting an enemy) </summary>
        private void OnPlayerBulletRemoved(PlayerBullet _bullet)
        {
            if (m_ownedBullets.Remove(_bullet))
            {
                m_bulletPool.ReleaseObj(_bullet);
            }
        }
    }
}