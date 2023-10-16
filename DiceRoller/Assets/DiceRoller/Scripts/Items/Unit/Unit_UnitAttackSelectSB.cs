using System;
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

			private AttackAreaRule lastAttackAreaRule = null;
			private int lastAttackRange = 0;
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
					self.ShowEffect(EffectType.SelectedSelf, true);

					// show occupied tiles on board, assume unit wont move during movement selection state
					lastOccupiedTiles.AddRange(self.OccupiedTiles);
					foreach (Tile tile in lastOccupiedTiles)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.SelfPosition, lastOccupiedTiles);
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
					// update attack area if needed
					if (CachedValueUtils.HasValueChanged(self.AttackAreaRule, ref lastAttackAreaRule) || CachedValueUtils.HasValueChanged(self.AttackRange, ref lastAttackRange))
					{
						foreach (Tile tile in lastAttackArea)
						{
							tile.RemoveDisplay(self, Tile.DisplayType.Attack);
						}
						board.GetTilesByRule(self.OccupiedTiles, self.AttackAreaRule, self.AttackRange, lastAttackArea);
						foreach (Tile tile in lastAttackArea)
						{
							tile.UpdateDisplayAs(self, Tile.DisplayType.Attack, lastAttackArea);
						}

						// find all targetable units
						lastTargetableUnits.Clear();
						foreach (Player player in game.GetAllPlayers())
						{
							if (player != self.Player)
							{
								foreach (Unit unit in player.units)
								{
									if (lastAttackArea.Intersect(unit.OccupiedTiles).Count() > 0)
									{
										lastTargetableUnits.Add(unit);
										unit.ShowEffect(EffectType.PossibleEnemy, true);
									}
								}
							}
						}
					}

					// detect hovering on enemy units
					Unit target = lastTargetableUnits.FirstOrDefault(x => x.IsHovering);
					if (CachedValueUtils.HasValueChanged(target, ref lastTargetedUnit, out Unit previous))
					{
						if (previous != null)
						{
							// remove previous pending damage and status from previous target
							previous.PendingHealthDelta = 0;
							previous.IsRecievingDamage = false;

							// hide tiles and effect
							foreach (Tile tile in previous.OccupiedTiles)
							{
								tile.UpdateDisplayAs(previous, Tile.DisplayType.EnemyPosition, Tile.EmptyTiles);
							}
							previous.IsBeingInspected = false;
							previous.ShowEffect(EffectType.InspectingEnemy, false);
						}
						
						if (target != null)
						{
							// calculate damage
							int damage = 0;
							if (self.CurrentAttackType == AttackType.Physical)
							{
								damage = Mathf.Max(self.Melee - target.Defence, 0);
							}
							else if (self.CurrentAttackType == AttackType.Magical)
							{
								damage = Mathf.Max(self.Magic - target.Defence, 0);
							}

							// add pending damage and status to new target
							target.PendingHealthDelta = damage * -1;
							target.IsRecievingDamage = true;

							// show tiles and effect
							foreach (Tile tile in target.OccupiedTiles)
							{
								tile.UpdateDisplayAs(target, Tile.DisplayType.EnemyPosition, target.OccupiedTiles);
							}
							target.IsBeingInspected = true;
							target.ShowEffect(EffectType.InspectingEnemy, true);
						}
					}

					// detect press on enemy unit
					if (target != null && target.IsPressed[0])
					{
						// calculate damage
						int damage = 0;
						if (self.CurrentAttackType == AttackType.Physical)
						{
							damage = Mathf.Max(self.Melee - target.Defence, 0);
						}
						else if (self.CurrentAttackType == AttackType.Magical)
						{
							damage = Mathf.Max(self.Magic - target.Defence, 0);
						}
						// fill in attack parameters
						self.NextAttack = new UnitAttack(target, damage, self.KnockbackForce);

						// use any activated equipment that are used at move state
						foreach (Equipment equipment in self.Equipments.Where(x => x.ApplyTime == Equipment.EffectApplyTime.AtAttack && x.IsActivated))
						{
							equipment.ApplyEffect();
						}

						stateMachine.ChangeState(SMState.UnitAttack);
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
					// hide selection effect
					self.ShowEffect(EffectType.SelectedSelf, false);

					// hide occupied tiles on board
					foreach (Tile tile in lastOccupiedTiles)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.SelfPosition, Tile.EmptyTiles);
					}
					lastOccupiedTiles.Clear();

					// hide possible attack area on board
					foreach (Tile tile in lastAttackArea)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.Attack, Tile.EmptyTiles);
					}
					lastAttackArea.Clear();
					CachedValueUtils.ResetValueCache(ref lastAttackRange);
					CachedValueUtils.ResetValueCache(ref lastAttackAreaRule);

					// clear targetable units
					foreach (Unit unit in lastTargetableUnits)
					{
						unit.ShowEffect(EffectType.PossibleEnemy, false);
					}
					lastTargetableUnits.Clear();

					// remove effect on targeted unit
					if (lastTargetedUnit != null)
					{
						lastTargetedUnit.PendingHealthDelta = 0;
						lastTargetedUnit.IsRecievingDamage = false;

						foreach (Tile tile in lastTargetedUnit.OccupiedTiles)
						{
							tile.UpdateDisplayAs(lastTargetedUnit, Tile.DisplayType.EnemyPosition, Tile.EmptyTiles);
						}
						lastTargetedUnit.IsBeingInspected = false;
						lastTargetedUnit.ShowEffect(EffectType.InspectingEnemy, false);
					}
					CachedValueUtils.ResetValueCache(ref lastTargetedUnit);

					// reset cache
					InputUtils.ResetPressCache(ref pressedPosition0);
					InputUtils.ResetPressCache(ref pressedPosition1);
				}
			}
		}

		// ========================================================= Other Methods =========================================================

		public void SkipAttackSelect()
		{
			if (stateMachine.State == SMState.UnitAttackSelect)
			{
				CurrentUnitState = UnitState.Depleted;
				IsSelected = false;

				stateMachine.ChangeState(SMState.Navigation);
			}
		}

		public void ChangeToMoveSelect()
		{
			if (stateMachine.State == SMState.UnitAttackSelect && CurrentUnitState == UnitState.Standby)
			{
				stateMachine.ChangeState(SMState.UnitMoveSelect);
			}
		}

		public void CancelAttackSelect()
		{
			if (stateMachine.State == SMState.UnitAttackSelect)
			{
				IsSelected = false;
				stateMachine.ChangeState(SMState.Navigation);
			}
		}
	}
}