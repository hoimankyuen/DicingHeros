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
		private StateMachine stateMachine { get { return StateMachine.current; } }

		[Header("Screen Space Components (Top)")]
		public UIInfoWindow infoWindow;

		[Header("Screen Space Components (Bottom)")]
		public UIGeneralControlWindow generalControlWindow;
		public UIUnitControlWindow unitControlWindow;
		public UIInspectionControlWindow inspectionControlWindow;

		[Header("Screen Space Components (Left)")]
		public UIUnitListWindow unitListWindow;
		public UIUnitDetailWindow unitDetailSelfWindow;

		[Header("Screen Space Components (Right)")]
		public UIUnitDetailWindow unitDetailOtherWindow;
		public UIDiceListWindow diceListWindow;
		public UIDiceDetailWindow diceDetailWindow;

		[Header("Screen Space Components (Center)")]
		public UIPrompt prompt;

		[Header("Screen Space Components (Floating)")]
		public UIInspectedItemWindow inspectedItemWindow;
		public UICursor cursor;
		public UIUnitIndicator unitIndicator;

		[Header("World Space Components")]
		public UIThrowDisplay throwDisplay;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		private void Awake()
		{
			current = this;
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		private void Start()
		{
			RegisterStateBehaviours();
			HideAllWindows();
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
			DeregisterStateBehaviours();
			current = null;
		}

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		private void RegisterStateBehaviours()
		{
			stateMachine.Register(gameObject, this, SMState.StartTurn, new StartTurnSB(this));
			stateMachine.Register(gameObject, this, SMState.Navigation, new NavigationSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitMoveSelect, new UnitMoveSelectSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitAttackSelect, new UnitAttackSelectSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitDepletedSelect, new UnitDepletedSelectSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitInspection, new UnitInspectionSB(this));
			stateMachine.Register(gameObject, this, SMState.DiceActionSelect, new DiceActionSB(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		private void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
				stateMachine.DeregisterAll(this);
		}

		// ========================================================= General Behaviour =========================================================

		/// <summary>
		/// Hide all windows in the uI.
		/// </summary>
		private void HideAllWindows()
		{
			infoWindow.Show = false;
			generalControlWindow.Show = false;
			unitControlWindow.Show = false;
			inspectedItemWindow.Show = false;
			diceListWindow.Show = false;
			unitListWindow.Show = false;
			diceDetailWindow.Show = false;
			unitDetailSelfWindow.Show = false;
			unitDetailOtherWindow.Show = false;
		}

		// ========================================================= Start Turn State =========================================================

		private class StartTurnSB : StateBehaviour
		{
			private readonly UIController self = null;

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

		private class NavigationSB : StateBehaviour
		{
			private readonly UIController self = null;

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
				self.infoWindow.Show = true;
				self.inspectedItemWindow.Show = true;
				self.diceListWindow.Show = true;
				self.unitListWindow.Show = true;
				self.generalControlWindow.Show = true;
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
				self.infoWindow.Show = false;
				self.inspectedItemWindow.Show = false;
				self.diceListWindow.Show = false;
				self.unitListWindow.Show = false;
				self.generalControlWindow.Show = false;
			}
		}

		// ========================================================= Unit Move Select State =========================================================

		private class UnitMoveSelectSB : StateBehaviour
		{
			private readonly UIController self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitMoveSelectSB(UIController self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				self.infoWindow.Show = true;
				self.inspectedItemWindow.Show = true;
				self.unitDetailSelfWindow.Show = true;
				self.diceListWindow.Show = true;
				self.unitControlWindow.Show = true;

				self.cursor.SetIcon(UICursor.IconType.SimpleMovement);
				//self.targetIndicator.Setup(UITargetIndicator.Mode.Mouse, UICursor.IconType.SimpleMovement);
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
				self.infoWindow.Show = false;
				self.inspectedItemWindow.Show = false;
				self.unitDetailSelfWindow.Show = false;
				self.diceListWindow.Show = false;
				self.unitControlWindow.Show = false;

				self.cursor.SetIcon(UICursor.IconType.None);
				//self.targetIndicator.Setup(UITargetIndicator.Mode.None, UICursor.IconType.None);
			}
		}

		// ========================================================= Unit Attack Select State =========================================================

		private class UnitAttackSelectSB : StateBehaviour
		{
			private readonly UIController self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitAttackSelectSB(UIController self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				self.infoWindow.Show = true;
				self.inspectedItemWindow.Show = true;
				self.unitDetailSelfWindow.Show = true;
				self.diceListWindow.Show = true;
				self.unitControlWindow.Show = true;

				self.cursor.SetIcon(UICursor.IconType.SimpleMelee);
				//self.targetIndicator.Setup(UITargetIndicator.Mode.Mouse, UICursor.IconType.SimpleMelee);
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
				self.infoWindow.Show = false;
				self.inspectedItemWindow.Show = false;
				self.unitDetailSelfWindow.Show = false;
				self.diceListWindow.Show = false;
				self.unitControlWindow.Show = false;

				self.cursor.SetIcon(UICursor.IconType.None);
				//self.targetIndicator.Setup(UITargetIndicator.Mode.None, UICursor.IconType.None);
			}
		}

		// ========================================================= Unit Depleted Select State =========================================================

		private class UnitDepletedSelectSB : StateBehaviour
		{
			private readonly UIController self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitDepletedSelectSB(UIController self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				self.infoWindow.Show = true;
				self.inspectedItemWindow.Show = true;
				self.unitDetailSelfWindow.Show = true;
				self.diceListWindow.Show = true;
				self.unitControlWindow.Show = true;

				//self.targetIndicator.Setup(UITargetIndicator.Mode.Mouse, UICursor.IconType.SimpleMelee);
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
				self.infoWindow.Show = false;
				self.inspectedItemWindow.Show = false;
				self.unitDetailSelfWindow.Show = false;
				self.diceListWindow.Show = false;
				self.unitControlWindow.Show = false;

				//self.targetIndicator.Setup(UITargetIndicator.Mode.None, UICursor.IconType.None);
			}
		}

		// ========================================================= Unit Depleted Select State =========================================================

		private class UnitInspectionSB : StateBehaviour
		{
			private readonly UIController self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitInspectionSB(UIController self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				self.infoWindow.Show = true;
				self.unitDetailOtherWindow.Show = true;
				self.inspectionControlWindow.Show = true;

				//self.targetIndicator.Setup(UITargetIndicator.Mode.Mouse, UICursor.IconType.SimpleMelee);
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
				self.infoWindow.Show = false;
				self.unitDetailOtherWindow.Show = false;
				self.inspectionControlWindow.Show = false;

				//self.targetIndicator.Setup(UITargetIndicator.Mode.None, UICursor.IconType.None);
			}
		}

		// ========================================================= Dice Action State =========================================================

		private class DiceActionSB : StateBehaviour
		{
			private readonly UIController self = null;

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
				self.infoWindow.Show = true;
				self.inspectedItemWindow.Show = true;
				//self.diceDetailWindow.Show = true;
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
				self.infoWindow.Show = false;
				self.inspectedItemWindow.Show = false;
				//self.diceDetailWindow.Show = false;
				self.diceListWindow.Show = false;
			}
		}
	}
}