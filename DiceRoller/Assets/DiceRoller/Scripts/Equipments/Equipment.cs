using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
	public abstract class Equipment : ItemComponent
	{
		public enum EffectApplyTime
		{
			AtMove,
			AtAttack,
			AtDefend,
			AtActivation,
		}

		// ========================================================= Constructor =========================================================

		/// <summary>
		/// Constructor.+
		/// </summary>
		public Equipment(Unit unit)
		{
			Unit = unit;

			// setup die slots
			FillDieSlots();
			foreach (EquipmentDieSlot dieSlot in DieSlots)
			{
				dieSlot.onFulfillmentChanged += CheckAllSlotFulfilled;
			}

			RegisterStateBehaviours();
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~Equipment()
		{
			// revert die slots
			foreach (EquipmentDieSlot dieSlot in DieSlots)
			{
				dieSlot.onFulfillmentChanged -= CheckAllSlotFulfilled;
			}

			DeregisterStateBehaviours();
		}

		// ========================================================= Message From External =========================================================

		/// <summary>
		/// Perform an monobehaviour update, should be driven by another monobehaviour as this object is not one.
		/// </summary>
		public override void Update()
		{
			base.Update();

			foreach (EquipmentDieSlot dieSlot in DieSlots)
			{
				dieSlot.Update();
			}
		}

		// ========================================================= Properties (IsBeingInspected) =========================================================

		/// <summary>
		/// Flag for if this equipment is currently being inspected.
		/// </summary>
		public bool IsBeingInspected
		{
			get 
			{
				return _InspectingEquipment.Contains(this);
			}
			private set 
			{
				if (!_InspectingEquipment.Contains(this) && value)
				{
					_InspectingEquipment.Add(this);
					OnInspectionChanged.Invoke();
					OnItemBeingInspectedChange.Invoke();
				}
				else if (_InspectingEquipment.Contains(this) && !value)
				{
					_InspectingEquipment.Remove(this);
					OnInspectionChanged.Invoke();
					OnItemBeingInspectedChange.Invoke();
				}
			}
		}
		private static UniqueList<Equipment> _InspectingEquipment = new UniqueList<Equipment>();

		/// <summary>
		/// Event raised when the inspection status of this equipment is changed.
		/// </summary>
		public event Action OnInspectionChanged = () => { };

		/// <summary>
		/// Event raised when the list of equipments being inspected is changed.
		/// </summary>
		public static event Action OnItemBeingInspectedChange = () => { };

		public static Equipment GetFirstBeingInspected()
		{
			return _InspectingEquipment.Count > 0 ? _InspectingEquipment[0] : null;
		}

		// ========================================================= Properties (Unit) =========================================================

		/// <summary>
		/// The unit that owns this equipment.
		/// </summary>
		public Unit Unit { get; private set; } = null;

		// ========================================================= Properties (DieSlots) =========================================================

		/// <summary>
		/// The list of all slots and their requiremnts.
		/// </summary>
		public abstract IReadOnlyList<EquipmentDieSlot> DieSlots { get; }

		/// <summary>
		/// Fill in all die slots. This will be called in the constructor.
		/// </summary>
		protected abstract void FillDieSlots();

		/// <summary>
		/// Assign a die to a specific slot.
		/// </summary>
		public void AssignDie(int slotNo, Die die)
		{
			if (slotNo >= DieSlots.Count)
				return;

			DieSlots[slotNo].AssignDie(die);
		}

		/// <summary>
		/// Clear all assigned dice of all slots.
		/// </summary>
		public void ClearAssignedDie()
		{
			foreach (EquipmentDieSlot dieSlot in DieSlots)
			{
				dieSlot.AssignDie(null);
			}
		}

		// ========================================================= Properties (IsRequirementFulfilled) =========================================================

		/// <summary>
		/// Flag for if this equipment has all its requirements fulfilled.
		/// </summary>
		public bool IsRequirementFulfilled 
		{ 
			get
			{
				return _IsRequirementFulfilled;
			}
			private set
			{
				if (_IsRequirementFulfilled != value)
				{
					_IsRequirementFulfilled = value;
					OnFulfillmentChanged.Invoke();
				}
			}
		} 
		private bool _IsRequirementFulfilled = false;

		/// <summary>
		/// Fmag for if this equipment has its all slots requirement fulfulled.
		/// </summary>
		public event Action OnFulfillmentChanged = () => {};

		/// <summary>
		/// Notify that a die slot has its die changed.
		/// </summary>
		private void CheckAllSlotFulfilled()
		{
			// check if all requirement is fulfilled, activate or deactivate effect when changed
			bool fulfilled = DieSlots.Aggregate(true, (acc, dieSlot) => acc && dieSlot.IsRequirementFulfilled);
			if (IsRequirementFulfilled != fulfilled)
			{
				if (!fulfilled && IsActivated)
				{
					IsActivated = false;
				}
				IsRequirementFulfilled = fulfilled;
			}
		}

		// ========================================================= Properties (Activated) =========================================================

		/// <summary>
		/// Flag for if this equipment is activated.
		/// </summary>
		public bool IsActivated
		{
			get
			{
				return _IsActivated;
			}
			private set
			{
				if (_IsActivated != value)
				{
					_IsActivated = value;
					if (value)
					{
						AddEffect();
					}
					else
					{
						RemoveEffect();
					}
					onActivationChanged.Invoke();
				}
			}
		}
		private bool _IsActivated = false;

		/// <summary>
		/// Event raised when activated on this equipment is changed.
		/// </summary>
		public event Action onActivationChanged = () => { };

		/// <summary>
		/// Apply tje effect of this equipment and expend all dice assigned.
		/// </summary>
		public void Apply()
		{
			if (IsActivated)
			{
				foreach (EquipmentDieSlot dieSlot in DieSlots)
				{
					dieSlot.Die.Expend();
					dieSlot.AssignDie(null);
				}
				IsActivated = false;
			}
		}

		/// <summary>
		/// The time of which should this eqipment apply its effect.
		/// </summary>
		public abstract EffectApplyTime ApplyTime { get; }

		/// <summary>
		/// Forward implementation of the effect of this equipment.
		/// </summary>
		protected abstract void AddEffect();

		/// <summary>
		/// Backward implementation of the effect of this equipment.
		/// </summary>
		protected abstract void RemoveEffect();

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.Register(Unit.gameObject, this, SMState.UnitMoveSelect, new UnitActionSelectSB(this));
			stateMachine.Register(Unit.gameObject, this, SMState.UnitAttackSelect, new UnitActionSelectSB(this));
			stateMachine.Register(Unit.gameObject, this, SMState.UnitDepletedSelect, new UnitActionSelectSB(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		protected void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
			{
				stateMachine.DeregisterAll(this);
			}
		}

		// ========================================================= UnitActionSelectSB State =========================================================

		protected class UnitActionSelectSB : StateBehaviour
		{
			// host reference
			private readonly Equipment self = null;

			// cache
			private bool lastIsHovering = false;

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitActionSelectSB(Equipment self)
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
				if (self.Unit.Player == game.CurrentPlayer)
				{
					// inspection
					if (CachedValueUtils.HasValueChanged(self.IsHovering, ref lastIsHovering))
					{
						self.IsBeingInspected = self.IsHovering;
					}

					// activation
					if (self.IsPressed[0])
					{
						if (!self.IsActivated && self.IsRequirementFulfilled)
						{
							self.IsActivated = true;
						}
						else if (self.IsActivated)
						{
							self.IsActivated = false;
						}
					}
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				if (self.Unit.Player == game.CurrentPlayer)
				{
					// inspection
					if (lastIsHovering)
					{
						self.IsBeingInspected = false;
					}

					// reset cache
					CachedValueUtils.ResetValueCache(ref lastIsHovering);
				}
			}
		}
	}
}