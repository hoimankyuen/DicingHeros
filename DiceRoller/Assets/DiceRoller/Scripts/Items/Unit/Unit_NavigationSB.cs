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
				// show action depleted effect
				if (self.ActionDepleted)
				{
					self.AddEffect(StatusType.Depleted);
				}
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
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
					if (self.IsHovering)
					{
						self.AddToInspection();
						self.AddEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingSelf : StatusType.InspectingEnemy);
					}
					else
					{
						self.RemoveFromInspection();
						self.RemoveEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingSelf : StatusType.InspectingEnemy);
					}
				}

				// go to unit movement selection state when this unit is pressed
				if (self.Player == game.CurrentPlayer && self.IsPressed[0] && !self.ActionDepleted && self.OccupiedTiles.Count > 0)
				{
					self.AddToSelection();
					stateMachine.ChangeState(State.UnitMoveSelect);
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// hide action depleted effect
				if (self.ActionDepleted)
				{
					self.RemoveEffect(StatusType.Depleted);
				}

				// hide occupied tiles on board
				foreach (Tile tile in lastOccupiedTiles)
				{
					tile.UpdateDisplayAs(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.SelfPosition : Tile.DisplayType.EnemyPosition, Tile.EmptyTiles);
				}

				// hide unit info on ui
				if (self.IsHovering)
				{
					self.RemoveFromInspection();
					self.RemoveEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingSelf : StatusType.InspectingEnemy);
				}

				// reset cache
				CachedValueUtils.ResetValueCache(ref lastIsHovering);
				CachedValueUtils.ResetCollectionCache(lastOccupiedTiles, affectedOccupiedTiles);
			}
		}
	}
}