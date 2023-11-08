using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DicingHeros
{
    public partial class Die 
    {
		protected class DiceActionSelectSB : StateBehaviour
		{
			// host reference
			private readonly Die self = null;

			// caches
			private bool lastIsHovering = false;
			private bool lastThrowDragging = false;

			// ========================================================= Constructor =========================================================

			/// <summary>
			/// Constructor.
			/// </summary>
			public DiceActionSelectSB(Die self)
			{
				this.self = self;
			}

			// ========================================================= State Enter Methods =========================================================

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
			}

			// ========================================================= State Update Methods =========================================================

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				// execute only if the selected unit is this unit
				if (game.CurrentPlayer == self.Player)
				{
					// inspect the die being hovering on
					if (CacheUtils.HasValueChanged(self.IsHovering, ref lastIsHovering) | CacheUtils.HasValueChanged(DiceThrower.current.ThrowDragging, ref lastThrowDragging))
					{
						self.IsBeingInspected = self.IsHovering && !DiceThrower.current.ThrowDragging;
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

			// ========================================================= State Exit Methods =========================================================

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// stop inspection due to hovering
				if (self.IsBeingInspected)
				{
					self.IsBeingInspected = false;
				}

				// reset caches
				CacheUtils.ResetValueCache(ref lastIsHovering);
				CacheUtils.ResetValueCache(ref lastThrowDragging);
			}
		}

		// ========================================================= Other Related Methods =========================================================

		/// <summary>
		/// Select all dice. Called only on diceActionSelect state.
		/// </summary>
		public static void SelectAll_DieActionSelect()
		{
			if (StateMachine.current.CurrentState != SMState.DiceActionSelect)
				return;

			IEnumerable<Die> dice = GameController.current.CurrentPlayer.Dice.Where(x => x.CurrentDieState != DieState.Expended);
			if (dice.All(x => x.IsSelected))
			{
				foreach (Die die in GameController.current.CurrentPlayer.Dice)
				{
					if (die.CurrentDieState != DieState.Expended)
					{
						die.IsSelected = false;
					}
				}
				StateMachine.current.ChangeState(SMState.Navigation);
			}
			else
			{
				foreach (Die die in GameController.current.CurrentPlayer.Dice)
				{
					if (die.CurrentDieState != DieState.Expended)
					{
						die.IsSelected = true;
					}
				}
				StateMachine.current.ChangeState(SMState.DiceActionSelect);
			}
		}
	}
}