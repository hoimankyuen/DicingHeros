using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickerEffects;
using System.Linq;

namespace DiceRoller
{
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

	public class Die : Item
	{
		public static Dictionary<int, List<Die>> DiceInTeam { get; protected set; } = new Dictionary<int, List<Die>>();
		public static UniqueList<Die> InspectingDice { get; protected set; } = new UniqueList<Die>();

		// parameters
		public List<Face> faces = new List<Face>();
		public Unit connectedUnit = null;

		// components
		protected Outline outline = null;
		protected Overlay overlay = null;
		protected Transform effectTransform = null;
		protected LineRenderer lineRenderer = null;

		// working variables
	   	public int Value => (IsMoving || rollInitiating) ? -1 : Quaternion.Angle(transform.rotation, lastRotation) < 1f ? lastValue : RefreshtValue();
		protected Quaternion lastRotation = Quaternion.identity;
		protected bool rollInitiating = false;
		protected int lastValue = 0;

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
			RegisterToTeam();
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected override void Update()
		{
			base.Update();
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
			DeregisterFromTeam();
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
			outline = GetComponent<Outline>();
			overlay = GetComponent<Overlay>();
			effectTransform = transform.Find("Effect");
			lineRenderer = transform.Find("Effect/Line").GetComponent<LineRenderer>();
		}


		/// <summary>
		/// Find the value got by this die.
		/// </summary>
		protected int RefreshtValue()
		{
			float nearestAngle = float.MaxValue;
			int value = 0;
			foreach (Face face in faces)
			{
				float angle = Vector3.Angle(transform.rotation * Quaternion.Euler(face.euler) * Vector3.up, Vector3.up);
				if (angle < nearestAngle)
				{
					nearestAngle = angle;
					value = face.value;
				}
			}
			lastRotation = transform.rotation;
			lastValue = value;
			return value;
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

		// ========================================================= Team Behaviour =========================================================

		/// <summary>
		/// Register this unit to a team.
		/// </summary>
		protected void RegisterToTeam()
		{
			if (!DiceInTeam.ContainsKey(team))
			{
				DiceInTeam[team] = new List<Die>();
			}
			DiceInTeam[team].Add(this);
		}

		/// <summary>
		///  Deregister this unit from a team.
		/// </summary>
		protected void DeregisterFromTeam()
		{
			DiceInTeam[team].Remove(this);
			if (DiceInTeam[team].Count == 0)
			{
				DiceInTeam.Remove(team);
			}
		}

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.RegisterStateBehaviour(this, State.Navigation, new NavigationSB(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		protected void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
				stateMachine.DeregisterStateBehaviour(this);
		}

		// ========================================================= Navigation State =========================================================

		protected class NavigationSB : StateBehaviour
		{
			protected readonly Die dice = null;
			
			protected bool lastIsHovering = false;
			protected List<Tile> lastOccupiedTiles = new List<Tile>();

			/// <summary>
			/// Constructor.
			/// </summary>
			public NavigationSB(Die dice)
			{
				this.dice = dice;
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
				// show hovering outline
				dice.outline.Show = dice.isHovering;

				// show occupied tiles on the board
				List<Tile> tiles = dice.isHovering ? dice.OccupiedTiles : Tile.EmptyTiles;
				foreach (Tile tile in tiles.Except(lastOccupiedTiles))
				{
					tile.AddDisplay(this, Tile.DisplayType.Position);
				}
				foreach (Tile tile in lastOccupiedTiles.Except(tiles))
				{
					tile.RemoveDisplay(this, Tile.DisplayType.Position);
				}
				lastOccupiedTiles.Clear();
				lastOccupiedTiles.AddRange(tiles);

				// show dice info on ui
				if (dice.isHovering != lastIsHovering)
				{
					if (dice.isHovering)
					{
						InspectingDice.Add(dice);
					}
					else
					{
						InspectingDice.Remove(dice);
					}
				}
				lastIsHovering = dice.isHovering;
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// hide hovering outline
				dice.outline.Show = false;

				// hide occupied tiles on board
				foreach (Tile tile in lastOccupiedTiles)
				{
					tile.RemoveDisplay(this, Tile.DisplayType.Position);
				}
				lastOccupiedTiles.Clear();

				// hide dice info on ui
				InspectingDice.Remove(dice);
				lastIsHovering = false;
			}
		}
	}
}