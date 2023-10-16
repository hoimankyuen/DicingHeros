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
			_DieSlots.Add(new EquipmentDieSlot(
				this,
				Die.Type.Unknown,
				EquipmentDieSlot.Requirement.GreaterThan,
				4));
		}

		// ========================================================= Information =========================================================

		/// <summary>
		/// The name of this equipment.
		/// </summary>
		public override EquipmentDictionary.Name EquipmentName
		{
			get
			{
				return EquipmentDictionary.Name.SimpleKnife;
			}
		}

		/// <summary>
		/// What type this equipment belongs to.
		/// </summary>
		public override EquipmentType Type
		{
			get
			{
				return EquipmentType.Melee;
			}
		}

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

		// ========================================================= Activation =========================================================

		/// <summary>
		/// Forward implementation of the effect of this equipment.
		/// </summary>
		protected override void AddEffect()
		{
			Unit.ChangeStat(meleeDelta: 6, knockbackForceDelta: 0.25f);
			Unit.ChangeAttackAreaRule(attackRule);
		}

		/// <summary>
		/// Backward implementation of the effect of this equipment.
		/// </summary>
		protected override void RemoveEffect()
		{
			Unit.ChangeStat(meleeDelta: -6, knockbackForceDelta: -0.25f);
			Unit.ResetAttackAreaRule();
		}

		private static AttackAreaRule attackRule =
			new AttackAreaRule((target, starting, range) => Int2.GridDistance(target.boardPos, starting.boardPos) <= range);
	}
}