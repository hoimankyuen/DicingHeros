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
					// show dice info on ui
					if (CachedValueUtils.HasValueChanged(self.IsHovering, ref lastIsHovering))
					{
						if (self.IsHovering)
						{
							self.AddToInspection();
							self.AddEffect(StatusType.InspectingSelf);
						}
						else
						{
							self.RemoveFromInspection();
							self.RemoveEffect(StatusType.InspectingSelf);
						}
					}

					// drag dice
					if (!self.IsBeingDragged)
					{	
						if (self.IsStartedDrag[0])
						{
							self.AddToDrag();
							InputUtils.StartDragging(self);
						}
					}
					if (self.IsBeingDragged)
					{
						if (self.IsCompletedDrag[0])
						{
							//check for dragged target here
							EquipmentDieSlot dieSlot = EquipmentDieSlot.GetFirstDragsRecipient();
							if (dieSlot != null)
							{
								if (dieSlot.IsFulfillBy(self))
								{
									if (self.EquipmentDieSlot != null)
									{
										self.EquipmentDieSlot.AssignDie(null);
									}
									dieSlot.AssignDie(self);
									self.EquipmentDieSlot = dieSlot;
								}
							}

							// always end drag afterwards
							self.RemoveFromDrag();
							InputUtils.StopDragging(self);
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
					// hide dice info on ui
					if (self.IsBeingInspected)
					{
						self.RemoveFromInspection();
						self.RemoveEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingSelf : StatusType.InspectingEnemy);
					}

					// reset caches
					CachedValueUtils.ResetValueCache(ref lastIsHovering);
				}

				// stop drag
				if (self.IsBeingDragged)
				{
					if (Input.GetMouseButtonUp(0))
					{
						self.RemoveFromDrag();
						InputUtils.StopDragging(self);
					}
				}
			}
		}

		/// <summary>
		/// Resign this die from the die slot currently assigned to.
		/// </summary>
		public void ResignFromCurrentSlot()
		{
			if (EquipmentDieSlot != null)
			{
				EquipmentDieSlot.AssignDie(null);
			}
		}
	}
}