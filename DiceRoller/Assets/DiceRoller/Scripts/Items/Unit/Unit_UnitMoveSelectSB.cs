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

			private bool attackAreaDirty = true;
			private bool allAttackableAreaDirty = true;
			private AttackType lastAttackType = AttackType.None;
			private int lastPhysicalRange = 0;
			private int lastMagicalRange = 0;
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
						allAttackableAreaDirty = true;
						board.ShowArea(self, Tile.DisplayType.Move, self.MovableArea);
					}

					// find the target tile that the mouse is pointing to
					Tile targetTile = board.HoveringTile;
					bool reachable = self.MovableArea.Contains(targetTile);

					// find a path to the target tile if possible
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
						attackAreaDirty = true;
					}

					// show attackable area on the board
					if (CacheUtils.HasEnumChanged(self.CurrentAttackType, ref lastAttackType))
					{
						attackAreaDirty = true;
						allAttackableAreaDirty = true;
					}
					if (CacheUtils.HasValueChanged(self.PhysicalRange, ref lastPhysicalRange))
					{
						attackAreaDirty = true;
						allAttackableAreaDirty = true;
					}
					if (CacheUtils.HasValueChanged(self.MagicalRange, ref lastMagicalRange))
					{
						attackAreaDirty = true;
						allAttackableAreaDirty = true;
					}
					if (CacheUtils.HasValueChanged(self.AttackAreaRule, ref lastAttackAreaRule))
					{
						attackAreaDirty = true;
						allAttackableAreaDirty = true;
					}

					// show attack area on the board
					if (attackAreaDirty)
					{		
						if (targetTile != null && reachable)
						{
							board.GetTilesByRule(targetTile, self.AttackAreaRule, self.CurrentAttackType == AttackType.Magical ? self.MagicalRange : self.PhysicalRange, nextAttackableArea);
							board.ShowArea(self, Tile.DisplayType.Attack, nextAttackableArea);
						}
						else
						{
							board.HideArea(self, Tile.DisplayType.Attack);
						}
						attackAreaDirty = false;
					}

					// show all attackable area on the board
					if (allAttackableAreaDirty)
					{
						board.ShowArea(self, Tile.DisplayType.AttackPossible, self.PredictedAttackableArea);

						// find all targetable units
						ClearTargetableUnits();
						foreach (Player player in game.GetAllPlayers().Where(x => x != self.Player))
						{
							foreach (Unit unit in player.Units)
							{
								if (self.PredictedAttackableArea.Intersect(unit.OccupiedTiles).Count() > 0)
								{
									unit.IsTargetable = true;
								}
							}
						}

						allAttackableAreaDirty = false;
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
						self.IsBlocking = isHoveringOnTiles;
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
					
					// clear targetable units
					ClearTargetableUnits();

					// hide attackable area on board
					board.HideArea(self, Tile.DisplayType.Attack);
					board.HideArea(self, Tile.DisplayType.AttackPossible);
					attackAreaDirty = true;
					allAttackableAreaDirty = true;
					CacheUtils.ResetEnumCache(ref lastAttackType);
					CacheUtils.ResetValueCache(ref lastPhysicalRange);
					CacheUtils.ResetValueCache(ref lastMagicalRange);
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
					self.IsBlocking = false;
					board.HideArea(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.SelfPosition : Tile.DisplayType.EnemyPosition);
					CacheUtils.ResetValueCache(ref lastIsHoveringFromTiles);

				}
			}
		}

		// ========================================================= Other Methods =========================================================

		public void SkipMoveSelect()
		{
			if (stateMachine.CurrentState == SMState.UnitMoveSelect)
			{
				CurrentUnitState = UnitState.Depleted;
				IsSelected = false;

				stateMachine.ChangeState(SMState.Navigation);
			}
		}

		public void ChangeToAttackSelect()
		{
			if (stateMachine.CurrentState == SMState.UnitMoveSelect)
			{
				stateMachine.ChangeState(SMState.UnitAttackSelect);
			}
		}

		public void CancelMoveSelect()
		{
			if (stateMachine.CurrentState == SMState.UnitMoveSelect)
			{
				IsSelected = false;
				stateMachine.ChangeState(SMState.Navigation);
			}
		}
	}
}