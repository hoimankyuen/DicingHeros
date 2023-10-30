using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
	[CreateAssetMenu(fileName = "NewDieEffectStyle", menuName = "Data/DieEffectStyle", order = 1)]
	public class DieEffectStyle : ScriptableObject
	{
		[Header("Inspected")]
		public Color inspectedSelfColor;
		public Color inspectedOtherColor;

		[Header("Selected")]
		public Color selectedSelfColor;
		public Color selectedOtherColor;
	}
}