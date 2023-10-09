using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DiceRoller
{
    public partial class Unit
    {
		class UnitMoveSelectSB : StateBehaviour
		{
			// host reference
			private readonly Unit self = null;

			// caches
			private bool isSelectedAtEnter = false;

			private List<Tile> lastOccupiedTiles = new List<Tile>();
			private List<Tile> otherOccupiedTiles = new List<Tile>();
			private List<Tile> lastMovementArea = new List<Tile>();
			private List<Tile> lastPath = new List<Tile>();
			private Tile lastTargetTile = null;
			private bool lastReachable = true;
			private Vector2 pressedPosition0 = Vector2.negativeInfinity;
			private Vector2 pressedPosition1 = Vector2.negativeInfinity;

			private List<Tile> nextPath = new List<Tile>();
			private List<Tile> appendPath = new List<Tile>();
			private List<Tile> appendExcludedTiles = new List<Tile>();

			private bool lastIsHoveringFromTiles = false;

			// ========================================================= Constructor =========================================================

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitMoveSelectSB(Unit self)
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
					// show selection effect
					self.AddEffect(StatusType.SelectedSelf);

					// show occupied tiles on board, assume unit wont move during movement selection state
					lastOccupiedTiles.AddRange(self.OccupiedTiles);
					foreach (Tile tile in lastOccupiedTiles)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.SelfPosition, lastOccupiedTiles);
					}

					// find all tiles that are occupied by other units
					otherOccupiedTiles.Clear();
					foreach (Player player in game.GetAllPlayers())
					{
						foreach (Unit unit in player.units)
						{
							if (unit != self)
							{
								otherOccupiedTiles.AddRange(unit.OccupiedTiles.Except(otherOccupiedTiles));
							}
						}
					}

					// show possible movement area on board, assume unit wont move during movement selection state
					board.GetConnectedTilesInRange(self.OccupiedTiles, otherOccupiedTiles, self.baseMovement, lastMovementArea);
					foreach (Tile tile in lastMovementArea)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.Move, lastMovementArea);
					}
				}

				// actions for other units
				if (!isSelectedAtEnter)
				{
					// show action depleted effect if needed
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
					// find the target tile that the mouse is pointing to
					Tile targetTile = board.HoveringTile;

					// check if target tile is reachable
					bool reachable = lastMovementArea.Contains(targetTile);

					if (lastTargetTile != targetTile)
					{
						// calculate path
						nextPath.Clear();
						nextPath.AddRange(lastPath);
						if (targetTile == null || !reachable)
						{
							// target tile is unreachable, no path is retrieved
							nextPath.Clear();
						}
						else if (self.OccupiedTiles.Contains(targetTile))
						{
							// target tile is withing the starting tiles, reset the path to the target tile
							nextPath.Clear();
							nextPath.Add(targetTile);
						}
						else if (nextPath.Count == 0)
						{
							// no path exist, find the shortest path to target tile
							nextPath.Clear();
							self.board.GetShortestPath(self.OccupiedTiles, otherOccupiedTiles, targetTile, self.baseMovement, in nextPath);
						}
						else if (nextPath.Contains(targetTile))
						{
							// trim the path if target tile is already in the current path
							nextPath.RemoveRange(nextPath.IndexOf(targetTile) + 1, nextPath.Count - nextPath.IndexOf(targetTile) - 1);
						}
						else
						{
							// target tile is valid and not on current path, attempt to append a path from the end of current path to the target tile
							appendPath.Clear();
							appendExcludedTiles.Clear();
							appendExcludedTiles.AddRange(nextPath);
							appendExcludedTiles.AddRange(otherOccupiedTiles);
							appendExcludedTiles.Remove(nextPath[nextPath.Count - 1]);
							self.board.GetShortestPath(nextPath[nextPath.Count - 1], appendExcludedTiles, targetTile, self.baseMovement + 1 - nextPath.Count, in appendPath);
							if (appendPath.Count > 0)
							{
								// append a path from last tile of the path to the target tile
								appendPath.RemoveAt(0);
								nextPath.AddRange(appendPath);
								appendPath.Clear();
							}
							else
							{
								// path is too long, retrieve a shortest path to target tile instead
								nextPath.Clear();
								self.board.GetShortestPath(self.OccupiedTiles, otherOccupiedTiles, targetTile, self.baseMovement, in nextPath);
							}
						}

						// show path to target tile on the board
						foreach (Tile tile in lastPath)
						{
							tile.HidePath();
						}
						foreach (Tile tile in nextPath)
						{
							tile.ShowPath(nextPath);
						}

						// show invalid path to target tile on the board
						if (lastTargetTile != null && !lastReachable)
						{
							lastTargetTile.HideInvalidPath();
						}
						if (targetTile != null && !reachable)
						{
							targetTile.ShowInvalidPath();
						}

						// show target tile on the board
						if (lastTargetTile != null && lastReachable)
						{
							lastTargetTile.UpdateDisplayAs(self, Tile.DisplayType.MoveTarget, (Tile)null);
						}
						if (targetTile != null && reachable)
						{
							targetTile.UpdateDisplayAs(self, Tile.DisplayType.MoveTarget, targetTile);
						}

						lastPath.Clear();
						lastPath.AddRange(nextPath);
						nextPath.Clear();
						lastTargetTile = targetTile;
						lastReachable = reachable;
					}

					// detect path selection by left mouse pressing
					if (InputUtils.GetMousePress(0, ref pressedPosition0))
					{
						if (reachable)
						{
							// pressed on a valid tile, initiate movement
							self.NextMovement = new UnitMovement(self.OccupiedTiles, lastPath);
							stateMachine.ChangeState(State.UnitMove);
						}
						/*
						else
						{
							// return to navigation otherwise
							self.RemoveFromSelection();
							stateMachine.ChangeState(State.Navigation);
						}
						*/
					}

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
					// show or hide hovering by tile
					bool isHoveringOnTiles = self.OccupiedTiles.Contains(board.HoveringTile);
					if (GetFirstSelected() != null)
					{
						isHoveringOnTiles &= !GetFirstSelected().OccupiedTiles.Contains(board.HoveringTile);
					}
					if (CachedValueUtils.HasValueChanged(isHoveringOnTiles, ref lastIsHoveringFromTiles))
					{
						if (isHoveringOnTiles)
						{
							//self.AddToInspection();
							self.AddEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingFriend : StatusType.InspectingEnemy);
							foreach (Tile t in self.OccupiedTiles)
							{
								t.UpdateDisplayAs(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.FriendPosition : Tile.DisplayType.EnemyPosition, self.OccupiedTiles);
							}
						}
						else
						{
							//self.RemoveFromInspection();
							self.RemoveEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingFriend : StatusType.InspectingEnemy);
							foreach (Tile t in self.OccupiedTiles)
							{
								t.UpdateDisplayAs(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.FriendPosition : Tile.DisplayType.EnemyPosition, Tile.EmptyTiles);
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
				// action for the selected unit
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
					foreach (Tile tile in lastMovementArea)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.Move, Tile.EmptyTiles);
					}

					// hide occupied tile of other units
					if (lastTargetTile != null && !lastOccupiedTiles.Contains(lastTargetTile))
					{
						foreach (Item item in lastTargetTile.Occupants)
						{
							if (item is Unit)
							{
								Unit unit = item as Unit;
								unit.RemoveEffect(unit.Player == game.CurrentPlayer ? StatusType.InspectingFriend : StatusType.InspectingEnemy);
								foreach (Tile t in unit.OccupiedTiles)
								{
									if (!lastOccupiedTiles.Contains(t))
									{
										t.UpdateDisplayAs(unit, unit.Player == game.CurrentPlayer ? Tile.DisplayType.FriendPosition : Tile.DisplayType.EnemyPosition, Tile.EmptyTiles);
									}
								}
							}
						}
					}

					// hide path to target tile on board
					foreach (Tile tile in lastPath)
					{
						tile.HidePath();
					}

					// hdie target tile on board
					if (lastTargetTile != null)
					{
						if (lastReachable)
						{
							lastTargetTile.UpdateDisplayAs(self, Tile.DisplayType.MoveTarget, Tile.EmptyTiles);
						}
						else
						{
							lastTargetTile.HideInvalidPath();
						}
					}

					// reset cache
					lastOccupiedTiles.Clear();
					otherOccupiedTiles.Clear();
					lastMovementArea.Clear();
					lastPath.Clear();
					lastTargetTile = null;
					lastReachable = true;

					InputUtils.ResetPressCache(ref pressedPosition0);
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

					// hide hovering by tile
					if (lastIsHoveringFromTiles)
					{
						//self.RemoveFromInspection();
						self.RemoveEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingFriend : StatusType.InspectingEnemy);
						foreach (Tile t in self.OccupiedTiles)
						{
							t.UpdateDisplayAs(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.FriendPosition : Tile.DisplayType.EnemyPosition, Tile.EmptyTiles);
						}
					}

					// reset cache
					CachedValueUtils.ResetValueCache(ref lastIsHoveringFromTiles);
				}
			}
		}

		// ========================================================= Other Methods =========================================================

		public void ChangeToAttackSelect()
		{
			if (stateMachine.Current == State.UnitMoveSelect)
			{
				stateMachine.ChangeState(State.UnitAttackSelect);
			}
		}

		public void CancelMoveSelect()
		{
			if (stateMachine.Current == State.UnitMoveSelect)
			{
				RemoveFromSelection();
				stateMachine.ChangeState(State.Navigation);
			}
		}
	}
}