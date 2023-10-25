using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
	public class Fireball : Equipment
	{
		// ========================================================= Constructor =========================================================

		/// <summary>
		/// Construcddtor.
		/// </summary>
		public Fireball(Unit unit) : base(unit)
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
				7));
		}

		// ========================================================= Properties (Information) =========================================================

		/// <summary>
		/// The name of this equipment.
		/// </summary>
		public override EquipmentDictionary.Name EquipmentName { get; } = EquipmentDictionary.Name.Fireball;

		/// <summary>
		/// What type this equipment belongs to.
		/// </summary>
		public override EquipmentType Type { get; } = EquipmentType.MagicAttack;

		/// <summary>
		/// The name to be displayed to the player.
		/// </summary>
		public override string DisplayableName { get; } = "Fireball";

		/// <summary>
		/// The effect discription to be displayed to the player.
		/// </summary>
		public override string DisplayableEffectDiscription { get; } = "+ 8 Magic\n 3 Range";

		// ========================================================= Properties (Effect) =========================================================

		/// <summary>
		/// The change in the magic value when this equipment is activated.
		/// </summary>
		public override int MagicDelta { get; } = 8;

		/// <summary>
		/// The change in the magic range value when this equipment is activated.
		/// </summary>
		public override int MagicRangeDelta { get; } = 2;

		/// <summary>
		/// The attack area rule used when this equipment is activated.
		/// </summary>
		public override AttackAreaRule AreaRule { get; } = new AttackAreaRule(
			(target, starting, range) =>
			{
				return Mathf.Max(Mathf.Abs(target.boardPos.x - starting.boardPos.x), Mathf.Abs(target.boardPos.z - starting.boardPos.z)) <= range &&
					Mathf.Max(Mathf.Abs(target.boardPos.x - starting.boardPos.x), Mathf.Abs(target.boardPos.z - starting.boardPos.z)) >= 2;
			});
	}
}