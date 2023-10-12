using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public partial class Unit
    {
		protected class UnitAttackSB : StateBehaviour
		{
			// host reference
			protected readonly Unit self = null;

			// caches
			private bool isSelectedAtEnter = false;

			// ========================================================= Constructor =========================================================

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitAttackSB(Unit self)
			{
				this.self = self;
			}

			// ========================================================= State Enter Methods =========================================================

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				isSelectedAtEnter = self.IsSelected;

				// actions for the selected unit
				if (isSelectedAtEnter)
				{
					self.StartCoroutine(AttackSequence());
				}

				// action for other units
				if (!isSelectedAtEnter)
				{
					// show depleted effect
					if (self.CurrentUnitState == UnitState.Depleted)
					{
						self.ShowEffect(EffectType.Depleted, true);
					}
				}
			}

			/// <summary>
			/// Attack coroutine for this unit.
			/// </summary>
			protected IEnumerator AttackSequence()
			{
				// setup
				Unit target = self.NextAttack.target;
				int healthDelta = self.NextAttack.healthDelta;
				int healthFrom = target.Health;
				int healthTo = target.Health + healthDelta;

				// hit and damage animation
				float duration = 0.5f;
				float startTime = Time.time;

				target.IsBeingInspected = true;

				while (Time.time < startTime + duration)
				{
					target.Health = Mathf.Clamp((int)Mathf.Round(Mathf.Lerp(healthFrom, healthTo, (Time.time - startTime) / duration)), 0, target.maxHealth);
					yield return null;
				}

				target.Health = healthTo;

				yield return new WaitForSeconds(0.5f);

				target.IsBeingInspected = false;

				// death animation
				if (target.Health <= 0)
				{
					// implement death animation here
				}

				// set flag
				self.CurrentUnitState = UnitState.Depleted;
				self.ShowEffect(EffectType.Depleted, true);
				self.IsSelected = false;

				// clear data
				self.NextAttack = null;

				// change state back to navigation
				stateMachine.ChangeState(SMState.Navigation);
			}

			// ========================================================= State Update Methods =========================================================

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
			}

			// ========================================================= State Exit Methods =========================================================

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// action for other units
				if (!isSelectedAtEnter)
				{
					if (self.CurrentUnitState == UnitState.Depleted)
					{
						self.ShowEffect(EffectType.Depleted, false);
					}
				}
			}
		}
	}
}
