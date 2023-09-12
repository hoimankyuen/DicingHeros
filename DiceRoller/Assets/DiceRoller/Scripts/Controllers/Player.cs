using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	[System.Serializable]
	public class Player
	{
		public int id;
		public string name;
		public int throws;

		[System.NonSerialized]
		public List<Unit> units = new List<Unit>();
		[System.NonSerialized]
		public List<Die> dice = new List<Die>();
	}
}