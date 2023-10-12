using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuickerEffects;
using UnityEngine;

namespace DiceRoller
{
	public partial class Unit : Item, IEquatable<Unit>
	{
		public enum UnitState
		{
			Standby,
			Moved,
			Depleted,
		}

		// parameters
		[Header("Unit Parameters")]
		public int maxHealth = 20;
		public int baseMelee = 3;
		public int baseDefence = 4;
		public int baseMagic = 1;
		public int baseMovement = 4;

		// readonly
		private readonly float moveTimePerTile = 0.2f;
		
		// ========================================================= Properties (IsBeingInspected) =========================================================

		/// <summary>
		/// Flag for if this unit is currently being inspected.
		/// </summary>
		public bool IsBeingInspected
		{
			get
			{
				return inspectingUnits.Contains(this);
			}
			private set
			{
				if (!inspectingUnits.Contains(this) && value)
				{
					inspectingUnits.Add(this);
					onInspectionChanged.Invoke();
				}
				if (inspectingUnits.Contains(this) && !value)
				{
					inspectingUnits.Remove(this);
					onInspectionChanged.Invoke();
				}
			}
		}
		private static UniqueList<Unit> inspectingUnits = new UniqueList<Unit>();

		/// <summary>
		/// Event raised when the inspection status of any unit is changed.
		/// </summary>
		public static event Action onInspectionChanged = () => { };

		/// <summary>
		/// Retrieve the first unit being currently inspected, return null if none is being inspected.
		/// </summary>
		public static Unit GetFirstBeingInspected()
		{
			return inspectingUnits.Count > 0 ? inspectingUnits[0] : null;
		}

		// ========================================================= Properties (IsSelected) =========================================================

		/// <summary>
		/// Flag for if this unit is currently selected.
		/// </summary>
		public bool IsSelected
		{
			get
			{
				return selectedUnits.Contains(this);
			}
			private set
			{
				if (!selectedUnits.Contains(this) && value)
				{
					selectedUnits.Add(this);
					onSelectionChanged.Invoke();
				}
				if (selectedUnits.Contains(this) && !value)
				{
					selectedUnits.Remove(this);
					onSelectionChanged.Invoke();
				}
			}
		}
		private static UniqueList<Unit> selectedUnits = new UniqueList<Unit>();

