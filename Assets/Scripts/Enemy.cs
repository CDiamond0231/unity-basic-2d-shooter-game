//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Enemy
//             Author: Christopher A
//             Date Created: 24th June, 2025
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//      Enemy Class. Handles movement along the path and collision detection.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
#nullable enable
using BasicUnity2DShooter.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BasicUnity2DShooter
{
	public class Enemy : MonoBehaviour
	{
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [Header("Parameter")]
		[SerializeField] private float m_rotationSpeed = 200;
		[SerializeField] private AudioClip m_deathSFX = null!;

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		//          Non-Inspector Fields
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		private float m_secondsSinceSpawn = 0.0f;
		private float m_totalPathTraversalDuration = 1.0f;
		private Vector3[] m_movementPoints = System.Array.Empty<Vector3>();

        private System.Action<bool>? m_onEnemyDestroyedCallback = null;
		private Rigidbody? m_rigidBody = null;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Properties
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private Rigidbody RigidBody
		{
			get
			{
				if (m_rigidBody != null)
					return m_rigidBody;

				m_rigidBody = GetComponent<Rigidbody>();
				return m_rigidBody;
			}
		}

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Unity Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        protected void Update()
		{
			m_secondsSinceSpawn += Time.deltaTime;
			if (m_secondsSinceSpawn > m_totalPathTraversalDuration)
			{
				gameObject.SetActive(false);
				m_onEnemyDestroyedCallback?.Invoke(false); // false => Enemy made it to end of path
				m_onEnemyDestroyedCallback = null;
                return;
			}

			float t = m_secondsSinceSpawn / m_totalPathTraversalDuration;
			transform.rotation *= Quaternion.AngleAxis(m_rotationSpeed * Time.deltaTime, new Vector3(1, 1, 0));
            RigidBody.position = BezierSpline.GetPointOnInterpolatedBezierSpline(m_movementPoints, t);
        }

		private void OnCollisionEnter(Collision collision)
		{
			PlayerBullet playerBullet = collision.transform.GetComponent<PlayerBullet>();
			if (playerBullet)
			{
				OnDestroyedByPlayer(playerBullet);
			}
		}

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		/// <summary> Call this when spawning an enemy from the pool. </summary>
		/// <param name="_movementPoints"> Where to go </param>
		/// <param name="_totalPathTraversalDuration"> How long to take </param>
		/// <param name="_onEnemyDestroyedCallback"> Who can I call when I either die or finish moving? </param>
		public void Initialise(Vector3[] _movementPoints, float _totalPathTraversalDuration, System.Action<bool>? _onEnemyDestroyedCallback)
		{
			m_secondsSinceSpawn = 0.0f;
            m_totalPathTraversalDuration = _totalPathTraversalDuration;
			m_movementPoints = _movementPoints;
			RigidBody.position = m_movementPoints[0];
			m_onEnemyDestroyedCallback = _onEnemyDestroyedCallback;

            gameObject.SetActive(true);
        }

		/// <summary> Turns off the enemy. </summary>
        public void DisableEnemy()
        {
            // Moes the enemy off the map so they don't respawn in the middle of the screen when selected via the pool again
            transform.position = Vector3.right * 1000f;
            RigidBody.position = transform.position;
            gameObject.SetActive(false);
        }

		/// <summary> Invoked when shot by player. </summary>
        private void OnDestroyedByPlayer(PlayerBullet _playerBullet)
		{
			if (StageLoop.Instance != null)
			{
				StageLoop.Instance.AddKill();
			}

			if (_playerBullet)
			{
				_playerBullet.RemoveObject();
			}

			AudioHandler.Instance.PlayOneShot(m_deathSFX);
			PFXHandler.Instance.PlayEnemyDeathPFX(transform.position);

			// Moes the enemy off the map so they don't respawn in the middle of the screen when selected via the pool again
			transform.position = BezierSpline.GetPointOnInterpolatedBezierSpline(m_movementPoints, 1f);
            RigidBody.position = transform.position;

            gameObject.SetActive(false);
            m_onEnemyDestroyedCallback?.Invoke(true); // true => Killed by player
            m_onEnemyDestroyedCallback = null;
        }
	}
}