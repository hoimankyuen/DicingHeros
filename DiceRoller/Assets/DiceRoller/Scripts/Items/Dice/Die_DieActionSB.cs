using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public partial class Die 
    {
		protected class DiceActionSelectSB : StateBehaviour
		{
			// host reference
			private readonly Die self = null;

			// caches
			private bool isSelectedAtStateEnter = false;
			private bool lastIsHovering = false;

			/// <summary>
			/// Constructor.
			/// </summary>
			public DiceActionSelectSB(Die self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				// display as selected
				if (self.IsSelected)
				{
					isSelectedAtStateEnter = true;
					self.ShowEffect(EffectType.SelectedSelf, true);
				}
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				// execute only if the selected unit is this unit
				if (game.CurrentPlayer == self.Player)
				{
					// show dice info on ui
					if (CachedValueUtils.HasValueChanged(self.IsHovering, ref lastIsHovering))
					{
						self.IsBeingInspected = self.IsHovering;
						self.ShowEffect(EffectType.InspectingSelf, self.IsHovering);
					}

					// toggle selection, and go to dice action selection state or navigation state when this dice is pressed
					if (self.IsPressed[0])
					{
						self.IsSelected = !self.IsSelected;

						if (GetFirstSelected() != null)
						{
							stateMachine.ChangeState(SMState.DiceActionSelect);
						}
						else
						{
							stateMachine.ChangeState(SMState.Navigation);
						}
					}
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// revert display as selected
				if (isSelectedAtStateEnter)
				{
					isSelectedAtStateEnter = false;
					self.ShowEffect(EffectType.SelectedSelf, false);
				}

				// revert display as inspecting
				if (self.IsBeingInspected)
				{
					self.IsBeingInspected = false;
					self.ShowEffect(EffectType.InspectingSelf, false);
				}

				// reset caches
				CachedValueUtils.ResetValueCache(ref lastIsHovering);
			}
		}
	}
}