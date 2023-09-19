using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuickerEffects;
using UnityEngine;

namespace DiceRoller
{
	public partial class Unit : Item
	{
		// parameters
		[Header("Unit Parameters")]
		public int maxHealth = 20;
		public int baseMelee = 3;
		public int baseDefence = 4;
		public int baseMagic = 1;
		public int baseMovement = 4;

		public float moveTimePerTile = 0.2f;

		// working variables

		// events
		public Action onInspectionChanged = () => { };
		public Action onSelectionChanged = () => { };
		public Action onHealthChanged = () => { };
		public Action onStatChanged = () => { };

		// ========================================================= Properties =========================================================

		/// <summary>
		/// Flag for if this unit is currently being inspected.
		/// </summary>
		public bool IsBeingInspected
		{
			get
			{
				return inspectingUnits.Contains(this);
			}
		}
		private static UniqueList<Unit> inspectingUnits = new UniqueList<Unit>();

		/// <summary>
		/// Flag for if this unit is currently selected.
		/// </summary>
		public bool IsSelected
		{
			get
			{
				return selectedUnits.Contains(this);
			}
		}
		private static UniqueList<Unit> selectedUnits = new UniqueList<Unit>();

		/// <summary>
		/// Flag for if this unit has deplete its actions.
		/// </summary>
		public bool ActionDepleted { get; private set; } = false;

		/// <summary>
		/// The current health value of this unit.
		/// </summary>
		public int Health
		{
			get
			{
				return _health;
			}
			private set
			{
				if (_health != value)
				{
					_health = value;
					onHealthChanged.Invoke();
				}
			}
		}
		private int _health = 0;

		/// <summary>
		/// The current melee value of this unit.
		/// </summary>
		public int Melee 
		{
			get
			{
				return _melee;
			}
			private set
			{
				if (_melee != value)
				{
					_melee = value;
					onStatChanged.Invoke();
				}
			}
		}
		private int _melee = 0;

		/// <summary>
		/// The current defence value of this unit.
		/// </summary>
		public int Defence
		{
			get
			{
				return _defence;
			}
			private set
			{
				if (_defence != value)
				{
					_defence = value;
					onStatChanged.Invoke();
				}
			}
		}
		private int _defence = 0;

		/// <summary>
		/// The current magic value of this unit.
		/// </summary>
		public int Magic
		{
			get
			{
				return _magic;
			}
			private set
			{
				if (_magic != value)
				{
					_magic = value;
					onStatChanged.Invoke();
				}
			}
		}
		private int _magic = 0;

		/// <summary>
		/// The current movement value of this unit.
		/// </summary>
		public int Movement
		{
			get
			{
				return _movement;
			}
			private set
			{
				if (_movement != value)
				{
					_movement = value;
					onStatChanged.Invoke();
				}
			}
		}
		private int _movement = 0;


		/// <summary>
		/// The starting tiles of a selected movement path.
		/// </summary>
		public List<Tile> MovementStartingTiles { get; private set; } = new List<Tile>();

		/// <summary>
		/// Tiles in a selected movment path from start to end.
		/// </summary>
		public List<Tile> MovementSelectedPath { get; private set; } = new List<Tile>();

		// ========================================================= Inspection and Selection =========================================================

		/// <summary>
		/// Retrieve the first unit being currently inspected, return null if none is being inspected.
		/// </summary>
		public static Unit GetFirstBeingInspected()
		{
			return inspectingUnits.Count > 0 ? inspectingUnits[0] : null;
		}

		/// <summary>
		/// Retrieve the first currently selected unit, return null if none is selected.
		/// </summary>
		public static Unit GetFirstSelected()
		{
			return selectedUnits.Count > 0 ? selectedUnits[0] : null;
		}

		/// <summary>
		/// Retrieve all currently selected unit.
		/// </summary>
		public static IReadOnlyCollection<Unit> GetAllSelected()
		{
			return selectedUnits.AsReadOnly();
		}

		/// <summary>
		/// Clear the list of selected unit. 
		/// /// </summary>
		public static void ClearSelectedUnits()
		{
			for (int i = selectedUnits.Count - 1; i >= 0; i--)
			{
				selectedUnits[i].RemoveFromSelection();
			}
		}

