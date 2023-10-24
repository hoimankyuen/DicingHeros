using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public class LongBow : Equipment
	{
		// ========================================================= Constructor =========================================================

		/// <summary>
		/// Construcddtor.
		/// </summary>
		public LongBow(Unit unit) : base(unit)
		{

		}

		// ========================================================= Properties (DieSlots) =========================================================

		/// <summary>
		/// The list of all slots and their requiremnts.
		/// </summary>
		public override IReadOnlyList<EquipmentDieSlot> DieSlots
		{
			get { return _DieSlots.AsReadOnly(); }
		}
		private List<EquipmentDieSlot> _DieSlots = new List<EquipmentDieSlot>();

		/// <summary>
		/// Fill in all die slots. This will be called in the constructor.
		/// </summary>
		protected override void FillDieSlots()
		{
			_DieSlots.Add(new EquipmentDieSlot(
				this,
				Die.Type.Unknown,
				EquipmentDieSlot.Requirement.GreaterThanEqual,
				4));
		}

		// ========================================================= Properties (Information) =========================================================

		/// <summary>
		/// The name of this equipment.
		/// </summary>
		public override EquipmentDictionary.Name EquipmentName { get; } = EquipmentDictionary.Name.LongBow;

		/// <summary>
		/// What type this equipment belongs to.
		/// </summary>
		public override EquipmentType Type { get; } = EquipmentType.MeleeAttack;

		/// <summary>
		/// The name to be displayed to the player.
		/// </summary>
		public override string DisplayableName { get; } = "Long Bow";

		/// <summary>
		/// The effect discription to be displayed to the player.
		/// </summary>
		public override string DisplayableEffectDiscription { get; } = "+ 3 Melee\n 3 Range";

		// ========================================================= Properties (Effect) =========================================================

		/// <summary>
		/// The change in the melee value when this equipment is activated.
		/// </summary>
		public override int MeleeDelta { get; } = 3;

		/// <summary>
		/// The change in the attack range value when this equipment is activated.
		/// </summary>
		public override int AttackRangeDelta { get; } = 2;

		/// <summary>
		/// The change in the attack range value when this equipment is activated.
		/// </summary>
		public override float KnockbackForceDelta { get; } = 0.1f;
	}
}