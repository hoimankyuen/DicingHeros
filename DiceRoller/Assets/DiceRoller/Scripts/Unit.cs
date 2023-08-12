using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuickerEffects;
using UnityEngine;

namespace DiceRoller
{
	public class Unit : MonoBehaviour
	{
		// parameters
		public float size = 1f;
		public int movement = 4;

		// reference
		protected GameController game { get { return GameController.Instance; } }
		protected StateMachine stateMachine { get { return StateMachine.Instance; } }
		protected Board board { get { return Board.Instance; } }

		// components
		protected Rigidbody rigidBody = null;
		protected Outline outline = null;

		// working variables
		protected bool isHovering = false;
		protected bool initatedPress = false;

		public bool IsMoving { get; protected set; }
		protected float lastMovingTime = 0;

		public List<Tile> OccupiedTiles => Vector3.Distance(transform.position, lastPosition) < 0.0001f ? lastOccupiedTiles : RefreshOccupiedTiles();
		protected Vector3 lastPosition = Vector3.zero;
		protected List<Tile> emptyTiles = new List<Tile>();
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
		void OnMouseExit()
		{
			isHovering = false;
			initatedPress = false;
		}

		/// <summary>
		/// OnMouseDown is called when a mouse button is pressed when pointing to the game object.
		/// </summary>
		void OnMouseDown()
		{
			initatedPress = true;
		}

		/// <summary>
		/// OnMouseUp is called when a mouse button is released when pointing to the game object.
		/// </summary>
		void OnMouseUp()
		{
			if (stateMachine.CurrentState == State.Navigation)
			{
				if (initatedPress)
				{
					stateMachine.ChangeState(State.UnitMovementSelection, this);
				}
			}
			initatedPress = false;
		}

		// ========================================================= General Behaviour =========================================================

