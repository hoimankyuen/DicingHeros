using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
	public class Thunderbolt : Equipment
	{
		// ========================================================= Constructor =========================================================

		/// <summary>
		/// Construcddtor.
		/// </summary>
		public Thunderbolt(Unit unit) : base(unit)
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
				EquipmentDieSlot.Requirement.LesserThanEqual,
				1));

			_DieSlots.Add(new EquipmentDieSlot(
				this,
				Die.Type.Unknown,
				EquipmentDieSlot.Requirement.GreaterThanEqual,
				6));
		}

		// ========================================================= Properties (Information) =========================================================

		/// <summary>
		/// The name of this equipment.
		/// </summary>
		public override EquipmentDictionary.Name EquipmentName { get; } = EquipmentDictionary.Name.Thunderbolt;

		/// <summary>
		/// What type this equipment belongs to.
		/// </summary>
		public override EquipmentType Type { get; } = EquipmentType.MagicAttack;

		/// <summary>
		/// The name to be displayed to the player.
		/// </summary>
		public override string DisplayableName { get; } = "Thunderbolt";

		/// <summary>
		/// The effect discription to be displayed to the player.
		/// </summary>
		public override string DisplayableEffectDiscription { get; } = "+ 6 Magic\n 5 Range";

		// ========================================================= Properties (Effect) =========================================================

		/// <summary>
		/// The change in the magic value when this equipment is activated.
		/// </summary>
		public override int MagicDelta { get; } = 6;

		/// <summary>
		/// The change in the magic range value when this equipment is activated.
		/// </summary>
		public override int MagicRangeDelta { get; } = 4;

		/// <summary>
		/// The attack area rule used when this equipment is activated.
		/// </summary>
		public override AttackAreaRule AreaRule { get; } = new AttackAreaRule(
			(target, starting, range) => {
				return (Mathf.Abs(target.boardPos.x - starting.boardPos.x) <= range && Mathf.Abs(target.boardPos.x - starting.boardPos.x) >= 2 && target.boardPos.z == starting.boardPos.z) ||
					(Mathf.Abs(target.boardPos.z - starting.boardPos.z) <= range && Mathf.Abs(target.boardPos.z - starting.boardPos.z) >= 2 && target.boardPos.x == starting.boardPos.x);
			});
	}
}