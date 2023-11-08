using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DicingHeros
{
	public class ShortSword : Equipment
	{
		// ========================================================= Constructor =========================================================

		/// <summary>
		/// Construcddtor.
		/// </summary>
		public ShortSword(Unit unit) : base(unit)
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
				3));
		}

		// ========================================================= Properties (Information) =========================================================

		/// <summary>
		/// The name of this equipment.
		/// </summary>
		public override EquipmentDictionary.Name EquipmentName { get; } = EquipmentDictionary.Name.ShortSword;

		/// <summary>
		/// What type this equipment belongs to.
		/// </summary>
		public override EquipmentType Type { get; } = EquipmentType.MeleeAttack;


		/// <summary>
		/// The name to be displayed to the player.
		/// </summary>
		public override string DisplayableName { get; } = "Short Sword";

		/// <summary>
		/// The effect discription to be displayed to the player.
		/// </summary>
		public override string DisplayableEffectDiscription { get; } = "+ 4 Melee";

		// ========================================================= Properties (Effect) =========================================================

		/// <summary>
		/// The change in the physical attack value when this equipment is activated.
		/// </summary>
		public override int PhysicalAttackDelta { get; } = 4;

		/// <summary>
		/// The change in the physical range value when this equipment is activated.
		/// </summary>
		public override float KnockbackForceDelta { get; } = 0.1f;
	}
}