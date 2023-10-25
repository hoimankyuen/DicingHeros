using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
	public abstract class Equipment : ItemComponent
	{
		public enum EquipmentType
		{
			MovementBuff,
			MeleeAttack,
			MagicAttack,
			MeleeSelfBuff,
			MagicSelfBuff,
			DefenceSelfBuff,
			RetainingSelfBuff,
			SelfHeal,
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

		/// <summary>
		/// Retrieve the first equipment being currently inspected, return null if none is being inspected.
		/// </summary>
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
		/// Clear all assigned dice of all slots.
		/// </summary>
		public void ClearAssignedDie()
		{
			foreach (EquipmentDieSlot dieSlot in DieSlots)
			{
				dieSlot.AssignDie(null, null);
			}
		}

		// ========================================================= Properties (Information) =========================================================

		/// <summary>
		/// The name of this equipment.
		/// </summary>
		public abstract EquipmentDictionary.Name EquipmentName { get; }

		/// <summary>
		/// What type this equipment belongs to.
		/// </summary>
		public abstract EquipmentType Type { get; }

		/// <summary>
		/// The name to be displayed to the player.
		/// </summary>
		public abstract string DisplayableName { get; }

		/// <summary>
		/// The effect discription to be displayed to the player.
		/// </summary>
		public abstract string DisplayableEffectDiscription { get; }

		// ========================================================= Properties (Effect) =========================================================

		/// <summary>
		/// The change in the melee value when this equipment is activated.
		/// </summary>
		public virtual int MeleeDelta { get; } = 0;

		/// <summary>
		/// The change in the magic value when this equipment is activated.
		/// </summary>
		public virtual int MagicDelta { get; } = 0;

		/// <summary>
		/// The change in the defence value when this equipment is activated.
		/// </summary>
		public virtual int DefenceDelta { get; } = 0;

		/// <summary>
		/// The change in the movement value when this equipment is activated.
		/// </summary>
		public virtual int MovementDelta { get; } = 0;

		/// <summary>
		/// The change in the melee range value when this equipment is activated.
		/// </summary>
		public virtual int MeleeRangeDelta { get; } = 0;

		/// <summary>
		/// The change in the magic range value when this equipment is activated.
		/// </summary>
		public virtual int MagicRangeDelta { get; } = 0;


		/// <summary>
		/// The change in the attack range value when this equipment is activated.
		/// </summary>
		public virtual float KnockbackForceDelta { get; } = 0;

		/// <summary>
		/// The attack area rule used when this equipment is activated.
		/// </summary>
		public virtual AttackAreaRule AreaRule { get; } = AttackAreaRule.Adjacent;

		/// <summary>
		/// Forward implementation of the other effects of this equipment.
		/// </summary>
		protected virtual void AddOtherEffects() { }

		/// <summary>
		/// Backward implementation of the other effects of this equipment.
		/// </summary>
		protected virtual void RemoveOtherEffects() { }

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
				if (fulfilled && !IsActivated)
				{
					// activate at the moment when this equipment is first fulfilled
					IsActivated = true;
				}
				else if (!fulfilled && IsActivated)
				{
					// deactivate when equipment is not fulfilled
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
					if (value)
					{
						// deactivate all other incompatible equipments first before activating self
						if (Type == EquipmentType.MovementBuff)
						{
							foreach (Equipment equipment in Unit.Equipments)
							{
								if (equipment != this && equipment.Type == EquipmentType.MovementBuff && equipment.IsActivated)
								{
									equipment.IsActivated = false;
								}
							}
						}
						else if (Type == EquipmentType.MeleeAttack || Type == EquipmentType.MagicAttack)
						{
							foreach (Equipment equipment in Unit.Equipments)
							{
								if (equipment != this && (equipment.Type == EquipmentType.MeleeAttack || equipment.Type == EquipmentType.MagicAttack) && equipment.IsActivated)
								{
									equipment.IsActivated = false;
								}
							}
						}
					}
					_IsActivated = value;

					// effects
					if (value)
					{
						AddEffects();
					}
					else
					{
						RemoveEffects();
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
		/// Apply the effect of this equipment and expend all dice assigned.
		/// </summary>
		public void ConsumeDie()
		{
			if (IsActivated)
			{
				foreach (EquipmentDieSlot dieSlot in DieSlots)
				{
					dieSlot.Die.Expend();
					dieSlot.AssignDie(null, null);
				}
				IsActivated = false;
			}
		}

		// ========================================================= Effect activation =========================================================

		/// <summary>
		/// Apply all effects of this equipment.
		/// </summary>
		private void AddEffects()
		{
			Unit.ChangeStat(
				meleeDelta: MeleeDelta,
				magicDelta: MagicDelta, 
				defenceDelta: DefenceDelta,
				movementDelta: MovementDelta,
				meleeRangeDelta: MeleeRangeDelta,
				magicRangeDelta: MagicRangeDelta,
				knockbackForceDelta: KnockbackForceDelta);
			if (AreaRule != AttackAreaRule.Adjacent)
			{
				Unit.ChangeAttackAreaRule(AreaRule);
			}
			if (Type == EquipmentType.MagicAttack)
			{
				Unit.ChangeAttackType(Unit.AttackType.Magical);
			}
			AddOtherEffects();
		}

		/// <summary>
		/// Remove all effects of this equipment.
		/// </summary>
		private void RemoveEffects()
		{
			Unit.ChangeStat(
				meleeDelta: -MeleeDelta,
				magicDelta: -MagicDelta,
				defenceDelta: -DefenceDelta,
				movementDelta: -MovementDelta,
				meleeRangeDelta: -MeleeRangeDelta,
				magicRangeDelta: -MagicRangeDelta,
				knockbackForceDelta: -KnockbackForceDelta);
			if (AreaRule != AttackAreaRule.Adjacent)
			{
				Unit.ResetAttackAreaRule();
			}
			if (Type == EquipmentType.MagicAttack)
			{
				Unit.ChangeAttackType(Unit.AttackType.Physical);
			}
			RemoveOtherEffects();
		}

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
			private bool isUserSelectedAtEnter = false;
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
				isUserSelectedAtEnter = self.Unit.IsSelected;

				if (isUserSelectedAtEnter)
				{
					if (stateMachine.State == SMState.UnitMoveSelect && self.Type == EquipmentType.MovementBuff)
					{

					}
					else if (stateMachine.State == SMState.UnitMoveSelect &&  (self.Type == EquipmentType.MeleeAttack || self.Type == EquipmentType.MagicAttack))
					{

					}
				}
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				if (isUserSelectedAtEnter)
				{
					// inspection
					if (CacheUtils.HasValueChanged(self.IsHovering, ref lastIsHovering))
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
				if (isUserSelectedAtEnter)
				{
					// inspection
					if (lastIsHovering)
					{
						self.IsBeingInspected = false;
					}

					// reset cache
					CacheUtils.ResetValueCache(ref lastIsHovering);
				}
			}
		}
	}
}