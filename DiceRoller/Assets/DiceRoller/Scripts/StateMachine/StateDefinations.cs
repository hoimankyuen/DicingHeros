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
		 * Transitions :
		 */
		StartTurn,
		/*
		 * - Registered : GameController, UIController, Unit
		 * - Transitions : GameController -> State.Navitaion
		 */
		Navigation,
		/*
		 * - Registered : UIController, Unit, Die 
		 * - Transitions : GameController -> State.EndTurn, Unit -> State.UnitActionSelect, Die -> State.DieActionSelect
		 */
		UnitActionSelect,
		/*
		 * Registered : UIController, Unit
		 * Transitions : Unit -> State.UnitMovement, Unit -> State.Navigation
		 */
		UnitMove,
		/*
		 * Registered : Unit
		 * Transitions : Unit -> State.Navigation
		 */
		DiceActionSelect,
		/*
		 * Registered : UIController, Unit(EffectOnly), Die, DiceThrower
		 * Transitions : Die -> State.Navigation, Die -> State.DiceActionSelect, DiceThrower -> State.DiceThrow, DiceThrower -> State.Navigation
		 */
		DiceThrow,
		/*
		 * Registered : DiceThrower, Unit(EffectOnly), Dice
		 * Transitions : DiceThrower -> State.Navigation
		 */
		DiceAttack,
		/*
		 * Registered :
		 * Transitions :
		 */
		EndTurn,
		/*
		 * Registered : GameController, Unit
		 * Transitions : GameController -> State.StartTurn
		 */
	}
}