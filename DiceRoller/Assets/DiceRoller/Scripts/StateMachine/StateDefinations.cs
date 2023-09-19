using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DiceRoller
{
	public enum State
	{
		None,
		/*
		 * Registered :
		 *     
		 * Transitions :
		 *     
		 */
		StartTurn,
		/*
		 * Registered : 
		 *     GameController
		 *     UIController
		 *     Unit
		 * Transitions :
		 *     GameController -> State.Navitaion
		 */
		Navigation,
		/*
		 * Registered : 
		 *     UIController
		 *     Unit
		 *     Die 
		 * Transitions : 
		 *     GameController -> State.EndTurn, 
		 *     Unit -> State.UnitActionSelect, 
		 *     Die -> State.DieActionSelect
		 */
		UnitMoveSelect,
		/*
		 * Registered : 
		 *     UIController
		 *     Unit
		 * Transitions : 
		 *     Unit -> State.UnitMovement, 
		 *     Unit -> State.Navigation
		 */
		UnitAttackSelect,
		/*
		 * Registered :
		 *	   UIController
		 *     Unit
		 * Transitions :
		 *     
		 */
		UnitMove,
		/*
		 * Registered :
		 *     Unit
		 * Transitions : 
		 *     Unit -> State.Navigation
		 */
		UnitAttack,
		/*
		 * Registered :
		 *     
		 * Transitions :
		 *     
		 */
		DiceActionSelect,
		/*
		 * Registered :
		 *     UIController
		 *     Unit(EffectOnly)
		 *     Die
		 *     DiceThrower
		 * Transitions : 
		 *     Die -> State.Navigation, 
		 *     Die -> State.DiceActionSelect, 
		 *     DiceThrower -> State.DiceThrow, 
		 *     DiceThrower -> State.Navigation
		 */
		DiceThrow,
		/*
		 * Registered : 
		 *     DiceThrower
		 *     Unit(EffectOnly)
		 *     Dice
		 * Transitions : 
		 *     DiceThrower -> State.Navigation
		 */
		DiceAttack,
		/*
		 * Registered :
		 *     
		 * Transitions :
		 *     
		 */
		EndTurn,
		/*
		 * Registered : 
		 *     GameController
		 *     Unit
		 * Transitions : 
		 *     GameController -> State.StartTurn
		 */
	}
}


/*
		// ========================================================= Start Turn State =========================================================

		protected class StartTurnSB : StateBehaviour
		{
			// host reference
			private readonly T self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public StartTurnSB(Unit self)
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
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
			}
		}
 */
