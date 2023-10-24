using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickerEffects;
using System.Linq;
using System;

namespace DiceRoller
{
	public partial class Die : Item, IEquatable<Die>
	{
		public enum Type
		{
			Unknown,
			D2,
			D4,
			D6,
			D8,
			D10,
			D12,
			D20,
		}

		public enum DieState
		{
			Holding,
			Casted,
			Assigned,
			Expended,

			//Ready,
			//Done,
			//Rolling,
			//Warning,
			//Locked,
			//Waiting,
			//Buffed,
			//Nerfed,
		}

		[System.Serializable]
		public class Face
		{
			public Vector3 euler;
			public int value;

			public string name()
			{
				return string.Format("Value: {0} at ({1}, {2}, {3})", value, euler.x, euler.y, euler.z);
			}
		}

		// parameters
		[Header("Die Information")]
		public Type type = Type.Unknown;
		public List<Face> faces = new List<Face>();
		public bool startAsHolding = false;

		// components 
		private LineRenderer lineRenderer = null;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			RetrieveComponentReferences();
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected override void Start()
		{
			base.Start();
			RegisterStateBehaviours();
			RegisterToPlayer();

			if (startAsHolding)
				CurrentDieState = DieState.Holding;
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected override void Update()
		{
			base.Update();

			DetectValue();

			// temporary here only
			effectTransform.rotation = Quaternion.identity;
			if (AssignedUnit != null)
			{
				lineRenderer.gameObject.SetActive(true);
				lineRenderer.SetPosition(0, AssignedUnit.transform.position + 0.1f * Vector3.up);
				lineRenderer.SetPosition(1, transform.position);
			}
			else
			{
				lineRenderer.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// FixedUpdate is called at a regular interval, along side with physics simulation.
		/// </summary>
		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			rollInitiating = false;
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
			// draw size and each face of the die
			if (Application.isEditor)
			{
				for (int i = 0; i < faces.Count; i++)
				{
					Gizmos.color = Color.HSVToRGB((float)i / faces.Count, 1, 1);
					Gizmos.DrawLine(transform.position, transform.position + transform.rotation * Quaternion.Euler(faces[i].euler) * Vector3.up * 0.25f);
				}
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(transform.position, size / 2);
			}
		}

		// ========================================================= IEqautable Methods =========================================================

		/// <summary>
		/// Check if this object is equal to the other object.
		/// </summary>
		public bool Equals(Die other)
		{
			return this == other;
		}

		// ========================================================= General Behaviour =========================================================

		/// <summary>
		/// Retrieve component references for this unit.
		/// </summary>
		private void RetrieveComponentReferences()
		{
			lineRenderer = transform.Find("Effect/Line").GetComponent<LineRenderer>();
		}

		// ========================================================= Team Behaviour =========================================================

		/// <summary>
		/// Register this unit to a player.
		/// </summary>
		private void RegisterToPlayer()
		{
			if (game == null)
				return;

			if (Player != null)
			{
				Player.AddDie(this);
			}
		}

		/// <summary>
		///  Deregister this unit from a player.
		/// </summary>
		private void DeregisterFromPlayer()
		{
			if (game == null)
				return;

			if (Player != null)
			{
				Player.RemoveDie(this);
			}
		}

		// ========================================================= Properties (IsBeingInspected) =========================================================

		/// <summary>
		/// Flag for if this die is currently being inspected.
		/// </summary>
		public bool IsBeingInspected
		{
			get
			{
				return _InspectingDice.Contains(this);
			}
			private set
			{
				if (!_InspectingDice.Contains(this) && value)
				{
					_InspectingDice.Add(this);
					OnInspectionChanged.Invoke();
					OnItemBeingInspectedChanged.Invoke();
				}
				else if (_InspectingDice.Contains(this) && !value)
				{
					_InspectingDice.Remove(this);
					OnInspectionChanged.Invoke();
					OnItemBeingInspectedChanged.Invoke();
				}
			}
		}
		private static UniqueList<Die> _InspectingDice = new UniqueList<Die>();

		/// <summary>
		/// Event raised when the inspection status of this die is changed.
		/// </summary>
		public event Action OnInspectionChanged = () => { };

		/// <summary>
		/// Event raised when the the list of dice being inspected is changed.
		/// </summary>
		public static event Action OnItemBeingInspectedChanged = () => { };

		/// <summary>
		/// Retrieve the first die being currently inspected, return null if none is being inspected.
		/// </summary>
		public static Die GetFirstBeingInspected()
		{
			return _InspectingDice.Count > 0 ? _InspectingDice[0] : null;
		}

		// ========================================================= Properties (IsSelected) =========================================================

		/// <summary>
		/// Flag for if this die is currently selected.
		/// </summary>
		public bool IsSelected
		{
			get 
			{
				return _SelectedDice.Contains(this);
			}
			private set
			{
				if (!_SelectedDice.Contains(this) && value)
				{
					_SelectedDice.Add(this);
					OnSelectionChanged.Invoke();
					OnItemSelectedChanged.Invoke();
				}
				else if(_SelectedDice.Contains(this) && !value)
				{
					_SelectedDice.Remove(this);
					OnSelectionChanged.Invoke();
					OnItemSelectedChanged.Invoke();
				}
			}
		}
		private static UniqueList<Die> _SelectedDice = new UniqueList<Die>();

		/// <summary>
		/// Event raised when the selection status of this die is changed.
		/// </summary>
		public event Action OnSelectionChanged = () => { };

		/// <summary>
		/// Event raised when the list of dice selected is changed.
		/// </summary>
		public static event Action OnItemSelectedChanged = () => { };

		/// <summary>
		/// Retrieve the first currently selected die, return null if none is selected.
		/// </summary>
		public static Die GetFirstSelected()
		{
			return _SelectedDice.Count > 0 ? _SelectedDice[0] : null;
		}

		/// <summary>
		/// Retrieve all currently selected die.
		/// </summary>
		public static IReadOnlyCollection<Die> GetAllSelected()
		{
			return _SelectedDice.AsReadOnly();
		}

		/// <summary>
		/// Clear the list of selected die. 
		/// </summary>
		public static void ClearSelected()
		{
			for (int i = _SelectedDice.Count - 1; i >= 0; i--)
			{
				_SelectedDice[i].IsSelected = false;
			}
		}

		// ========================================================= Properties (IsBeingDragged) =========================================================

		/// <summary>
		/// Flag for if this die is currently begin dragged.
		/// </summary>
		public bool IsBeingDragged
		{
			get
			{
				return _DraggingDice.Contains(this);
			}
			private set
			{
				if (!_DraggingDice.Contains(this) && value)
				{
					_DraggingDice.Add(this);
					OnDragChanged.Invoke();
					OnItemBeingDraggedChanged.Invoke();
				}
				else if(_DraggingDice.Contains(this) && !value)
				{
					_DraggingDice.Remove(this);
					OnDragChanged.Invoke();
					OnItemBeingDraggedChanged.Invoke();
				}
			}
		}
		private static UniqueList<Die> _DraggingDice = new UniqueList<Die>();

		/// <summary>
		/// Event raised when the drag status of this die is changed.
		/// </summary>
		public event Action OnDragChanged = () => { };

		/// <summary>
		/// Event raised when the list of dice being dragged is changed.
		/// </summary>
		public static event Action OnItemBeingDraggedChanged = () => { };

		/// <summary>
		/// Retrieve the first die being currently dragged, return null if none is selected.
		/// </summary>
		public static Die GetFirstBeingDragged()
		{
			return _DraggingDice.Count > 0 ? _DraggingDice[0] : null;
		}

		// ========================================================= Properties (IsRolling) =========================================================

		/// <summary>
		/// Flag for if this die is still rolling.
		/// </summary>
		public bool IsRolling
		{
			get
			{
				return IsMoving && rollInitiating;
			}
		}
		private bool rollInitiating = false;
		private float lastRotatingTime = 0;

		// ========================================================= Properties (Value) =========================================================

		/// <summary>
		/// The current value of this die, -1 if value is invalid.
		/// </summary>
		public int Value
		{ 
			get
			{
				return _Value; 
			}
			private set
			{
				if (_Value != value)
				{
					_Value = value;
					onValueChanged.Invoke();
				}
			}
		}
		private int _Value = -1;
		private Quaternion lastRotation = Quaternion.identity;

		/// <summary>
		/// Event raised when the value of this die is changed.
		/// </summary>
		public event Action onValueChanged = () => { };

		/// <summary>
		/// Detect the current displayed value of this die.
		/// </summary>
		private void DetectValue()
		{
			// record last moving time for stationary checking
			if (rigidBody.velocity.sqrMagnitude > 0.01f || rigidBody.angularVelocity.sqrMagnitude > 0.01f)
			{
				lastRotatingTime = Time.time;
			}

			// determine the value of this die
			int lastValue = Value;
			if (IsRolling || Time.time - lastRotatingTime < 0.25f)
			{
				// die is stiill moving, set value to invalid
				Value = -1;
			}
			else if (Value == -1 || Quaternion.Angle(transform.rotation, lastRotation) < 1f)
			{
				// die is stationary and either value is invalid or the rotation is changed, calculate the current value
				float nearestAngle = float.MaxValue;
				int foundValue = 0;
				foreach (Face face in faces)
				{
					float angle = Vector3.Angle(transform.rotation * Quaternion.Euler(face.euler) * Vector3.up, Vector3.up);
					if (angle < nearestAngle)
					{
						nearestAngle = angle;
						foundValue = face.value;
					}
				}
				lastRotation = transform.rotation;
				Value = foundValue;
			}
		}

		// ========================================================= Properties (CurrentDieState) =========================================================

		/// <summary>
		/// The current state of this die.
		/// </summary>
		public DieState CurrentDieState 
		{
			get
			{
				return _CurrentDieState;
			}
			private set 
			{
				if (_CurrentDieState != value)
				{
					switch (value)
					{	
						case DieState.Holding:
							IsHidden = true;
							break;
						case DieState.Casted:
							IsHidden = false;
							break;
						case DieState.Assigned:
							IsHidden = false;
							break;
						case DieState.Expended:
							IsHidden = true;
							break;
					}
					_CurrentDieState = value;
					onDieStateChanged.Invoke();
				}
			}
		}
		private DieState _CurrentDieState = DieState.Casted;

		/// <summary>
		/// Event raised when the current die state or this die is changed.
		/// </summary>
		public event Action onDieStateChanged = () => { };

		/// <summary>
		/// Change a holding die to a casted die.
		/// </summary>
		public void RetieveFromHold()
		{
			if (CurrentDieState == DieState.Holding)
			{
				CurrentDieState = DieState.Casted;
			}
		}

		/// <summary>
		/// Expend a die that is either casted or assigned to an equipment die slot.
		/// </summary>
		public void Expend()
		{
			if (CurrentDieState == DieState.Casted || CurrentDieState == DieState.Assigned)
			{
				CurrentDieState = DieState.Expended;
			}
		}

		// ========================================================= Properties (EquipmentDieSlot) =========================================================

		/// <summary>
		/// The Equipment that this die is assigned to.
		/// </summary>
		public EquipmentDieSlot AssignedDieSlot
		{
			get
			{
				return _AssignedDieSlot;
			}
		}
		private EquipmentDieSlot _AssignedDieSlot = null;

		/// <summary>
		/// The unit that this die is indirectly assigned to.
		/// </summary>
		public Unit AssignedUnit
		{
			get
			{
				return AssignedDieSlot != null ? AssignedDieSlot.Equipment.Unit : null;
			}
		}
		
		/// <summary>
		/// Event raised when this die is assigned to another die slot.
		/// </summary>
		public event Action onAssignedDieSlotChanged = () => { };

		/// <summary>
		/// Assign this die to a die slot, regardless if the slot is fulfilled. Actual assigning will be done via callback.
		/// </summary>
		public void AssignToSlot(EquipmentDieSlot slot)
		{
			if (_AssignedDieSlot != slot)
			{
				// modify previous die slot
				if (_AssignedDieSlot != null)
				{
					_AssignedDieSlot.AssignDie(null, null);
				}

				// modify next die slot
				if (slot != null)
				{
					if (slot.Die != null)
					{
						slot.Die.AssignToSlot(null);
					}
					slot.AssignDie(this, AssignedToSlotCallback);
				}
			}
		}

		/// <summary>
		/// A callback for when the equipment slot accepts or not the assignment from the die.
		/// </summary>
		private void AssignedToSlotCallback(EquipmentDieSlot slot)
		{
			_AssignedDieSlot = slot;
			if (CurrentDieState != DieState.Holding && CurrentDieState != DieState.Expended)
			{
				if (slot != null)
				{
					CurrentDieState = DieState.Assigned;
				}
				else
				{
					CurrentDieState = DieState.Casted;
				}
			}
			onAssignedDieSlotChanged.Invoke();
		}

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		private void RegisterStateBehaviours()
		{
			stateMachine.Register(gameObject, this, SMState.Navigation, new NavigationSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitMoveSelect, new UnitActionSelectSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitAttackSelect, new UnitActionSelectSB(this));
			stateMachine.Register(gameObject, this, SMState.UnitDepletedSelect, new UnitActionSelectSB(this));
			stateMachine.Register(gameObject, this, SMState.DiceActionSelect, new DiceActionSelectSB(this));
			stateMachine.Register(gameObject, this, SMState.DiceThrow, new DiceThrowSB(this));
			stateMachine.Register(gameObject, this, SMState.EndTurn, new EndTurnSB(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		private void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
			{
				stateMachine.DeregisterAll(this);
			}
		}

		// ========================================================= Dice Behaviour =========================================================

		/// <summary>
		/// Throw this die from a specific position with specific force and torque.
		/// </summary>
		public void Throw(Vector3 position, Vector3 force, Vector3 torque)
		{
			rigidBody.velocity = Vector3.zero;
			rigidBody.angularVelocity = Vector3.zero;
			rigidBody.MovePosition(position);
			rigidBody.AddForce(force, ForceMode.VelocityChange);
			rigidBody.AddTorque(torque, ForceMode.VelocityChange);

			rollInitiating = true;
		}
	}
}