using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DicingHeros
{
    public class UIPrompt : MonoBehaviour
    {
		[Header("Components")]
		public CanvasGroup canvasGroup;
		public TextMeshProUGUI text;

		// working variables
		private Coroutine promptCoroutine = null;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		private void Awake()
		{
		
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		private void Start()
		{
			canvasGroup.alpha = 0;
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		private void Update()
		{
			
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
			
		}

		// ========================================================= Animations =========================================================

		public Coroutine StartTurnAnimation(Player player, Turn turn)
		{
			text.text = string.Format("{0}  Turn {1}\n\nStart!", player.name, turn.turnNumber);
			if (promptCoroutine != null)
			{
				StopCoroutine(promptCoroutine);
			}
			promptCoroutine = StartCoroutine(PromptAnimation());
			return promptCoroutine;
		}

		public Coroutine EndTurnAnimation(Player player, Turn turn)
		{
			text.text = string.Format("{0}  Turn {1}\n\nCompleted!", player.name, turn.turnNumber);
			if (promptCoroutine != null)
			{
				StopCoroutine(promptCoroutine);
			}
			promptCoroutine = StartCoroutine(PromptAnimation());
			return promptCoroutine;
		}

		private IEnumerator PromptAnimation()
		{
			float duration = 0.5f;
			float startTime = Time.time;

			canvasGroup.alpha = 0;
			while (Time.time < startTime + duration)
			{
				canvasGroup.alpha = Mathf.Lerp(0, 1, Mathf.InverseLerp(startTime, startTime + duration, Time.time));
				yield return null;
			}
			canvasGroup.alpha = 1;

			yield return new WaitForSeconds(1f);

			startTime = Time.time;
			canvasGroup.alpha = 1;
			while (Time.time < startTime + duration)
			{
				canvasGroup.alpha = Mathf.Lerp(1, 0, Mathf.InverseLerp(startTime, startTime + duration, Time.time));
				yield return null;
			}
			canvasGroup.alpha = 0;

			promptCoroutine = null;
		}
	}
}