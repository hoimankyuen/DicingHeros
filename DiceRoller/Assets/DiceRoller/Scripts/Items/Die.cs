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

		public static UniqueList<Die> InspectingDice { get; protected set; } = new UniqueList<Die>();
		public bool IsInspecting
		{
			get
			{
				return InspectingDice.Contains(this);
			}
		}
		public static UniqueList<Die> SelectedDice { get; protected set; } = new UniqueList<Die>();
		public bool IsSelected
		{
			get
			{
				return SelectedDice.Contains(this);
			}
		}

		// parameters
		[Header("Die Information")]
		public Type type = Type.Unknown;
		public List<Face> faces = new List<Face>();
		public Unit connectedUnit = null;

		// components
		protected Transform effectTransform = null;
		protected LineRenderer lineRenderer = null;

		// working variables
		public int Value { get; protected set; } = -1;
		protected Quaternion lastRotation = Quaternion.identity;
		protected float lastRotatingTime = 0;
		protected bool rollInitiating = false;

		// properties
		public DieState CurrentDieState { get; protected set; } = DieState.Normal;

		// events
		public Action onValueChanged = () => { };
		public Action onDiceStateChanged = () => { };

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
		protected void RetrieveComponentReferences()
		{
			effectTransform = transform.Find("Effect");
			lineRenderer = transform.Find("Effect/Line").GetComponent<LineRenderer>();
		}

		/// <summary>
		/// Detect the current displayed value of this die.
		/// </summary>
		protected void DetectValue()
		{
			// record last moving time for stationary checking
			if (rigidBody.velocity.sqrMagnitude > 0.01f || rigidBody.angularVelocity.sqrMagnitude > 0.01f)
			{
				lastRotatingTime = Time.time;
			}

			// determine the value of this die
			int lastValue = Value;
			if (IsMoving || rollInitiating)
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

			// fire onValueChange event if needed
			if (Value != lastValue)
			{
				onValueChanged.Invoke();
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

		/// <summary>
		/// Change the current die state of the die and fire the relavent onchange event
		/// </summary>
		protected void ChangeDieState(DieState state)
		{
			if (CurrentDieState != state)
			{
				CurrentDieState = state;
				onDiceStateChanged();
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

			Player p = game.GetPlayerById(playerId);
			if (p != null)
			{
				p.dice.Add(this);
			}
		}

		/// <summary>
		///  Deregister this unit from a player.
		/// </summary>
		protected void DeregisterFromPlayer()
		{
			if (game == null)
				return;

			Player p = game.GetPlayerById(playerId);
			if (p != null)
			{
				p.dice.Remove(this);
			}
		}

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.Register(this, State.Navigation, new NavigationSB(this));
			stateMachine.Register(this, State.DiceActionSelect, new DiceActionSelectSB(this));
			stateMachine.Register(this, State.DiceThrow, new DiceThrowSB(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		protected void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
				stateMachine.DeregisterAll(this);
		}

		// ========================================================= Navigation State =========================================================
		protected class NavigationSB : StateBehaviour
		{
			protected readonly Die self = null;
			
			protected bool lastIsHovering = false;
			protected List<Tile> lastOccupiedTiles = new List<Tile>();

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
				if (self.isHovering != lastIsHovering)
				{
					if (self.isHovering)
					{
						InspectingDice.Add(self);
						self.AddEffect(StatusType.InspectingSelf);
						
					}
					else
					{ 
						InspectingDice.Remove(self);
						self.RemoveEffect(StatusType.InspectingSelf);
					}
				}
				lastIsHovering = self.isHovering;

				// go to dice action selection state when this dice is pressed
				if (self.playerId == stateMachine.Params.player.id && self.isPressed)
				{
					SelectedDice.Add(self);
					List<Die> dice = new List<Die>();
					dice.Add(self);
					stateMachine.ChangeState(State.DiceActionSelect,
						new StateParams()
						{
							player = stateMachine.Params.player,
							dice = dice
						});
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// hide occupied tiles on board
				/*
				foreach (Tile tile in lastOccupiedTiles)
				{
					tile.RemoveDisplay(this, Tile.DisplayType.Position);
				}
				lastOccupiedTiles.Clear();
				*/

				// hide dice info on ui
				if (self.IsInspecting)
				{
					InspectingDice.Remove(self);
					self.RemoveEffect(StatusType.InspectingSelf);
				}

				lastIsHovering = false;
			}
		}

		// ========================================================= Dice Action State =========================================================
		
		protected class DiceActionSelectSB : StateBehaviour
		{
			protected readonly Die self = null;

			protected bool lastIsHovering = false;
			protected List<Tile> lastOccupiedTiles = new List<Tile>();

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
				if (stateMachine.Params.dice.Contains(self))
				{
					self.AddEffect(StatusType.SelectedSelf);
				}
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				// execute only if the selected unit is this unit
				if (stateMachine.Params.player.id == self.playerId)
				{
					// show dice info on ui
					if (self.isHovering != lastIsHovering)
					{
						if (self.isHovering)
						{
							InspectingDice.Add(self);
							self.AddEffect(StatusType.InspectingSelf);
						}
						else
						{
							InspectingDice.Remove(self);
							self.RemoveEffect(StatusType.InspectingSelf);
						}
					}
					lastIsHovering = self.isHovering;

					// go to dice action selection state or navigation state when this dice is pressed
					if (self.playerId == stateMachine.Params.player.id && self.isPressed)
					{
						if (self.IsSelected)
						{
							SelectedDice.Remove(self);
							self.RemoveEffect(StatusType.SelectedSelf);
						}
						else
						{
							SelectedDice.Add(self);
							self.AddEffect(StatusType.SelectedSelf);
						}

						List<Die> dice = stateMachine.Params.dice;
						if (dice.Contains(self))
						{
							dice.Remove(self);
						}
						else
						{
							dice.Add(self);
						}

						if (dice.Count > 0)
						{
							stateMachine.ChangeState(State.DiceActionSelect,
								new StateParams()
								{
									player = stateMachine.Params.player,
									dice = dice
								});
						}
						else
						{
							stateMachine.ChangeState(State.Navigation,
								new StateParams()
								{
									player = stateMachine.Params.player
								});
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
				if (stateMachine.Params.dice.Contains(self))
				{
					self.RemoveEffect(StatusType.SelectedSelf);
				}

				// revert display as inspecting
				if (self.IsInspecting)
				{
					InspectingDice.Remove(self);
					self.RemoveEffect(StatusType.InspectingSelf);
				}

				lastIsHovering = false;
			}
		}

		// ========================================================= Dice Throw State =========================================================

		protected class DiceThrowSB : StateBehaviour
		{
			protected readonly Die self = null;

			protected bool lastIsHovering = false;
			protected List<Tile> lastOccupiedTiles = new List<Tile>();

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
				if (stateMachine.Params.dice.Contains(self))
				{
					SelectedDice.Remove(self);
				}
			}
		}
	}
}