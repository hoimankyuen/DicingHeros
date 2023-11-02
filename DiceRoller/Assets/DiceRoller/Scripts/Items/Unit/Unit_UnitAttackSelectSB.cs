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

			private List<Tile> lastAttackableArea = new List<Tile>();
			private Unit targetedUnit = null;

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
					// update attack area if needed
					if (CacheUtils.HasCollectionChanged(self.AttackableArea, lastAttackableArea))
					{
						board.ShowArea(self, Tile.DisplayType.Attack, self.AttackableArea);

						// find all targetable units
						foreach (Unit unit in Unit.GetAllTargetable())
						{
							unit.PendingHealthDelta = 0;
							unit.IsRecievingDamage = false;
						}
						ClearTargetableUnits();
						foreach (Player player in game.GetAllPlayers().Where(x => x != self.Player))
						{
							foreach (Unit unit in player.Units)
							{
								if (lastAttackableArea.Intersect(unit.OccupiedTiles).Count() > 0)
								{
									// calculate damage
									int damage = 0;
									if (self.CurrentAttackType == AttackType.Physical)
									{
										damage = Mathf.Max(self.PhysicalAttack - unit.PhysicalDefence, 0);
									}
									else if (self.CurrentAttackType == AttackType.Magical)
									{
										damage = Mathf.Max(self.MagicalAttack - unit.PhysicalDefence, 0);
									}

									unit.PendingHealthDelta = damage * -1;
									unit.IsRecievingDamage = true;
									unit.IsTargetable = true; 
								}
							}
						}
					}

					// detect hovering on enemy units
					Unit target = GetAllTargetable().FirstOrDefault(x => x.IsHovering);
					if (CacheUtils.HasValueChanged(target, ref targetedUnit, out Unit previous))
					{
						if (previous != null)
						{
							// hide tiles and effect
							board.HideArea(previous, Tile.DisplayType.EnemyPosition);
							previous.IsBeingInspected = false;
						}
						
						if (target != null)
						{
							// add pending damage and status to new target
							target.IsRecievingDamage = true;

							// show tiles and effect
							board.ShowArea(target, Tile.DisplayType.EnemyPosition, target.OccupiedTiles);
							target.IsBeingInspected = true;
						}
					}

					// detect press on enemy unit
					if (target != null && target.IsPressed[0])
					{
						// preventing action when some equipment is in preview
						if (!self.Equipments.Any(x => x.IsBeingInspected && !x.IsActivated))
						{
							// calculate damage
							int damage = 0;
							if (self.CurrentAttackType == AttackType.Physical)
							{
								damage = Mathf.Max(self.PhysicalAttack - target.PhysicalDefence, 0);
							}
							else if (self.CurrentAttackType == AttackType.Magical)
							{
								damage = Mathf.Max(self.MagicalAttack - target.MagicalDefence, 0);
							}

							// fill in attack parameters
							self.NextAttack = new UnitAttack(target, damage, self.KnockbackForce);

							// use any activated equipment that are used at attack state
							AttackType attackType = self.CurrentAttackType;
							foreach (Equipment equipment in self.Equipments)
							{
								if (attackType == AttackType.Physical)
								{
									if (equipment.IsActivated && (equipment.Type == Equipment.EquipmentType.MeleeAttack || equipment.Type == Equipment.EquipmentType.MeleeSelfBuff))
									{
										equipment.ConsumeDie();
									}
								}
								else if (attackType == AttackType.Magical)
								{
									if (equipment.IsActivated && (equipment.Type == Equipment.EquipmentType.MagicAttack || equipment.Type == Equipment.EquipmentType.MagicSelfBuff))
									{
										equipment.ConsumeDie();
									}
								}
							}

							// use any activated equipment on the target that are used as defence
							foreach (Equipment equipment in target.Equipments)
							{
								if (equipment.IsActivated && equipment.Type == Equipment.EquipmentType.DefenceSelfBuff)
								{
									equipment.ConsumeDie();
								}
							}

							stateMachine.ChangeState(SMState.UnitAttack);
						}
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
					board.HideArea(self, Tile.DisplayType.SelfPosition);

					// hide possible attack area on board
					board.HideArea(self, Tile.DisplayType.Attack);
					CacheUtils.ResetCollectionCache(lastAttackableArea);

					// clear targetable units
					foreach (Unit unit in Unit.GetAllTargetable())
					{
						unit.PendingHealthDelta = 0;
						unit.IsRecievingDamage = false;
					}
					ClearTargetableUnits();

					// remove effect on targeted unit
					if (targetedUnit != null)
					{
						board.HideArea(targetedUnit, Tile.DisplayType.EnemyPosition);
						targetedUnit.IsBeingInspected = false;
					}
					CacheUtils.ResetValueCache(ref targetedUnit);

					// reset cache
					InputUtils.ResetPressCache(ref pressedPosition0);
					InputUtils.ResetPressCache(ref pressedPosition1);
				}
			}
		}

		// ========================================================= Other Methods =========================================================

		public void SkipAttackSelect()
		{
			if (stateMachine.CurrentState == SMState.UnitAttackSelect)
			{
				CurrentUnitState = UnitState.Depleted;
				IsSelected = false;

				stateMachine.ChangeState(SMState.Navigation);
			}
		}

		public void ChangeToMoveSelect()
		{
			if (stateMachine.CurrentState == SMState.UnitAttackSelect && CurrentUnitState == UnitState.Standby)
			{
				stateMachine.ChangeState(SMState.UnitMoveSelect);
			}
		}

		public void CancelAttackSelect()
		{
			if (stateMachine.CurrentState == SMState.UnitAttackSelect)
			{
				IsSelected = false;
				stateMachine.ChangeState(SMState.Navigation);
			}
		}
	}
}