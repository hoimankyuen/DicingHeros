using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DiceRoller
{
	public enum State
	{
		None,
		/*
		 * Parameters :
		 * Registered :
		 * Transitions :
		 */
		StartTurn,
		/*
		 * - Parameters :
		 * - Registered : GameController, UIController, Unit
		 * - Transitions : GameController -> State.Navitaion
		 */
		Navigation,
		/*
		 * - Parameters : player
		 * - Registered : UIController, Unit, Die 
		 * - Transitions : GameController -> State.EndTurn, Unit -> State.UnitActionSelect, Die -> State.DieActionSelect
		 */
		UnitActionSelect,
		/*
		 * Parameters : player, unit
		 * Registered : UIController, Unit
		 * Transitions : Unit -> State.UnitMovement, Unit -> State.Navigation
		 */
		UnitMove,
		/*
		 * Parameters : player, unit, startingTiles, path
		 * Registered : Unit
		 * Transitions : Unit -> State.Navigation
		 */
		DiceActionSelect,
		/*
		 * Parameters : player,  dice
		 * Registered : UIController, Unit(EffectOnly), Die, DiceThrower
		 * Transitions : Die -> State.Navigation, Die -> State.DiceActionSelect, DiceThrower -> State.DiceThrow, DiceThrower -> State.Navigation
		 */
		DiceThrow,
		/*
		 * Parameters : player,  dice
		 * Registered : DiceThrower, Unit(EffectOnly), Dice
		 * Transitions : DiceThrower -> State.Navigation
		 */
		DiceAttack,
		/*
		 * Parameters :
		 * Registered :
		 * Transitions :
		 */
		EndTurn,
		/*
		 * Parameters : player
		 * Registered : GameController, Unit
		 * Transitions : GameController -> State.StartTurn
		 */
	}

	public struct StateParams
	{
		public Player player; // -> GameController.CurrentPlayer ?
		public Unit unit; // -> Unit.SelectedUnit ?
		public List<Tile> startingTiles;  // -> Unit.StartingTiles ?
		public List<Tile> path; // -> Unit.Path ?
		public List<Die> dice; // -> Unit.SelectedDice ?
	}
}