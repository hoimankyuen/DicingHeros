using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public class EquipmentDieSlot : ItemComponent
	{
		public Die.Type dieType;
		public Requirement requirement;
		public int parameter;
		public enum Requirement
		{
			None,
			GreaterThan,
			GreaterThanEqual,
			LesserThan,
			LesserThanEqual,
			Equals,
			NotEquals,
			IsEven,
			IsOdd,
		}

		// events
		public event Action onDieChanged = () => { };
		public event Action onFulfillmentChanged = () => { };

		// ========================================================= Properties =========================================================

		/// <summary>
		/// The equipment that owns this die slot.
		/// </summary>
		public Equipment Equipment { get; private set; } = null;

		/// <summary>
		/// The die that was assigned to this die slot.
		/// </summary>
		public Die Die { get; private set; } = null;

		/// <summary>
		/// Flag for if this a drag ended here at this die slot.
		/// </summary>
		public bool IsDragRecipient
		{
			get 
			{
				return dragEndSlots.Contains(this);
			}
		}
		public static UniqueList<EquipmentDieSlot> dragEndSlots = new UniqueList<EquipmentDieSlot>();

		/// <summary>
		/// Flag for if this equipment has all its requirements fulfilled.
		/// </summary>
		public bool IsRequirementFulfilled { get; private set; } = false;

		// ========================================================= Constructor =========================================================

		/// <summary>
		/// Constructor.
		/// </summary>
		public EquipmentDieSlot(Equipment equipment, Die.Type dieType, Requirement requirement, int parameter = 0)
		{
			this.Equipment = equipment;
			this.dieType = dieType;
			this.requirement = requirement;
			this.parameter = parameter;

			RegisterStateBehaviours();
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~EquipmentDieSlot()
		{
			DeregisterStateBehaviours();

			if (this.Die != null)
				this.Die.onValueChanged -= RefreshFulfillment;
		}

		// ========================================================= Message From External =========================================================

		/// <summary>
		/// Perform an monobehaviour update, should be driven by another monobehaviour as this object is not one.
		/// </summary>
		public override void Update()
		{
			base.Update();
		}

		// ========================================================= Behaviour =========================================================

		/// <summary>
		/// Assign an equipment to this die slot to be the owner of this die slot.
		/// </summary>
		public void AsignOwnership(Equipment equipment)
		{
			this.Equipment = equipment;
		}

		/// <summary>
		/// Check if a die can fulfill the requirement of this die slot.
		/// </summary>
		public bool IsFulfillBy(Die die)
		{
			if (die == null)
				return false;
			if (dieType != Die.Type.Unknown && die.type != dieType)
				return false;
			switch (requirement)
			{
				case Requirement.None:
					return true;
				case Requirement.GreaterThan:
					return die.Value > parameter;
				case Requirement.GreaterThanEqual:
					return die.Value >= parameter;
				case Requirement.LesserThan:
					return die.Value < parameter;
				case Requirement.LesserThanEqual:
					return die.Value <= parameter;
				case Requirement.Equals:
					return die.Value == parameter;
				case Requirement.NotEquals:
					return die.Value != parameter;
				case Requirement.IsEven:
					return die.Value % 2 == 0;
				case Requirement.IsOdd:
					return die.Value % 2 == 1;
				default:
					return true;
			}
		}

		/// <summary>
		/// Assign a die to this equipment die slot.
		/// </summary>
		public void AssignDie(Die die)
		{
			if (this.Die != die)
			{
				// register event callbacks
				if (this.Die != null)
					this.Die.onValueChanged -= RefreshFulfillment;
				if (die != null)
					die.onValueChanged += RefreshFulfillment;

				// change value
				this.Die = die;
				RefreshFulfillment();

				onDieChanged.Invoke();
			}
		}

		/// <summary>
		/// Updates the value of requirement fulfillment and trigger callback if needed.
		/// </summary>
		private void RefreshFulfillment()
		{
			if (IsRequirementFulfilled != IsFulfillBy(Die))
			{
				IsRequirementFulfilled = IsFulfillBy(Die);
				onFulfillmentChanged.Invoke();
			}
		}

		/// <summary>
		/// Retrieve the first equipment that is a recipient of a drag action, return null if none is.
		/// </summary>
		/// <returns></returns>
		public static EquipmentDieSlot GetFirstDragsRecipient()
		{
			return dragEndSlots.Count > 0 ? dragEndSlots[0] : null;
		}

		/// <summary>
		/// Add this die slot to as a recipient of a drag action.
		/// </summary>
		private void AddToDragRecipient()
		{
			if (!dragEndSlots.Contains(this))
			{
				dragEndSlots.Add(this);
			}
		}

		/// <summary>
		/// Remove this die slot from as a recipient of a drag action.
		/// </summary>
		private void RemoveFromDragRecipient()
		{
			if (dragEndSlots.Contains(this))
			{
				dragEndSlots.Remove(this);
			}
		}

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.Register(Equipment.Unit.gameObject, this, SMState.UnitMoveSelect, new UnitActionSelectSB(this));
			stateMachine.Register(Equipment.Unit.gameObject, this, SMState.UnitAttackSelect, new UnitActionSelectSB(this));
			stateMachine.Register(Equipment.Unit.gameObject, this, SMState.UnitDepletedSelect, new UnitActionSelectSB(this));

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
			private readonly EquipmentDieSlot self = null;

			// caches
			private bool isEquipmentUnitSelectedAtEnter = false;
			private bool lastIsDragRecipient = false;

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitActionSelectSB(EquipmentDieSlot self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				isEquipmentUnitSelectedAtEnter = self.Equipment.Unit.IsSelected;
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				if (isEquipmentUnitSelectedAtEnter)
				{
					if (CachedValueUtils.HasValueChanged(self.IsHovering && InputUtils.IsDragging, ref lastIsDragRecipient))
					{
						if (self.IsHovering && InputUtils.IsDragging)
						{
							self.AddToDragRecipient();
						}
						else
						{
							self.RemoveFromDragRecipient();
						}
					}

					if (self.IsPressed[1])
					{
						if (self.Die != null)
						{
							self.Die.ResignFromCurrentSlot();
						}
					}
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				if (isEquipmentUnitSelectedAtEnter)
				{
					if (lastIsDragRecipient)
					{
						self.RemoveFromDragRecipient();
					}

					// reset cache
					CachedValueUtils.ResetValueCache(ref lastIsDragRecipient);
				}
			}
		}
	}
}