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








		// ========================================================= Message From External (Drive) =========================================================

		/// <summary>
		/// Perform an monobehaviour update, should be driven by another monobehaviour as this object is not one.
		/// </summary>
		public virtual void Update()
		{
			DetectHover();
			DetectPress();
			DetectDrag();
		}

		// ========================================================= Message From External (Input) =========================================================

		/// <summary>
		/// An OnMouseEnter triggered from UI.
		/// </summary>
		public void OnUIMouseEnter()
		{
			_IsUIHovering = true;
		}

		/// <summary>
		/// An OnMouseExit triggered from UI.
		/// </summary>
		public void OnUIMouseExit()
		{
			_IsUIHovering = false;
		}

		/// <summary>
		/// An OnMouseDown triggered from UI.
		/// </summary>
		public void OnUIMouseDown(int mouseButton)
		{
			_StartedUIPress[mouseButton] = true;
			_StartedUIDrag[mouseButton] = true;
			_LastMousePosition[mouseButton] = Input.mousePosition;
		}

		/// <summary>
		/// An OnMouseUp triggered from UI.
		/// </summary>
		public void OnUIMouseUp(int mouseButton)
		{
			if (_StartedUIPress[mouseButton] && Vector2.Distance(_LastMousePosition[mouseButton], Input.mousePosition) < 2f)
			{
				_StartedUIPress[mouseButton] = false;
				_StartedUIDrag[mouseButton] = false;
				_CompletedUIPress[mouseButton] = true;
				_LastMousePosition[mouseButton] = Vector2.negativeInfinity;
			}
		}

		// ========================================================= Message From External (AI) =========================================================

		/// <summary>
		/// An OnMouseEnter triggered from AI.
		/// </summary>
		public void OnAIMouseEnter()
		{
			_IsAIHovering = true;
		}

		/// <summary>
		/// An OnMouseExit triggered from AI.
		/// </summary>
		public void OnAIMouseExit()
		{
			_IsAIHovering = false;
		}

		/// <summary>
		/// An OnMousePress triggered from AI.
		/// </summary>
		public void OnAIMousePress(int mouseButton)
		{
			_CompletedAIPress[mouseButton] = true;
		}

		/// <summary>
		/// An OnMouseStartDrag triggered from AI.
		/// </summary>
		public void OnAIMouseStartDrag(int mouseButton)
		{
			_StartedAIDrag[mouseButton] = true;
		}

		/// <summary>
		/// An OnMouseCompleteDrag triggered from AI.
		/// </summary>
		public void OnAIMouseCompletetDrag(int mouseButton)
		{
			_CompletedAIDrag[mouseButton] = true;
		}

		// ========================================================= Properties (IsHovering) =========================================================

		/// <summary>
		/// Flag for if user is hovering on this item component by any means.
		/// </summary>
		protected bool IsHovering { get; private set; } = false;
		private bool _IsUIHovering = false;
		private bool _IsAIHovering = false;

		/// <summary>
		/// Detect hover events.
		/// </summary>
		private void DetectHover()
		{
			if (!game.IsAITurn)
			{
				// player inputs
				IsHovering = _IsUIHovering;
			}
			else
			{
				// ai inputs
				IsHovering = _IsAIHovering;
			}
		}

		// ========================================================= Properties (IsPressed) =========================================================

		/// <summary>
		/// Flag for if user has pressed on this item component by any means.
		/// </summary>
		protected bool[] IsPressed { get; private set; } = new bool[] { false, false, false };
		private Vector2[] _LastMousePosition = new Vector2[] { Vector2.negativeInfinity, Vector2.negativeInfinity, Vector2.negativeInfinity };
		private bool[] _StartedUIPress = new bool[] { false, false, false };
		private bool[] _CompletedUIPress = new bool[] { false, false, false };
		private bool[] _CompletedAIPress = new bool[] { false, false, false };

		/// <summary>
		/// Detect press event and trim to a single frame flag.
		/// </summary>
		private void DetectPress()
		{
			if (!game.IsAITurn)
			{
				// player inputs
				for (int i = 0; i < 3; i++)
				{
					IsPressed[i] = false;
					if (_CompletedUIPress[i])
					{
						_CompletedUIPress[i] = false;
						IsPressed[i] = true;
					}
				}
			}
			else
			{
				// ai inputs
				for (int i = 0; i < 3; i++)
				{
					IsPressed[i] = false;
					if (_CompletedAIPress[i])
					{
						_CompletedAIPress[i] = false;

						IsPressed[i] = true;
					}
				}
			}
		}

		// ========================================================= Properties (IsStartedDrag & IsCompletedDrag) =========================================================

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
		private bool[] _StartedUIDrag = new bool[] { false, false, false };
		private bool[] _StartedAIDrag = new bool[] { false, false, false };
		private bool[] _CompletedAIDrag = new bool[] { false, false, false };

		/// <summary>
		/// Detect draga event and trim to a single frame flag.
		/// </summary>
		private void DetectDrag()
		{

			if (!game.IsAITurn)
			{
				// player inputs
				for (int i = 0; i < 3; i++)
				{
					IsStartedDrag[i] = false;
					if (_StartedUIDrag[i] && Vector2.Distance(_LastMousePosition[i], Input.mousePosition) >= 2f)
					{
						_StartedUIPress[i] = false;
						_StartedUIDrag[i] = false;

						IsStartedDrag[i] = true;
					}

					IsCompletedDrag[i] = false;
					if (Input.GetMouseButtonUp(i))
					{
						IsCompletedDrag[i] = true;
					}
				}
			}
			else
			{
				// ai inputs
				for (int i = 0; i < 3; i++)
				{
					IsStartedDrag[i] = false;
					if (_StartedAIDrag[i])
					{
						_StartedAIDrag[i] = false;

						IsStartedDrag[i] = true;
					}

					IsCompletedDrag[i] = false;
					if (_CompletedAIDrag[i])
					{
						_CompletedAIDrag[i] = false;

						IsCompletedDrag[i] = true;
					}
				}
			}

		}
	}
}