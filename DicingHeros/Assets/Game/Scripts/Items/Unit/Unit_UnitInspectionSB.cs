using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DicingHeros
{
    public partial class Unit
    {
		protected class UnitInspectionSB : StateBehaviour
		{
			// host reference
			protected readonly Unit self = null;

			// caches
			private bool isSelectedAtEnter = false;

			private List<Tile> lastMoveableArea = new List<Tile>();
			private List<Tile> lastPredictedAttackableArea = new List<Tile>();

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
					// show movement
					if (CacheUtils.HasCollectionChanged(self.MovableArea, lastMoveableArea))
					{
						board.ShowArea(self, Tile.DisplayType.MovePossible, self.MovableArea);
					}

					// show attack range
					if (CacheUtils.HasCollectionChanged(self.PredictedAttackableArea, lastPredictedAttackableArea))
					{
						board.ShowArea(self, Tile.DisplayType.AttackPossible, self.PredictedAttackableArea);
					}

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

					// hide movement
					board.HideArea(self, Tile.DisplayType.MovePossible);

					// hide attack range
					board.HideArea(self, Tile.DisplayType.AttackPossible);

					// reset cache
					CacheUtils.ResetCollectionCache(lastMoveableArea);
					CacheUtils.ResetCollectionCache(lastPredictedAttackableArea);
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