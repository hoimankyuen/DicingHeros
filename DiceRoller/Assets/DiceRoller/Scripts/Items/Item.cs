using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public class Item : MonoBehaviour
	{
		// parameters
		public Sprite icon = null;
		public float size = 1f;
		public int team = 0;

		// reference
		protected GameController game { get { return GameController.current; } }
		protected StateMachine stateMachine { get { return StateMachine.current; } }
		protected Board board { get { return Board.current; } }

		// component
		protected Rigidbody rigidBody = null;

		// working variables
		protected bool isHovering = false;
		protected bool initiatedPress = false;
		protected bool completedPress = false;
		protected bool isPressed = false;

		public bool IsMoving { get; protected set; }
		protected float lastMovingTime = 0;

		public List<Tile> OccupiedTiles => Vector3.Distance(transform.position, lastPosition) < 0.0001f ? lastOccupiedTiles : RefreshOccupiedTiles();
		protected Vector3 lastPosition = Vector3.zero;
		protected List<Tile> lastOccupiedTiles = new List<Tile>();

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected virtual void Awake()
		{
			rigidBody = GetComponent<Rigidbody>();
		}


		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected virtual void Start()
		{
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected virtual void Update()
		{
			DetectMovement();
			DetectPress();
		}

		/// <summary>
		/// FixedUpdate is called at a regular interval, along side with physics simulation.
		/// </summary>
		protected virtual void FixedUpdate()
		{

		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected virtual void OnDestroy()
		{
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
			initiatedPress = false;
		}

		/// <summary>
		/// OnMouseDown is called when a mouse button is pressed when pointing to the game object.
		/// </summary>
		protected void OnMouseDown()
		{
			initiatedPress = true;
		}


		/// <summary>
		/// OnMouseUp is called when a mouse button is released when pointing to the game object.
		/// </summary>
		protected void OnMouseUp()
		{
			if (initiatedPress)
			{
				initiatedPress = false;
				completedPress = true;
			}
		}

		// ========================================================= General Behaviour =========================================================

		/// <summary>
		/// Detect movement and update the IsMoving flag accordingly.
		/// </summary>
		protected void DetectMovement()
		{
			if (rigidBody.velocity.sqrMagnitude > 0.01f || rigidBody.angularVelocity.sqrMagnitude > 0.01f)
			{
				lastMovingTime = Time.time;
			}
			IsMoving = Time.time - lastMovingTime < 0.25f;
		}

		// Detect press event and trim to a single frame flag.
		protected void DetectPress()
		{
			isPressed = false;
			if (completedPress)
			{
				completedPress = false;
				isPressed = true;
			}
		}

		/// <summary>
		/// Find which tiles this game object is in.
		/// </summary>
		protected List<Tile> RefreshOccupiedTiles()
		{
			lastOccupiedTiles.Clear();
			lastOccupiedTiles.AddRange(Board.current.GetCurrentTiles(transform.position, size));
			return lastOccupiedTiles;
		}
	}
}