		/// <summary>
		/// Retrieve component references for this unit.
		/// </summary>
		protected void RetrieveComponentReferences()
		{
			rigidBody = GetComponent<Rigidbody>();
			outline = GetComponent<Outline>();
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
			IsMoving = Time.time - lastMovingTime < 0.25f;
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

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.RegisterStateBehaviour(this, State.Navigation, new NavitigationStateBehaviour(this));
			stateMachine.RegisterStateBehaviour(this, State.UnitMovementSelection, new UnitMovementSelectionStateBehaviour(this));
			stateMachine.RegisterStateBehaviour(this, State.UnitMovement, new UnitMovementStateBehaviour(this));
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
			protected readonly Unit unit = null;
			protected GameController game { get { return GameController.Instance; } }
			protected StateMachine stateMachine { get { return StateMachine.Instance; } }
			protected Board board { get { return Board.Instance; } }	

			protected List<Tile> lastOccupiedTiles = new List<Tile>();

			/// <summary>
			/// Constructor.
			/// </summary>
			public NavitigationStateBehaviour(Unit unit)
			{
				this.unit = unit;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public void OnStateEnter()
			{
				unit.outline.Show = unit.isHovering;
				List<Tile> tiles = unit.isHovering ? unit.OccupiedTiles : unit.emptyTiles;

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
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public void OnStateUpdate()
			{
				unit.outline.Show = unit.isHovering;
				List<Tile> tiles = unit.isHovering ? unit.OccupiedTiles : unit.emptyTiles;

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
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public void OnStateExit()
			{
				unit.outline.Show = false;
				foreach (Tile tile in lastOccupiedTiles)
				{
					tile.RemoveDisplay(this, Tile.DisplayType.Position);
				}
				lastOccupiedTiles.Clear();
			}
		}

		// ========================================================= Unit Movement Selection State =========================================================

		class UnitMovementSelectionStateBehaviour : IStateBehaviour
		{
			protected readonly Unit unit = null;
			protected GameController game { get { return GameController.Instance; } }
			protected StateMachine stateMachine { get { return StateMachine.Instance; } }
			protected Board board { get { return Board.Instance; } }

			protected List<Tile> lastOccupiedTiles = new List<Tile>();
			protected List<Tile> lastMovementArea = new List<Tile>();
			protected List<Tile> lastPath = new List<Tile>();
			protected Tile lastPointedTile = null;
			protected bool isStartedPressing = false;

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitMovementSelectionStateBehaviour(Unit unit)
			{
				this.unit = unit;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public void OnStateEnter()
			{
				if ((Object)stateMachine.StateParams[0] == unit)
				{
					unit.outline.Show = true;
					foreach (Tile tile in unit.OccupiedTiles)
					{
						tile.AddDisplay(this, Tile.DisplayType.Position);
					}
					lastOccupiedTiles.AddRange(unit.OccupiedTiles);

					foreach (Tile tile in unit.board.GetTileWithinRange(unit.OccupiedTiles, unit.movement))
					{
						tile.AddDisplay(this, Tile.DisplayType.Move);
					}
					lastMovementArea.AddRange(unit.board.GetTileWithinRange(unit.OccupiedTiles, unit.movement));
				}
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public void OnStateUpdate()
			{
				if ((Object)stateMachine.StateParams[0] == unit)
				{
					Tile hitTile = null;
					if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Camera.main.farClipPlane, LayerMask.GetMask("Tile")))
					{
						Tile tile = hit.collider.GetComponentInParent<Tile>();
						if (lastMovementArea.Contains(tile))
						{
							hitTile = tile;
						}
					}

					if (lastPointedTile != hitTile)
					{
						if (lastPointedTile != null)
						{
							foreach (Tile tile in lastPath)
							{
								tile.HidePath();
							}
							lastPointedTile.RemoveDisplay(this, Tile.DisplayType.MoveTarget);
						}
						if (hitTile != null)
						{
							lastPath.Clear();
							lastPath.AddRange(unit.board.GetShortestPath(unit.OccupiedTiles, hitTile, unit.movement));
							foreach (Tile tile in lastPath)
							{
								tile.ShowPath(lastPath);
							}
							hitTile.AddDisplay(this, Tile.DisplayType.MoveTarget);
						}
						else
						{
							lastPath.Clear();
						}
					}
					lastPointedTile = hitTile;

					if (Input.GetMouseButtonDown(0))
					{
						isStartedPressing = true;
					}
					if (Input.GetMouseButtonUp(0) && isStartedPressing)
					{
						if (lastPointedTile != null)
						{
							stateMachine.ChangeState(State.UnitMovement, unit, new List<Tile>(lastPath));
						}
						else
						{
							stateMachine.ChangeState(State.Navigation);
						}
						isStartedPressing = false;
					}
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public void OnStateExit()
			{
				if ((Object)stateMachine.StateParams[0] == unit)
				{
					foreach (Tile tile in lastOccupiedTiles)
					{
						tile.RemoveDisplay(this, Tile.DisplayType.Position);
					}
					lastOccupiedTiles.Clear();

					foreach (Tile tile in lastMovementArea)
					{
						tile.RemoveDisplay(this, Tile.DisplayType.Move);
					}
					lastMovementArea.Clear();

					foreach (Tile tile in lastPath)
					{
						tile.HidePath();
					}
					lastPath.Clear();

					if (lastPointedTile != null)
					{
						lastPointedTile.RemoveDisplay(this, Tile.DisplayType.MoveTarget);
					}
					lastPointedTile = null;
				}
			}

		}


		// ========================================================= Unit Movement State =========================================================

		class UnitMovementStateBehaviour : IStateBehaviour
		{
			// reference to host
			protected readonly Unit unit = null;

			// references
			protected GameController game { get { return GameController.Instance; } }
			protected StateMachine stateMachine { get { return StateMachine.Instance; } }
			protected Board board { get { return Board.Instance; } }

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitMovementStateBehaviour(Unit unit) 
			{ 
				this.unit = unit; 
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public void OnStateEnter()
			{
				if ((Object)stateMachine.StateParams[0] == unit)
				{
					unit.StartCoroutine(MoveUnitCoroutine((List<Tile>)stateMachine.StateParams[1]));
				}
			}

			protected IEnumerator MoveUnitCoroutine(List<Tile> path)
			{
				List<Tile> startTiles = new List<Tile>(unit.OccupiedTiles);
				startTiles.ForEach(x => x.AddDisplay(this, Tile.DisplayType.Position));
				path.ForEach(x => { x.AddDisplay(this, Tile.DisplayType.Move); x.ShowPath(path); });
				path[path.Count - 1].AddDisplay(this, Tile.DisplayType.MoveTarget);

				List<Vector3> pathPos = path.Select(x => x.transform.position).ToList();

				if (Vector3.Distance(unit.transform.position, pathPos[0]) > 0.01f)
				{
					Vector3 startPosition = unit.transform.position;
					float startTime = Time.time;
					float duration = Vector3.Distance(startPosition, pathPos[0]) / path[0].tileSize * 0.25f;
					while (Time.time - startTime <= duration)
					{
						unit.rigidBody.MovePosition(Vector3.Lerp(startPosition, pathPos[0], (Time.time - startTime) / duration));
						yield return new WaitForFixedUpdate();
					}
				}

				for (int i = 0; i < pathPos.Count - 1; i++)
				{
					float startTime = Time.time;
					while (Time.time - startTime <= 0.25f)
					{
						unit.rigidBody.MovePosition(Vector3.Lerp(pathPos[i], pathPos[i + 1], (Time.time - startTime) / 0.25f));
						yield return new WaitForFixedUpdate();
					}
				}
				unit.rigidBody.MovePosition(pathPos[pathPos.Count - 1]);
				yield return new WaitForFixedUpdate();

				startTiles.ForEach(x => x.RemoveDisplay(this, Tile.DisplayType.Position));
				path.ForEach(x => { x.RemoveDisplay(this, Tile.DisplayType.Move); x.HidePath(); });
				path[path.Count - 1].RemoveDisplay(this, Tile.DisplayType.MoveTarget);
				stateMachine.ChangeState(State.Navigation);
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is ing the current state.
			/// </summary>
			public void OnStateUpdate()
			{
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public void OnStateExit()
			{
			}
		}


		// ========================================================= Apparence =========================================================


	}
}