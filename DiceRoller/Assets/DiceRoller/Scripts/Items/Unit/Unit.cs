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
		[Header("Unit Parameters")]
		public int maxHealth = 20;
		public int baseMelee = 3;
		public int baseMagic = 1;
		public int baseDefence = 4;
		public int baseMovement = 4;
		public int baseRange = 1;
		public float baseKnockbackForce = 0.25f;
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
					OnItemBeingInspectedChanged.Invoke();
				}
				else if (_InspectingUnits.Contains(this) && !value)
				{
					_InspectingUnits.Remove(this);
					OnInspectionChanged.Invoke();
					OnItemBeingInspectedChanged.Invoke();
				}
			}
		}
		private static UniqueList<Unit> _InspectingUnits = new UniqueList<Unit>();

		/// <summary>
		/// Event raised when the inspection status of this unit is changed.
		/// </summary>
		public event Action OnInspectionChanged = () => { };

		/// <summary>
		/// Event raised when the the list of units being inspected is changed.
		/// </summary>
		public static event Action OnItemBeingInspectedChanged = () => { };

		/// <summary>
		/// Retrieve the first unit being currently inspected, return null if none is being inspected.
		/// </summary>
		public static Unit GetFirstBeingInspected()
		{
			return _InspectingUnits.Count > 0 ? _InspectingUnits[0] : null;
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
					OnItemSelectedChanged.Invoke();
				}
				else if (_SelectedUnits.Contains(this) && !value)
				{
					_SelectedUnits.Remove(this);
					OnSelectionChanged.Invoke();
					OnItemSelectedChanged.Invoke();
				}
			}
		}
		private static UniqueList<Unit> _SelectedUnits = new UniqueList<Unit>();

		/// <summary>
		/// Event raised when the selection status of this die is changed.
		/// </summary>
		public event Action OnSelectionChanged = () => { };

		/// <summary>
		/// Event raised when the list of units selected is changed.
		/// </summary>
		public static event Action OnItemSelectedChanged = () => { };

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
					OnMeleeChanged.Invoke();
				}
			}
		}
		private int _Melee = 0;

		/// <summary>
		/// Event raised when the melee stat of this unit is changed.
		/// </summary>
		public event Action OnMeleeChanged = () => { };

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
					OnMagicChanged.Invoke();
				}
			}
		}
		private int _Magic = 0;

		/// <summary>
		/// Event raised when the melee stat of this unit is changed.
		/// </summary>
		public event Action OnMagicChanged = () => { };

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
					OnDefenceChanged.Invoke();
				}
			}
		}
		private int _Defence = 0;

		/// <summary>
		/// Event raised when the melee stat of this unit is changed.
		/// </summary>
		public event Action OnDefenceChanged = () => { };

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
		/// Event raised when the melee stat of this unit is changed.
		/// </summary>
		public event Action OnMovementChanged = () => { };

		/// <summary>
		/// The current attack range value of this unit.
		/// </summary>
		public int AttackRange
		{
			get
			{
				return _AttackRange;
			}
			private set
			{
				int clampedValue = value < 0 ? 0 : value;
				if (_AttackRange != clampedValue)
				{
					_AttackRange = clampedValue;
					OnAttackRangeChanged.Invoke();
				}
			}
		}
		private int _AttackRange = 0;

		/// <summary>
		/// Event raised when the melee stat of this unit is changed.
		/// </summary>
		public event Action OnAttackRangeChanged = () => { };

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
		/// Event raised when the melee stat of this unit is changed.
		/// </summary>
		public event Action OnKnockbackChanged = () => { };

		/// <summary>
		/// Set the stat values to the inital value.
		/// </summary>
		public void SetupInitalStat()
		{
			Melee = baseMelee;
			Magic = baseMagic;
			Defence = baseDefence;
			Movement = baseMovement;
			AttackRange = baseRange;
			KnockbackForce = baseKnockbackForce;
		}

		/// <summary>
		/// Apply a change on one or more stat variables.
		/// </summary>
		public void ChangeStat(int meleeDelta = 0, int magicDelta = 0, int defenceDelta = 0, int movementDelta = 0, int attackRangeDelta = 0, float knockbackForceDelta = 0)
		{
			Melee += meleeDelta;
			Defence += defenceDelta;
			Magic += magicDelta;
			Movement += movementDelta;
			AttackRange += attackRangeDelta;
			KnockbackForce += knockbackForceDelta;
		}

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
		private static bool _IsAllOccupiedTilesExceptSelfDirty = true;

		/// <summary>
		/// Event raised when the list of all tiles occupied by every unit needs updating.
		/// </summary>
		public static event Action OnAllOcupiedTilesExceptSelfDirty = () => { };

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
		private static void SetAllOccupiedTilesExceptSelfDirty()
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
			OnAllOcupiedTilesDirty += SetMovableAreaDirty;
			OnMovementChanged += SetMovableAreaDirty;
		}

		/// <summary>
		/// Deregister all necessary callbacks needed for movableArea.
		/// </summary>
		private void DeregisterCallbacksForMovableArea()
		{
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
			OnAllOcupiedTilesDirty += SetAttackableAreaDirty;
			OnAttackAreaRuleChanged += SetAttackableAreaDirty;
			OnAttackRangeChanged += SetAttackableAreaDirty;
		}

		/// <summary>
		/// Deregister all necessary callbacks needed for attackableArea.
		/// </summary>
		private void DeregisterCallbacksForAttackableArea()
		{
			OnAllOcupiedTilesDirty -= SetAttackableAreaDirty;
			OnAttackAreaRuleChanged -= SetAttackableAreaDirty;
			OnAttackRangeChanged -= SetAttackableAreaDirty;
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
			board.GetTilesByRule(OccupiedTiles, AttackAreaRule, AttackRange, _AttackableArea);
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
			OnMoveableAreaDirty += SetPredictedAttakableAreaDirty;
			OnAttackAreaRuleChanged += SetPredictedAttakableAreaDirty;
			OnAttackRangeChanged += SetPredictedAttakableAreaDirty;
		}

		/// <summary>
		/// Deregister all necessary callbacks needed for predictedAttackableArea.
		/// </summary>
		private void DeregisterCallbacksForPredictedAttackableArea()
		{
			OnMoveableAreaDirty -= SetPredictedAttakableAreaDirty;
			OnAttackAreaRuleChanged -= SetPredictedAttakableAreaDirty;
			OnAttackRangeChanged -= SetPredictedAttakableAreaDirty;
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
			board.GetTilesByRule(MovableArea, AttackAreaRule, AttackRange, _PredictedAttackableArea);
			_IsPredictedAttackableAreaDirty = false;
		}

		// ========================================================= Properties (Equipment) =========================================================

		/// <summary>
		/// All equpiments this unit pocesses.
		/// </summary>
		public List<Equipment> Equipments { get; private set; } = new List<Equipment>();

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
				Equipments.Add(EquipmentDictionary.NewEquipment(name, this));
			}
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

		private static readonly AttackAreaRule DefaultMeleeRule =
			new AttackAreaRule((target, starting, range) => Int2.GridDistance(target.boardPos, starting.boardPos) <= range);

		/// <summary>
		/// Reset the rule to determine which tiles are attackable to the basic melee form.
		/// </summary>
		public void ResetAttackAreaRule()
		{
			_AttackAreaRule = DefaultMeleeRule;
		}

		/// <summary>
		/// Apply a different attack area rule tooo this unit.
		/// </summary>
		public void ChangeAttackAreaRule(AttackAreaRule rule)
		{
			AttackAreaRule = rule;
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
					switch (value)
					{
						case UnitState.Standby:
							ShowEffect(EffectType.Depleted, false);
							IsHidden = false;
							break;
						case UnitState.Moved:
							ShowEffect(EffectType.Depleted, false);
							IsHidden = false;
							break;
						case UnitState.Depleted:				
							ShowEffect(EffectType.Depleted, true);
							IsHidden = false;
							break;
						case UnitState.Defeated:
							ShowEffect(EffectType.Depleted, false);
							IsHidden = true;
							break;
					}

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

	}
}

