using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DicingHeros
{
	public class SimpleShoes : Equipment
	{
		// ========================================================= Constructor =========================================================

		/// <summary>
		/// Construcddtor.
		/// </summary>
		public SimpleShoes(Unit unit) : base(unit)
		{

		}

		// ========================================================= Properties (DieSlots) =========================================================

		/// <summary>
		/// The list of all slots and their requiremnts.
		/// </summary>
		public override IReadOnlyList<EquipmentDieSlot> DieSlots
		{
			get { return _slotTypes.AsReadOnly(); }
		}
		private List<EquipmentDieSlot> _slotTypes = new List<EquipmentDieSlot>();


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

		// ========================================================= Properties (Information) =========================================================

		/// <summary>
		/// The name of this equipment.
		/// </summary>
		public override EquipmentDictionary.Name EquipmentName { get; } = EquipmentDictionary.Name.SimpleShoes;

		/// <summary>
		/// What type this equipment belongs to.
		/// </summary>
		public override EquipmentType Type { get; } = EquipmentType.MovementBuff;

		/// <summary>
		/// The name to be displayed to the player.
		/// </summary>
		public override string DisplayableName { get; } = "Simple Shoe";

		/// <summary>
		/// The effect discription to be displayed to the player.
		/// </summary>
		public override string DisplayableEffectDiscription { get; } ="+ 1 Speed";

		// ========================================================= Properties (Effect) =========================================================

		/// <summary>
		/// The change in the movement value when this equipment is activated.
		/// </summary>
		public override int MovementDelta { get; } = 1;
	}
}