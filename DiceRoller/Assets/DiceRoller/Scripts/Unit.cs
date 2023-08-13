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
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public void OnStateUpdate()
			{
				// show hovering outline
				unit.outline.Show = unit.isHovering;

				// show occupied tiles on the board
				List<Tile> tiles = unit.isHovering ? unit.OccupiedTiles : Tile.EmptyTiles;
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
				// hide hovering outline
				unit.outline.Show = false;

				// hide occupied tiles on board
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
			protected Tile lastTargetTile = null;
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
				// execute only if the selected unit is this unit
				if ((Object)stateMachine.StateParams[0] == unit)
				{
					// show selection outline
					unit.outline.Show = true;

					// show occupied tiles on board, assume unit wont move during movement selection state
					foreach (Tile tile in unit.OccupiedTiles)
					{
						tile.AddDisplay(this, Tile.DisplayType.Position);
					}
					lastOccupiedTiles.AddRange(unit.OccupiedTiles);

					// show possible movement area on board, assume unit wont move during movement selection state
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
				// execute only if the selected unit is this unit
				if ((Object)stateMachine.StateParams[0] == unit)
				{
					// find the target tile that the mouse is pointing to
					Tile targetTile = null;
					if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Camera.main.farClipPlane, LayerMask.GetMask("Tile")))
					{
						Tile tile = hit.collider.GetComponentInParent<Tile>();
						if (lastMovementArea.Contains(tile))
						{
							targetTile = tile;
						}
					}

					// calculate path towards the target tile
					List<Tile> nextPath = new List<Tile>(lastPath);
					if (lastTargetTile == targetTile)
					{
						if (targetTile == null)
						{
							// no available path found
							nextPath.Clear();
						}
						else
						{ 
							if (unit.OccupiedTiles.Contains(targetTile))
							{
								// restart a path
								nextPath.Clear();
								nextPath.Add(targetTile);
							}
							else
							{
								if (nextPath.Contains(targetTile))
								{
									// trim the path if target tile is already in the current path
									nextPath.RemoveRange(nextPath.IndexOf(targetTile) + 1, nextPath.Count - nextPath.IndexOf(targetTile) - 1);
								}
								else
								{
									if (nextPath.Count == 0)
									{
										// no path exist, find the shortest path to target tile
										nextPath = unit.board.GetShortestPath(unit.OccupiedTiles, targetTile, unit.movement);
									}
									else
									{
										List<Tile> appendPath = unit.board.GetShortestPath(nextPath[nextPath.Count - 1], targetTile, unit.movement + 1 - nextPath.Count);
										if (appendPath != null)
										{
											// append a path from last tile of the path to the target tile
											appendPath.RemoveAt(0);
											nextPath.AddRange(appendPath);
										}
										else
										{
											// path is too long, retrieve a shortest path to target tile instead
											nextPath = unit.board.GetShortestPath(unit.OccupiedTiles, targetTile, unit.movement);
										}
									}
								}
							}
						}
					}

					// show target tile on the board
					if (lastTargetTile != targetTile)
					{
						if (lastTargetTile != null)
						{
							lastTargetTile.RemoveDisplay(this, Tile.DisplayType.MoveTarget);
						}
						if (targetTile != null)
						{
							targetTile.AddDisplay(this, Tile.DisplayType.MoveTarget);
						}
					}
					lastTargetTile = targetTile;

					// show path to target tile on the board
					foreach (Tile tile in lastPath)
					{
						tile.HidePath();
					}
					foreach (Tile tile in nextPath)
					{
						tile.ShowPath(nextPath);
					}
					lastPath.Clear();
					lastPath.AddRange(nextPath);

					// detect mouse pressing
					if (Input.GetMouseButtonDown(0))
					{
						isStartedPressing = true;
					}
					if (Input.GetMouseButtonUp(0) && isStartedPressing)
					{
						if (lastTargetTile != null)
						{
							// pressed on a valid tile, initiate movement
							stateMachine.ChangeState(State.UnitMovement, unit, new List<Tile>(unit.OccupiedTiles), new List<Tile>(lastPath));
						}
						else
						{
							// return to navigation otherwise
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
				// execute only if the selected unit is this unit
				if ((Object)stateMachine.StateParams[0] == unit)
				{
					// hide hovering outline
					unit.outline.Show = false;

					// hide occupied tiles on board
					foreach (Tile tile in lastOccupiedTiles)
					{
						tile.RemoveDisplay(this, Tile.DisplayType.Position);
					}
					lastOccupiedTiles.Clear();

					// hide possible movement area on board
					foreach (Tile tile in lastMovementArea)
					{
						tile.RemoveDisplay(this, Tile.DisplayType.Move);
					}
					lastMovementArea.Clear();

					// hdie target tile on board
					if (lastTargetTile != null)
					{
						lastTargetTile.RemoveDisplay(this, Tile.DisplayType.MoveTarget);
					}
					lastTargetTile = null;

					// hide path to target tile on board
					foreach (Tile tile in lastPath)
					{
						tile.HidePath();
					}
					lastPath.Clear();
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
				// execute only if the moving unit is this unit
				if ((Object)stateMachine.StateParams[0] == unit)
				{
					unit.StartCoroutine(MoveUnitCoroutine((List<Tile>)stateMachine.StateParams[1], (List<Tile>)stateMachine.StateParams[2]));
				}
			}

			/// <summary>
			/// Movement coroutine for this unit.
			/// </summary>
			protected IEnumerator MoveUnitCoroutine(List<Tile> startTiles, List<Tile> path)
			{
				// show all displays on grid
				startTiles.ForEach(x => x.AddDisplay(this, Tile.DisplayType.Position));
				path.ForEach(x => { x.AddDisplay(this, Tile.DisplayType.Move); x.ShowPath(path); });
				path[path.Count - 1].AddDisplay(this, Tile.DisplayType.MoveTarget);

				// move to first tile on path if needed
				if (Vector3.Distance(unit.transform.position, path[0].transform.position) > 0.01f)
				{
					Vector3 startPosition = unit.transform.position;
					float startTime = Time.time;
					float duration = Vector3.Distance(startPosition, path[0].transform.position) / path[0].tileSize * 0.25f;
					while (Time.time - startTime <= duration)
					{
						unit.rigidBody.MovePosition(Vector3.Lerp(startPosition, path[0].transform.position, (Time.time - startTime) / duration));
						yield return new WaitForFixedUpdate();
					}
				}

				// move to next tile on path one by one
				for (int i = 0; i < path.Count - 1; i++)
				{
					float startTime = Time.time;
					while (Time.time - startTime <= 0.25f)
					{
						unit.rigidBody.MovePosition(Vector3.Lerp(path[i].transform.position, path[i + 1].transform.position, (Time.time - startTime) / 0.25f));
						yield return new WaitForFixedUpdate();
					}
				}

				// file tile on path reached, final movement to destination tile 
				unit.rigidBody.MovePosition(path[path.Count - 1].transform.position);
				yield return new WaitForFixedUpdate();

				// hide all displays on grid
				startTiles.ForEach(x => x.RemoveDisplay(this, Tile.DisplayType.Position));
				path.ForEach(x => { x.RemoveDisplay(this, Tile.DisplayType.Move); x.HidePath(); });
				path[path.Count - 1].RemoveDisplay(this, Tile.DisplayType.MoveTarget);

				// change state back to navigation
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