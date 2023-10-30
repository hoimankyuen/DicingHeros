using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
	[CreateAssetMenu(fileName = "NewUnitEffectStyle", menuName = "Data/UnitEffectStyle", order = 1)]
	public class UnitEffectStyle : ScriptableObject
	{
		[Header("Depleted")]
		public Color depletedColor;

		[Header("Blocking")]
		public Color blockingFriendColor;
		public Color blockingOtherColor;

		[Header("Targetable")]
		public Color targetableSelfColor;
		public Color targetableOtherColor;

		[Header("Inspected")]
		public Color inspectedSelfColor;
		public Color inspectedFriendColor;
		public Color inspectedOtherColor;

		[Header("Selected")]
		public Color selectedSelfColor;
		public Color selectedOtherColor;
	}
}