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
		public float knockbackForce;

		public UnitAttack(Unit target, int healthDelta, float knockbackForce)
		{
			this.target = target;
			this.healthDelta = healthDelta;
			this.knockbackForce = knockbackForce;
		}
	}
}