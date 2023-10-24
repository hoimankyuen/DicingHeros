using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
	public class MagesRing : Equipment
	{
		// ========================================================= Constructor =========================================================

		/// <summary>
		/// Construcddtor.
		/// </summary>
		public MagesRing(Unit unit) : base(unit)
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
				EquipmentDieSlot.Requirement.IsEven));

			_DieSlots.Add(new EquipmentDieSlot(
				this,
				Die.Type.Unknown,
				EquipmentDieSlot.Requirement.IsOdd));
		}

		// ========================================================= Properties (Information) =========================================================

		/// <summary>
		/// The name of this equipment.
		/// </summary>
		public override EquipmentDictionary.Name EquipmentName { get; } = EquipmentDictionary.Name.MagesRing;

		/// <summary>
		/// What type this equipment belongs to.
		/// </summary>
		public override EquipmentType Type { get; } = EquipmentType.MagicSelfBuff;


		/// <summary>
		/// The name to be displayed to the player.
		/// </summary>
		public override string DisplayableName { get; } = "Mage's Ring";

		/// <summary>
		/// The effect discription to be displayed to the player.
		/// </summary>
		public override string DisplayableEffectDiscription { get; } = "+ 3 Magic\n+ 1 Range";

		// ========================================================= Properties (Effect) =========================================================

		/// <summary>
		/// The change in the magic value when this equipment is activated.
		/// </summary>
		public override int MagicDelta { get; } = 3;

		/// <summary>
		/// The change in the attack range value when this equipment is activated.
		/// </summary>
		public override int AttackRangeDelta { get; } = 1;
	}
}