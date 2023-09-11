using QuickerEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public class Item : MonoBehaviour
	{
		[System.Serializable]
		public enum StatusType
		{
			Depleted,
			SelectedEnemy,
			SelectedSelf,
			InspectingEnemy,
			InspectingSelf,
		}

		// parameters
		[Header("Basic Information")]
		public new string name = "";
		public Sprite iconSprite = null;
		public Sprite outlineSprite = null;
		public Sprite overlaySprite = null;
		public float size = 1f;
		public int playerId = 0;

		[Header("Data")]
		public ItemEffectStyle effectStyle = null;

		// reference
		protected GameController game => GameController.current;
		protected StateMachine stateMachine => StateMachine.current;
		protected Board board => Board.current;

		// component
		protected Rigidbody rigidBody = null;
		protected Outline outline = null;
		protected Overlay overlay = null;

		// working variables
		protected bool isHovering = false;
		protected bool isSelfHovering = false;
		protected bool isUIHovering = false;

		protected bool isPressed = false;
		protected bool initiatedSelfPress = false;
		protected bool completedSelfPress = false;
		protected bool isUIPressed = false;

		protected HashSet<StatusType> statusList = new HashSet<StatusType>();

		// properties
		public bool IsFallen { get; protected set; } = false;

		public bool IsMoving { get; protected set; } = false;
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

		// events
		public Action onStatusChanged = () => { }; 

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected virtual void Awake()
		{
			rigidBody = GetComponent<Rigidbody>();
			outline = GetComponent<Outline>();
			overlay = GetComponent<Overlay>();		
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
		/// Detect if fallen through and update flag accordingly.
		/// </summary>
		protected void DetectFallen()
		{
			IsFallen = transform.position.y < -10;
		}

		/// <summary>
		/// Detect hover events.
		/// </summary>
		protected void DetectHover()
		{
			isHovering = isSelfHovering || isUIHovering;
		}

		/// <summary>
		/// Detect press event and trim to a single frame flag.
		/// </summary>
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

		// ========================================================= Effects =========================================================
		
		/// <summary>
		/// Add a new item effect to be shown.
		/// </summary>
		protected void AddEffect(StatusType effectType)
		{
			statusList.Add(effectType);
			effectStyle.ResolveEffect(statusList, out Color outlineColor, out Color overlayColor);

			if (outline.Color != outlineColor)
			{
				outline.Color = outlineColor;
			}

			if (overlay.Color != overlayColor)
			{
				overlay.Color = overlayColor;
			}

			onStatusChanged.Invoke();
		}

		/// <summary>
		/// Remove an existing item effect to be shown. 
		/// </summary>
		protected void RemoveEffect(StatusType effectType)
		{
			statusList.Remove(effectType);
			effectStyle.ResolveEffect(statusList, out Color outlineColor, out Color overlayColor);

			if (outline.Color != outlineColor)
			{
				outline.Color = outlineColor;
			}

			if (overlay.Color != overlayColor)
			{
				overlay.Color = overlayColor;
			}

			onStatusChanged.Invoke();
		}
	}
}