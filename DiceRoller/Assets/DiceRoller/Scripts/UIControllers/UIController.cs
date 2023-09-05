using System.Collections;
using System.Collections.Generic;
using SimpleMaskCutoff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceRoller
{
	public class UIController : MonoBehaviour
	{
		// singleton
		public static UIController current { get; protected set; }

		// references
		protected StateMachine stateMachine { get { return StateMachine.current; } }

		[Header("Components")]
		public UIDiceListWindow diceListWindow;
		public UIUnitListWindow unitListWindow;
		public UIUnitDetailWindow unitDetailWindow;

		[Header("Simple Die Display")]
		public RectTransform simpleDieWindow;
		public UIDie simpleDieDisplay;
		protected bool selectableDisplayDirty = false;

		[Header("Simple Unit Display")]
		public RectTransform simpleUnitWindow;
		public Image simpleUnitImage;
		public UIHealthDisplay simpleHealthDisplay;
		public UIStatDisplay simpleStatDisplay;

		[Header("Throw Display")]
		public GameObject throwTarget = null;
		public SpriteRenderer throwArrow = null;
		public SpriteRenderer throwCross = null;
		public GameObject throwPowerIndicator = null;
		public CutoffSpriteRenderer throwPowerIndicatorCutoff = null;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected void Awake()
		{
			current = this;
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected void Start()
		{
			RegisterStateBehaviours();
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected void Update()
		{
			RefreshSelectableDisplay();
			UpdateThrowDisplay();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected void OnDestroy()
		{
			DeregisterStateBehaviours();
			current = null;
		}

		// ========================================================= Selectable Display =========================================================

		/// <summary>
		/// Change the dice display to reflect the current selected die.
		/// </summary>
		protected void RefreshSelectableDisplay()
		{
			// display selectable information
			if (Unit.InspectingUnit != null && Unit.InspectingUnit.Count > 0)
			{
				// show unit information
				simpleUnitWindow.gameObject.SetActive(true);
				simpleUnitImage.sprite = Unit.InspectingUnit[0].iconSprite;
				simpleHealthDisplay.SetDisplay(Unit.InspectingUnit[0]);
				simpleStatDisplay.SetDisplay(Unit.InspectingUnit[0]);

				simpleDieDisplay.SetDisplay(null);
				simpleDieWindow.gameObject.SetActive(false);

			}
			else if (Die.InspectingDice != null && Die.InspectingDice.Count > 0 && Die.InspectingDice[0].Value != -1)
			{
				// show dice information
				simpleUnitWindow.gameObject.SetActive(false);

				simpleDieWindow.gameObject.SetActive(true);
				simpleDieDisplay.SetDisplay(Die.InspectingDice[0]);
			}
			else
			{
				// show nothing selected
				simpleUnitWindow.gameObject.SetActive(false);

				simpleDieWindow.gameObject.SetActive(true);
				simpleDieDisplay.SetDisplay(Die.Type.Unknown, -1);
			}
		}

		// ========================================================= Throw Display =========================================================

		/// <summary>
		/// Update the apparence of the throw indicator UI.
		/// </summary>
		protected void UpdateThrowDisplay()
		{
			if (DiceThrower.current.ThrowDragging)
			{
				// user throwing
				throwTarget.SetActive(true);
				throwPowerIndicator.SetActive(true);
				throwTarget.transform.position = DiceThrower.current.ThrowDragPosition;
				throwPowerIndicator.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0)) * Quaternion.FromToRotation(Vector3.forward, DiceThrower.current.ThrowDirection) * Quaternion.Euler(new Vector3(90, 0, 0));
				if (DiceThrower.current.ThrowPower != -1f)
				{
					// throw have enough power
					throwArrow.gameObject.SetActive(true);
					throwCross.gameObject.SetActive(false);
					throwPowerIndicatorCutoff.CutoffTo(DiceThrower.current.ThrowPower);
				}
				else
				{
					// throw does not have enough power
					throwArrow.gameObject.SetActive(false);
					throwCross.gameObject.SetActive(true);
					throwPowerIndicatorCutoff.CutoffTo(0);
				}
			}
			else
			{
				// user not throwing, disable throw indicator
				throwTarget.SetActive(false);
				throwPowerIndicator.SetActive(false);
			}
		}

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.RegisterStateBehaviour(this, State.StartTurn, new StartTurnSB(this));
			stateMachine.RegisterStateBehaviour(this, State.Navigation, new NavigationSB(this));
			stateMachine.RegisterStateBehaviour(this, State.UnitActionSelect, new UnitActionSB(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		protected void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
				stateMachine.DeregisterStateBehaviour(this);
		}

		// ========================================================= Start Turn State =========================================================

		protected class StartTurnSB : StateBehaviour
		{
			protected readonly UIController self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public StartTurnSB(UIController self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
			}
		}

		// ========================================================= Navigation State =========================================================

		protected class NavigationSB : StateBehaviour
		{
			protected readonly UIController self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public NavigationSB(UIController self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				self.diceListWindow.Show = true;
				self.unitListWindow.Show = true;
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				self.diceListWindow.Show = false;
				self.unitListWindow.Show = false;
			}
		}

		// ========================================================= Unit Action State =========================================================

		protected class UnitActionSB : StateBehaviour
		{
			protected readonly UIController self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitActionSB(UIController self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				self.unitDetailWindow.Show = true;
				self.diceListWindow.Show = true;
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				self.unitDetailWindow.Show = false;
				self.diceListWindow.Show = false;
			}
		}
	}
}