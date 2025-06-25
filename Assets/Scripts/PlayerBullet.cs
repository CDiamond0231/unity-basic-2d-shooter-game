//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Player Bullet
//             Author: Christopher A
//             Date Created: 25th June, 2025
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//      Manages the bullets shot by the player
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Player Bullet </summary>
public class PlayerBullet : MonoBehaviour
{
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //          Inspector Fields
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("Parameter")]
	[SerializeField] private float m_moveSpeed = 5f;
	[SerializeField] private float m_lifeTimeSeconds = 2f;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //          Non-Inspector Fields
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private float m_remainingLifeTime = 2f;
    private System.Action<PlayerBullet>? _onRemovedCallback = null;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //          Unity Methods
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    protected void Update()
	{
		transform.position += m_moveSpeed * Time.deltaTime * Vector3.up;

		m_remainingLifeTime -= Time.deltaTime;
		if (m_remainingLifeTime < 0.00001)
		{
			RemoveObject();
		}
	}

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //          Methods
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    /// <summary> Invoked when the bullet is fired by the player. </summary>
	public void Initialise(System.Action<PlayerBullet> _onRemoved)
	{
        m_remainingLifeTime = m_lifeTimeSeconds;

        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.Clear();
            trail.time = UnityEngine.Random.Range(0.05f, 0.15f);
        }

        gameObject.SetActive(true);
        _onRemovedCallback = _onRemoved;
    }

    /// <summary> Invoked either when timeout occurs or an enemy is struck. </summary>
    public void RemoveObject()
	{
        gameObject.SetActive(false);
        _onRemovedCallback?.Invoke(this);
    }
}
