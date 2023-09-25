using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public class UnitAttack
	{
		public Unit target;
		public int healthDelta;

		public UnitAttack(Unit target, int healthDelta)
		{
			this.target = target;
			this.healthDelta = healthDelta;
		}
	}
}