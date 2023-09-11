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
			Top,
			Bottom,
		}

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

		protected RectTransform rectTransform => GetComponent<RectTransform>();


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
			if (side == Side.Left)
			{
				rectTransform.anchorMin = new Vector2(0, 0);
				rectTransform.anchorMax = new Vector2(0, 1);
				rectTransform.pivot = new Vector2(0, 0.5f);
			}
			else if (side == Side.Right)
			{
				rectTransform.anchorMin = new Vector2(1, 0);
				rectTransform.anchorMax = new Vector2(1, 1);
				rectTransform.pivot = new Vector2(1, 0.5f);
			}
			else if (side == Side.Top)
			{
				rectTransform.anchorMin = new Vector2(0, 1);
				rectTransform.anchorMax = new Vector2(1, 1);
				rectTransform.pivot = new Vector2(0.5f, 1);
			}
			else if (side == Side.Bottom)
			{
				rectTransform.anchorMin = new Vector2(0, 0);
				rectTransform.anchorMax = new Vector2(1, 0);
				rectTransform.pivot = new Vector2(0.5f, 0);
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
				rectTransform.anchoredPosition = show ? GetShownPosition() : GetHiddenPosition();
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
			if (show)
			{
				// animation
				while (showInterpolation < 1)
				{
					showInterpolation += 1f / showDuration * Time.deltaTime;
					rectTransform.anchoredPosition = Vector2.Lerp(GetHiddenPosition(), GetShownPosition(), showInterpolation);
					yield return null;
				}

				// finalize apparences
				showInterpolation = 1;
				rectTransform.anchoredPosition = GetShownPosition();
			}
			else
			{
				// animation
				while (showInterpolation > 0)
				{
					showInterpolation -= 1f / showDuration * Time.deltaTime;
					rectTransform.anchoredPosition = Vector2.Lerp(GetHiddenPosition(), GetShownPosition(), showInterpolation);
					yield return null;
				}

				// finalize apparences
				showInterpolation = 0;
				rectTransform.anchoredPosition = GetHiddenPosition();
			}

			// finalize values
			this.show = show;
			showCoroutine = null;
		}

		/// <summary>
		/// Retrieve the position of the window when it is not showing.
		/// </summary>
		protected Vector2 GetHiddenPosition()
		{
			switch (dockedSide)
			{
				case Side.Left:
					return new Vector2(rectTransform.rect.width * -1, rectTransform.anchoredPosition.y);
				case Side.Right:
					return new Vector2(rectTransform.rect.width, rectTransform.anchoredPosition.y);
				case Side.Top:
					return new Vector2(rectTransform.anchoredPosition.x, rectTransform.rect.height);
				case Side.Bottom:
					return new Vector2(rectTransform.anchoredPosition.x, rectTransform.rect.height * -1f);
				default:
					return Vector2.zero;
			}
		}

		/// <summary>
		/// Retrieve the position of the window when it is showing.
		/// </summary>
		protected Vector2 GetShownPosition()
		{
			switch (dockedSide)
			{
				case Side.Left:
					return new Vector2(0, rectTransform.anchoredPosition.y);
				case Side.Right:
					return new Vector2(0, rectTransform.anchoredPosition.y);
				case Side.Top:
					return new Vector2(rectTransform.anchoredPosition.x, 0);
				case Side.Bottom:
					return new Vector2(rectTransform.anchoredPosition.x, 0);
				default:
					return Vector2.zero;
			}
		}
	}
}