		/// <summary>
		/// Add this unit to as being inspecting.
		/// </summary>
		private void AddToInspection()
		{
			if (!inspectingUnits.Contains(this))
			{
				inspectingUnits.Add(this);
				onInspectionChanged.Invoke();
			}
		}

		/// <summary>
		/// Remove this unit from as being inspecting.
		/// </summary>
		private void RemoveFromInspection()
		{
			if (inspectingUnits.Contains(this))
			{
				inspectingUnits.Remove(this);
				onInspectionChanged.Invoke();
			}
		}

		/// <summary>
		/// Add this unit to as selected.
		/// </summary>
		private void AddToSelection()
		{
			if (!selectedUnits.Contains(this))
			{
				selectedUnits.Add(this);
				onSelectionChanged.Invoke();
			}
		}

		/// <summary>
		/// Remove this unit from as selected.
		/// </summary>
		private void RemoveFromSelection()
		{
			if (selectedUnits.Contains(this))
			{
				selectedUnits.Remove(this);
				onSelectionChanged.Invoke();
			}
		}

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected override void Start()
		{
			base.Start();
			RegisterStateBehaviours();
			RegisterToPlayer();

			SetHealth(maxHealth);
			ResetStat();
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected override void Update()
		{
			base.Update();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected override void OnDestroy()
		{
			base.OnDestroy();
			DeregisterStateBehaviours();
			DeregisterFromPlayer();
		}

		/// <summary>
		/// OnDrawGizmos is called when the game object is in editor mode
		/// </summary>
		protected void OnDrawGizmos()
		{
			// draw size of the unit
			if (Application.isEditor)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(transform.position, size / 2);
			}
		}

		// ========================================================= Team Behaviour =========================================================

		/// <summary>
		/// Register this unit to a player.
		/// </summary>
		protected void RegisterToPlayer()
		{
			if (game == null)
				return;

			if (Player != null)
			{
				Player.units.Add(this);
			}
		}

		/// <summary>
		///  Deregister this unit from a player.
		/// </summary>
		protected void DeregisterFromPlayer()
		{
			if (game == null)
				return;

			if (Player != null)
			{
				Player.units.Remove(this);
			}
		}

		// ========================================================= Unit Behaviour =========================================================

		/// <summary>
		/// Add or remove health by a set amount.
		/// </summary>
		public void ChangeHealth(int delta)
		{
			SetHealth(Health + delta);
		}

		/// <summary>
		/// Directly set health to a specific value.
		/// </summary>
		public void SetHealth(int value)
		{
			Health = value;
		}

		/// <summary>
		/// Add or remove value of each stat by a set amount.
		/// </summary>
		public void ChangeStat(int meleeDelta = 0, int defenceDelta = 0, int magicDelta = 0, int movementDelta = 0)
		{
			SetStat(Melee + meleeDelta, Defence + defenceDelta, Magic + magicDelta, Movement + movementDelta);
		}

		/// <summary>
		/// Reset set to their original value.
		/// </summary>
		public void ResetStat()
		{
			SetStat(baseMelee, baseDefence, baseMagic, baseMovement);
		}

		/// <summary>
		/// Directly set each stat to a specific value.
		/// </summary>
		public void SetStat(int meleeValue, int defencevalue, int magicValue, int movementValue)
		{
			Melee = meleeValue;
			Defence = defencevalue;
			Magic = magicValue;
			Movement = movementValue;
		}

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.Register(this, State.Navigation, new NavigationSB(this));
			stateMachine.Register(this, State.UnitMoveSelect, new UnitMovSelectSB(this));
			stateMachine.Register(this, State.UnitAttackSelect, new UnitAttackSelectSB(this));
			stateMachine.Register(this, State.UnitMove, new UnitMoveSB(this));
			stateMachine.Register(this, State.DiceActionSelect, new EffectOnlySB(this));
			stateMachine.Register(this, State.DiceThrow, new EffectOnlySB(this));
			stateMachine.Register(this, State.EndTurn, new EndTurnSB(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		protected void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
			{
				stateMachine.DeregisterAll(this);
			}
		}
	}
}

