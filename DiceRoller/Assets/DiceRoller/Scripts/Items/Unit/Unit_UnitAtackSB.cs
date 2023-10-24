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
			}

			/// <summary>
			/// Attack coroutine for this unit.
			/// </summary>
			protected IEnumerator AttackSequence()
			{
				// setup
				Unit target = self.NextAttack.target;
				int healthDelta = self.NextAttack.damage * -1;
				int healthFrom = target.Health;
				int healthTo = target.Health + healthDelta;

				// add knockback
				Vector3 knockbackDirection = (target.transform.position - self.transform.position).normalized;
				target.rigidBody.AddForce(knockbackDirection * self.NextAttack.knockbackForce, ForceMode.Impulse);

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

				// trigger death of target unit if target has no health
				if (target.Health <= 0)
				{
					target.CurrentUnitState = UnitState.Defeated;
					foreach (Equipment equipment in target.Equipments)
					{
						equipment.ClearAssignedDie();
					}
					yield return new WaitForSeconds(0.25f);
				}

				// set flag
				self.CurrentUnitState = UnitState.Depleted;
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
			}
		}
	}
}
