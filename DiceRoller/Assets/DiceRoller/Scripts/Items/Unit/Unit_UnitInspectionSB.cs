using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
    public partial class Unit
    {
		protected class UnitInspectionSB : StateBehaviour
		{
			// host reference
			protected readonly Unit self = null;

			// caches
			private bool isSelectedAtEnter = false;

			private Vector2 pressedPosition1 = Vector2.negativeInfinity;

			// ========================================================= Constructor =========================================================

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitInspectionSB(Unit self)
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
					// show occupied tiles on board, assume unit wont move during movement selection state
					board.ShowArea(self, Tile.DisplayType.EnemyPosition, self.OccupiedTiles);

					// show attack range
					board.ShowArea(self, Tile.DisplayType.AttackPossible, self.PredictedAttackableArea);
				}
			}

			// ========================================================= State Update Methods =========================================================

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				// actions for the selected unit
				if (isSelectedAtEnter)
				{
					// detect return to navitation by right mouse pressing
					if (InputUtils.GetMousePress(1, ref pressedPosition1))
					{
						// return to navigation otherwise
						self.IsSelected = false;
						stateMachine.ChangeState(SMState.Navigation);
					}
				}
			}

			// ========================================================= State Exit Methods =========================================================

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// actions for the selected unit
				if (isSelectedAtEnter)
				{
					// hide occupied tiles on board
					board.HideArea(self, Tile.DisplayType.EnemyPosition);

					// show attack range
					board.HideArea(self, Tile.DisplayType.AttackPossible);

					// reset cache
					InputUtils.ResetPressCache(ref pressedPosition1);
				}
			}
		}

		// ========================================================= Other Methods =========================================================

		public void CancelInspection()
		{
			if (stateMachine.CurrentState == SMState.UnitInspection)
			{
				IsSelected = false;
				stateMachine.ChangeState(SMState.Navigation);
			}
		}
	}
}