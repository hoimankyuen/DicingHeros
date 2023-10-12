﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceRoller
{
	public partial class Die
	{
		private class NavigationSB : StateBehaviour
		{
			// host reference
			private readonly Die self = null;

			// caches
			private bool lastIsHovering = false;

			/// <summary>
			/// Constructor.
			/// </summary>
			public NavigationSB(Die self)
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
				// show dice info on ui
				if (CachedValueUtils.HasValueChanged(self.IsHovering, ref lastIsHovering))
				{
					self.IsBeingInspected = self.IsHovering;
					self.ShowEffect(self.Player == game.CurrentPlayer ? EffectType.InspectingSelf : EffectType.InspectingEnemy, self.IsHovering);
				}

				// go to dice action selection state when this dice is pressed
				if (game.CurrentPlayer == self.Player && self.IsPressed[0])
				{
					self.IsSelected = true;
					stateMachine.ChangeState(SMState.DiceActionSelect);
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// hide dice info on ui
				if (self.IsBeingInspected)
				{
					self.IsBeingInspected = false;
					self.ShowEffect(self.Player == game.CurrentPlayer ? EffectType.InspectingSelf : EffectType.InspectingEnemy, false);
				}

				// reset caches
				CachedValueUtils.ResetValueCache(ref lastIsHovering);
			}
		}
	}
}
