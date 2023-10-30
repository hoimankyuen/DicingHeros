using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

			// ========================================================= Constructor =========================================================

			/// <summary>
			/// Constructor.
			/// </summary>
			public NavigationSB(Die self)
			{
				this.self = self;
			}

			// ========================================================= Constructor =========================================================

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
			}

			// ========================================================= State Enter Methods =========================================================

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				// inspect the die being hovering on
				if (CacheUtils.HasValueChanged(self.IsHovering, ref lastIsHovering))
				{
					self.IsBeingInspected = self.IsHovering;
				}

				// go to dice action selection state when this dice is pressed
				if (game.CurrentPlayer == self.Player && self.IsPressed[0] && self.CurrentDieState != DieState.Expended)
				{
					self.IsSelected = true;
					stateMachine.ChangeState(SMState.DiceActionSelect);
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
			}
		}

		// ========================================================= Other Related Methods =========================================================


		/// <summary>
		/// Select all dice. Called only on navigation state.
		/// </summary>
		public static void SelectAll_Navigation()
		{
			if (StateMachine.current.CurrentState != SMState.Navigation)
				return;

			IEnumerable<Die> dice = GameController.current.CurrentPlayer.Dice.Where(x => x.CurrentDieState != DieState.Expended);
			if (dice.Count() > 0)
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
