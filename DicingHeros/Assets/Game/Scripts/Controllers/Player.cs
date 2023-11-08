using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DicingHeros
{
	[System.Serializable]
	public class Player
	{
		public int id;
		public string name;
		public int throws;
		public bool isAI;

		// ========================================================= Units =========================================================

		/// <summary>
		/// A read only list of all units of this player.
		/// </summary>
		public IReadOnlyList<Unit> Units
		{
			get
			{
				return _Units.AsReadOnly();
			}
		}
		private List<Unit> _Units = new List<Unit>();

		/// <summary>
		/// Event raised when the list of units of this player is changed.
		/// </summary>
		public event Action OnUnitsChanged = () => { };

		/// <summary>
		/// Add an unit to this player.
		/// </summary>
		public void AddUnit(Unit unit)
		{
			if (!_Units.Contains(unit))
			{
				_Units.Add(unit);
				_Units.Sort((a, b) => a.name.CompareTo(b.name));
				OnUnitsChanged.Invoke();
			}
		}

		/// <summary>
		/// Remove an unit from this player.
		/// </summary>
		public void RemoveUnit(Unit unit)
		{
			if (_Units.Contains(unit))
			{
				_Units.Remove(unit);
				_Units.Sort((a, b) => a.name.CompareTo(b.name));
				OnUnitsChanged.Invoke();
			}
		}

		// ========================================================= Dice =========================================================

		/// <summary>
		/// A read only list of all dice of this player.
		/// </summary>
		public IReadOnlyList<Die> Dice
		{
			get
			{
				return _Dice.AsReadOnly();
			}
		}
		private List<Die> _Dice = new List<Die>();

		/// <summary>
		/// Event raised when the list of dice of this player is changed.
		/// </summary>
		public event Action OnDiceChanged = () => { };

		/// <summary>
		/// Add a die to this player.
		/// </summary>
		public void AddDie(Die die)
		{
			if (!_Dice.Contains(die))
			{
				_Dice.Add(die);
				_Dice.Sort((a, b) => a.type.CompareTo(b.type));
				OnDiceChanged.Invoke();
			}
		}

		/// <summary>
		/// Remove a die from this player.
		/// </summary>
		public void RemoveDie(Die die)
		{
			if (_Dice.Contains(die))
			{
				_Dice.Remove(die);
				_Dice.Sort((a, b) => a.type.CompareTo(b.type));
				OnDiceChanged.Invoke();
			}
		}
	}
}