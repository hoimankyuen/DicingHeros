using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public class UnitAttack
	{
		public Unit target;
		public int damage;
		public float knockbackForce;

		public UnitAttack(Unit target, int damage, float knockbackForce)
		{
			this.target = target;
			this.damage = damage;
			this.knockbackForce = knockbackForce;
		}
	}
}