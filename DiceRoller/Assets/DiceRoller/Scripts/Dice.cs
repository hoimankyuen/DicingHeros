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

	public class Dice : MonoBehaviour
	{
		public static List<Dice> InspectingDice { get; protected set; } = new List<Dice>();

		// parameters
		public Sprite icon = null;
		public float size = 1f;
		public List<Face> faces = new List<Face>();
		public Unit connectedUnit = null;

		// reference
		protected GameController Game { get { return GameController.Instance; } }
		protected StateMachine stateMachine { get { return StateMachine.Instance; } }
		protected Board board { get { return Board.Instance; } }

		// components
		protected Rigidbody rigidBody = null;
		protected Outline outline = null;
		protected Transform effectTransform = null;
		protected LineRenderer lineRenderer = null;

		// working variables
		protected bool isHovering = false;
		protected bool initatedPress = false;
		
		public bool IsMoving { get; protected set; }
		protected float lastMovingTime = 0;
		protected bool rollInitiating = false;

		public int Value => IsMoving ? -1 : Quaternion.Angle(transform.rotation, lastRotation) < 1f ? lastValue : RefreshtValue();
		protected Quaternion lastRotation = Quaternion.identity;
		protected int lastValue = 0;

		public List<Tile> OccupiedTiles => Vector3.Distance(transform.position, lastPosition) < 0.0001f ? lastOccupiedTiles : RefreshOccupiedTiles();
		protected Vector3 lastPosition = Vector3.zero;
		protected List<Tile> lastOccupiedTiles = new List<Tile>();

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected void Awake()
		{
			RetrieveComponentReferences();
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected void Start()
		{
			RegisterStateBehaviours();
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected void Update()
		{
			DetectMovement();

			effectTransform.rotation = Quaternion.identity;

			if (connectedUnit != null)
			{
				lineRenderer.gameObject.SetActive(true);
				lineRenderer.SetPosition(0, transform.position);
				lineRenderer.SetPosition(1, connectedUnit.transform.position + 0.1f * Vector3.up);
			}
			else
			{
				lineRenderer.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// FixedUpdate is called at a regular interval, along side with physics simulation.
		/// </summary>
		protected void FixedUpdate()
		{
			rollInitiating = false;
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected void OnDestroy()
		{
			DeregisterStateBehaviours();
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

		/// <summary>
		/// OnMouseEnter is called when the mouse is start pointing to the game object.
		/// </summary>
		protected void OnMouseEnter()
		{
			isHovering = true;

			
		}

		/// <summary>
		/// OnMouseExit is called when the mouse is stop pointing to the game object.
		/// </summary>
		protected void OnMouseExit()
		{
			isHovering = false;
			initatedPress = false;

			InspectingDice.Remove(this);
		}

		/// <summary>
		/// OnMouseDown is called when a mouse button is pressed when pointing to the game object.
		/// </summary>
		protected void OnMouseDown()
		{
			initatedPress = true;
		}


		/// <summary>
		/// OnMouseUp is called when a mouse button is released when pointing to the game object.
		/// </summary>
		void OnMouseUp()
		{
			initatedPress = false;
		}

		// ========================================================= General Behaviour =========================================================

		/// <summary>
		/// Retrieve component references for this unit.
		/// </summary>
		protected void RetrieveComponentReferences()
		{
			rigidBody = GetComponentInChildren<Rigidbody>();
			outline = GetComponentInChildren<Outline>();
			effectTransform = transform.Find("Effect");
			lineRenderer = transform.Find("Effect/Line").GetComponent<LineRenderer>();
		}

		/// <summary>
		/// Detect movement and update the IsMoving flag accordingly.
		/// </summary>
		protected void DetectMovement()
		{
			if (rigidBody.velocity.sqrMagnitude > 0.01f || rigidBody.angularVelocity.sqrMagnitude > 0.01f)
			{
				lastMovingTime = Time.time;
			}
			IsMoving = rollInitiating || (Time.time - lastMovingTime < 0.25f);
		}

		/// <summary>
		/// Find which tiles this game object is in.
		/// </summary>
		protected List<Tile> RefreshOccupiedTiles()
		{
			lastOccupiedTiles.Clear();
			lastOccupiedTiles.AddRange(Board.Instance.GetCurrentTiles(transform.position, size));
			return lastOccupiedTiles;
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


		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.RegisterStateBehaviour(this, State.Navigation, new NavitigationStateBehaviour(this));
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

		protected class NavitigationStateBehaviour : IStateBehaviour
		{
			protected readonly Dice dice = null;
			protected GameController game { get { return GameController.Instance; } }
			protected StateMachine stateMachine { get { return StateMachine.Instance; } }
			protected Board board { get { return Board.Instance; } }

			protected bool lastIsHovering = false;
			protected List<Tile> lastOccupiedTiles = new List<Tile>();

			/// <summary>
			/// Constructor.
			/// </summary>
			public NavitigationStateBehaviour(Dice dice)
			{
				this.dice = dice;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public void OnStateEnter()
			{
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public void OnStateUpdate()
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

				// show tile info on ui
				if (dice.isHovering != lastIsHovering)
				{
					if (dice.isHovering)
					{
						if (!InspectingDice.Contains(dice))
						{
							InspectingDice.Add(dice);
						}
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
			public void OnStateExit()
			{
				// hide hovering outline
				dice.outline.Show = false;

				// hide occupied tiles on board
				foreach (Tile tile in lastOccupiedTiles)
				{
					tile.RemoveDisplay(this, Tile.DisplayType.Position);
				}
				lastOccupiedTiles.Clear();
			}
		}
	}
}