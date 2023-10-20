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

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitActionSelectSB(Die self)
			{
				this.self = self;
			}

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
				if (self.Player == game.CurrentPlayer)
				{
					if (self.CurrentDieState == DieState.Casted || self.CurrentDieState == DieState.Assigned)
					{
						// show dice info on ui
						if (CacheUtils.HasValueChanged(self.IsHovering, ref lastIsHovering))
						{
							self.IsBeingInspected = self.IsHovering;
							self.ShowEffect(EffectType.InspectingSelf, self.IsHovering);
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
								if (targetDieSlot != null)
								{
									if (targetDieSlot.IsFulfillBy(self))
									{
										self.AssignedDieSlot = targetDieSlot;
									}
								}

								// always end drag afterwards
								self.IsBeingDragged = false;
								InputUtils.StopDragging(self);
							}
						}
					}
					
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				if (self.Player == game.CurrentPlayer)
				{
					if (self.CurrentDieState == DieState.Casted || self.CurrentDieState == DieState.Assigned)
					{
						// hide dice info on ui
						if (self.IsBeingInspected)
						{
							self.IsBeingInspected = false;
							self.ShowEffect(EffectType.InspectingSelf, false);
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

		/// <summary>
		/// Resign this die from the die slot currently assigned to.
		/// </summary>
		public void ResignFromCurrentSlot()
		{
			if (AssignedDieSlot != null)
			{
				AssignedDieSlot = null;
			}
		}
	}
}