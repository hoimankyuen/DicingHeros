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
			Defeated,
		}

		public enum AttackType
		{
			None,
			Physical,
			Magical,
		}


		[System.Serializable]
		public enum AITendency
		{
			None,
			Rush,
			Follow,
			Stay,
		}

		// parameters
		[Header("Unit Parameters (General)")]
		public int maxHealth = 20;
		public int baseMovement = 4;

		[Header("Unit Parameters (Physical)")]
		public int basePhysicalAttack = 3;
		public int basePhysicalDefence = 4;
		public int basePhysicalRange = 1;

		[Header("Unit Parameters (Magical)")]
		public int baseMagicalAttack = 1;
		public int baseMagicalDefence = 4;
		public int baseMagicalRange = 1;

		[HideInInspector]
		public float baseKnockbackForce = 0.25f;

		[Header("Unit Equipments")]
		public List<EquipmentDictionary.Name> startingEquipment = new List<EquipmentDictionary.Name>();

		[Header("AI Parameters (Only used if AI Controlled)")]
		public AITendency aiTendency = AITendency.None;

		// readonly
		private readonly float moveTimePerTile = 0.2f;

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

			RegisterCallbacksForAllOccupiedTiles();
			RegisterCallbacksForAllOccupiedTilesExceptSelf();
			RegisterCallbacksForMovableArea();
			RegisterCallbacksForAttackableArea();
			RegisterCallbacksForPredictedAttackableArea();

			SetupInitialEquipments();

			SetupInitialHealth();
			SetupInitalStat();
			SetupInitalAttackType();
			ResetAttackAreaRule();
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected override void Update()
		{
			base.Update();

			UpdateEquipments();
			DetectFallen();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected override void OnDestroy()
		{
			base.OnDestroy();

			DeregisterStateBehaviours();
			DeregisterFromPlayer();

			DeregisterCallbacksForAllOccupiedTiles();
			DeregisterCallbacksForAllOccupiedTilesExceptSelf();
			DeregisterCallbacksForMovableArea();
			DeregisterCallbacksForAttackableArea();
			DeregisterCallbacksForPredictedAttackableArea();
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
				Player.AddUnit(this);
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
				Player.RemoveUnit(this);
			}
		}

		// ========================================================= Properties (IsBlocking) =========================================================

		/// <summary>
		/// Flag for if this unit is currently blocking a movement.
		/// </summary>
		public bool IsBlocking
		{
			get
			{
				return _BlockingUnits.Contains(this);
			}
			private set
			{
				if (!_BlockingUnits.Contains(this) && value)
				{
					_BlockingUnits.Add(this);
					OnBlockingChanged.Invoke();
					OnAnyBlockingChanged.Invoke();
				}
				else if (_BlockingUnits.Contains(this) && !value)
				{
					_BlockingUnits.Remove(this);
					OnBlockingChanged.Invoke();
					OnAnyBlockingChanged.Invoke();
				}
			}
		}
		private readonly static UniqueList<Unit> _BlockingUnits = new UniqueList<Unit>();

		/// <summary>
		/// Event raised when the blocking status of this unit is changed.
		/// </summary>
		public event Action OnBlockingChanged = () => { };

		/// <summary>
		/// Event raised when the list of units blocking a movement is changed.
		/// </summary>
		public static event Action OnAnyBlockingChanged = () => { };

		/// <summary>
		/// Retrieve all units currently blocking a movement.
		/// </summary>
		public static IReadOnlyCollection<Unit> GetAllBlocking()
		{
			return _BlockingUnits.AsReadOnly();
		}

		/// <summary>
		/// Clear the list of unit currently blocking a movement. 
		/// /// </summary>
		public static void ClearBlockingUnits()
		{
			for (int i = _BlockingUnits.Count - 1; i >= 0; i--)
			{
				_BlockingUnits[i].IsBlocking = false;
			}
		}

		// ========================================================= Properties (IsInRange) =========================================================

		/// <summary>
		/// Flag for if this unit is currently is in range of the current unit's attack.
		/// </summary>
		public bool IsInRange
		{
			get
			{
				return _InRangeUnits.Contains(this);
			}
			private set
			{
				if (!_InRangeUnits.Contains(this) && value)
				{
					_InRangeUnits.Add(this);
					OnInRangeChanged.Invoke();
					OnAnyInRangeChanged.Invoke();
				}
				else if (_InRangeUnits.Contains(this) && !value)
				{
					_InRangeUnits.Remove(this);
					OnInRangeChanged.Invoke();
					OnAnyInRangeChanged.Invoke();
				}
			}
		}
		private readonly static UniqueList<Unit> _InRangeUnits = new UniqueList<Unit>();

		/// <summary>
		/// Event raised when the in range status of this unit is changed.
		/// </summary>
		public event Action OnInRangeChanged = () => { };

		/// <summary>
		/// Event raised when the list of units in range is changed.
		/// </summary>
		public static event Action OnAnyInRangeChanged = () => { };

		/// <summary>
		/// Retrieve all currently unit in range of current unit's attack.
		/// </summary>
		public static IReadOnlyCollection<Unit> GetAllInRange()
		{
			return _InRangeUnits.AsReadOnly();
		}

		/// <summary>
		/// Clear the list of units currently in range of current unit's attack. 
		/// /// </summary>
		public static void ClearInRangeUnits()
		{
			for (int i = _InRangeUnits.Count - 1; i >= 0; i--)
			{
				_InRangeUnits[i].IsInRange = false;
			}
		}

		// ========================================================= Properties (IsTargetable) =========================================================

		/// <summary>
		/// Flag for if this unit is currently targetable by the current unit's attack.
		/// </summary>
		public bool IsTargetable
		{
			get
			{
				return _TargetableUnits.Contains(this);
			}
			private set
			{
				if (!_TargetableUnits.Contains(this) && value)
				{
					_TargetableUnits.Add(this);
					OnTargetableChanged.Invoke();
					OnAnyTargetableChanged.Invoke();
				}
				else if (_TargetableUnits.Contains(this) && !value)
				{
					_TargetableUnits.Remove(this);
					OnTargetableChanged.Invoke();
					OnAnyTargetableChanged.Invoke();
				}
			}
		}
		private readonly static UniqueList<Unit> _TargetableUnits = new UniqueList<Unit>();

		/// <summary>
		/// Event raised when the targetable status of this unit is changed.
		/// </summary>
		public event Action OnTargetableChanged = () => { };

		/// <summary>
		/// Event raised when the list of units targetable is changed.
		/// </summary>
		public static event Action OnAnyTargetableChanged = () => { };

		/// <summary>
		/// Retrieve all currently targetable unit.
		/// </summary>
		public static IReadOnlyCollection<Unit> GetAllTargetable()
		{
			return _TargetableUnits.AsReadOnly();
		}

		/// <summary>
		/// Clear the list of selected unit. 
		/// /// </summary>
		public static void ClearTargetableUnits()
		{
			for (int i = _TargetableUnits.Count - 1; i >= 0; i--)
			{
				_TargetableUnits[i].IsTargetable = false;
			}
		}

		// ========================================================= Properties (IsBeingInspected) =========================================================

		/// <summary>
		/// Flag for if this unit is currently being inspected.
		/// </summary>
		public bool IsBeingInspected
		{
			get
			{
				return _InspectingUnits.Contains(this);
			}
			private set
			{
				if (!_InspectingUnits.Contains(this) && value)
				{
					_InspectingUnits.Add(this);
					OnInspectionChanged.Invoke();
					OnAnyBeingInspectedChanged.Invoke();
				}
				else if (_InspectingUnits.Contains(this) && !value)
				{
					_InspectingUnits.Remove(this);
					OnInspectionChanged.Invoke();
					OnAnyBeingInspectedChanged.Invoke();
				}
			}
		}
		private readonly static UniqueList<Unit> _InspectingUnits = new UniqueList<Unit>();

		/// <summary>
		/// Event raised when the inspection status of this unit is changed.
		/// </summary>
		public event Action OnInspectionChanged = () => { };

		/// <summary>
		/// Event raised when the the list of units being inspected is changed.
		/// </summary>
		public static event Action OnAnyBeingInspectedChanged = () => { };

		/// <summary>
		/// Retrieve the first unit being currently inspected, return null if none is being inspected.
		/// </summary>
		public static Unit GetFirstBeingInspected()
		{
			return _InspectingUnits.Count > 0 ? _InspectingUnits[0] : null;
		}

		/// <summary>
		/// Retrieve all units being currently inspected.
		/// </summary>
		public static IReadOnlyCollection<Unit> GetAllBeingInspected()
		{
			return _InspectingUnits.AsReadOnly();
		}

		// ========================================================= Properties (IsSelected) =========================================================

		/// <summary>
		/// Flag for if this unit is currently selected.
		/// </summary>
		public bool IsSelected
		{
			get
			{
				return _SelectedUnits.Contains(this);
			}
			private set
			{
				if (!_SelectedUnits.Contains(this) && value)
				{
					_SelectedUnits.Add(this);
					OnSelectionChanged.Invoke();
					OnAnySelectedChanged.Invoke();
				}
				else if (_SelectedUnits.Contains(this) && !value)
				{
					_SelectedUnits.Remove(this);
					OnSelectionChanged.Invoke();
					OnAnySelectedChanged.Invoke();
				}
			}
		}
		private readonly static UniqueList<Unit> _SelectedUnits = new UniqueList<Unit>();

		/// <summary>
		/// Event raised when the selection status of this unit is changed.
		/// </summary>
		public event Action OnSelectionChanged = () => { };

		/// <summary>
		/// Event raised when the list of units selected is changed.
		/// </summary>
		public static event Action OnAnySelectedChanged = () => { };

		/// <summary>
		/// Retrieve the first currently selected unit, return null if none is selected.
		/// </summary>
		public static Unit GetFirstSelected()
		{
			return _SelectedUnits.Count > 0 ? _SelectedUnits[0] : null;
		}

		/// <summary>
		/// Retrieve all currently selected unit.
		/// </summary>
		public static IReadOnlyCollection<Unit> GetAllSelected()
		{
			return _SelectedUnits.AsReadOnly();
		}

		/// <summary>
		/// Clear the list of selected unit. 
		/// /// </summary>
		public static void ClearSelectedUnits()
		{
			for (int i = _SelectedUnits.Count - 1; i >= 0; i--)
			{
				_SelectedUnits[i].IsSelected = false;
			}
		}

		// ========================================================= Properties (IsBeingDragged) =========================================================

		/*
		/// <summary>
		/// Flag for if this unit is currently begin dragged.
		/// </summary>
		public bool IsBeingDragged
		{
			get
			{
				return _DraggingUnits.Contains(this);
			}
			private set
			{
				if (!_DraggingUnits.Contains(this) && value)
				{
					_DraggingUnits.Add(this);
					OnDragChanged.Invoke();
					OnItemBeingDraggedChanged.Invoke();
				}
				else if (_DraggingUnits.Contains(this) && !value)
				{
					_DraggingUnits.Remove(this);
					OnDragChanged.Invoke();
					OnItemBeingDraggedChanged.Invoke();
				}
			}
		}
		private static UniqueList<Unit> _DraggingUnits = new UniqueList<Unit>();

		/// <summary>
		/// Event raised when the drag status of any this is changed.
		/// </summary>
		public event Action OnDragChanged = () => { };

		/// <summary>
		/// Event raised when the list of units being dragged is changed.
		/// </summary>
		public static event Action OnItemBeingDraggedChanged = () => { };

		/// <summary>
		/// Retrieve the first unit being currently dragged, return null if none is selected.
		/// </summary>
		public static Unit GetFirstBeingDragged()
		{
			return _DraggingUnits.Count > 0 ? _DraggingUnits[0] : null;
		}
		*/

		// ========================================================= Properties (AllOccupiedTiles) =========================================================

		/// <summary>
		/// All tiles that are occupied by every unit.
		/// </summary>
		public static IReadOnlyList<Tile> AllOccupiedTiles
		{
			get
			{
				if (_IsAllOccupiedTilesDirty)
				{
					RefreshAllOccupiedTiles();
				}
				return _AllOccupiedTiles.AsReadOnly();
			}
		}
		private static List<Tile> _AllOccupiedTiles = new List<Tile>();
		private static bool _IsAllOccupiedTilesDirty = true;

		/// <summary>
		/// Event raised when the list of all tiles occupied by every unit needs updating.
		/// </summary>
		public static event Action OnAllOcupiedTilesDirty = () => { };

		/// <summary>
		/// Register all necessary callbacks needed for AllOccupiedTiles.
		/// </summary>
		private void RegisterCallbacksForAllOccupiedTiles()
		{
			OnOccupiedTilesChanged += SetAllOccupiedTilesDirty;
		}

		/// <summary>
		/// Deregister all necessary callbacks needed for AllOccupiedTiles.
		/// </summary>
		private void DeregisterCallbacksForAllOccupiedTiles()
		{
			OnOccupiedTilesChanged -= SetAllOccupiedTilesDirty;
		}

		/// <summary>
		/// Notify AllOccupiedTiles needs updating.
		/// </summary>
		private static void SetAllOccupiedTilesDirty()
		{
			if (!_IsAllOccupiedTilesDirty)
			{
				_IsAllOccupiedTilesDirty = true;
				OnAllOcupiedTilesDirty.Invoke();
			}
		}

		/// <summary>
		/// Retrieve again the list of all tiles that are occupied by any unit.
		/// </summary>
		private static void RefreshAllOccupiedTiles()
		{
			_AllOccupiedTiles.Clear();
			foreach (Player player in GameController.current.GetAllPlayers())
			{
				foreach (Unit unit in player.Units.Where(x => x.CurrentUnitState != UnitState.Defeated))
				{
					_AllOccupiedTiles.AddRange(unit.OccupiedTiles.Except(_AllOccupiedTiles));
				}
			}
			_IsAllOccupiedTilesDirty = false;
		}

		// ========================================================= Properties (AllOccupiedTilesExceptSelf) =========================================================

		/// <summary>
		/// A list of all occupied tiles except the ones that this unit occupied. Useful for movement calculation.
		/// </summary>
		public IReadOnlyCollection<Tile> AllOccupiedTilesExceptSelf
		{
			get
			{
				if (_IsAllOccupiedTilesExceptSelfDirty)
				{
					RefreshAllOccupiedTilesExceptSelf();
				}
				return _AllOccupiedTilesExceptSelf.AsReadOnly();
			}
		}
		private List<Tile> _AllOccupiedTilesExceptSelf = new List<Tile>();
		private bool _IsAllOccupiedTilesExceptSelfDirty = true;

		/// <summary>
		/// Event raised when the list of all tiles occupied by every unit needs updating.
		/// </summary>
		public event Action OnAllOcupiedTilesExceptSelfDirty = () => { };

		/// <summary>
		/// Register all necessary callbacks needed for AllOccupiedTilesExceptSelf.
		/// </summary>
		private void RegisterCallbacksForAllOccupiedTilesExceptSelf()
		{
			OnAllOcupiedTilesDirty += SetAllOccupiedTilesExceptSelfDirty;
			OnOccupiedTilesChanged += SetAllOccupiedTilesExceptSelfDirty;
		}

		/// <summary>
		/// Deregister all necessary callbacks needed for AllOccupiedTilesExceptSelf.
		/// </summary>
		private void DeregisterCallbacksForAllOccupiedTilesExceptSelf()
		{
			OnAllOcupiedTilesDirty -= SetAllOccupiedTilesExceptSelfDirty;
			OnOccupiedTilesChanged -= SetAllOccupiedTilesExceptSelfDirty;
		}


		/// <summary>
		/// Notify AllOccupiedTilesExecptSelf needs updating.
		/// </summary>
		private void SetAllOccupiedTilesExceptSelfDirty()
		{
			if (!_IsAllOccupiedTilesExceptSelfDirty)
			{
				_IsAllOccupiedTilesExceptSelfDirty = true;
				OnAllOcupiedTilesExceptSelfDirty.Invoke();
			}
		}

		/// <summary>
		/// Retrieve again the list of all tiles that are occupied by any unit except the one this unit occupied.
		/// </summary>
		private void RefreshAllOccupiedTilesExceptSelf()
		{
			_AllOccupiedTilesExceptSelf.Clear();
			_AllOccupiedTilesExceptSelf.AddRange(AllOccupiedTiles.Except(OccupiedTiles));
			_IsAllOccupiedTilesExceptSelfDirty = false;
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
					OnHealthChanged.Invoke();
				}
			}
		}
		private int _Health = 0;

		/// <summary>
		/// Event raised when health of this unit is changed.
		/// </summary>
		public event Action OnHealthChanged = () => { };

		/// <summary>
		/// The proposed health change delta (damage or heal) to this unit.
		/// </summary>
		public int PendingHealthDelta
		{
			get
			{
				return _PendingHealthDelta;
			}
			private set
			{
				if (_PendingHealthDelta != value)
				{
					_PendingHealthDelta = value;
					OnPendingHealthDeltaChanged.Invoke();
				}
			}
		}
		private int _PendingHealthDelta;

		/// <summary>
		/// Event raised when pending health delta of this unit is changed.
		/// </summary>
		public event Action OnPendingHealthDeltaChanged = () => { };

		/// <summary>
		/// Flag for if this unit is recieving damage from and other source, regardless of damage value.
		/// </summary>
		public bool IsRecievingDamage
		{
			get
			{
				return _IsRecievingDamage;
			}
			private set
			{
				if (_IsRecievingDamage != value)
				{
					_IsRecievingDamage = value;
					OnPendingHealthDeltaChanged.Invoke();
				}
			}
		}
		private bool _IsRecievingDamage = false;

		/// <summary>
		/// Event raised when the flag for is reciving damage is changed.
		/// </summary>
		public event Action OnIsRecievingDamageChanged = () => { };

		/// <summary>
		/// Set the health value to the inital value.
		/// </summary>
		public void SetupInitialHealth()
		{
			Health = maxHealth;
		}

		// ========================================================= Properties (Stats) =========================================================

		/// <summary>
		/// The current physical attack value of this unit.
		/// </summary>
		public int PhysicalAttack
		{
			get
			{
				return _PhysicalAttack;
			}
			private set
			{
				int clampedValue = value < 0 ? 0 : value;
				if (_PhysicalAttack != clampedValue)
				{
					_PhysicalAttack = clampedValue;
					OnPhysicalAttackChanged.Invoke();
				}
			}
		}
		private int _PhysicalAttack = 0;

		/// <summary>
		/// Event raised when the physical attack stat of this unit is changed.
		/// </summary>
		public event Action OnPhysicalAttackChanged = () => { };

		/// <summary>
		/// The current magical defence value of this unit.
		/// </summary>
		public int PhysicalDefence
		{
			get
			{
				return _PhysicalDefence;
			}
			private set
			{
				int clampedValue = value < 0 ? 0 : value;
				if (_PhysicalDefence != clampedValue)
				{
					_PhysicalDefence = clampedValue;
					OnPhysicalDefenceChanged.Invoke();
				}
			}
		}
		private int _PhysicalDefence = 0;

		/// <summary>
		/// Event raised when the phhysical defence stat of this unit is changed.
		/// </summary>
		public event Action OnPhysicalDefenceChanged = () => { };

		/// <summary>
		/// The current physical range value of this unit.
		/// </summary>
		public int PhysicalRange
		{
			get
			{
				return _PhysicalRange;
			}
			private set
			{
				int clampedValue = value < 0 ? 0 : value;
				if (_PhysicalRange != clampedValue)
				{
					_PhysicalRange = clampedValue;
					OnPhysicalRangeChanged.Invoke();
				}
			}
		}
		private int _PhysicalRange = 0;

		/// <summary>
		/// Event raised when the physical range stat of this unit is changed.
		/// </summary>
		public event Action OnPhysicalRangeChanged = () => { };

		/// The current magical attack value of this unit.
		/// </summary>
		public int MagicalAttack
		{
			get
			{
				return _MagicalAttack;
			}
			private set
			{
				int clampedValue = value < 0 ? 0 : value;
				if (_MagicalAttack != clampedValue)
				{
					_MagicalAttack = clampedValue;
					OnMagicalAttackChanged.Invoke();
				}
			}
		}
		private int _MagicalAttack = 0;

		/// <summary>
		/// Event raised when the magical attack stat of this unit is changed.
		/// </summary>
		public event Action OnMagicalAttackChanged = () => { };

		/// <summary>
		/// The current magical defence value of this unit.
		/// </summary>
		public int MagicalDefence
		{
			get
			{
				return _MagicalDefence;
			}
			private set
			{
				int clampedValue = value < 0 ? 0 : value;
				if (_MagicalDefence != clampedValue)
				{
					_MagicalDefence = clampedValue;
					OnMagicalDefenceChanged.Invoke();
				}
			}
		}
		private int _MagicalDefence = 0;

		/// <summary>
		/// Event raised when the magical defence stat of this unit is changed.
		/// </summary>
		public event Action OnMagicalDefenceChanged = () => { };

		/// <summary>
		/// The current magic range value of this unit.
		/// </summary>
		public int MagicalRange
		{
			get
			{
				return _MagicalRange;
			}
			private set
			{
				int clampedValue = value < 0 ? 0 : value;
				if (_MagicalRange != clampedValue)
				{
					_MagicalRange = clampedValue;
					OnMagicalRangeChanged.Invoke();
				}
			}
		}
		private int _MagicalRange = 0;

		/// <summary>
		/// Event raised when the magical range stat of this unit is changed.
		/// </summary>
		public event Action OnMagicalRangeChanged = () => { };

		/// <summary>
		/// The current knockback force of this unit.
		/// </summary>
		public float KnockbackForce
		{
			get
			{
				return _KnockbackForce;
			}
			private set
			{
				float clampedValue = value < 0 ? 0 : value;
				if (_KnockbackForce != clampedValue)
				{
					_KnockbackForce = clampedValue;
					OnKnockbackChanged.Invoke();
				}
			}
		}
		private float _KnockbackForce = 0;

		/// <summary>
		/// Event raised when the knock back stat of this unit is changed.
		/// </summary>
		public event Action OnKnockbackChanged = () => { };

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
					OnMovementChanged.Invoke();
				}
			}
		}
		private int _Movement = 0;

		/// <summary>
		/// Event raised when the movement stat of this unit is changed.
		/// </summary>
		public event Action OnMovementChanged = () => { };

		/// <summary>
		/// Set the stat values to the inital value.
		/// </summary>
		public void SetupInitalStat()
		{
			PhysicalAttack = basePhysicalAttack;
			PhysicalDefence = basePhysicalDefence;
			PhysicalRange = basePhysicalRange;
			MagicalAttack = baseMagicalAttack;
			MagicalDefence = baseMagicalDefence;
			MagicalRange = baseMagicalRange;
			KnockbackForce = baseKnockbackForce;
			Movement = baseMovement;
		}

		/// <summary>
		/// Apply a change on one or more stat variables.
		/// </summary>
		public void ChangeStat(int physicalAttackDelta = 0, int physicalDefenceDelta = 0, int physicalRangeDelta = 0, int magicalAttackDelta = 0, int magicalDefenceDelta = 0, int magicalRangeDelta = 0, float knockbackForceDelta = 0, int movementDelta = 0)
		{
			PhysicalAttack += physicalAttackDelta;
			PhysicalDefence += physicalDefenceDelta;
			PhysicalRange += physicalRangeDelta;
			MagicalAttack += magicalAttackDelta;
			MagicalDefence += magicalDefenceDelta;
			MagicalRange += magicalRangeDelta;
			KnockbackForce += knockbackForceDelta;
			Movement += movementDelta;
		}

		// ========================================================= Properties (AttackRangeRule) =========================================================

		/// <summary>
		/// The Rule that dictates the attack area of this unit.
		/// </summary>
		public AttackAreaRule AttackAreaRule
		{
			get
			{
				return _AttackAreaRule;
			}
			private set
			{
				if (_AttackAreaRule != value)
				{
					_AttackAreaRule = value;
					OnAttackAreaRuleChanged.Invoke();
				}
			}
		}
		private AttackAreaRule _AttackAreaRule = null;

		/// <summary>
		/// Event raised when the rule that dictates the attack area of this unit is changed. 
		/// </summary>
		public event Action OnAttackAreaRuleChanged = () => { };

		/// <summary>
		/// Reset the rule to determine which tiles are attackable to the basic melee form.
		/// </summary>
		public void ResetAttackAreaRule()
		{
			_AttackAreaRule = AttackAreaRule.Adjacent;
		}

		/// <summary>
		/// Apply a different attack area rule tooo this unit.
		/// </summary>
		public void ChangeAttackAreaRule(AttackAreaRule rule)
		{
			AttackAreaRule = rule;
		}

		// ========================================================= Properties (AttackType) =========================================================

		/// <summary>
		/// The current attack type to be performed by this unit.
		/// </summary>
		public AttackType CurrentAttackType
		{
			get
			{
				return _CurrentAttackType;
			}
			private set
			{
				if (_CurrentAttackType != value)
				{
					_CurrentAttackType = value;
					OnCurrentAttackTypeChanged.Invoke();
				}
			}
		}
		private AttackType _CurrentAttackType = AttackType.None;

		/// <summary>
		/// Event raised when the current attack type to be performed by this unit is changed.
		/// </summary>
		public event Action OnCurrentAttackTypeChanged = () => { };

		/// <summary>
		/// Set the attack type to the inital value.
		/// </summary>
		public void SetupInitalAttackType()
		{
			CurrentAttackType = AttackType.Physical;
		}

		/// <summary>
		/// Change the attack type to something else.
		/// </summary>
		public void ChangeAttackType(AttackType type)
		{
			CurrentAttackType = type;
		}

		// ========================================================= Properties (MoveableArea) =========================================================

		/// <summary>
		/// A Readonly list of all tiles that are movable to by this unit.
		/// </summary>
		public IReadOnlyList<Tile> MovableArea
		{
			get
			{
				if (_IsMoveableAreaDirty)
				{
					RefreshMoveableArea();
				}
				return _MoveableArea.AsReadOnly();
			}
		}
		private List<Tile> _MoveableArea = new List<Tile>();
		private bool _IsMoveableAreaDirty = true;

		/// <summary>
		/// Event raised when the list of movable tiles by this unit needs updating.
		/// </summary>
		public event Action OnMoveableAreaDirty = () => { };

		/// <summary>
		/// Register all necessary callbacks needed for movableArea.
		/// </summary>
		private void RegisterCallbacksForMovableArea()
		{
			if (board != null)
				board.OnBoardChanged += SetMovableAreaDirty;
			OnAllOcupiedTilesDirty += SetMovableAreaDirty;
			OnMovementChanged += SetMovableAreaDirty;
		}

		/// <summary>
		/// Deregister all necessary callbacks needed for movableArea.
		/// </summary>
		private void DeregisterCallbacksForMovableArea()
		{
			if (board != null)
				board.OnBoardChanged -= SetMovableAreaDirty;
			OnAllOcupiedTilesDirty -= SetMovableAreaDirty;
			OnMovementChanged -= SetMovableAreaDirty;
		}

		/// <summary>
		/// Notify MovableArea needs updating.
		/// </summary>
		private void SetMovableAreaDirty()
		{
			if (!_IsMoveableAreaDirty)
			{
				_IsMoveableAreaDirty = true;
				OnMoveableAreaDirty.Invoke();
			}
		}

		/// <summary>
		/// Retrieve again the list of all tiles that are movable to by this unit.
		/// </summary>
		private void RefreshMoveableArea()
		{
			board.GetConnectedTilesInRange(OccupiedTiles, AllOccupiedTiles.Except(OccupiedTiles), Movement, _MoveableArea);
			_IsMoveableAreaDirty = false;
		}

		// ========================================================= Properties (AttackableArea) =========================================================

		/// <summary>
		/// A Readonly list of all tiles that are attackable by this unit.
		/// </summary>
		public IReadOnlyList<Tile> AttackableArea
		{
			get
			{
				if (_IsAttackableAreaDirty)
				{
					RefreshAttackableArea();
				}
				return _AttackableArea.AsReadOnly();
			}
		}
		private List<Tile> _AttackableArea = new List<Tile>();
		private bool _IsAttackableAreaDirty = true;

		/// <summary>
		/// Event raised when the list of attackable tile by this unit needs updating.
		/// </summary>
		public event Action OnAttackableAreaDirty = () => { };

		/// <summary>
		/// Register all necessary callbacks needed for attackableArea.
		/// </summary>
		private void RegisterCallbacksForAttackableArea()
		{
			if (board != null)
				board.OnBoardChanged += SetAttackableAreaDirty;
			OnAllOcupiedTilesDirty += SetAttackableAreaDirty;
			OnAttackAreaRuleChanged += SetAttackableAreaDirty;
			OnCurrentAttackTypeChanged += SetAttackableAreaDirty;
			OnPhysicalRangeChanged += SetAttackableAreaDirty;
			OnMagicalRangeChanged += SetAttackableAreaDirty;
		}

		/// <summary>
		/// Deregister all necessary callbacks needed for attackableArea.
		/// </summary>
		private void DeregisterCallbacksForAttackableArea()
		{
			if (board != null)
				board.OnBoardChanged -= SetAttackableAreaDirty;
			OnAllOcupiedTilesDirty -= SetAttackableAreaDirty;
			OnAttackAreaRuleChanged -= SetAttackableAreaDirty;
			OnCurrentAttackTypeChanged -= SetAttackableAreaDirty;
			OnPhysicalRangeChanged -= SetAttackableAreaDirty;
			OnMagicalRangeChanged -= SetAttackableAreaDirty;
		}

		/// <summary>
		/// Notify AttackableArea needs updating.
		/// </summary>
		private void SetAttackableAreaDirty()
		{
			if (!_IsAttackableAreaDirty)
			{
				_IsAttackableAreaDirty = true;
				OnAttackableAreaDirty.Invoke();
			}
		}

		/// <summary>
		/// Retrieve again the list of all tiles that are attackable to by this unit.
		/// </summary>
		private void RefreshAttackableArea()
		{
			if (CurrentAttackType == AttackType.Physical)
			{
				board.GetTilesByRule(OccupiedTiles, AttackAreaRule, PhysicalRange, _AttackableArea);
			}
			else
			{
				board.GetTilesByRule(OccupiedTiles, AttackAreaRule, MagicalRange, _AttackableArea);
			}
			_IsAttackableAreaDirty = false;
		}

		// ========================================================= Properties (PredictedAttackableArea) =========================================================

		/// <summary>
		/// A Readonly list of all tiles that will be attackable after movement by this unit.
		/// </summary>
		public IReadOnlyList<Tile> PredictedAttackableArea
		{
			get
			{
				if (_IsPredictedAttackableAreaDirty)
				{
					RefreshPredictedAttackableArea();
				}
				return _PredictedAttackableArea.AsReadOnly();
			}
		}
		private List<Tile> _PredictedAttackableArea = new List<Tile>();
		private bool _IsPredictedAttackableAreaDirty = true;

		/// <summary>
		/// Event raised when the list of attackable tile after movement by this unit needs updating.
		/// </summary>
		public event Action OnPredictedAttackableAreaDirty = () => { };

		/// <summary>
		/// Register all necessary callbacks needed for predictedAttackableArea.
		/// </summary>
		private void RegisterCallbacksForPredictedAttackableArea()
		{
			if (board != null)
				board.OnBoardChanged += SetPredictedAttakableAreaDirty;
			OnMoveableAreaDirty += SetPredictedAttakableAreaDirty;
			OnAttackAreaRuleChanged += SetPredictedAttakableAreaDirty;
			OnCurrentAttackTypeChanged += SetPredictedAttakableAreaDirty;
			OnPhysicalRangeChanged += SetPredictedAttakableAreaDirty;
			OnMagicalRangeChanged += SetPredictedAttakableAreaDirty;
		}

		/// <summary>
		/// Deregister all necessary callbacks needed for predictedAttackableArea.
		/// </summary>
		private void DeregisterCallbacksForPredictedAttackableArea()
		{
			if (board != null)
				board.OnBoardChanged -= SetPredictedAttakableAreaDirty;
			OnMoveableAreaDirty -= SetPredictedAttakableAreaDirty;
			OnAttackAreaRuleChanged -= SetPredictedAttakableAreaDirty;
			OnCurrentAttackTypeChanged -= SetPredictedAttakableAreaDirty;
			OnPhysicalRangeChanged -= SetPredictedAttakableAreaDirty;
			OnMagicalRangeChanged -= SetPredictedAttakableAreaDirty;
		}

		/// <summary>
		/// Notify PredictedAttackableArea needs updating.
		/// </summary>
		private void SetPredictedAttakableAreaDirty()
		{
			if (!_IsPredictedAttackableAreaDirty)
			{
				_IsPredictedAttackableAreaDirty = true;
				OnPredictedAttackableAreaDirty.Invoke();
			}
		}

		/// <summary>
		/// Retrieve again the list of all tiles that are attackable after movement by this unit.
		/// </summary>
		private void RefreshPredictedAttackableArea()
		{
			if (CurrentAttackType == AttackType.Physical)
			{
				board.GetTilesByRule(MovableArea, AttackAreaRule, PhysicalRange, _PredictedAttackableArea);
			}
			else
			{
				board.GetTilesByRule(MovableArea, AttackAreaRule, MagicalRange, _PredictedAttackableArea);
			}
			_IsPredictedAttackableAreaDirty = false;
		}

		// ========================================================= Properties (Equipment) =========================================================

		/// <summary>
		/// All equpiments this unit pocesses.
		/// </summary>
		public IReadOnlyList<Equipment> Equipments
		{
			get
			{
				return _Equipments.AsReadOnly();
			}
		}
		private List<Equipment> _Equipments = new List<Equipment>();

		/// <summary>
		/// Event raised when the equipments of this unit is changed.
		/// </summary>
		public event Action OnEquipmentChanged = () => { };

		/// <summary>
		/// Add the starting equipments to this unit.
		/// </summary>
		public void SetupInitialEquipments()
		{
			foreach (EquipmentDictionary.Name name in startingEquipment)
			{
				AddEquipment(EquipmentDictionary.NewEquipment(name, this));
			}
		}

		/// <summary>
		/// Add an equipment to this unit.
		/// </summary>
		public void AddEquipment(Equipment equipment)
		{
			_Equipments.Add(equipment);
			equipment.OnIsActivatedChanged += RefreshEquipmentEffects;
			equipment.OnInspectionChanged += RefreshEquipmentEffects;
			OnEquipmentChanged.Invoke();
		}

		/// <summary>
		/// Remove an equipment from this unit.
		/// </summary>
		public void RemoveEquipment(Equipment equipment)
		{
			_Equipments.Remove(equipment);
			equipment.OnIsActivatedChanged -= RefreshEquipmentEffects;
			equipment.OnInspectionChanged -= RefreshEquipmentEffects;
			OnEquipmentChanged.Invoke();
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

		/// <summary>
		/// Refresh the effect caused by all equipments that this unit has.
		/// </summary>
		private void RefreshEquipmentEffects()
		{
			int physicalAttack = basePhysicalAttack;
			int physicalDefence = basePhysicalDefence;
			int physicalRange = basePhysicalRange;
			int magicalAttack = baseMagicalAttack;
			int magicalDefence = baseMagicalDefence;
			int magicalRange = baseMagicalRange;
			float knockbackForce = baseKnockbackForce;
			int movement = baseMovement;
			AttackAreaRule attackAreaRule = AttackAreaRule.Adjacent;
			AttackType attackType = AttackType.Physical;

			void AddSimpleStats(Equipment equipment)
			{
				physicalAttack += equipment.PhysicalAttackDelta;
				physicalDefence += equipment.PhysicalDefenceDelta;
				physicalRange += equipment.PhysicalRangeDelta;
				magicalAttack += equipment.MagicalAttackDelta;
				magicalDefence += equipment.MagicalDefenceDelta;
				magicalRange += equipment.MagicalRangeDelta;
				knockbackForce += equipment.KnockbackForceDelta;
				movement += equipment.MovementDelta;
			}

			void RemoveSimpleStats(Equipment equipment)
			{
				physicalAttack -= equipment.PhysicalAttackDelta;
				physicalDefence -= equipment.PhysicalDefenceDelta;
				physicalRange -= equipment.PhysicalRangeDelta;
				magicalAttack -= equipment.MagicalAttackDelta;
				magicalDefence -= equipment.MagicalDefenceDelta;
				magicalRange -= equipment.MagicalRangeDelta;
				knockbackForce -= equipment.KnockbackForceDelta;
				movement -= equipment.MovementDelta;
			}

			Equipment movementEquipment = null;
			Equipment attackEquipment = null;

			// calculate effect for all activated equipments
			foreach (Equipment equipment in Equipments.Where(x => x.IsActivated))
			{
				AddSimpleStats(equipment);
				if (equipment.Type == Equipment.EquipmentType.MovementBuff)
				{
					movementEquipment = equipment;
				}
				else if (equipment.Type == Equipment.EquipmentType.MeleeAttack)
				{
					attackEquipment = equipment;
					attackAreaRule = equipment.AreaRule;
					attackType = AttackType.Physical;
				}
				else if (equipment.Type == Equipment.EquipmentType.MagicAttack)
				{
					attackEquipment = equipment;
					attackAreaRule = equipment.AreaRule;
					attackType = AttackType.Magical;
				}
			}

			// calculate effect for all previewing equipments
			foreach (Equipment equipment in Equipments.Where(x => x.IsBeingInspected && !x.IsActivated))
			{
				AddSimpleStats(equipment);
				if (equipment.Type == Equipment.EquipmentType.MovementBuff)
				{
					if (movementEquipment != null)
					{
						RemoveSimpleStats(movementEquipment);
					}
				}
				else if (equipment.Type == Equipment.EquipmentType.MeleeAttack)
				{
					if (attackEquipment != null)
					{
						RemoveSimpleStats(attackEquipment);
					}
					attackAreaRule = equipment.AreaRule;
					attackType = AttackType.Physical;
				}
				else if (equipment.Type == Equipment.EquipmentType.MagicAttack)
				{
					if (attackEquipment != null)
					{
						RemoveSimpleStats(attackEquipment);
					}
					attackAreaRule = equipment.AreaRule;
					attackType = AttackType.Magical;
				}
			}

			// apply effects to stats
			PhysicalAttack = physicalAttack;
			PhysicalDefence = physicalDefence;
			PhysicalRange = physicalRange;
			MagicalAttack = magicalAttack;
			MagicalDefence = magicalDefence;
			MagicalRange = magicalRange;
			KnockbackForce = knockbackForce;
			Movement = movement;
			AttackAreaRule = attackAreaRule;
			CurrentAttackType = attackType;
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
					IsHidden = value == UnitState.Defeated;

					_CurrentUnitState = value;
					OnUnitStateChange.Invoke();
				}
			}
		}
		private UnitState _CurrentUnitState = UnitState.Standby;

		/// <summary>
		/// Event raised when the unit state of this unit is changed.
		/// </summary>
		public event Action OnUnitStateChange = () => { };

		// ========================================================= Liminal Properties (Actions) =========================================================

		/// <summary>
		/// The next movement action chosen by the player.
		/// </summary>
		public UnitMovement NextMovement { get; private set; } = null;

		/// <summary>
		/// The next acttack action chosen by the player.
		/// </summary>
		public UnitAttack NextAttack { get; private set; } = null;

		// ========================================================= Other Signals =========================================================

		public event Action OnTakingDamage = () => {};

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.Register(gameObject, this, SMState.Navigation, new NavigationSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitMoveSelect, new UnitMoveSelectSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitAttackSelect, new UnitAttackSelectSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitDepletedSelect, new UnitDepletedSelectSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitInspection, new UnitInspectionSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitMove, new UnitMoveSB(this));
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

		// ========================================================= Other Signals =========================================================

		/// <summary>
		/// Detect and perform necessary action for fallen die.
		/// </summary>
		private void DetectFallen()
		{
			if (IsFallen)
			{
				CurrentUnitState = UnitState.Defeated;
			}
		}

	}
}

