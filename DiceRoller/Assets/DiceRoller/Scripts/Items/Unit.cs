using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuickerEffects;
using UnityEngine;

namespace DiceRoller
{
	public class Unit : Item
	{
		public static List<Unit> AllUnits { get; protected set; } = new List<Unit>();
		public static UniqueList<Unit> InspectingUnit { get; protected set; } = new UniqueList<Unit>();

		public int CurrentHealth { get; protected set; }

		// parameters
		public int maxHealth = 20;
		public int melee = 3;
		public int defence = 4;
		public int magic = 1;
		public int movement = 4;
		public float moveTimePerTile = 0.2f;

		// components
		protected Outline outline = null;
		protected Overlay overlay = null;

		// working variables
		protected bool actionDepleted = false;

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

			CurrentHealth = maxHealth;
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected override void Update()
		{
			base.Update();
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
		}

		// ========================================================= Team Behaviour =========================================================

		/// <summary>
		/// Register this unit to a team.
		/// </summary>
		protected void RegisterToTeam()
		{
			AllUnits.Add(this);
		}

		/// <summary>
		///  Deregister this unit from a team.
		/// </summary>
		protected void DeregisterFromTeam()
		{
			AllUnits.Remove(this);
		}

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.RegisterStateBehaviour(this, State.StartTurn, new StartTurnSB(this));
			stateMachine.RegisterStateBehaviour(this, State.Navigation, new NavigationSB(this));
			stateMachine.RegisterStateBehaviour(this, State.UnitMoveSelect, new UnitMoveSelectSB(this));
			stateMachine.RegisterStateBehaviour(this, State.UnitMovement, new UnitMoveSB(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		protected void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
				stateMachine.DeregisterStateBehaviour(this);
		}


		// ========================================================= Start Turn State =========================================================

		protected class StartTurnSB : StateBehaviour
		{
			protected readonly Unit unit = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public StartTurnSB(Unit unit)
			{
				this.unit = unit;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				unit.actionDepleted = false;
				unit.overlay.Show = false;
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
			}
		}

		// ========================================================= Navigation State =========================================================

		protected class NavigationSB : StateBehaviour
		{
			protected readonly Unit unit = null;

			protected bool lastIsHovering = false;
			protected List<Tile> lastOccupiedTiles = new List<Tile>();

			/// <summary>
			/// Constructor.
			/// </summary>
			public NavigationSB(Unit unit)
			{
				this.unit = unit;
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

				// show unit info on ui
				if (unit.isHovering != lastIsHovering)
				{
					if (unit.isHovering)
					{
						InspectingUnit.Add(unit);
					}
					else
					{
						InspectingUnit.Remove(unit);
					}
				}
				lastIsHovering = unit.isHovering;

				// go to unit movement selection state when this unit is pressed
				if (unit.team == stateMachine.Params.team && unit.isPressed && !unit.actionDepleted && unit.OccupiedTiles.Count > 0)
				{
					stateMachine.ChangeState(State.UnitMoveSelect,
						new StateParams()
						{
							team = stateMachine.Params.team,
							unit = unit
						});
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// hide hovering outline
				unit.outline.Show = false;

				// hide occupied tiles on board
				foreach (Tile tile in lastOccupiedTiles)
				{
					tile.RemoveDisplay(this, Tile.DisplayType.Position);
				}
				lastOccupiedTiles.Clear();

				// hide unit info on ui
				InspectingUnit.Remove(unit);

				// clear flags
				lastIsHovering = false;
			}
		}

		// ========================================================= Unit Movement Selection State =========================================================

		class UnitMoveSelectSB : StateBehaviour
		{
			protected readonly Unit unit = null;

			protected List<Tile> lastOccupiedTiles = new List<Tile>();
			protected List<Tile> otherOccupiedTiles = new List<Tile>();
			protected List<Tile> lastMovementArea = new List<Tile>();
			protected List<Tile> lastPath = new List<Tile>();
			protected Tile lastTargetTile = null;
			protected bool isPressing0 = false;
			protected bool isPressing1 = false;

			protected List<Tile> nextPath = new List<Tile>();
			protected List<Tile> appendPath = new List<Tile>();
			protected List<Tile> appendExcludedTiles = new List<Tile>();

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitMoveSelectSB(Unit unit)
			{
				this.unit = unit;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				// execute only if the selected unit is this unit
				if (stateMachine.Params.unit == unit)
				{
					// show selection outline
					unit.outline.Show = true;

					// show occupied tiles on board, assume unit wont move during movement selection state
					lastOccupiedTiles.AddRange(unit.OccupiedTiles);
					foreach (Tile tile in lastOccupiedTiles)
					{
						tile.AddDisplay(this, Tile.DisplayType.Position);
					}

					// find all tiles that are occupied by other units
					otherOccupiedTiles.Clear();
					AllUnits.ForEach(x => {
						if (x != unit)
						{
							otherOccupiedTiles.AddRange(x.OccupiedTiles.Except(otherOccupiedTiles));
						}
					});

					// show possible movement area on board, assume unit wont move during movement selection state
					board.GetConnectedTilesInRange(unit.OccupiedTiles, otherOccupiedTiles, unit.movement, lastMovementArea);
					foreach (Tile tile in lastMovementArea)
					{
						tile.AddDisplay(this, Tile.DisplayType.Move);
					}

					// shwo unit info on ui  
					InspectingUnit.Add(unit);
				}
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				// execute only if the selected unit is this unit
				if (stateMachine.Params.unit == unit)
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
					Debug.Log(targetTile == null ? "NULL" : targetTile.boardPos.ToString());

					// calculate path towards the target tile
					if (lastTargetTile != targetTile)
					{
						nextPath.Clear();
						nextPath.AddRange(lastPath);
						if (targetTile == null)
						{
							Debug.Log("HERE1");
							// target tile is unreachable, no path is retrieved
							nextPath.Clear();
						}
						else if (unit.OccupiedTiles.Contains(targetTile))
						{
							Debug.Log("HERE2");
							// target tile is withing the starting tiles, reset the path to the target tile
							nextPath.Clear();
							nextPath.Add(targetTile);
						}
						else if (nextPath.Count == 0)
						{
							Debug.Log("HERE3");
							// no path exist, find the shortest path to target tile
							nextPath.Clear();
							unit.board.GetShortestPath(unit.OccupiedTiles, otherOccupiedTiles, targetTile, unit.movement, in nextPath);
						}
						else if (nextPath.Contains(targetTile))
						{
							Debug.Log("HERE4");
							// trim the path if target tile is already in the current path
							nextPath.RemoveRange(nextPath.IndexOf(targetTile) + 1, nextPath.Count - nextPath.IndexOf(targetTile) - 1);
						}
						else
						{
							// target tile is valid and not on current path, attempt to append a path from the end of current path to the target tile
							appendPath.Clear();
							appendExcludedTiles.Clear();
							appendExcludedTiles.AddRange(nextPath);
							appendExcludedTiles.AddRange(otherOccupiedTiles);
							appendExcludedTiles.Remove(nextPath[nextPath.Count - 1]);
							unit.board.GetShortestPath(nextPath[nextPath.Count - 1], appendExcludedTiles, targetTile, unit.movement + 1 - nextPath.Count, in appendPath);
							if (appendPath.Count > 0)
							{
								Debug.Log("HERE5");
								// append a path from last tile of the path to the target tile
								appendPath.RemoveAt(0);
								nextPath.AddRange(appendPath);
								appendPath.Clear();
							}
							else
							{
								Debug.Log("HERE6");
								// path is too long, retrieve a shortest path to target tile instead
								nextPath.Clear();
								unit.board.GetShortestPath(unit.OccupiedTiles, otherOccupiedTiles, targetTile, unit.movement, in nextPath);
							}
						}

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
						nextPath.Clear();
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

					// detect path selection by left mouse pressing
					if (Input.GetMouseButtonDown(0))
					{
						isPressing0 = true;
					}
					if (Input.GetMouseButtonUp(0) && isPressing0)
					{
						if (lastTargetTile != null)
						{
							// pressed on a valid tile, initiate movement
							stateMachine.ChangeState(State.UnitMovement,
								new StateParams()
								{
									team = stateMachine.Params.team,
									unit = unit,
									startingTiles = new List<Tile>(unit.OccupiedTiles),
									path = new List<Tile>(lastPath)
								});
						}
						else
						{
							// return to navigation otherwise
							stateMachine.ChangeState(State.Navigation,
								new StateParams()
								{
									team = stateMachine.Params.team
								});
						}
						isPressing0 = false;
					}

					// detect return to navitation by right mose pressing
					if (Input.GetMouseButtonDown(1))
					{
						isPressing1 = true;
					}
					if (Input.GetMouseButtonUp(1) && isPressing1)
					{
						// return to navigation otherwise
						stateMachine.ChangeState(State.Navigation,
							new StateParams()
							{
								team = stateMachine.Params.team
							});
						isPressing1 = false;
					}
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// execute only if the selected unit is this unit
				if (stateMachine.Params.unit == unit)
				{
					// hide hovering outline
					unit.outline.Show = false;

					// hide occupied tiles on board
					foreach (Tile tile in lastOccupiedTiles)
					{
						tile.RemoveDisplay(this, Tile.DisplayType.Position);
					}
					lastOccupiedTiles.Clear();

					// clear other occulied tiles
					otherOccupiedTiles.Clear();

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

					// hide unit info on ui
					InspectingUnit.Remove(unit);

					// clear flags
					isPressing0 = false;
					isPressing1 = false;
				}
			}

		}

		// ========================================================= Unit Movement State =========================================================

		class UnitMoveSB : StateBehaviour
		{
			protected readonly Unit unit = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitMoveSB(Unit unit) 
			{ 
				this.unit = unit; 
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				// execute only if the moving unit is this unit
				if (stateMachine.Params.unit == unit)
				{
					unit.StartCoroutine(MoveSequence(stateMachine.Params.startingTiles, stateMachine.Params.path));
				}
			}

			/// <summary>
			/// Movement coroutine for this unit.
			/// </summary>
			protected IEnumerator MoveSequence(List<Tile> startTiles, List<Tile> path)
			{
				// show all displays on grid
				startTiles.ForEach(x => x.AddDisplay(this, Tile.DisplayType.Position));
				path.ForEach(x => { x.AddDisplay(this, Tile.DisplayType.Move); x.ShowPath(path); });
				path[path.Count - 1].AddDisplay(this, Tile.DisplayType.MoveTarget);

				// show unit info on ui
				InspectingUnit.Add(unit);

				float startTime = 0;
				float duration = 0;

				// move to first tile on path if needed
				if (Vector3.Distance(unit.transform.position, path[0].transform.position) > 0.01f)
				{
					Vector3 startPosition = unit.transform.position;
					startTime = Time.time;
					duration = Vector3.Distance(startPosition, path[0].transform.position) / path[0].tileSize * unit.moveTimePerTile;
					while (Time.time - startTime <= duration)
					{
						unit.rigidBody.MovePosition(Vector3.Lerp(startPosition, path[0].transform.position, (Time.time - startTime) / duration));
						yield return new WaitForFixedUpdate();
					}
				}

				// move to next tile on path one by one
				for (int i = 0; i < path.Count - 1; i++)
				{
					startTime = Time.time;
					duration = unit.moveTimePerTile;
					while (Time.time - startTime <= unit.moveTimePerTile)
					{
						unit.rigidBody.MovePosition(Vector3.Lerp(path[i].transform.position, path[i + 1].transform.position, (Time.time - startTime) / duration));
						yield return new WaitForFixedUpdate();
					}
				}

				// file tile on path reached, final movement to destination tile 
				startTime = Time.time;
				duration = unit.moveTimePerTile;
				while (Time.time - startTime <= unit.moveTimePerTile)
				{
					unit.rigidBody.MovePosition(path[path.Count - 1].transform.position);
					yield return new WaitForFixedUpdate();
				}

				// hide all displays on grid
				startTiles.ForEach(x => x.RemoveDisplay(this, Tile.DisplayType.Position));
				path.ForEach(x => { x.RemoveDisplay(this, Tile.DisplayType.Move); x.HidePath(); });
				path[path.Count - 1].RemoveDisplay(this, Tile.DisplayType.MoveTarget);

				// hide unit info on ui
				InspectingUnit.Remove(unit);

				// set flag
				unit.actionDepleted = true;
				unit.overlay.Show = true;

				// change state back to navigation
				stateMachine.ChangeState(State.Navigation,
					new StateParams()
					{
						team = stateMachine.Params.team
					});
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is ing the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
			}
		}
	}
}