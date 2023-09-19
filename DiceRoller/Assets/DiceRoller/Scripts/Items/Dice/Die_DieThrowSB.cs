using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public partial class Die
	{
		private class DiceThrowSB : StateBehaviour
		{
			// host reference
			private readonly Die self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public DiceThrowSB(Die self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
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
			}
		}
	}
}