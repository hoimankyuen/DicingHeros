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
					self.AddEffect(StatusType.SelectedSelf);
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
					if (CachedValueUtils.HasValueChanged(self.IsHoveringOnObject, ref lastIsHovering))
					{
						if (self.IsHoveringOnObject)
						{
							self.AddToInspection();
							self.AddEffect(StatusType.InspectingSelf);
						}
						else
						{
							self.RemoveFromInspection();
							self.RemoveEffect(StatusType.InspectingSelf);
						}
					}

					// go to dice action selection state or navigation state when this dice is pressed
					if (self.IPressedOnObject)
					{
						if (self.IsSelected)
						{
							self.RemoveFromSelection();
						}
						else
						{
							self.AddToSelection();
						}

						if (GetFirstSelected() != null)
						{
							stateMachine.ChangeState(State.DiceActionSelect);
						}
						else
						{
							stateMachine.ChangeState(State.Navigation);
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
					self.RemoveEffect(StatusType.SelectedSelf);
				}

				// revert display as inspecting
				if (self.IsBeingInspected)
				{
					self.RemoveFromInspection();
					self.RemoveEffect(StatusType.InspectingSelf);
				}

				// reset caches
				CachedValueUtils.ResetValueCache(ref lastIsHovering);
			}
		}
	}
}