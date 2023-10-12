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
		private Transform modelTransform = null;
		private Transform effectTransform = null;	  
		private LineRenderer lineRenderer = null;

		// ========================================================= Properties (IsBeingInspected) =========================================================
	
		/// <summary>
		/// Flag for if this die is currently being inspected.
		/// </summary>
		public bool IsBeingInspected
		{
			get
			{
				return inspectingDice.Contains(this);
			}
			private set
			{
				if (!inspectingDice.Contains(this) && value)
				{
					inspectingDice.Add(this);
					onInspectionChanged.Invoke();
				}
				if (inspectingDice.Contains(this) && !value)
				{
					inspectingDice.Remove(this);
					onInspectionChanged.Invoke();
				}
			}
		}
		private static UniqueList<Die> inspectingDice = new UniqueList<Die>();

		/// <summary>
		/// Event raised when the inspection status of any die is changed.
		/// </summary>
		public static event Action onInspectionChanged = () => { };

		/// <summary>
		/// Retrieve the first die being currently inspected, return null if none is being inspected.
		/// </summary>
		public static Die GetFirstBeingInspected()
		{
			return inspectingDice.Count > 0 ? inspectingDice[0] : null;
		}

		// ========================================================= Properties (IsSelected) =========================================================

		/// <summary>
		/// Flag for if this die is currently selected.
		/// </summary>
		public bool IsSelected
		{
			get 
			{
				return selectedDice.Contains(this);
			}
			private set
			{
				if (!selectedDice.Contains(this) && value)
				{
					selectedDice.Add(this);
					onSelectionChanged.Invoke();
				}
				if (selectedDice.Contains(this) && !value)
				{
					selectedDice.Remove(this);
					onSelectionChanged.Invoke();
				}
			}
		}
		private static UniqueList<Die> selectedDice = new UniqueList<Die>();

		/// <summary>
		/// Event raised when the selection status of any die is changed.
		/// </summary>
		public static event Action onSelectionChanged = () => { };

		/// <summary>
		/// Retrieve the first currently selected die, return null if none is selected.
		/// </summary>
		public static Die GetFirstSelected()
		{
			return selectedDice.Count > 0 ? selectedDice[0] : null;
		}

		/// <summary>
		/// Retrieve all currently selected die.
		/// </summary>
		public static IReadOnlyCollection<Die> GetAllSelected()
		{
			return selectedDice.AsReadOnly();
		}

		/// <summary>
		/// Clear the list of selected die. 
		/// /// </summary>
		public static void ClearSelected()
		{
			for (int i = selectedDice.Count - 1; i >= 0; i--)
			{
				selectedDice[i].IsSelected = false;
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
				return draggingDice.Contains(this);
			}
			private set
			{
				if (!draggingDice.Contains(this) && value)
				{
					draggingDice.Add(this);
					onDragChanged.Invoke();
				}
				if (draggingDice.Contains(this) && !value)
				{
					draggingDice.Remove(this);
					onDragChanged.Invoke();
				}
			}
		}
		private static UniqueList<Die> draggingDice = new UniqueList<Die>();

		/// <summary>
		/// Event raised when the drag status of any die is changed.
		/// </summary>
		public static event Action onDragChanged = () => { };

		/// <summary>
		/// Retrieve the first die being currently dragged, return null if none is selected.
		/// </summary>
		public static Die GetFirstBeingDragged()
		{
			return draggingDice.Count > 0 ? draggingDice[0] : null;
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
							rigidBody.isKinematic = true;
							modelTransform.gameObject.SetActive(false);
							effectTransform.gameObject.SetActive(false);
							break;
						case DieState.Casted:
							rigidBody.isKinematic = false;
							modelTransform.gameObject.SetActive(true);
							effectTransform.gameObject.SetActive(true);
							break;
						case DieState.Assigned:
							rigidBody.isKinematic = false;
							modelTransform.gameObject.SetActive(true);
							effectTransform.gameObject.SetActive(true);
							break;
						case DieState.Expended:
							rigidBody.isKinematic = true;
							modelTransform.gameObject.SetActive(false);
							effectTransform.gameObject.SetActive(false);
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

		// ========================================================= Properties (EquipmentDieSlot) =========================================================

		/// <summary>
		/// The Equipment that this die is assigned to.
		/// </summary>
		public EquipmentDieSlot AssignedDieSlot {
			get
			{
				return _AssignedDieSlot;
			}
			private set
			{
				if (_AssignedDieSlot != value)
				{
					// modify previous die slot
					if (_AssignedDieSlot != null)
					{
						_AssignedDieSlot.AssignDie(null);
					}
					// modify next die slot
					if (value != null)
					{
						if (value.Die != null)
						{
							value.Die.AssignedDieSlot = null;
						}

						value.AssignDie(this);
					}
					// modify self
					_AssignedDieSlot = value;
					onAssignedDieSlotChanged.Invoke();
				}
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
			rigidBody = GetComponent<Rigidbody>();
			modelTransform = transform.Find("Model");
			effectTransform = transform.Find("Effect");
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
				Player.dice.Add(this);
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
				Player.dice.Remove(this);
			}
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