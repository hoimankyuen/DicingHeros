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
            get { return _DieSlots.AsReadOnly(); }
        }
        private List<EquipmentDieSlot> _DieSlots = new List<EquipmentDieSlot>();

        // ========================================================= Constructor =========================================================

        /// <summary>
        /// Construcddtor.
        /// </summary>
        public SimpleKnife(Unit unit) : base(unit)
        {

        }

        // ========================================================= Die Assignment =========================================================

        /// <summary>
        /// Fill in all die slots. This will be called in the constructor.
        /// </summary>
        protected override void FillDieSlots()
        {
            _DieSlots.Add(new EquipmentDieSlot(this, Die.Type.Unknown, EquipmentDieSlot.Requirement.GreaterThan, 4));
        }

        // ========================================================= Activation =========================================================

        /// <summary>
        /// The time of which should this eqipment apply its effect.
        /// </summary>
        public override EffectApplyTime ApplyTime
        { 
            get
            {
                return EffectApplyTime.AtAttack;
            }
        }

        /// <summary>
        /// Forward implementation of the effect of this equipment.
        /// </summary>
        protected override void AddEffect()
        {
            Unit.ChangeStat(meleeDelta: 6, knockbackForceDelta: 0.25f);
        }

        /// <summary>
        /// Backward implementation of the effect of this equipment.
        /// </summary>
        protected override void RemoveEffect()
        {
            Unit.ChangeStat(meleeDelta: -6, knockbackForceDelta: -0.25f);
        }
    }
}