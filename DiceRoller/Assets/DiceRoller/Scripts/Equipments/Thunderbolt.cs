using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
	public class Thunderbolt : Equipment
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
		public Thunderbolt(Unit unit) : base(unit)
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
				EquipmentDieSlot.Requirement.LesserThanEqual,
				1));

			_DieSlots.Add(new EquipmentDieSlot(
				this,
				Die.Type.Unknown,
				EquipmentDieSlot.Requirement.GreaterThanEqual,
				6));
		}

		// ========================================================= Information =========================================================

		/// <summary>
		/// The name of this equipment.
		/// </summary>
		public override EquipmentDictionary.Name EquipmentName
		{
			get
			{
				return EquipmentDictionary.Name.Thunderbolt;
			}
		}

		/// <summary>
		/// What type this equipment belongs to.
		/// </summary>
		public override EquipmentType Type 
		{ 
			get
			{
				return EquipmentType.MagicAttack;
			}
		}

		/// <summary>
		/// The name to be displayed to the player.
		/// </summary>
		public override string DisplayableName
		{
			get
			{
				return "Thunderbolt";
			}
		}

		/// <summary>
		/// The effect discription to be displayed to the player.
		/// </summary>
		public override string DisplayableEffectDiscription
		{
			get
			{
				return "+ 6 Magic\n 5 Range";
			}
		}

		// ========================================================= Activation =========================================================

		/// <summary>
		/// Forward implementation of the effect of this equipment.
		/// </summary>
		protected override void AddEffect()
		{
			Unit.ChangeAttackType(Unit.AttackType.Magical);
			Unit.ChangeStat(magicDelta: 6, attackRangeDelta: 4);
			Unit.ChangeAttackAreaRule(attackRule);
		}

		/// <summary>
		/// Backward implementation of the effect of this equipment.
		/// </summary>
		protected override void RemoveEffect()
		{
			Unit.ChangeAttackType(Unit.AttackType.Physical);
			Unit.ChangeStat(magicDelta: -6, attackRangeDelta: -4);
			Unit.ResetAttackAreaRule();
		}

		private static AttackAreaRule attackRule = new AttackAreaRule(
			(target, starting, range) => {
				return (Mathf.Abs(target.boardPos.x - starting.boardPos.x) <= range && Mathf.Abs(target.boardPos.x - starting.boardPos.x) >= 2 && target.boardPos.z == starting.boardPos.z) ||
					(Mathf.Abs(target.boardPos.z - starting.boardPos.z) <= range  && Mathf.Abs(target.boardPos.z - starting.boardPos.z) >= 2 && target.boardPos.x == starting.boardPos.x);
				});
	}
}