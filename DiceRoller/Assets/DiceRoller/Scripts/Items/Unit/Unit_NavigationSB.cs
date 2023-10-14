using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public partial class Unit
    {
		protected class NavigationSB : StateBehaviour
		{
			// host reference
			protected readonly Unit self = null;

			// caches
			private bool lastIsHovering = false;
			private List<Tile> lastOccupiedTiles = new List<Tile>();
			private List<Tile> affectedOccupiedTiles = new List<Tile>();

			// ========================================================= Constructor =========================================================

			/// <summary>
			/// Constructor.
			/// </summary>
			public NavigationSB(Unit self)
			{
				this.self = self;
			}

			// ========================================================= State Machine Methods =========================================================

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
				// only work on still alive units
				if (self.CurrentUnitState != UnitState.Defeated)
				{
					// show occupied tiles on the board
					IReadOnlyCollection<Tile> tiles = self.IsHovering ? self.OccupiedTiles : Tile.EmptyTiles;
					if (CachedValueUtils.HasCollectionChanged(tiles, lastOccupiedTiles, affectedOccupiedTiles))
					{
						foreach (Tile tile in affectedOccupiedTiles)
						{
							tile.UpdateDisplayAs(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.SelfPosition : Tile.DisplayType.EnemyPosition, tiles);
						}
					}

					// show unit info on ui
					if (CachedValueUtils.HasValueChanged(self.IsHovering, ref lastIsHovering))
					{
						self.IsBeingInspected = self.IsHovering;
						self.ShowEffect(self.Player == game.CurrentPlayer ? EffectType.InspectingSelf : EffectType.InspectingEnemy, self.IsHovering);
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

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// hide occupied tiles on board
				foreach (Tile tile in lastOccupiedTiles)
				{
					tile.UpdateDisplayAs(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.SelfPosition : Tile.DisplayType.EnemyPosition, Tile.EmptyTiles);
				}

				// hide unit info on ui
				if (self.IsHovering)
				{
					self.IsBeingInspected = false;
					self.ShowEffect(self.Player == game.CurrentPlayer ? EffectType.InspectingSelf : EffectType.InspectingEnemy, false);
				}

				// reset cache
				CachedValueUtils.ResetValueCache(ref lastIsHovering);
				CachedValueUtils.ResetCollectionCache(lastOccupiedTiles, affectedOccupiedTiles);
			}
		}
	}
}