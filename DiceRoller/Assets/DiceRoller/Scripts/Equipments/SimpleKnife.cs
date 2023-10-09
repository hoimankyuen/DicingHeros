using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public class SimpleKnife : Equipment
    {
        // ========================================================= Properties =========================================================

        /// <summary>
        /// The list of all slots and their requiremnts.
        /// </summary>
        public override IReadOnlyList<EquipmentDieSlot> DieSlots
        {
            get { return _slotTypes.AsReadOnly(); }
        }
        private List<EquipmentDieSlot> _slotTypes = new List<EquipmentDieSlot>();

        // ========================================================= Constructor =========================================================

        /// <summary>
        /// Construcddtor.
        /// </summary>
        public SimpleKnife(Unit unit) : base(unit)
        {

        }

        // ========================================================= Functionality =========================================================

        /// <summary>
        /// Fill in all die slots. This will be called in the constructor.
        /// </summary>
        protected override void FillDieSlots()
        {
            _slotTypes.Add(new EquipmentDieSlot(this, Die.Type.Unknown, EquipmentDieSlot.Requirement.GreaterThan, 4));
        }

        /// <summary>
        /// Forward implementation of the effect of this equipment.
        /// </summary>
        protected override void AddEffect()
        {
            Unit.ChangeStat(meleeDelta: 5);
        }

        /// <summary>
        /// Backward implementation of the effect of this equipment.
        /// </summary>
        protected override void RemoveEffect()
        {
            Unit.ChangeStat(meleeDelta: -5);
        }
    }
}