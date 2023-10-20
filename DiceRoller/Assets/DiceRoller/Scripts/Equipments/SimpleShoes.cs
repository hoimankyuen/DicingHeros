using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public class SimpleShoes : Equipment
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
		public SimpleShoes(Unit unit) : base(unit)
		{

		}

		// ========================================================= Die Assignment =========================================================

		/// <summary>
		/// Fill in all die slots. This will be called in the constructor.
		/// </summary>
		protected override void FillDieSlots()
		{
			_slotTypes.Add(new EquipmentDieSlot(
				this, 
				Die.Type.Unknown,
				EquipmentDieSlot.Requirement.GreaterThanEqual, 
				2));
		}

		// ========================================================= Information =========================================================

		/// <summary>
		/// The name of this equipment.
		/// </summary>
		public override EquipmentDictionary.Name EquipmentName 
		{ 
			get
			{
				return EquipmentDictionary.Name.SimpleShoes;
			}
		}

		/// <summary>
		/// What type this equipment belongs to.
		/// </summary>
		public override EquipmentType Type
		{
			get
			{
				return EquipmentType.MovementBuff;
			}
		}


		/// <summary>
		/// The name to be displayed to the player.
		/// </summary>
		public override string DisplayableName
		{
			get
			{
				return "Simple Shoe";
			}
		}

		/// <summary>
		/// The effect discription to be displayed to the player.
		/// </summary>
		public override string DisplayableEffectDiscription
		{
			get
			{
				return "+ 1 Speed";
			}
		}

		// ========================================================= Activation =========================================================

		/// <summary>
		/// Forward implementation of the effect of this equipment.
		/// </summary>
		protected override void AddEffect()
		{
			Unit.ChangeStat(movementDelta: 1);
		}

		/// <summary>
		/// Backward implementation of the effect of this equipment.
		/// </summary>
		protected override void RemoveEffect()
		{
			Unit.ChangeStat(movementDelta: -1);
		}
	}
}