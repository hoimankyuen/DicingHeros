using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuickerEffects;
using UnityEngine;

namespace DiceRoller
{
	public class Unit : Item
	{
		public static UniqueList<Unit> InspectingUnits { get; protected set; } = new UniqueList<Unit>();
		public bool IsInspecting
		{
			get
			{
				return InspectingUnits.Contains(this);
			}
		}

		public static UniqueList<Unit> SelectedUnits { get; protected set; } = new UniqueList<Unit>();
		public bool IsSelected
		{
			get
			{
				return SelectedUnits.Contains(this);
			}
		}

		public int Health { get; protected set; }
		public int Melee { get; protected set; }
		public int Defence { get; protected set; }
		public int Magic { get; protected set; }
		public int Movement { get; protected set; }

		// parameters
		[Header("Unit Parameters")]
		public int maxHealth = 20;
		public int baseMelee = 3;
		public int baseDefence = 4;
		public int baseMagic = 1;
		public int baseMovement = 4;

		public float moveTimePerTile = 0.2f;

		// properties
		public bool ActionDepleted { get; protected set; } = false;

		// events
		public Action onHealthChanged = () => { };
		public Action onStatChanged = () => { };


		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected override void Start()
		{
			base.Start();
			RegisterStateBehaviours();
			RegisterToPlayer();

			SetHealth(maxHealth);
			ResetStat();
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
			DeregisterFromPlayer();
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
		/// Add or remove health by a set amount.
		/// </summary>
		public void ChangeHealth(int delta)
		{
			SetHealth(Health + delta);
		}

		/// <summary>
		/// Directly set health to a specific value.
		/// </summary>
		public void SetHealth(int value)
		{
			if (Health != value)
			{
				Health = value;
				onHealthChanged.Invoke();
			}
		}

		/// <summary>
		/// Add or remove value of each stat by a set amount.
		/// </summary>
		public void ChangeStat(int meleeDelta = 0, int defenceDelta = 0, int magicDelta = 0, int movementDelta = 0)
		{
			SetStat(Melee + meleeDelta, Defence + defenceDelta, Magic + magicDelta, Movement + movementDelta);
		}

		/// <summary>
		/// Reset set to their original value.
		/// </summary>
		public void ResetStat()
		{
			SetStat(baseMelee, baseDefence, baseMagic, baseMovement);
		}

		/// <summary>
		/// Directly set each stat to a specific value.
		/// </summary>
		public void SetStat(int meleeValue, int defencevalue, int magicValue, int movementValue)
		{
			bool changed = false;
			if (Melee != meleeValue)
			{
				Melee = meleeValue;
				changed = true;
			}
			if (Defence != defencevalue)
			{
				Defence = defencevalue;
				changed = true;
			}
			if (Magic != magicValue)
			{
				Magic = magicValue;
				changed = true;
			}
			if (Movement != movementValue)
			{
				Movement = movementValue;
				changed = true;
			}
			if (changed)
			{
				onStatChanged.Invoke();
			}
		}

		// ========================================================= Team Behaviour =========================================================

		/// <summary>
		/// Register this unit to a player.
		/// </summary>
		protected void RegisterToPlayer()
		{
			if (game == null)
				return;

			Player p = game.GetPlayerById(playerId);
			if (p != null)
			{
				p.units.Add(this);
			}
		}

		/// <summary>
		///  Deregister this unit from a player.
		/// </summary>
		protected void DeregisterFromPlayer()
		{
			if (game == null)
				return;

			Player p = game.GetPlayerById(playerId);
			if (p != null)
			{
				p.units.Remove(this);
			}
		}

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.Register(this, State.StartTurn, new StartTurnSB(this));
			stateMachine.Register(this, State.Navigation, new NavigationSB(this));
			stateMachine.Register(this, State.UnitActionSelect, new UnitActionSelectSB(this));
			stateMachine.Register(this, State.UnitMove, new UnitMoveSB(this));
			stateMachine.Register(this, State.DiceActionSelect, new EffectOnlySB(this));
			stateMachine.Register(this, State.DiceThrow, new EffectOnlySB(this));
			stateMachine.Register(this, State.EndTurn, new EndTurnSB(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		protected void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
				stateMachine.DeregisterAll(this);
		}


		// ========================================================= Start Turn State =========================================================

		protected class StartTurnSB : StateBehaviour
		{
			protected readonly Unit self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public StartTurnSB(Unit self)
			{
				this.self = self;
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
			protected readonly Unit self = null;

			protected bool lastIsHovering = false;
			protected List<Tile> lastOccupiedTiles = new List<Tile>();

			/// <summary>
			/// Constructor.
			/// </summary>
			public NavigationSB(Unit self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				// show action depleted effect
				if (self.ActionDepleted)
				{
					self.AddEffect(StatusType.Depleted);
				}
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				// show occupied tiles on the board
				List<Tile> tiles = self.isHovering ? self.OccupiedTiles : Tile.EmptyTiles;
				foreach (Tile tile in tiles.Except(lastOccupiedTiles))
				{
					tile.AddDisplay(self, self.playerId == stateMachine.Params.player.id ? Tile.DisplayType.SelfPosition : Tile.DisplayType.EnemyPosition);
				}
				foreach (Tile tile in lastOccupiedTiles.Except(tiles))
				{
					tile.RemoveDisplay(self, self.playerId == stateMachine.Params.player.id ? Tile.DisplayType.SelfPosition : Tile.DisplayType.EnemyPosition);
				}
				lastOccupiedTiles.Clear();
				lastOccupiedTiles.AddRange(tiles);

				// show unit info on ui
				if (self.isHovering != lastIsHovering)
				{
					if (self.isHovering)
					{
						InspectingUnits.Add(self);
						self.AddEffect(self.playerId == stateMachine.Params.player.id ? StatusType.InspectingSelf : StatusType.InspectingEnemy);
					}
					else
					{
						InspectingUnits.Remove(self);
						self.RemoveEffect(self.playerId == stateMachine.Params.player.id ? StatusType.InspectingSelf : StatusType.InspectingEnemy);
					}
				}
				lastIsHovering = self.isHovering;

				// go to unit movement selection state when this unit is pressed
				if (self.playerId == stateMachine.Params.player.id && self.isPressed && !self.ActionDepleted && self.OccupiedTiles.Count > 0)
				{
					stateMachine.ChangeState(State.UnitActionSelect,
						new StateParams()
						{
							player = stateMachine.Params.player,
							unit = self
						});
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// hide action depleted effect
				if (self.ActionDepleted)
				{
					self.RemoveEffect(StatusType.Depleted);
				}

				// hide occupied tiles on board
				foreach (Tile tile in lastOccupiedTiles)
				{
					tile.RemoveDisplay(self, self.playerId == stateMachine.Params.player.id ? Tile.DisplayType.SelfPosition : Tile.DisplayType.EnemyPosition);
				}
				lastOccupiedTiles.Clear();

				// hide unit info on ui
				if (self.isHovering)
				{
					InspectingUnits.Remove(self);
					self.RemoveEffect(self.playerId == stateMachine.Params.player.id ? StatusType.InspectingSelf : StatusType.InspectingEnemy);
				}
				lastIsHovering = false;
			}
		}

		// ========================================================= Unit Action Selection State =========================================================

		class UnitActionSelectSB : StateBehaviour
		{
			protected readonly Unit self = null;

			protected List<Tile> lastOccupiedTiles = new List<Tile>();
			protected List<Tile> otherOccupiedTiles = new List<Tile>();
			protected List<Tile> lastMovementArea = new List<Tile>();
			protected List<Tile> lastPath = new List<Tile>();
			protected Tile lastTargetTile = null;
			protected bool lastReachable = true;
			protected Vector2 pressedPosition0 = Vector2.negativeInfinity;
			protected Vector2 pressedPosition1 = Vector2.negativeInfinity;

			protected List<Tile> nextPath = new List<Tile>();
			protected List<Tile> appendPath = new List<Tile>();
			protected List<Tile> appendExcludedTiles = new List<Tile>();

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitActionSelectSB(Unit self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				// execute only if the selected unit is this unit
				if (stateMachine.Params.unit == self)
				{
					// show selection effect
					self.AddEffect(StatusType.SelectedSelf);

					// show occupied tiles on board, assume unit wont move during movement selection state
					lastOccupiedTiles.AddRange(self.OccupiedTiles);
					foreach (Tile tile in lastOccupiedTiles)
					{
						tile.AddDisplay(self, Tile.DisplayType.SelfPosition);
					}

					// find all tiles that are occupied by other units
					otherOccupiedTiles.Clear();
					foreach (Player player in game.GetAllPlayers())
					{
						foreach (Unit unit in player.units)
						{
							if (unit != self)
							{
								otherOccupiedTiles.AddRange(unit.OccupiedTiles.Except(otherOccupiedTiles));
							}
						}
					}

					// show possible movement area on board, assume unit wont move during movement selection state
					board.GetConnectedTilesInRange(self.OccupiedTiles, otherOccupiedTiles, self.baseMovement, lastMovementArea);
					foreach (Tile tile in lastMovementArea)
					{
						tile.AddDisplay(self, Tile.DisplayType.Move);
					}

					// shwo unit info on ui  
					InspectingUnits.Add(self);
				}
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				// execute only if the selected unit is this unit
				if (stateMachine.Params.unit == self)
				{
					// find the target tile that the mouse is pointing to
					Tile targetTile = null;
					bool reachable = false;
					if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Camera.main.farClipPlane, LayerMask.GetMask("Tile")))
					{
						Tile tile = hit.collider.GetComponentInParent<Tile>();
						targetTile = tile;
						if (lastMovementArea.Contains(tile))
						{
							reachable = true;
						}
					}

					// calculate path towards the target tile
					if (lastTargetTile != targetTile)
					{
						nextPath.Clear();
						nextPath.AddRange(lastPath);
						if (targetTile == null || !reachable)
						{
							// target tile is unreachable, no path is retrieved
							nextPath.Clear();
						}
						else if (self.OccupiedTiles.Contains(targetTile))
						{
							// target tile is withing the starting tiles, reset the path to the target tile
							nextPath.Clear();
							nextPath.Add(targetTile);
						}
						else if (nextPath.Count == 0)
						{
							// no path exist, find the shortest path to target tile
							nextPath.Clear();
							self.board.GetShortestPath(self.OccupiedTiles, otherOccupiedTiles, targetTile, self.baseMovement, in nextPath);
						}
						else if (nextPath.Contains(targetTile))
						{
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
							self.board.GetShortestPath(nextPath[nextPath.Count - 1], appendExcludedTiles, targetTile, self.baseMovement + 1 - nextPath.Count, in appendPath);
							if (appendPath.Count > 0)
							{
								// append a path from last tile of the path to the target tile
								appendPath.RemoveAt(0);
								nextPath.AddRange(appendPath);
								appendPath.Clear();
							}
							else
							{
								// path is too long, retrieve a shortest path to target tile instead
								nextPath.Clear();
								self.board.GetShortestPath(self.OccupiedTiles, otherOccupiedTiles, targetTile, self.baseMovement, in nextPath);
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

						// show invalid path to target tile on the board
						if (lastTargetTile != null && !lastReachable)
						{
							lastTargetTile.HideInvalidPath();
						}
						if (targetTile != null && !reachable)
						{
							targetTile.ShowInvalidPath();
						}

						// show target tile on the board
						if (lastTargetTile != null && lastReachable)
						{
							lastTargetTile.RemoveDisplay(self, Tile.DisplayType.MoveTarget);
						}
						if (targetTile != null && reachable)
						{
							targetTile.AddDisplay(self, Tile.DisplayType.MoveTarget);
						}

						lastPath.Clear();
						lastPath.AddRange(nextPath);
						nextPath.Clear();
						lastTargetTile = targetTile;
						lastReachable = reachable;
					}

					// detect path selection by left mouse pressing
					if (InputUtils.GetMousePress(0, ref pressedPosition0))
					{
						if (reachable)
						{
							// pressed on a valid tile, initiate movement
							stateMachine.ChangeState(State.UnitMove,
								new StateParams()
								{
									player = stateMachine.Params.player,
									unit = self,
									startingTiles = new List<Tile>(self.OccupiedTiles),
									path = new List<Tile>(lastPath)
								});
						}
						else
						{
							// return to navigation otherwise
							stateMachine.ChangeState(State.Navigation,
								new StateParams()
								{
									player = stateMachine.Params.player
								});
						}
					}

					// detect return to navitation by right mouse pressing
					if (InputUtils.GetMousePress(1, ref pressedPosition1))
					{	
						// return to navigation otherwise
						stateMachine.ChangeState(State.Navigation,
						new StateParams()
						{
							player = stateMachine.Params.player
						});
					}
					pressedPosition1 = Vector2.negativeInfinity;
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// execute only if the selected unit is this unit
				if (stateMachine.Params.unit == self)
				{
					// hide selection effect
					self.RemoveEffect(StatusType.SelectedSelf);

					// hide occupied tiles on board
					foreach (Tile tile in lastOccupiedTiles)
					{
						tile.RemoveDisplay(self, Tile.DisplayType.SelfPosition);
					}
					lastOccupiedTiles.Clear();

					// clear other occulied tiles
					otherOccupiedTiles.Clear();

					// hide possible movement area on board
					foreach (Tile tile in lastMovementArea)
					{
						tile.RemoveDisplay(self, Tile.DisplayType.Move);
					}
					lastMovementArea.Clear();

					// hide path to target tile on board
					foreach (Tile tile in lastPath)
					{
						tile.HidePath();
					}
					lastPath.Clear();

					// hdie target tile on board
					if (lastTargetTile != null)
					{
						if (lastReachable)
						{
							lastTargetTile.RemoveDisplay(this, Tile.DisplayType.MoveTarget);
						}
						else
						{
							lastTargetTile.HideInvalidPath();
						}
					}
					lastTargetTile = null;
					lastReachable = true;

					// hide unit info on ui
					InspectingUnits.Remove(self);

					// clear flags
					InputUtils.ResetPressCache(ref pressedPosition0);
					InputUtils.ResetPressCache(ref pressedPosition1);
				}
			}

		}

		// ========================================================= Unit Movement State =========================================================

		class UnitMoveSB : StateBehaviour
		{
			protected readonly Unit self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitMoveSB(Unit self) 
			{ 
				this.self = self; 
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				// execute only if the moving unit is this unit
				if (stateMachine.Params.unit == self)
				{
					self.StartCoroutine(MoveSequence(stateMachine.Params.startingTiles, stateMachine.Params.path));
				}
			}

			/// <summary>
			/// Movement coroutine for this unit.
			/// </summary>
			protected IEnumerator MoveSequence(List<Tile> startTiles, List<Tile> path)
			{
				// show all displays on grid
				startTiles.ForEach(x => x.AddDisplay(self, Tile.DisplayType.SelfPosition));
				path.ForEach(x => { x.AddDisplay(self, Tile.DisplayType.Move); x.ShowPath(path); });
				path[path.Count - 1].AddDisplay(self, Tile.DisplayType.MoveTarget);

				// show unit info on ui
				InspectingUnits.Add(self);

				float startTime = 0;
				float duration = 0;

				// move to first tile on path if needed
				if (Vector3.Distance(self.transform.position, path[0].transform.position) > 0.01f)
				{
					Vector3 startPosition = self.transform.position;
					startTime = Time.time;
					duration = Vector3.Distance(startPosition, path[0].transform.position) / path[0].tileSize * self.moveTimePerTile;
					while (Time.time - startTime <= duration)
					{
						self.rigidBody.MovePosition(Vector3.Lerp(startPosition, path[0].transform.position, (Time.time - startTime) / duration));
						yield return new WaitForFixedUpdate();
					}
				}

				// move to next tile on path one by one
				for (int i = 0; i < path.Count - 1; i++)
				{
					startTime = Time.time;
					duration = self.moveTimePerTile;
					while (Time.time - startTime <= self.moveTimePerTile)
					{
						self.rigidBody.MovePosition(Vector3.Lerp(path[i].transform.position, path[i + 1].transform.position, (Time.time - startTime) / duration));
						yield return new WaitForFixedUpdate();
					}
				}

				// file tile on path reached, final movement to destination tile 
				startTime = Time.time;
				duration = self.moveTimePerTile;
				while (Time.time - startTime <= self.moveTimePerTile)
				{
					self.rigidBody.MovePosition(path[path.Count - 1].transform.position);
					yield return new WaitForFixedUpdate();
				}

				// hide all displays on grid
				startTiles.ForEach(x => x.RemoveDisplay(self, Tile.DisplayType.SelfPosition));
				path.ForEach(x => { x.RemoveDisplay(self, Tile.DisplayType.Move); x.HidePath(); });
				path[path.Count - 1].RemoveDisplay(self, Tile.DisplayType.MoveTarget);

				// hide unit info on ui
				InspectingUnits.Remove(self);

				// set flag
				self.ActionDepleted = true;

				// change state back to navigation
				stateMachine.ChangeState(State.Navigation,
					new StateParams()
					{
						player = stateMachine.Params.player
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

		// ========================================================= Effect Only State =========================================================

		protected class EffectOnlySB : StateBehaviour
		{
			protected readonly Unit self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public EffectOnlySB(Unit self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				if (self.ActionDepleted)
				{
					self.AddEffect(StatusType.Depleted);
				}
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
				if (self.ActionDepleted)
				{
					self.RemoveEffect(StatusType.Depleted);
				}
			}
		}

		// ========================================================= End Turn State =========================================================

		protected class EndTurnSB : StateBehaviour
		{
			protected readonly Unit self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public EndTurnSB(Unit self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				if (self.ActionDepleted)
				{
					self.ActionDepleted = false;
					self.RemoveEffect(StatusType.Depleted);
				}
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
	}
}

