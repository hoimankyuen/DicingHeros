using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

			private List<Tile> lastOccupiedTiles = new List<Tile>();
			private List<Tile> lastAttackArea = new List<Tile>();
			private List<Unit> lastTargetableUnits = new List<Unit>();

			private Unit lastTargetedUnit = null;

			private Vector2 pressedPosition0 = Vector2.negativeInfinity;
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

					// find all targetable units
					foreach (Player player in game.GetAllPlayers())
					{
						if (player != self.Player)
						{
							foreach (Unit unit in player.units)
							{
								if (lastAttackArea.Intersect(unit.OccupiedTiles).Count() > 0)
								{
									lastTargetableUnits.Add(unit);
									unit.AddEffect(StatusType.PossibleEnemy);
								}
							}
						}
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
					// detect hovering on enemy units
					Unit target = lastTargetableUnits.FirstOrDefault(x => x.IsHovering);
					if (CachedValueUtils.HasValueChanged(target, ref lastTargetedUnit, out Unit previous))
					{
						if (previous != null)
						{
							previous.PendingHealthDelta = 0;

							foreach (Tile tile in previous.OccupiedTiles)
							{
								tile.UpdateDisplayAs(previous, Tile.DisplayType.EnemyPosition, Tile.EmptyTiles);
							}
							previous.RemoveFromInspection();
							previous.RemoveEffect(StatusType.InspectingEnemy);
						}
						if (target != null)
						{
							target.PendingHealthDelta = self.Melee * -1;

							foreach (Tile tile in target.OccupiedTiles)
							{
								tile.UpdateDisplayAs(target, Tile.DisplayType.EnemyPosition, target.OccupiedTiles);
							}
							target.AddToInspection();
							target.AddEffect(StatusType.InspectingEnemy);
						}
					}

					// detect press on enemy unit
					if (target != null && target.IsPressed[0])
					{
						self.NextAttack = new UnitAttack(target, self.Melee * -1);
						stateMachine.ChangeState(State.UnitAttack);
					}

					// detect press on anywhere other than enemy unit
					/*
					if (InputUtils.GetMousePress(0, ref pressedPosition0) && target == null)
					{
						self.RemoveFromSelection();
						stateMachine.ChangeState(State.Navigation);
					}
					*/

					// detect return to navitation by right mouse pressing
					if (InputUtils.GetMousePress(1, ref pressedPosition1))
					{
						// return to navigation otherwise
						self.RemoveFromSelection();
						stateMachine.ChangeState(State.Navigation);
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
					lastOccupiedTiles.Clear();

					// hide possible movement area on board
					foreach (Tile tile in lastAttackArea)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.Attack, Tile.EmptyTiles);
					}
					lastAttackArea.Clear();

					// clear targetable units
					foreach (Unit unit in lastTargetableUnits)
					{
						unit.RemoveEffect(StatusType.PossibleEnemy);
					}
					lastTargetableUnits.Clear();

					// remove effect on targeted unit
					if (lastTargetedUnit != null)
					{
						lastTargetedUnit.PendingHealthDelta = 0;

						foreach (Tile tile in lastTargetedUnit.OccupiedTiles)
						{
							tile.UpdateDisplayAs(lastTargetedUnit, Tile.DisplayType.EnemyPosition, Tile.EmptyTiles);
						}
						lastTargetedUnit.RemoveFromInspection();
						lastTargetedUnit.RemoveEffect(StatusType.InspectingEnemy);
					}
					CachedValueUtils.ResetValueCache(ref lastTargetedUnit);

					// reset cache
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
				}
			}
		}

		// ========================================================= Other Methods =========================================================

		public void ChangeToMoveSelect()
		{
			if (stateMachine.Current == State.UnitAttackSelect)
			{
				stateMachine.ChangeState(State.UnitMoveSelect);
			}
		}

		public void CancelAttackSelect()
		{
			if (stateMachine.Current == State.UnitAttackSelect)
			{
				RemoveFromSelection();
				stateMachine.ChangeState(State.Navigation);
			}
		}
	}
}