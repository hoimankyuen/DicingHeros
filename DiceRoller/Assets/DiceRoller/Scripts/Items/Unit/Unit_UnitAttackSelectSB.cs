using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public partial class Unit
    {
		protected class UnitAttackSelectSB : StateBehaviour
		{
			// host reference
			protected readonly Unit self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitAttackSelectSB(Unit self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				// show action depleted effect
				if (self.ActionDepleted)
				{
					self.AddEffect(StatusType.Depleted);
				}
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// hide depleted effect
				if (self.ActionDepleted)
				{
					self.RemoveEffect(StatusType.Depleted);
				}
			}
		}
	}
}