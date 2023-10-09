using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
    public abstract class Equipment : ItemComponent
    {
        // working variables
        protected bool activated = false;

        // events
        public event Action onDieChanged = () => { };

        // ========================================================= Properties =========================================================

        /// <summary>
        /// The unit that owns this equipment.
        /// </summary>
        public Unit Unit { get; private set; } = null;

        /// <summary>
        /// The list of all slots and their requiremnts.
        /// </summary>
        public abstract IReadOnlyList<EquipmentDieSlot> DieSlots { get; }

        /// <summary>
        /// Flag for if this equipment has all its requirements fulfilled.
        /// </summary>
		public bool IsRequirementFulfilled { get; private set; } = false;

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
                dieSlot.onDieChanged += IndividualDieChanged;
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
                dieSlot.onDieChanged -= IndividualDieChanged;
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

        // ========================================================= Functionality =========================================================

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
                CheckAllSlotFulfilled();
            }
        }

        /// <summary>
        /// Notify that a die slot has its die changed.
        /// </summary>
        private void IndividualDieChanged()
        {
            IsRequirementFulfilled = CheckAllSlotFulfilled();
            onDieChanged.Invoke();
        }

        /// <summary>
        /// Check if the requirement of this dice is fulfilled.
        /// </summary>
        public bool CheckAllSlotFulfilled()
        {
            bool fulfilled = true;
            foreach (EquipmentDieSlot dieSlot in DieSlots)
            {
                fulfilled &= dieSlot.IsRequirementFulfilled;
            }
            return fulfilled;
        }

        /// <summary>
        /// Activate the effect of this equipment.
        /// </summary>
        public void Activate()
        {
            if (!activated)
            {
                AddEffect();
                activated = true;
            }
        }

        /// <summary>
        /// Deactivate the effect of this equipment.
        /// </summary>
        public void Deactivate()
        {
            if (activated)
            {
                RemoveEffect();
                activated = false;
            }
        }

        // ========================================================= Abstract Functionalities =========================================================

        /// <summary>
        /// Fill in all die slots. This will be called in the constructor.
        /// </summary>
        protected abstract void FillDieSlots();

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
            stateMachine.Register(Unit.gameObject, this, State.UnitMoveSelect, new UnitActionSelectSB(this));
            stateMachine.Register(Unit.gameObject, this, State.UnitAttackSelect, new UnitActionSelectSB(this));
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
            }

            /// <summary>
            /// OnStateExit is called when the centralized state machine is leaving the current state.
            /// </summary>
            public override void OnStateExit()
            {
            }
        }
    }
}