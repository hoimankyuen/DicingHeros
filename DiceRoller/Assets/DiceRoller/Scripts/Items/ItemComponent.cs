using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public abstract class ItemComponent
	{
		// reference
		protected GameController game => GameController.current;
		protected StateMachine stateMachine => StateMachine.current;

		// ========================================================= Properties =========================================================

		/// <summary>
		/// Flag for if user is hovering on this item component by any means.
		/// </summary>
		protected bool IsHovering { get; private set; } = false;
		private bool isUIHovering = false;

		/// <summary>
		/// Flag for if user has started drag on this item component by any means.
		/// </summary>
		protected bool[] IsStartedDrag { get; private set; } = new bool[] { false, false, false };
		/// <summary>
		/// Flag for if user has completed drag started by this item component by any means.
		/// </summary>
		protected bool[] IsCompletedDrag { get; private set; } = new bool[] { false, false, false };
		/// <summary>
		/// Flag for if user has ended drag on this item component by any means.
		/// </summary>
		private bool[] startedUIDrag = new bool[] { false, false, false };
		private Vector2[] lastMousePosition = new Vector2[] { Vector2.negativeInfinity, Vector2.negativeInfinity, Vector2.negativeInfinity };

		/// <summary>
		/// Flag for if user has pressed on this item component by any means.
		/// </summary>
		protected bool[] IsPressed { get; private set; } = new bool[] { false, false, false };
		private bool[] startedUIPress = new bool[] { false, false, false };
		private bool[] completedUIPress = new bool[] { false, false, false };

		// ========================================================= Message From External =========================================================

		/// <summary>
		/// An OnMouseEnter triggered from UI.
		/// </summary>
		public void OnUIMouseEnter()
		{
			isUIHovering = true;
		}

		/// <summary>
		/// An OnMouseExit triggered from UI.
		/// </summary>
		public void OnUIMouseExit()
		{
			isUIHovering = false;
		}

		/// <summary>
		/// An OnMouseDown triggered from UI.
		/// </summary>
		public void OnUIMouseDown(int mouseButton)
		{
			startedUIPress[mouseButton] = true;
			startedUIDrag[mouseButton] = true;
			lastMousePosition[mouseButton] = Input.mousePosition;
		}

		/// <summary>
		/// An OnMouseUp triggered from UI.
		/// </summary>
		public void OnUIMouseUp(int mouseButton)
		{
			if (startedUIPress[mouseButton] && Vector2.Distance(lastMousePosition[mouseButton], Input.mousePosition) < 2f)
			{
				startedUIPress[mouseButton] = false;
				startedUIDrag[mouseButton] = false;
				completedUIPress[mouseButton] = true;
				lastMousePosition[mouseButton] = Vector2.negativeInfinity;
			}
		}


		/// <summary>
		/// Perform an monobehaviour update, should be driven by another monobehaviour as this object is not one.
		/// </summary>
		public virtual void Update()
		{
			DetectHover();
			DetectPress();
			DetectDrag();
		}

		// ========================================================= Input Interpetation =========================================================

		/// <summary>
		/// Detect hover events.
		/// </summary>
		private void DetectHover()
		{
			IsHovering = isUIHovering;
		}

		/// <summary>
		/// Detect press event and trim to a single frame flag.
		/// </summary>
		private void DetectPress()
		{
			for (int i = 0; i < 3; i++)
			{
				IsPressed[i] = false;
				if (completedUIPress[i])
				{
					completedUIPress[i] = false;
					IsPressed[i] = true;
				}
			}
		}

		/// <summary>
		/// Detect draga event and trim to a single frame flag.
		/// </summary>
		private void DetectDrag()
		{
			for (int i = 0; i < 3; i++)
			{
				IsStartedDrag[i] = false;
				if (startedUIDrag[i] && Vector2.Distance(lastMousePosition[i], Input.mousePosition) >= 2f)
				{
					startedUIPress[i] = false;
					startedUIDrag[i] = false;

					IsStartedDrag[i] = true;
				}

				IsCompletedDrag[i] = false;
				if (Input.GetMouseButtonUp(i))
				{
					IsCompletedDrag[i] = true;
				}
			}
		}
	}
}