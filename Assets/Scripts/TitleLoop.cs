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
		[SerializeField] private StageLoop m_stage_loop;
		[SerializeField] private SceneTransitionEffect m_sceneTransitionEffect;

		[Header("Layout")]
		[SerializeField] private Transform m_ui_title;

		//------------------------------------------------------------------------------

		private void Start()
		{
			//default start
			StartTitleLoop();
		}

		//
		#region loop
		public void StartTitleLoop()
		{
			StartCoroutine(TitleCoroutine());
		}

		/// <summary>
		/// Title loop
		/// </summary>
		private IEnumerator TitleCoroutine()
		{
			Debug.Log($"Start TitleCoroutine");

			SetupTitle();

			//waiting game start
			while (true)
			{
				if (Input.GetKeyDown(KeyCode.Space)
                    && m_sceneTransitionEffect.CurrentTransitionState == SceneTransitionEffect.TransitionState.Idle)
				{
					m_sceneTransitionEffect.ShowRandomTransition(0.5f, _onFadeOutCompleted: () =>
					{

						CleanupTitle();

						//Start StageLoop
						m_stage_loop.StartStageLoop();
					});

					yield break;
				}
				yield return null;
			}
		}
		#endregion

		//
		void SetupTitle()
		{
			m_ui_title.gameObject.SetActive(true);
		}

		void CleanupTitle()
		{
			m_ui_title.gameObject.SetActive(false);
		}
	}
}