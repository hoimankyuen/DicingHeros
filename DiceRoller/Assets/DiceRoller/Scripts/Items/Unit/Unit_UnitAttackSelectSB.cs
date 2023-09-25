using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public partial class Unit
    {
		protected class UnitAttackSelectSB : StateBehaviour
		{
			// host reference
			protected readonly Unit self = null;

			// caches
			private bool isSelectedAtEnter = false;
			private bool lastIsHovering = false;

			private List<Tile> lastOccupiedTiles = new List<Tile>();
			private List<Tile> affectedOccupiedTiles = new List<Tile>();
			private List<Tile> lastAttackArea = new List<Tile>();
			private Unit lastTargetedUnit = null;

			private Vector2 pressedPosition1 = Vector2.negativeInfinity;

			// ========================================================= Constructor =========================================================

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitAttackSelectSB(Unit self)
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
					isSelectedAtEnter = true;

					// show selection effect
					self.AddEffect(StatusType.SelectedSelf);

					// show occupied tiles on board, assume unit wont move during movement selection state
					lastOccupiedTiles.AddRange(self.OccupiedTiles);
					foreach (Tile tile in lastOccupiedTiles)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.SelfPosition, lastOccupiedTiles);
					}

					// show possible movement area on board, assume unit wont move during movement selection state
					board.GetTilesInRange(self.OccupiedTiles, 1, lastAttackArea);
					foreach (Tile tile in lastAttackArea)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.Attack, lastAttackArea);
					}
				}

				// action for other units
				if (!isSelectedAtEnter)
				{
					// show action depleted effect
					if (self.ActionDepleted)
					{
						self.AddEffect(StatusType.Depleted);
					}
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
						self.RemoveFromSelection();
						stateMachine.ChangeState(State.Navigation);
					}
				}

				// action for other units
				if (!isSelectedAtEnter)
				{
					if (self.Player != game.CurrentPlayer)
					{
						if (CachedValueUtils.HasValueChanged(self.IsHoveringOnObject, ref lastIsHovering))
						{
							// show occupied tiles on the board
							IReadOnlyCollection<Tile> tiles = self.IsHoveringOnObject ? self.OccupiedTiles : Tile.EmptyTiles;
							if (CachedValueUtils.HasCollectionChanged(tiles, lastOccupiedTiles, affectedOccupiedTiles))
							{
								foreach (Tile tile in affectedOccupiedTiles)
								{
									tile.UpdateDisplayAs(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.FriendPosition : Tile.DisplayType.EnemyPosition, tiles);
								}
							}

							// show unit info on ui
							if (self.IsHoveringOnObject)
							{
								self.AddToInspection();
								self.AddEffect(StatusType.InspectingEnemy);
							}
							else
							{
								self.RemoveFromInspection();
								self.RemoveEffect(StatusType.InspectingEnemy);
							}
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
				// actions for the selected unit
				if (isSelectedAtEnter)
				{
					// hide selection effect
					self.RemoveEffect(StatusType.SelectedSelf);

					// hide occupied tiles on board
					foreach (Tile tile in lastOccupiedTiles)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.SelfPosition, Tile.EmptyTiles);
					}

					// hide possible movement area on board
					foreach (Tile tile in lastAttackArea)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.Attack, Tile.EmptyTiles);
					}

					// reset cache
					lastOccupiedTiles.Clear();
					lastAttackArea.Clear();

					InputUtils.ResetPressCache(ref pressedPosition1);
				}

				// action for other units
				if (!isSelectedAtEnter)
				{
					// hide depleted effect
					if (self.ActionDepleted)
					{
						self.RemoveEffect(StatusType.Depleted);
					}
		
					if (self.Player != game.CurrentPlayer)
					{
						if (self.IsHoveringOnObject)
						{
							// hide occupied tiles on board
							foreach (Tile tile in lastOccupiedTiles)
							{
								tile.UpdateDisplayAs(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.SelfPosition : Tile.DisplayType.EnemyPosition, Tile.EmptyTiles);
							}

							// hide unit info on ui
							self.RemoveFromInspection();
							self.RemoveEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingFriend : StatusType.InspectingEnemy);
						}

						// reset cache
						CachedValueUtils.ResetValueCache(ref lastIsHovering);
						CachedValueUtils.ResetCollectionCache(lastOccupiedTiles, affectedOccupiedTiles);
					}
				}
			}
		}

		public void ChangeToMoveSelect()
		{
			if (stateMachine.Current == State.UnitAttackSelect)
			{
				stateMachine.ChangeState(State.UnitMoveSelect);
			}
		}
	}
}