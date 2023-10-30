using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public partial class Die
	{
		private class UnitActionSelectSB : StateBehaviour
		{
			// host reference
			private readonly Die self = null;

			// caches
			private bool lastIsHovering = false;

			// ========================================================= Constructor =========================================================

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitActionSelectSB(Die self)
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
				if (self.Player == game.CurrentPlayer)
				{
					if (self.CurrentDieState == DieState.Casted || self.CurrentDieState == DieState.Assigned)
					{
						// inspect the die being hovering on
						if (CacheUtils.HasValueChanged(self.IsHovering, ref lastIsHovering))
						{
							self.IsBeingInspected = self.IsHovering;
						}

						// drag dice
						if (!self.IsBeingDragged)
						{
							if (self.IsStartedDrag[0])
							{
								self.IsBeingDragged = true;
								InputUtils.StartDragging(self);
							}
						}
						if (self.IsBeingDragged)
						{
							if (self.IsCompletedDrag[0])
							{
								//check for dragged target here
								EquipmentDieSlot targetDieSlot = EquipmentDieSlot.GetFirstDragsRecipient();
								if (targetDieSlot != null && targetDieSlot.IsFulfillBy(self))
								{
									self.AssignToSlot(targetDieSlot);
								}

								// always end drag afterwards
								self.IsBeingDragged = false;
								InputUtils.StopDragging(self);
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
				if (self.Player == game.CurrentPlayer)
				{
					if (self.CurrentDieState == DieState.Casted || self.CurrentDieState == DieState.Assigned)
					{
						// stop inspection due to hovering
						if (self.IsBeingInspected)
						{
							self.IsBeingInspected = false;
						}

						// stop drag
						if (self.IsBeingDragged)
						{
							self.IsBeingDragged = false;
							InputUtils.StopDragging(self);
						}
					}

					// reset caches
					CacheUtils.ResetValueCache(ref lastIsHovering);
				}
			}
		}
	}
}