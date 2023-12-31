using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DicingHeros
{
    public partial class Unit
    {
		protected class NavigationSB : StateBehaviour
		{
			// host reference
			protected readonly Unit self = null;

			// caches
			private bool lastIsHovering = false;

			// ========================================================= Constructor =========================================================

			/// <summary>
			/// Constructor.
			/// </summary>
			public NavigationSB(Unit self)
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
				// only work on still alive units
				if (self.CurrentUnitState != UnitState.Defeated)
				{
					// show relative information the unit being hovering on
					if (CacheUtils.HasValueChanged(self.IsHovering, ref lastIsHovering))
					{
						// inspect
						self.IsBeingInspected = self.IsHovering;

						// show occupied tiles on the board
						board.ShowArea(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.SelfPosition : Tile.DisplayType.EnemyPosition, self.IsHovering ? self.OccupiedTiles : Tile.EmptyTiles);

						// show possible movement or attack range on the board
						if (self.Player == game.CurrentPlayer)
						{
							// show possible movement area for not yet moved units of the current player
							if (self.CurrentUnitState == UnitState.Standby)
							{
								board.ShowArea(self, Tile.DisplayType.MovePossible, self.IsHovering ? self.MovableArea : Tile.EmptyTiles);
								board.ShowArea(self, Tile.DisplayType.AttackPossible, self.IsHovering ? self.PredictedAttackableArea : Tile.EmptyTiles);
							}
							else if (self.CurrentUnitState == UnitState.Moved)
							{
								board.ShowArea(self, Tile.DisplayType.AttackPossible, self.IsHovering ? self.AttackableArea : Tile.EmptyTiles);
							}
						}
						else
						{
							// show possible attack area for units of the other players
							board.ShowArea(self, Tile.DisplayType.AttackPossible, self.IsHovering ? self.PredictedAttackableArea : Tile.EmptyTiles);
						}
					}

					// go to unit movement selection state, unit attack selection state or unit depleted select staet when this unit is pressed
					if (self.Player == game.CurrentPlayer && self.IsPressed[0] && self.OccupiedTiles.Count > 0)
					{
						if (self.CurrentUnitState == UnitState.Standby)
						{
							self.IsSelected = true;
							stateMachine.ChangeState(SMState.UnitMoveSelect);
						}
						else if (self.CurrentUnitState == UnitState.Moved)
						{
							self.IsSelected = true;
							stateMachine.ChangeState(SMState.UnitAttackSelect);
						}
						else if (self.CurrentUnitState == UnitState.Depleted)
						{
							self.IsSelected = true;
							stateMachine.ChangeState(SMState.UnitDepletedSelect);
						}
					}

					// go to unit inspection state when this unit as not a player unit is pressed
					if (self.Player != game.CurrentPlayer && self.IsPressed[0])
					{
						self.IsSelected = true;
						stateMachine.ChangeState(SMState.UnitInspection);
					}
				}
			}

			// ========================================================= State Exit Methods =========================================================

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// hide relative information the unit being hovering on
				if (self.IsHovering)
				{
					// hide occupied tiles on board
					board.HideArea(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.SelfPosition : Tile.DisplayType.EnemyPosition);

					// hide possible movement or attack range on the board
					if (self.Player == game.CurrentPlayer)
					{
						// hide possible movement area for not yet moved units of the current player
						if (self.CurrentUnitState == UnitState.Standby)
						{
							board.HideArea(self, Tile.DisplayType.MovePossible);
							board.HideArea(self, Tile.DisplayType.AttackPossible);
						}
						else if (self.CurrentUnitState == UnitState.Moved)
						{
							board.HideArea(self, Tile.DisplayType.AttackPossible);
						}
					}
					else
					{
						// hide possible attack area for units of the other players
						board.HideArea(self, Tile.DisplayType.AttackPossible);
					}

					// hide unit info on ui
					self.IsBeingInspected = false;
				}

				// reset cache
				CacheUtils.ResetValueCache(ref lastIsHovering);
			}
		}
	}
}