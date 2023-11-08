using QuickerEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DicingHeros
{
	public class Item : MonoBehaviour
	{
		[System.Serializable]
		public enum EffectType
		{
			Depleted,
			SelectedEnemy,
			SelectedFriend,
			SelectedSelf,
			InspectingEnemy,
			InspectingFriend,
			InspectingSelf,
			PossibleEnemy,
			PossibleFriend,
			PossibleSelf,
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

		// reference
		protected GameController game => GameController.current;
		protected StateMachine stateMachine => StateMachine.current;
		protected Board board => Board.current;

		// component
		protected Rigidbody rigidBody = null;
		protected Transform modelTransform = null;
		protected Transform effectTransform = null;
		protected Outline outline = null;
		protected Overlay overlay = null;

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
			modelTransform = transform.Find("Model");
			effectTransform = transform.Find("Effect");
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
			DetectEnteringUI();
			DetectTilesOccupation();
			DetectMovement();
			DetectFallen();
			DetectHover();
			DetectPress();
			DetectDrag();
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

		// ========================================================= Monobehaviour Methods (Inputs) =========================================================

		/// <summary>
		/// OnMouseEnter is called when the mouse is start pointing to the game object.
		/// </summary>
		private void OnMouseEnter()
		{
			if (EventSystem.current.IsPointerOverGameObject())
				return;

			_IsSelfHovering = true;
		}

		/// <summary>
		/// OnMouseExit is called when the mouse is stop pointing to the game object.
		/// </summary>
		private void OnMouseExit()
		{
			_IsSelfHovering = false;
		}

		/// <summary>
		/// OnMouseDown is called when a mouse button is pressed when pointing to the game object.
		/// </summary>
		private void OnMouseDown()
		{
			if (EventSystem.current.IsPointerOverGameObject())
				return;

			for (int i = 0; i < 3; i++)
			{
				_StartedSelfPress[i] = true;
				_StartedSelfDrag[i] = true;
				_LastMousePosition[i] = Input.mousePosition;
			}
		}

		/// <summary>
		/// OnMouseUp is called when a mouse button is released when pointing to the game object.
		/// </summary>
		private void OnMouseUp()
		{
			for (int i = 0; i < 3; i++)
			{
				if (_StartedSelfPress[i] && Vector2.Distance(_LastMousePosition[i], Input.mousePosition) < 2f)
				{
					_StartedSelfPress[i] = false;
					_StartedSelfDrag[i] = false;
					_CompletedSelfPress[i] = true;
					_LastMousePosition[i] = Vector2.negativeInfinity;
				}
			}
		}

		/// <summary>
		/// Force stop input when the mouse enter UI.
		/// </summary>
		private void DetectEnteringUI()
		{
			if (EventSystem.current.IsPointerOverGameObject())
			{
				// stop hovering
				if (_IsSelfHovering)
				{
					_IsSelfHovering = false;
				}

				// stop press
				for (int i = 0; i < 3; i++)
				{
					if (_StartedSelfPress[i])
					{
						_StartedSelfPress[i] = false;
						_LastMousePosition[i] = Vector2.negativeInfinity;
					}
				}

				// note: Drag is not stopped since dragging may passes through UI
			}
		}

		// ========================================================= Message From External (Inputs) =========================================================

		/// <summary>
		/// An OnMouseEnter triggered from UI.
		/// </summary>
		public void OnUIMouseEnter()
		{
			_IsUIHovering = true;
		}

		/// <summary>
		/// An OnMouseExit triggered from UI.
		/// </summary>
		public void OnUIMouseExit()
		{
			_IsUIHovering = false;
		}

		/// <summary>
		/// An OnMouseDown triggered from UI.
		/// </summary>
		public void OnUIMouseDown(int mouseButton)
		{
			_StartedUIPress[mouseButton] = true;
			_StartedUIDrag[mouseButton] = true;
			_LastMousePosition[mouseButton] = Input.mousePosition;
		}

		/// <summary>
		/// An OnMouseUp triggered from UI.
		/// </summary>
		public void OnUIMouseUp(int mouseButton)
		{
			if (_StartedUIPress[mouseButton] && Vector2.Distance(_LastMousePosition[mouseButton], Input.mousePosition) < 2f)
			{
				_StartedUIPress[mouseButton] = false;
				_StartedUIDrag[mouseButton] = false;
				_CompletedUIPress[mouseButton] = true;
				_LastMousePosition[mouseButton] = Vector2.negativeInfinity;
			}
		}

		// ========================================================= Message From AI (Inputs) =========================================================

		/// <summary>
		/// An OnMouseEnter triggered from AI.
		/// </summary>
		public void OnAIMouseEnter()
		{
			_IsAIHovering = true;
		}

		/// <summary>
		/// An OnMouseExit triggered from AI.
		/// </summary>
		public void OnAIMouseExit()
		{
			_IsAIHovering = false;
		}

		/// <summary>
		/// An OnMousePress triggered from AI.
		/// </summary>
		public void OnAIMousePress(int mouseButton)
		{
			_CompletedAIPress[mouseButton] = true;
		}

		/// <summary>
		/// An OnMouseStartDrag triggered from AI.
		/// </summary>
		public void OnAIMouseStartDrag(int mouseButton)
		{
			_StartedAIDrag[mouseButton] = true;
		}

		/// <summary>
		/// An OnMouseCompleteDrag triggered from AI.
		/// </summary>
		public void OnAIMouseCompletetDrag(int mouseButton)
		{
			_CompletedAIDrag[mouseButton] = true;
		}

		// ========================================================= Properties (IsHovering) =========================================================

		/// <summary>
		/// Flag for if user is hovering on this item by any means.
		/// </summary>
		protected bool IsHovering { get; private set; } = false;
		private bool _IsSelfHovering = false;
		private bool _IsUIHovering = false;
		private bool _IsAIHovering = false;

		/// <summary>
		/// Detect hover events.
		/// </summary>
		private void DetectHover()
		{
			if (game.PersonInControl == GameController.Person.Player)
			{
				IsHovering = _IsSelfHovering || _IsUIHovering;
			}
			else if (game.PersonInControl == GameController.Person.AI)
			{
				IsHovering = _IsAIHovering;
			}
		}

		// ========================================================= Properties (IsPressed) =========================================================

		/// <summary>
		/// Flag for if user has pressed on this item by any means.
		/// </summary>
		protected bool[] IsPressed { get; private set; } = new bool[] { false, false, false };
		private readonly Vector2[] _LastMousePosition = new Vector2[] { Vector2.negativeInfinity, Vector2.negativeInfinity, Vector2.negativeInfinity };
		private readonly bool[] _StartedSelfPress = new bool[] { false, false, false };
		private readonly bool[] _CompletedSelfPress = new bool[] { false, false, false };
		private readonly bool[] _StartedUIPress = new bool[] { false, false, false };
		private readonly bool[] _CompletedUIPress = new bool[] { false, false, false };
		private readonly bool[] _CompletedAIPress = new bool[] { false, false, false };

		/// <summary>
		/// Detect press event and trim to a single frame flag.
		/// </summary>
		private void DetectPress()
		{
			if (game.PersonInControl == GameController.Person.Player)
			{
				for (int i = 0; i < 3; i++)
				{
					IsPressed[i] = false;
					if (_CompletedSelfPress[i] || _CompletedUIPress[i])
					{
						_CompletedSelfPress[i] = false;
						_CompletedUIPress[i] = false;

						IsPressed[i] = true;
					}
				}
			}
			else if (game.PersonInControl == GameController.Person.AI)
			{
				for (int i = 0; i < 3; i++)
				{
					IsPressed[i] = false;
					if (_CompletedAIPress[i])
					{
						_CompletedAIPress[i] = false;

						IsPressed[i] = true;
					}
				}
			}
		}

		// ========================================================= Properties (IsStartedDrag & IsCompletedDrag) =========================================================

		/// <summary>
		/// Flag for if user has started drag on this item by any means.
		/// </summary>
		protected bool[] IsStartedDrag { get; private set; } = new bool[] { false, false, false };
		/// <summary>
		/// Flag for if user has completed a drag for this item.
		/// </summary>
		protected bool[] IsCompletedDrag { get; private set; } = new bool[] { false, false, false };
		/// <summary>
		/// Flag for if user has ended drag on this item component by any means.
		/// </summary>
		private readonly bool[] _StartedSelfDrag = new bool[] { false, false, false };
		private readonly bool[] _StartedUIDrag = new bool[] { false, false, false };
		private readonly bool[] _StartedAIDrag = new bool[] { false, false, false };
		private readonly bool[] _CompletedAIDrag = new bool[] { false, false, false };

		/// <summary>
		/// Detect draga event and trim to a single frame flag.
		/// </summary>
		private void DetectDrag()
		{
			if (game.PersonInControl == GameController.Person.Player)
			{
				for (int i = 0; i < 3; i++)
				{
					IsStartedDrag[i] = false;
					if ((_StartedSelfDrag[i] || _StartedUIDrag[i]) && Vector2.Distance(_LastMousePosition[i], Input.mousePosition) >= 2f)
					{
						_StartedSelfPress[i] = false;
						_StartedUIPress[i] = false;
						_StartedSelfDrag[i] = false;
						_StartedUIDrag[i] = false;

						IsStartedDrag[i] = true;
					}

					IsCompletedDrag[i] = false;
					if (Input.GetMouseButtonUp(i))
					{
						IsCompletedDrag[i] = true;
					}
				}
			}
			else if (game.PersonInControl == GameController.Person.AI)
			{
				for (int i = 0; i < 3; i++)
				{
					IsStartedDrag[i] = false;
					if (_StartedAIDrag[i])
					{
						_StartedAIDrag[i] = false;

						IsStartedDrag[i] = true;
					}

					IsCompletedDrag[i] = false;
					if (_CompletedAIDrag[i])
					{
						_CompletedAIDrag[i] = false;

						IsCompletedDrag[i] = true;
					}
				}
			}
		}

		// ========================================================= Properties (IsHidden) =========================================================

		/// <summary>
		/// Flag for if this item is hidden from the game.
		/// </summary>
		public bool IsHidden 
		{ 
			get
			{
				return _IsHidden;
			}
			protected set
			{
				if (_IsHidden != value)
				{
					_IsHidden = value;
					rigidBody.isKinematic = value;
					modelTransform.gameObject.SetActive(!value);
					effectTransform.gameObject.SetActive(!value);
					OnIsHiddenChanged.Invoke();
				}
			}
		}
		private bool _IsHidden = false;

		/// <summary>
		/// Event raised when the hidden state of this item is changed.
		/// </summary>
		public event Action OnIsHiddenChanged = () => { };

		// ========================================================= Properties (IsHoveringOnTile) =========================================================

		/// <summary>
		/// Flag for if user is hovering on the tiles occupied by this item.
		/// </summary>
		protected bool IsHoveringOnTile { get; private set; } = false;
		private Dictionary<Tile, bool> _OccupiedTilesHovering = new Dictionary<Tile, bool>();

		/// <summary>
		/// Set the hovering flag from tile elements.
		/// </summary>
		public void SetHoveringFromTile(Tile tile, bool hovering)
		{
			if (_OccupiedTilesHovering.ContainsKey(tile))
			{
				_OccupiedTilesHovering[tile] = hovering;
				IsHoveringOnTile = _OccupiedTilesHovering.Aggregate(true, (result, x) => result && x.Value);
			}
		}

		// ========================================================= Properties (OccupiedTiles) =========================================================

		/// <summary>
		///A read only list of tiles that this item occupies.
		/// </summary>
		public IReadOnlyCollection<Tile> OccupiedTiles
		{
			get
			{
				return _OccupiedTiles.AsReadOnly();
			}
		}
		private List<Tile> _OccupiedTiles = new List<Tile>();
		private Vector3 _LastOccupiedPosition = Vector3.zero;
		private List<Tile> _LastOccupiedTiles = new List<Tile>();
		private bool _LastIsHidden = false;
		private float _LastBoardUpdatedTime = 0;

		/// <summary>
		/// Flag raised when the tiles occupied by this item are changed.
		/// </summary>
		public event Action OnOccupiedTilesChanged = () => { };

		/// <summary>
		/// Detect tile occupation and update the occupation list.
		/// </summary>
		private void DetectTilesOccupation()
		{
			if (!IsMoving && (Vector3.SqrMagnitude(transform.position - _LastOccupiedPosition) > 0.00000001f || IsHidden != _LastIsHidden || board.BoardUpdatedTime != _LastBoardUpdatedTime))
			{
				// retrieve tile that this item are in
				if (IsHidden)
				{
					_OccupiedTiles.Clear();
				}
				else
				{
					Board.current.GetCurrentTiles(transform.position, size, _OccupiedTiles);
				}

				if (!_OccupiedTiles.SequenceEqual(_LastOccupiedTiles))
				{
					// change occupant information of the tiles
					foreach (Tile t in _LastOccupiedTiles.Except(_OccupiedTiles))
					{
						t.RemoveOccupant(this);
						_OccupiedTilesHovering.Remove(t);
					}
					foreach (Tile t in _OccupiedTiles.Except(_LastOccupiedTiles))
					{
						_OccupiedTilesHovering.Add(t, false);
						t.AddOccupant(this);
					}

					// update cache
					_LastOccupiedTiles.Clear();
					_LastOccupiedTiles.AddRange(_OccupiedTiles);

					// raise event
					OnOccupiedTilesChanged.Invoke();
				}

				// update cache
				_LastOccupiedPosition = transform.position;
				_LastIsHidden = IsHidden;
				_LastBoardUpdatedTime = board.BoardUpdatedTime;
			}
		}

		// ========================================================= Properties (Player) =========================================================

		/// <summary>
		/// The player that owns this item.
		/// </summary>
		public Player Player { get; private set; } = null;

		// ========================================================= Properties (IsMoving) =========================================================

		/// <summary>
		/// Flag for if this item is still moving.
		/// </summary>
		public bool IsMoving { get; private set; } = false;
		private float _lastMovingTime = 0;
		private Vector3 _lastMovePosition = Vector3.zero;

		/// <summary>
		/// Detect movement and update the flags accordingly.
		/// </summary>
		private void DetectMovement()
		{
			if (!IsHidden && (rigidBody.velocity.sqrMagnitude > 0.01f || rigidBody.angularVelocity.sqrMagnitude > 0.01f || Vector3.SqrMagnitude(transform.position -  _lastMovePosition) > 0.00000001f))
			{
				_lastMovePosition = transform.position;
				_lastMovingTime = Time.time;
			}
			IsMoving = (Time.time - _lastMovingTime) < 0.1f;
		}

		// ========================================================= Properties (IsFalling) =========================================================

		/// <summary>
		/// Flag for if this item is fallen out of the board.
		/// </summary>
		public bool IsFallen { get; private set; } = false;

		/// <summary>
		/// Detect if fallen through and update flags accordingly.
		/// </summary>
		private void DetectFallen()
		{
			IsFallen = transform.position.y < -10;
		}

		// ========================================================= Effects =========================================================

		/*
		private HashSet<EffectType> effectSet = new HashSet<EffectType>();

		/// <summary>
		/// Show or hide an item effect.
		/// </summary>
		protected void ShowEffect(EffectType effectType, bool show)
		{
			// determine either show effect, hide effect or do nothing, preventing excessive calls
			if (!effectSet.Contains(effectType) && show)
			{
				effectSet.Add(effectType);
			}
			else if (effectSet.Contains(effectType) && !show)
			{
				effectSet.Remove(effectType);
			}
			else
			{
				return;
			}

			// resolve the effects
			effectStyle.ResolveEffect(effectSet, out Color outlineColor, out Color overlayColor);
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
		}

		/// <summary>
		/// Hide an item effect.
		/// </summary>
		protected void HideEffect(EffectType effectType)
		{
			ShowEffect(effectType, false);
		}
		*/
	}
}