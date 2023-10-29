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

			private List<Tile> lastMovableArea = new List<Tile>();

			private List<Tile> targetPath = new List<Tile>();

			private Tile lastTargetTile = null;

			private bool attackableAreaDirty = true;
			private int lastAttackRange = 0;
			private AttackAreaRule lastAttackAreaRule = null;
			private List<Tile> nextAttackableArea = new List<Tile>();

			private Vector2 pressedPosition0 = Vector2.negativeInfinity;
			private Vector2 pressedPosition1 = Vector2.negativeInfinity;

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
					self.ShowEffect(EffectType.SelectedSelf, true);

					// show occupied tiles on board, assume unit wont move during movement selection state
					board.ShowArea(self, Tile.DisplayType.SelfPosition, self.OccupiedTiles);
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
					// update movement area if needed
					if (CacheUtils.HasCollectionChanged(self.MovableArea, lastMovableArea))
					{
						board.ShowArea(self, Tile.DisplayType.Move, self.MovableArea);
					}

					// find the target tile that the mouse is pointing to
					Tile targetTile = board.HoveringTile;
					bool reachable = self.MovableArea.Contains(targetTile);
					if (CacheUtils.HasValueChanged(targetTile, ref lastTargetTile))
					{
						// calculate path
						if (targetTile == null || !reachable)
						{
							// target tile is unreachable, no path is retrieved
							targetPath.Clear();
						}
						else if (self.OccupiedTiles.Contains(targetTile))
						{
							// target tile is withing the starting tiles, reset the path to the target tile
							targetPath.Clear();
							targetPath.Add(targetTile);
						}
						else if (targetPath.Count == 0)
						{
							// no path exist, find the shortest path to target tile
							targetPath.Clear();
							self.board.GetShortestPath(self.OccupiedTiles, self.AllOccupiedTilesExceptSelf, targetTile, self.Movement, targetPath);
						}
						else if (targetPath.Contains(targetTile))
						{
							// trim the path if target tile is already in the current path
							targetPath.RemoveRange(targetPath.IndexOf(targetTile) + 1, targetPath.Count - targetPath.IndexOf(targetTile) - 1);
						}
						else
						{
							// target tile is valid and not on current path, attempt to append a path from the end of current path to the target tile
							List<Tile> appendPath = new List<Tile>();
							List<Tile> appendExcludedTiles = new List<Tile>();
							appendExcludedTiles.AddRange(targetPath);
							appendExcludedTiles.AddRange(self.AllOccupiedTilesExceptSelf);
							appendExcludedTiles.Remove(targetPath[targetPath.Count - 1]);
							self.board.GetShortestPath(targetPath[targetPath.Count - 1], appendExcludedTiles, targetTile, self.Movement + 1 - targetPath.Count, appendPath);
							appendExcludedTiles.Clear();
							if (appendPath.Count > 0)
							{
								// append a path from last tile of the path to the target tile
								appendPath.RemoveAt(0);
								targetPath.AddRange(appendPath);
							}
							else
							{
								// path is too long, retrieve a shortest path to target tile instead
								targetPath.Clear();
								self.board.GetShortestPath(self.OccupiedTiles, self.AllOccupiedTilesExceptSelf, targetTile, self.Movement, targetPath);
							}
						}

						// show valid, invalid or no path on the board
						if (targetPath.Count > 0)
						{
							board.ShowPath(targetPath);
						}
						else if (targetTile != null && !reachable)
						{
							board.ShowInvalidPath(targetTile);
						}
						else
						{
							board.HidePath();
						}

						// show target tile on the board
						board.ShowArea(self, Tile.DisplayType.MoveTarget, (targetTile != null && reachable) ? targetTile : null);

						// force attaack area recalculation
						attackableAreaDirty = true;
					}

					// show attackable area on the board
					if (CacheUtils.HasValueChanged(self.PhysicalRange, ref lastAttackRange))
					{
						attackableAreaDirty = true;
					}
					if (CacheUtils.HasValueChanged(self.AttackAreaRule, ref lastAttackAreaRule))
					{
						attackableAreaDirty = true;
					}
					if (attackableAreaDirty)
					{		
						if (targetTile != null && reachable)
						{
							board.GetTilesByRule(targetTile, self.AttackAreaRule, self.PhysicalRange, nextAttackableArea);
							board.ShowArea(self, Tile.DisplayType.AttackPossible, nextAttackableArea);
						}
						else
						{
							board.ShowArea(self, Tile.DisplayType.AttackPossible, self.AttackableArea);
						}
						attackableAreaDirty = false;
					}

					// detect path selection by left mouse pressing
					if (InputUtils.GetMousePress(0, ref pressedPosition0) && reachable)
					{
						// pressed on a valid tile, initiate movement
						self.NextMovement = new UnitMovement(self.OccupiedTiles, targetPath);

						// use any activated equipment that are used at move state
						foreach (Equipment equipment in self.Equipments.Where(x => x.Type == Equipment.EquipmentType.MovementBuff && x.IsActivated))
						{
							equipment.ConsumeDie();
						}

						stateMachine.ChangeState(SMState.UnitMove);
					}

					// detect return to navitation by right mouse pressing
					if (InputUtils.GetMousePress(1, ref pressedPosition1))
					{
						// return to navigation otherwise
						self.IsSelected = false;
						stateMachine.ChangeState(SMState.Navigation);
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
					if (CacheUtils.HasValueChanged(isHoveringOnTiles, ref lastIsHoveringFromTiles))
					{
						self.ShowEffect(self.Player == game.CurrentPlayer ? EffectType.InspectingFriend : EffectType.InspectingEnemy, isHoveringOnTiles);
						board.ShowArea(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.FriendPosition : Tile.DisplayType.EnemyPosition, isHoveringOnTiles ? self.OccupiedTiles : Tile.EmptyTiles);
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
					self.HideEffect(EffectType.SelectedSelf);

					// hide occupied tiles on board
					board.HideArea(self, Tile.DisplayType.SelfPosition);

					// hide possible movement area on board
					board.HideArea(self, Tile.DisplayType.Move);
					CacheUtils.ResetCollectionCache(lastMovableArea);

					// hide path to target tile on board
					board.HidePath();
					targetPath.Clear();

					// hdie target tile on board
					board.HideArea(self, Tile.DisplayType.MoveTarget);
					CacheUtils.ResetValueCache(ref lastTargetTile);

					// hide attackable area on board
					board.HideArea(self, Tile.DisplayType.AttackPossible);
					attackableAreaDirty = true;
					CacheUtils.ResetValueCache(ref lastAttackRange);
					CacheUtils.ResetValueCache(ref lastAttackAreaRule);
					nextAttackableArea.Clear();

					// reset input cache
					InputUtils.ResetPressCache(ref pressedPosition0);
					InputUtils.ResetPressCache(ref pressedPosition1);
				}

				// action for other units
				if (!isSelectedAtEnter)
				{
					// hide hovering by tile
					self.HideEffect(self.Player == game.CurrentPlayer ? EffectType.InspectingFriend : EffectType.InspectingEnemy);
					board.HideArea(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.FriendPosition : Tile.DisplayType.EnemyPosition);
					CacheUtils.ResetValueCache(ref lastIsHoveringFromTiles);

				}
			}
		}

		// ========================================================= Other Methods =========================================================

		public void SkipMoveSelect()
		{
			if (stateMachine.State == SMState.UnitMoveSelect)
			{
				CurrentUnitState = UnitState.Depleted;
				IsSelected = false;

				stateMachine.ChangeState(SMState.Navigation);
			}
		}

		public void ChangeToAttackSelect()
		{
			if (stateMachine.State == SMState.UnitMoveSelect)
			{
				stateMachine.ChangeState(SMState.UnitAttackSelect);
			}
		}

		public void CancelMoveSelect()
		{
			if (stateMachine.State == SMState.UnitMoveSelect)
			{
				IsSelected = false;
				stateMachine.ChangeState(SMState.Navigation);
			}
		}
	}
}