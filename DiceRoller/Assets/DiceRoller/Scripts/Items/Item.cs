using QuickerEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
			SelectedFriend,
			SelectedSelf,
			InspectingEnemy,
			InspectingFriend,
			InspectingSelf,
		}

		// parameters
		[Header("Basic Information")]
		public new string name = "";
		public Sprite iconSprite = null;
		public Sprite outlineSprite = null;
		public Sprite overlaySprite = null;
		public float size = 1f;
		public float height = 1f;
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
		private HashSet<StatusType> statusList = new HashSet<StatusType>();

		// events
		public Action onStatusChanged = () => { };

		// ========================================================= Properties =========================================================

		/// <summary>
		/// The player that owns this item.
		/// </summary>
		public Player Player
		{
			get
			{
				return _player;
			}
			private set
			{
				_player = value;
			}
		}
		protected Player _player = null;


		/// <summary>
		/// Flag for if this item is fallen out of the board.
		/// </summary>
		public bool IsFallen { get; private set; } = false;

		/// <summary>
		/// Flog for if this item is still moving.
		/// </summary>
		public bool IsMoving { get; private set; } = false;
		private float lastMovingTime = 0;

		/// <summary>
		/// Flag for if user is hovering on this item by any means.
		/// </summary>
		protected bool IsHoveringOnObject { get; private set; } = false;
		private bool isSelfHovering = false;
		private bool isUIHovering = false;

		/// <summary>
		/// Flag for if user has pressed on this item by any means.
		/// </summary>
		protected bool IPressedOnObject { get; private set; } = false;
		private bool initiatedSelfPress = false;
		private bool completedSelfPress = false;
		private bool isUIPressed = false;


		/// <summary>
		///A read only list of tiles that this item occupies.
		/// </summary>
		public IReadOnlyCollection<Tile> OccupiedTiles
		{
			get
			{
				return occupiedTiles.AsReadOnly();
			}
		}
		private List<Tile> occupiedTiles = new List<Tile>();
		private Vector3 lastOccupiedPosition = Vector3.zero;
		private List<Tile> lastOccupiedTile = new List<Tile>();

		/// <summary>
		/// Flag for if user is hovering on the tiles occupied by this item.
		/// </summary>
		protected bool IsHoveringOnTile { get; private set; } = false;
		private Dictionary<Tile, bool> occupiedTilesHovering = new Dictionary<Tile, bool>();

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
			Player = game.GetPlayerById(playerId);
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected virtual void Update()
		{
			DetectTilesOccupation();
			DetectMovement();
			DetectFallen();
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

		// ========================================================= Message From External =========================================================

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

		/// <summary>
		/// Set the hovering flag from tile elements.
		/// </summary>
		public void SetHoveringFromTile(Tile tile, bool hovering)
		{
			if (occupiedTilesHovering.ContainsKey(tile))
			{
				occupiedTilesHovering[tile] = hovering;
				IsHoveringOnTile = occupiedTilesHovering.Aggregate(true, (result, x) => result && x.Value);
			}
		}
		// ========================================================= General Behaviour =========================================================

		/// <summary>
		/// Detect tile occupation and update the occupation list.
		/// </summary>
		private void DetectTilesOccupation()
		{
			if (Vector3.Distance(transform.position, lastOccupiedPosition) > 0.0001f)
			{
				Board.current.GetCurrentTiles(transform.position, size, ref occupiedTiles);
				if (!occupiedTiles.SequenceEqual(lastOccupiedTile))
				{
					foreach (Tile t in lastOccupiedTile.Except(occupiedTiles))
					{
						t.RemoveOccupant(this);
						occupiedTilesHovering.Remove(t);
					}
					foreach (Tile t in occupiedTiles.Except(lastOccupiedTile))
					{
						occupiedTilesHovering.Add(t, false);
						t.AddOccupant(this);
					}

					// update cache
					lastOccupiedTile.Clear();
					lastOccupiedTile.AddRange(occupiedTiles);
				}

				// update cache
				lastOccupiedPosition = transform.position;
			}
		}

		/// <summary>
		/// Detect movement and update the flags accordingly.
		/// </summary>
		private void DetectMovement()
		{
			if (rigidBody.velocity.sqrMagnitude > 0.01f || rigidBody.angularVelocity.sqrMagnitude > 0.01f)
			{
				lastMovingTime = Time.time;
			}
			IsMoving = Time.time - lastMovingTime < 0.25f;
		}

		/// <summary>
		/// Detect if fallen through and update flags accordingly.
		/// </summary>
		private void DetectFallen()
		{
			IsFallen = transform.position.y < -10;
		}

		/// <summary>
		/// Detect hover events.
		/// </summary>
		private void DetectHover()
		{
			IsHoveringOnObject = isSelfHovering || isUIHovering;
		}

		/// <summary>
		/// Detect press event and trim to a single frame flag.
		/// </summary>
		private void DetectPress()
		{
			IPressedOnObject = false;
			if (completedSelfPress || isUIPressed)
			{
				completedSelfPress = false;
				isUIPressed = false;

				IPressedOnObject = true;
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
				outline.enabled = outlineColor.a != 0;
				outline.Color = outlineColor;
			}

			if (overlay.Color != overlayColor)
			{
				overlay.enabled = overlayColor.a != 0;
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
				outline.enabled = outline.Color.a != 0;
				outline.Color = outlineColor;
			}

			if (overlay.Color != overlayColor)
			{
				overlay.enabled = overlayColor.a != 0;
				overlay.Color = overlayColor;
			}

			onStatusChanged.Invoke();
		}
	}
}