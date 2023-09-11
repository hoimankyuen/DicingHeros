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
		public UIInfoWindow infoWindow;
		public UIControlWindow controlWindow;
		public UIInspectedItemWindow inspectedItemWindow;
		public UIDiceListWindow diceListWindow;
		public UIUnitListWindow unitListWindow;
		public UIDiceDetailWindow diceDetailWindow;
		public UIUnitDetailWindow unitDetailWindow;
		public UIThrowDisplay throwDisplay;

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
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected void OnDestroy()
		{
			DeregisterStateBehaviours();
			current = null;
		}
		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.Register(this, State.StartTurn, new StartTurnSB(this));
			stateMachine.Register(this, State.Navigation, new NavigationSB(this));
			stateMachine.Register(this, State.UnitActionSelect, new UnitActionSB(this));
			stateMachine.Register(this, State.DiceActionSelect, new DiceActionSB(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		protected void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
				stateMachine.DeregisterAll(this);
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
				self.inspectedItemWindow.Show = true;
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
				self.inspectedItemWindow.Show = false;
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
				self.inspectedItemWindow.Show = true;
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
				self.inspectedItemWindow.Show = false;
				self.unitDetailWindow.Show = false;
				self.diceListWindow.Show = false;
			}
		}

		// ========================================================= Unit Action State =========================================================

		protected class DiceActionSB : StateBehaviour
		{
			protected readonly UIController self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public DiceActionSB(UIController self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				self.inspectedItemWindow.Show = true;
				self.diceDetailWindow.Show = true;
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
				self.inspectedItemWindow.Show = false;
				self.diceDetailWindow.Show = false;
				self.diceListWindow.Show = false;
			}
		}
	}
}