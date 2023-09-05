using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public class UISideWindow : MonoBehaviour
	{
		public enum Side
		{
			Left,
			Right,
		}

		[Header("Main Frame Components")]
		public RectTransform frame = null;

		[Header("Main Frame Settings")]
		[SerializeField]
		protected Side dockedSide = Side.Right;
		public Side DockedSide
		{
			get
			{
				return dockedSide;
			}
			set
			{
				DockToSide(value);
			}
		}

		[SerializeField]
		protected bool show = true;
		public bool Show
		{
			get 
			{
				return show;
			}
			set
			{
				SetShow(value, false);
			}
		}

		protected readonly float showDuration = 0.125f;
		protected float showInterpolation = 1;
		protected Coroutine showCoroutine = null;


		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected virtual void Awake()
		{
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected virtual void Start()
		{
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected virtual void Update()
		{
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected virtual void OnDestroy()
		{
		}

		/// <summary>
		/// OnValidate is called when values where changed in the inspector.
		/// </summary>
		protected virtual void OnValidate()
		{
			DockToSide(dockedSide);
			SetShow(show, true);
		}

		// ========================================================= Behaviour Methods =========================================================
		
		/// <summary>
		/// Dock this side window to one side of the screen.
		/// </summary>
		protected void DockToSide(Side side)
		{
			RectTransform rectTransform = GetComponent<RectTransform>();
			if (side == Side.Right)
			{
				rectTransform.anchorMin = new Vector2(1, 0);
				rectTransform.anchorMax = new Vector2(1, 1);
				rectTransform.pivot = new Vector2(1, 0.5f);
			}
			else if (side == Side.Left)
			{
				rectTransform.anchorMin = new Vector2(0, 0);
				rectTransform.anchorMax = new Vector2(0, 1);
				rectTransform.pivot = new Vector2(0, 0.5f);
			}
			dockedSide = side;
		}

		/// <summary>
		/// Set if this side window is showing, and set if showing animation if needed.
		/// </summary>
		protected void SetShow(bool show, bool immediate)
		{
			RectTransform rectTransform = GetComponent<RectTransform>();
			if (immediate)
			{
				// directly set, used in editor
				frame.gameObject.SetActive(show);
				rectTransform.anchoredPosition = new Vector2(show ? 0 : frame.rect.width * (dockedSide == Side.Right ? 1 : -1), 0);
				showInterpolation = show ? 1 : 0;
				this.show = show;
			}
			else
			{
				// show with an animation sequence
				if (showCoroutine != null)
				{
					StopCoroutine(showCoroutine);
				}
				showCoroutine = StartCoroutine(ShowSequence(show));
			}
		}

		/// <summary>
		/// The animated sequence for showing the side window.
		/// </summary>
		protected IEnumerator ShowSequence(bool show)
		{
			RectTransform rectTransform = GetComponent<RectTransform>();

			// initialize apparences
			frame.gameObject.SetActive(true);

			if (show)
			{
				// animation
				while (showInterpolation < 1)
				{
					showInterpolation += 1f / showDuration * Time.deltaTime;
					rectTransform.anchoredPosition = Vector2.Lerp(
						new Vector2(frame.rect.width * (dockedSide == Side.Right ? 1 : -1), 0),
						Vector2.zero,
						showInterpolation);
					yield return null;
				}

				// finalize apparences
				showInterpolation = 1;
				rectTransform.anchoredPosition = new Vector2(0, 0);
				frame.gameObject.SetActive(true);
			}
			else
			{
				// animation
				while (showInterpolation > 0)
				{
					showInterpolation -= 1f / showDuration * Time.deltaTime;
					rectTransform.anchoredPosition = Vector2.Lerp(
						new Vector2(frame.rect.width * (dockedSide == Side.Right ? 1 : -1), 0),
						Vector2.zero,
						showInterpolation);
					yield return null;
				}

				// finalize apparences
				showInterpolation = 0;
				rectTransform.anchoredPosition = new Vector2(frame.rect.width * (dockedSide == Side.Right ? 1 : -1), 0);
				frame.gameObject.SetActive(false);
			}

			// finalize values
			this.show = show;
			showCoroutine = null;
		}
	}
}