		/// <summary>
		/// Event raised when the selection status of any die is changed.
		/// </summary>
		public static event Action onSelectionChanged = () => { };

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
				selectedUnits[i].IsSelected = false;
			}
		}

		// ========================================================= Properties (IsBeingDragged) =========================================================

		/// <summary>
		/// Flag for if this unit is currently begin dragged.
		/// </summary>
		public bool IsBeingDragged
		{
			get
			{
				return draggingUnits.Contains(this);
			}
			private set
			{
				if (!draggingUnits.Contains(this) && value)
				{
					draggingUnits.Add(this);
					onDragChanged.Invoke();
				}
				if (draggingUnits.Contains(this) && !value)
				{
					draggingUnits.Remove(this);
					onDragChanged.Invoke();
				}
			}
		}
		private static UniqueList<Unit> draggingUnits = new UniqueList<Unit>();

		/// <summary>
		/// Event raised when the drag status of any die is changed.
		/// </summary>
		public static event Action onDragChanged = () => { };

		/// <summary>
		/// Retrieve the first unit being currently dragged, return null if none is selected.
		/// </summary>
		public static Unit GetFirstBeingDragged()
		{
			return draggingUnits.Count > 0 ? draggingUnits[0] : null;
		}

		// ========================================================= Properties (Health) =========================================================

		/// <summary>
		/// The current health value of this unit.
		/// </summary>
		public int Health 
		{
			get
			{
				return _Health;
			}
			private set
			{
				int clampedValue = Mathf.Clamp(value, 0, maxHealth);
				if (_Health != clampedValue)
				{
					_Health = clampedValue;
					onHealthChanged.Invoke();
				}
			}
		}
		private int _Health = 0;

		/// <summary>
		/// Event raised when health of this unit is changed.
		/// </summary>
		public event Action onHealthChanged = () => { };

		/// <summary>
		/// The proposed health change delta (damage or heal) to this unit.
		/// </summary>
		public int PendingHealthDelta
		{
			get
			{
				return _pendingHealthDelta;
			}
			private set
			{
				if (_pendingHealthDelta != value)
				{
					_pendingHealthDelta = value;
					onPendingHealthDeltaChange.Invoke();
				}
			}
		}
		private int _pendingHealthDelta;

		/// <summary>
		/// Event raised when pending health delta of this unit is changed.
		/// </summary>
		public event Action onPendingHealthDeltaChange = () => { };

		/// <summary>
		/// Set the health value to the inital value.
		/// </summary>
		public void SetupInitialHealth()
		{
			Health = maxHealth;
		}

		// ========================================================= Properties (Stats) =========================================================


		/// <summary>
		/// The current melee value of this unit.
		/// </summary>
		public int Melee 
		{ 
			get
			{
				return _Melee;
			}
			private set
			{
				int clampedValue = value < 0 ? 0 : value;
				if (_Melee != clampedValue)
				{
					_Melee = clampedValue;
					onStatChanged.Invoke();
				}
			}
		}
		private int _Melee = 0;

		/// <summary>
		/// The current defence value of this unit.
		/// </summary>
		public int Defence
		{
			get
			{
				return _Defence;
			}
			private set
			{
				int clampedValue = value < 0 ? 0 : value;
				if (_Defence != clampedValue)
				{
					_Defence = clampedValue;
					onStatChanged.Invoke();
				}
			}
		}
		private int _Defence = 0;

		/// <summary>
		/// The current magic value of this unit.
		/// </summary>
		public int Magic
		{
			get
			{
				return _Magic;
			}
			private set
			{
				int clampedValue = value < 0 ? 0 : value;
				if (_Magic != clampedValue)
				{
					_Magic = clampedValue;
					onStatChanged.Invoke();
				}
			}
		}
		private int _Magic = 0;

		/// <summary>
		/// The current movement value of this unit.
		/// </summary>
		public int Movement
		{
			get
			{
				return _Movement;
			}
			private set
			{
				int clampedValue = value < 0 ? 0 : value;
				if (_Movement != clampedValue)
				{
					_Movement = clampedValue;
					onStatChanged.Invoke();
				}
			}
		}
		private int _Movement = 0;

		/// <summary>
		/// Event raised when either stat of this unit is changed.
		/// </summary>
		public event Action onStatChanged = () => { };

		/// <summary>
		/// Set the stat values to the inital value.
		/// </summary>
		public void SetupInitalStat()
		{
			Melee = baseMelee;
			Defence = baseDefence;
			Magic = baseMagic;
			Movement = baseMovement;
		}

		/// <summary>
		/// Apply a change on one or more stat variables.
		/// </summary>
		public void ChangeStat(int meleeDelta = 0, int defenceDelta = 0, int magicDelta = 0, int movementDelta = 0)
		{
			Melee += meleeDelta;
			Defence += defenceDelta;
			Magic += magicDelta;
			Movement += movementDelta;
		}

		// ========================================================= Properties (Equipment) =========================================================

		/// <summary>
		/// All equpiments this unit pocesses.
		/// </summary>
		public List<Equipment> Equipments { get; private set; } = new List<Equipment>();

		/// <summary>
		/// Event raised when the equipments of this unit is changed.
		/// </summary>
		public event Action onEquipmentChanged = () => { };

		/// <summary>
		/// Add the starting equipments to this unit.
		/// </summary>
		public void SetupInitialEquipments()
		{
			Equipments.Add(new SimpleKnife(this));
			Equipments.Add(new SimpleShoe(this));
			onEquipmentChanged.Invoke();
		}

		/// <summary>
		/// Allows the Update method to function on each equipments.
		/// </summary>
		public void UpdateEquipments()
		{
			foreach (Equipment equipment in Equipments)
			{
				equipment.Update();
			}
		}

		// ========================================================= Properties (CurrentUnitState) =========================================================

		/// <summary>
		/// The state of action of this unit.
		/// </summary>
		public UnitState CurrentUnitState
		{ 
			get
			{
				return _CurrentUnitState;
			}
			private set
			{
				if (_CurrentUnitState != value)
				{
					/*
					switch (value)
					{
						
					
					}
					*/

					_CurrentUnitState = value;
					onUnitStateChange.Invoke();
				}
			}
		} 
		private UnitState _CurrentUnitState = UnitState.Standby;

		/// <summary>
		/// Event raised when the unit state of this unit is changed.
		/// </summary>
		public event Action onUnitStateChange = () => { };

		// ========================================================= Properties (Actions) =========================================================

		/// <summary>
		/// The next movement action chosen by the player.
		/// </summary>
		public UnitMovement NextMovement { get; private set; } = null;

		/// <summary>
		/// The next acttack action chosen by the player.
		/// </summary>
		public UnitAttack NextAttack { get; private set; } = null;

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

			SetupInitialEquipments();

			SetupInitialHealth();
			SetupInitalStat();
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected override void Update()
		{
			base.Update();

			UpdateEquipments();
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

		// ========================================================= IEqautable Methods =========================================================

		/// <summary>
		/// Check if this object is equal to the other object.
		/// </summary>
		public bool Equals(Unit other)
		{
			return this == other;
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

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.Register(gameObject, this, SMState.Navigation, new NavigationSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitMoveSelect, new UnitMoveSelectSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitMove, new UnitMoveSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitAttackSelect, new UnitAttackSelectSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitAttack, new UnitAttackSB(this));
			stateMachine.Register(gameObject, this, SMState.DiceActionSelect, new EffectOnlySB(this));
			stateMachine.Register(gameObject, this, SMState.DiceThrow, new EffectOnlySB(this));
			stateMachine.Register(gameObject, this, SMState.EndTurn, new EndTurnSB(this));
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

