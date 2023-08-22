using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public class Item : MonoBehaviour
	{
		// parameters
		public Sprite iconSprite = null;
		public Sprite outlineSprite = null;
		public float size = 1f;
		public int team = 0;

		// reference
		protected GameController game { get { return GameController.current; } }
		protected StateMachine stateMachine { get { return StateMachine.current; } }
		protected Board board { get { return Board.current; } }

		// component
		protected Rigidbody rigidBody = null;

		// working variables
		protected bool isSelfHovering = false;
		protected bool isUIHovering = false;
		protected bool isHovering = false;

		protected bool initiatedSelfPress = false;
		protected bool completedSelfPress = false;
		protected bool isUIPressed = false;
		protected bool isPressed = false;

		public bool IsMoving { get; protected set; }
		protected float lastMovingTime = 0;

		public List<Tile> OccupiedTiles
		{
			get 
			{
				if (Vector3.Distance(transform.position, lastOccupiedPosition) > 0.0001f)
				{
					lastOccupiedTiles.Clear();
					Board.current.GetCurrentTiles(transform.position, size, in lastOccupiedTiles);
					lastOccupiedPosition = transform.position;
				}
				return lastOccupiedTiles;
			}
		}
		protected Vector3 lastOccupiedPosition = Vector3.zero;
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
			DetectHover();
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
			isSelfHovering = true;
		}

		/// <summary>
		/// OnMouseExit is called when the mouse is stop pointing to the game object.
		/// </summary>
		protected void OnMouseExit()
		{
			isSelfHovering = false;
			initiatedSelfPress = false;
		}

		/// <summary>
		/// OnMouseDown is called when a mouse button is pressed when pointing to the game object.
		/// </summary>
		protected void OnMouseDown()
		{
			initiatedSelfPress = true;
		}


		/// <summary>
		/// OnMouseUp is called when a mouse button is released when pointing to the game object.
		/// </summary>
		protected void OnMouseUp()
		{
			if (initiatedSelfPress)
			{
				initiatedSelfPress = false;
				completedSelfPress = true;
			}
		}

		// ========================================================= Message From UI =========================================================

		/// <summary>
		/// Set the hovering flag from ui elements.
		/// </summary>
		public void SetHoveringFromUI(bool hovering)
		{
			isUIHovering = hovering;
		}

		/// <summary>
		/// Set the pressed flag from ui.
		/// </summary>
		public void SetPressedFromUI()
		{
			isUIPressed = true;
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

		/// <summary>
		/// Detect hover events.
		/// </summary>
		protected void DetectHover()
		{
			isHovering = isSelfHovering || isUIHovering;
		}

		// Detect press event and trim to a single frame flag.
		protected void DetectPress()
		{
			isPressed = false;
			if (completedSelfPress || isUIPressed)
			{
				completedSelfPress = false;
				isUIPressed = false;

				isPressed = true;
			}
		}
	}
}