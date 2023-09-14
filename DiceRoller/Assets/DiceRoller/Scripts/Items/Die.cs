using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickerEffects;
using System.Linq;
using System;

namespace DiceRoller
{
	public class Die : Item
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
			Normal,
			Ready,
			Done,
			Rolling,
			Problem,
			Locked,
			Waiting,
			Buffed,
			Nerfed,
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
		public Unit connectedUnit = null;

		// components
		private Transform effectTransform = null;
		private LineRenderer lineRenderer = null;

		// events
		public Action onInspectionChanged = () => { };
		public Action onSelectionChanged = () => { };
		public Action onValueChanged = () => { };
		public Action onDieStateChanged = () => { };

		// ========================================================= Properties =========================================================

		/// <summary>
		/// Flag for if this die is currently being inspected.
		/// </summary>
		public bool IsBeingInspected
		{
			get
			{
				return inspectingDice.Contains(this);
			}
		}
		private static UniqueList<Die> inspectingDice = new UniqueList<Die>();

		/// <summary>
		/// Flag for if this die is currently selected.
		/// </summary>
		public bool IsSelected
		{
			get 
			{
				return selectedDice.Contains(this);
			} 
		}
		private static UniqueList<Die> selectedDice = new UniqueList<Die>();

		/// <summary>
		/// The current value of this die, -1 if value is invalid.
		/// </summary>
		public int Value
		{ 
			get
			{
				return _value; 
			}
			private set
			{
				if (_value != value)
				{
					_value = value;
					onValueChanged.Invoke();
				}
			}
		}
		private int _value = -1;
		private Quaternion lastRotation = Quaternion.identity;
		private float lastRotatingTime = 0;
		private bool rollInitiating = false;

		/// <summary>
		/// The current state of this die.
		/// </summary>
		public DieState State 
		{
			get
			{
				return _state;
			}
			private set 
			{
				if (_state != value)
				{
					_state = value;
					onDieStateChanged.Invoke();
				}
			}
		}
		private DieState _state = DieState.Normal;

		// ========================================================= Inspection and Selection =========================================================

		/// <summary>
		/// Retrieve the first die being currently inspected, return null if none is being inspected.
		/// </summary>
		public static Die GetFirstBeingInspected()
		{
			return inspectingDice.Count > 0 ? inspectingDice[0] : null;
		}

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
				selectedDice[i].RemoveFromSelection();
			}
		}

		/// <summary>
		/// Add this die to as being inspecting.
		/// </summary>
		private void AddToInspection()
		{
			if (!inspectingDice.Contains(this))
			{
				inspectingDice.Add(this);
				onInspectionChanged.Invoke();
			} 
		}

		/// <summary>
		/// Remove this die from as being inspecting.
		/// </summary>
		private void RemoveFromInspection()
		{
			if (inspectingDice.Contains(this))
			{
				inspectingDice.Remove(this);
				onInspectionChanged.Invoke();
			}
		}

		/// <summary>
		/// Add this die to as selected.
		/// </summary>
		private void AddToSelection()
		{
			if (!selectedDice.Contains(this))
			{
				selectedDice.Add(this);
				onSelectionChanged.Invoke();
			}
		}

		/// <summary>
		/// Remove this die from as selected.
		/// </summary>
		private void RemoveFromSelection()
		{
			if (selectedDice.Contains(this))
			{
				selectedDice.Remove(this);
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
			if (connectedUnit != null)
			{
				lineRenderer.gameObject.SetActive(true);
				lineRenderer.SetPosition(0, connectedUnit.transform.position + 0.1f * Vector3.up);
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

		// ========================================================= General Behaviour =========================================================

		/// <summary>
		/// Retrieve component references for this unit.
		/// </summary>
		private void RetrieveComponentReferences()
		{
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

		// ========================================================= Dice Behaviour =========================================================

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
			if (IsMoving || rollInitiating || Time.time - lastRotatingTime < 0.25f)
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

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		private void RegisterStateBehaviours()
		{
			stateMachine.Register(this, DiceRoller.State.Navigation, new NavigationSB(this));
			stateMachine.Register(this, DiceRoller.State.DiceActionSelect, new DiceActionSelectSB(this));
			stateMachine.Register(this, DiceRoller.State.DiceThrow, new DiceThrowSB(this));
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

		// ========================================================= Navigation State =========================================================
		private class NavigationSB : StateBehaviour
		{
			// host reference
			private readonly Die self = null;

			// caches
			private bool lastIsHovering = false;

			/// <summary>
			/// Constructor.
			/// </summary>
			public NavigationSB(Die self)
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
				// show dice info on ui
				if (CachedValueUtils.HasValueChanged(self.IsUserHovering, ref lastIsHovering))
				{
					if (self.IsUserHovering)
					{
						self.AddToInspection();
						self.AddEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingSelf : StatusType.InspectingEnemy);
					}
					else
					{
						self.RemoveFromInspection();
						self.RemoveEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingSelf : StatusType.InspectingEnemy);
					}
				}

				// go to dice action selection state when this dice is pressed
				if (game.CurrentPlayer == self.Player && self.IsUserPressed)
				{
					self.AddToSelection();
					stateMachine.ChangeState(DiceRoller.State.DiceActionSelect);
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// hide dice info on ui
				if (self.IsBeingInspected)
				{
					self.RemoveFromInspection();
					self.RemoveEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingSelf : StatusType.InspectingEnemy);
				}

				// reset caches
				CachedValueUtils.ResetValueCache(ref lastIsHovering);
			}
		}

		// ========================================================= Dice Action State =========================================================
		
		protected class DiceActionSelectSB : StateBehaviour
		{
			// host reference
			private readonly Die self = null;

			// caches
			private bool isSelectedAtStateEnter = false;
			private bool lastIsHovering = false;

			/// <summary>
			/// Constructor.
			/// </summary>
			public DiceActionSelectSB(Die self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				// display as selected
				if (self.IsSelected)
				{
					isSelectedAtStateEnter = true;
					self.AddEffect(StatusType.SelectedSelf);
				}
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				// execute only if the selected unit is this unit
				if (game.CurrentPlayer == self.Player)
				{
					// show dice info on ui
					if (CachedValueUtils.HasValueChanged(self.IsUserHovering, ref lastIsHovering))
					{
						if (self.IsUserHovering)
						{
							self.AddToInspection();
							self.AddEffect(StatusType.InspectingSelf);
						}
						else
						{
							self.RemoveFromInspection();
							self.RemoveEffect(StatusType.InspectingSelf);
						}
					}

					// go to dice action selection state or navigation state when this dice is pressed
					if (self.IsUserPressed)
					{
						if (self.IsSelected)
						{
							self.RemoveFromSelection();
						}
						else
						{
							self.AddToSelection();
						}

						if (GetFirstSelected() != null)
						{
							stateMachine.ChangeState(DiceRoller.State.DiceActionSelect);
						}
						else
						{
							stateMachine.ChangeState(DiceRoller.State.Navigation);
						}
					}
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// revert display as selected
				if (isSelectedAtStateEnter)
				{
					isSelectedAtStateEnter = false;
					self.RemoveEffect(StatusType.SelectedSelf);
				}

				// revert display as inspecting
				if (self.IsBeingInspected)
				{
					self.RemoveFromInspection();
					self.RemoveEffect(StatusType.InspectingSelf);
				}

				// reset caches
				CachedValueUtils.ResetValueCache(ref lastIsHovering);
			}
		}

		// ========================================================= Dice Throw State =========================================================

		private class DiceThrowSB : StateBehaviour
		{
			// host reference
			private readonly Die self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public DiceThrowSB(Die self)
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
	}
}