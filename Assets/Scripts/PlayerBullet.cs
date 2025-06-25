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
	public float m_moveSpeed = 5;
	public float m_remainingLifeTime = 2;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //          Non-Inspector Fields
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private System.Action<PlayerBullet>? _onRemovedCallback = null;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //          Unity Methods
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    protected void Update()
	{
		transform.position += Vector3.up * m_moveSpeed * Time.deltaTime;

		m_remainingLifeTime -= Time.deltaTime;
		if (m_remainingLifeTime < 0.00001)
		{
			RemoveObject();
		}
	}

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //          Methods
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void Initialise(System.Action<PlayerBullet> _onRemoved)
	{
        m_remainingLifeTime = 2.0f;
        gameObject.SetActive(true);
        _onRemovedCallback = _onRemoved;
    }

    public void RemoveObject()
	{
        gameObject.SetActive(false);
        _onRemovedCallback?.Invoke(this);
    }
}
