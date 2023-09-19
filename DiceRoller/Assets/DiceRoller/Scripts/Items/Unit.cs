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
		// parameters
		[Header("Unit Parameters")]
		public int maxHealth = 20;
		public int baseMelee = 3;
		public int baseDefence = 4;
		public int baseMagic = 1;
		public int baseMovement = 4;

		public float moveTimePerTile = 0.2f;

		// working variables

		// events
		public Action onInspectionChanged = () => { };
		public Action onSelectionChanged = () => { };
		public Action onHealthChanged = () => { };
		public Action onStatChanged = () => { };

		// ========================================================= Properties =========================================================

		/// <summary>
		/// Flag for if this unit is currently being inspected.
		/// </summary>
		public bool IsBeingInspected
		{
			get
			{
				return inspectingUnits.Contains(this);
			}
		}
		private static UniqueList<Unit> inspectingUnits = new UniqueList<Unit>();

		/// <summary>
		/// Flag for if this unit is currently selected.
		/// </summary>
		public bool IsSelected
		{
			get
			{
				return selectedUnits.Contains(this);
			}
		}
		private static UniqueList<Unit> selectedUnits = new UniqueList<Unit>();

		/// <summary>
		/// Flag for if this unit has deplete its actions.
		/// </summary>
		public bool ActionDepleted { get; private set; } = false;

		/// <summary>
		/// The current health value of this unit.
		/// </summary>
		public int Health
		{
			get
			{
				return _health;
			}
			private set
			{
				if (_health != value)
				{
					_health = value;
					onHealthChanged.Invoke();
				}
			}
		}
		private int _health = 0;

		/// <summary>
		/// The current melee value of this unit.
		/// </summary>
		public int Melee 
		{
			get
			{
				return _melee;
			}
			private set
			{
				if (_melee != value)
				{
					_melee = value;
					onStatChanged.Invoke();
				}
			}
		}
		private int _melee = 0;

		/// <summary>
		/// The current defence value of this unit.
		/// </summary>
		public int Defence
		{
			get
			{
				return _defence;
			}
			private set
			{
				if (_defence != value)
				{
					_defence = value;
					onStatChanged.Invoke();
				}
			}
		}
		private int _defence = 0;

		/// <summary>
		/// The current magic value of this unit.
		/// </summary>
		public int Magic
		{
			get
			{
				return _magic;
			}
			private set
			{
				if (_magic != value)
				{
					_magic = value;
					onStatChanged.Invoke();
				}
			}
		}
		private int _magic = 0;

		/// <summary>
		/// The current movement value of this unit.
		/// </summary>
		public int Movement
		{
			get
			{
				return _movement;
			}
			private set
			{
				if (_movement != value)
				{
					_movement = value;
					onStatChanged.Invoke();
				}
			}
		}
		private int _movement = 0;


		/// <summary>
		/// The starting tiles of a selected movement path.
		/// </summary>
		public List<Tile> MovementStartingTiles { get; private set; } = new List<Tile>();

		/// <summary>
		/// Tiles in a selected movment path from start to end.
		/// </summary>
		public List<Tile> MovementSelectedPath { get; private set; } = new List<Tile>();

		// ========================================================= Inspection and Selection =========================================================

		/// <summary>
		/// Retrieve the first unit being currently inspected, return null if none is being inspected.
		/// </summary>
		public static Unit GetFirstBeingInspected()
		{
			return inspectingUnits.Count > 0 ? inspectingUnits[0] : null;
		}

		/// <summary>
		/// Retrieve the first currently selected unit, return null if none is selected.
		/// </summary>
		public static Unit GetFirstSelected()
		{
			return selectedUnits.Count > 0 ? selectedUnits[0] : null;
		}

		/// <summary>
		/// Retrieve all currently selected unit.
		/// </summary>
		public static IReadOnlyCollection<Unit> GetAllSelected()
		{
			return selectedUnits.AsReadOnly();
		}

		/// <summary>
		/// Clear the list of selected unit. 
		/// /// </summary>
		public static void ClearSelectedUnits()
		{
			for (int i = selectedUnits.Count - 1; i >= 0; i--)
			{
				selectedUnits[i].RemoveFromSelection();
			}
		}

		/// <summary>
		/// Add this unit to as being inspecting.
		/// </summary>
		private void AddToInspection()
		{
			if (!inspectingUnits.Contains(this))
			{
				inspectingUnits.Add(this);
				onInspectionChanged.Invoke();
			}
		}

		/// <summary>
		/// Remove this unit from as being inspecting.
		/// </summary>
		private void RemoveFromInspection()
		{
			if (inspectingUnits.Contains(this))
			{
				inspectingUnits.Remove(this);
				onInspectionChanged.Invoke();
			}
		}

		/// <summary>
		/// Add this unit to as selected.
		/// </summary>
		private void AddToSelection()
		{
			if (!selectedUnits.Contains(this))
			{
				selectedUnits.Add(this);
				onSelectionChanged.Invoke();
			}
		}

		/// <summary>
		/// Remove this unit from as selected.
		/// </summary>
		private void RemoveFromSelection()
		{
			if (selectedUnits.Contains(this))
			{
				selectedUnits.Remove(this);
				onSelectionChanged.Invoke();
			}
		}

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
			// draw size of the unit
			if (Application.isEditor)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(transform.position, size / 2);
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

			if (Player != null)
			{
				Player.units.Add(this);
			}
		}

		/// <summary>
		///  Deregister this unit from a player.
		/// </summary>
		protected void DeregisterFromPlayer()
		{
			if (game == null)
				return;

			if (Player != null)
			{
				Player.units.Remove(this);
			}
		}

		// ========================================================= Unit Behaviour =========================================================

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
			Health = value;
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
			Melee = meleeValue;
			Defence = defencevalue;
			Magic = magicValue;
			Movement = movementValue;
		}

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
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
			{
				stateMachine.DeregisterAll(this);
			}
		}


		// ========================================================= Navigation State =========================================================

		protected class NavigationSB : StateBehaviour
		{
			// host reference
			protected readonly Unit self = null;

			// caches
			private bool lastIsHovering = false;
			private List<Tile> lastOccupiedTiles = new List<Tile>();
			private List<Tile> affectedOccupiedTiles = new List<Tile>();

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
				IReadOnlyCollection<Tile> tiles = self.IsHoveringOnObject ? self.OccupiedTiles : Tile.EmptyTiles;
				if (CachedValueUtils.HasCollectionChanged(tiles, lastOccupiedTiles, affectedOccupiedTiles))
				{
					foreach (Tile tile in affectedOccupiedTiles)
					{
						tile.UpdateDisplayAs(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.SelfPosition : Tile.DisplayType.EnemyPosition, tiles);
					}
				}

				// show unit info on ui
				if (CachedValueUtils.HasValueChanged(self.IsHoveringOnObject, ref lastIsHovering))
				{
					if (self.IsHoveringOnObject)
					{
						self.AddToInspection();
						self.AddEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingSelf : StatusType.InspectingEnemy);
					}
					else
					{
						self.RemoveFromInspection();
						self.RemoveEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingSelf : StatusType.InspectingEnemy);
					}
				}

				// go to unit movement selection state when this unit is pressed
				if (self.Player == game.CurrentPlayer && self.IPressedOnObject && !self.ActionDepleted && self.OccupiedTiles.Count > 0)
				{
					self.AddToSelection();
					stateMachine.ChangeState(State.UnitActionSelect);
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
					tile.UpdateDisplayAs(self, self.Player == game.CurrentPlayer ? Tile.DisplayType.SelfPosition : Tile.DisplayType.EnemyPosition, Tile.EmptyTiles);
				}

				// hide unit info on ui
				if (self.IsHoveringOnObject)
				{
					self.RemoveFromInspection();
					self.RemoveEffect(self.Player == game.CurrentPlayer ? StatusType.InspectingSelf : StatusType.InspectingEnemy);
				}

				// reset cache
				CachedValueUtils.ResetValueCache(ref lastIsHovering);
				CachedValueUtils.ResetCollectionCache(lastOccupiedTiles, affectedOccupiedTiles);
			}
		}

		// ========================================================= Unit Action Selection State =========================================================

		class UnitActionSelectSB : StateBehaviour
		{
			// host reference
			private readonly Unit self = null;

			// caches
			private bool isSelectedAtStateEnter = false;

			private List<Tile> lastOccupiedTiles = new List<Tile>();
			private List<Tile> otherOccupiedTiles = new List<Tile>();
			private List<Tile> lastMovementArea = new List<Tile>();
			private List<Tile> lastPath = new List<Tile>();
			private Tile lastTargetTile = null;
			private bool lastReachable = true;
			private Vector2 pressedPosition0 = Vector2.negativeInfinity;
			private Vector2 pressedPosition1 = Vector2.negativeInfinity;

			private List<Tile> nextPath = new List<Tile>();
			private List<Tile> appendPath = new List<Tile>();
			private List<Tile> appendExcludedTiles = new List<Tile>();

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
				// show action depleted effect
				if (self.ActionDepleted)
				{
					self.AddEffect(StatusType.Depleted);
				}

				// execute only if the selected unit is this unit
				if (self.IsSelected)
				{
					isSelectedAtStateEnter = true;

					// show selection effect
					self.AddEffect(StatusType.SelectedSelf);

					// show occupied tiles on board, assume unit wont move during movement selection state
					lastOccupiedTiles.AddRange(self.OccupiedTiles);
					foreach (Tile tile in lastOccupiedTiles)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.SelfPosition, lastOccupiedTiles);
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
						tile.UpdateDisplayAs(self, Tile.DisplayType.Move, lastMovementArea);
					}
				}
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				// execute only if the selected unit is this unit
				if (self.IsSelected)
				{
					// find the target tile that the mouse is pointing to
					Tile targetTile = null;
					
					if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Camera.main.farClipPlane, LayerMask.GetMask("Tile")))
					{
						Tile tile = hit.collider.GetComponentInParent<Tile>();
						targetTile = tile;
					}

					// check if target tile is reachable
					bool reachable = lastMovementArea.Contains(targetTile);

					if (lastTargetTile != targetTile)
					{
						// show occupied tile of other units if needed
						if (lastTargetTile != null && !lastOccupiedTiles.Contains(lastTargetTile))
						{
							foreach (Item item in lastTargetTile.Occupants)
							{
								if (item is Unit)
								{
									Unit unit = item as Unit;
									foreach (Tile t in unit.OccupiedTiles)
									{
										if (!lastOccupiedTiles.Contains(t))
										{
											t.UpdateDisplayAs(unit, unit.Player == game.CurrentPlayer ? Tile.DisplayType.FriendPosition : Tile.DisplayType.EnemyPosition, Tile.EmptyTiles);
										}
									}
								}
							}
						}
						if (targetTile != null && !lastOccupiedTiles.Contains(targetTile))
						{
							foreach (Item item in targetTile.Occupants)
							{
								if (item is Unit)
								{
									Unit unit = item as Unit;
									foreach (Tile t in unit.OccupiedTiles)
									{
										if (!lastOccupiedTiles.Contains(t))
										{
											t.UpdateDisplayAs(unit, unit.Player == game.CurrentPlayer ? Tile.DisplayType.FriendPosition : Tile.DisplayType.EnemyPosition, unit.OccupiedTiles);
										}
									}
								}
							}
						}

						// calculate path
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
							lastTargetTile.UpdateDisplayAs(self, Tile.DisplayType.MoveTarget, (Tile)null);
						}
						if (targetTile != null && reachable)
						{
							targetTile.UpdateDisplayAs(self, Tile.DisplayType.MoveTarget, targetTile);
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
							self.MovementStartingTiles.Clear();
							self.MovementStartingTiles.AddRange(self.OccupiedTiles);
							self.MovementSelectedPath.Clear();
							self.MovementSelectedPath.AddRange(lastPath);

							stateMachine.ChangeState(State.UnitMove);
						}
						else
						{
							// return to navigation otherwise
							self.RemoveFromSelection();
							stateMachine.ChangeState(State.Navigation);
						}
					}

					// detect return to navitation by right mouse pressing
					if (InputUtils.GetMousePress(1, ref pressedPosition1))
					{
						// return to navigation otherwise
						self.RemoveFromSelection();
						stateMachine.ChangeState(State.Navigation);
					}
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// hide depleted effect
				if (self.ActionDepleted)
				{
					self.RemoveEffect(StatusType.Depleted);
				}

				// execute only if the selected unit is this unit
				if (isSelectedAtStateEnter)
				{
					// hide selection effect
					self.RemoveEffect(StatusType.SelectedSelf);

					// hide occupied tiles on board
					foreach (Tile tile in lastOccupiedTiles)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.SelfPosition, Tile.EmptyTiles);
					}

					// hide possible movement area on board
					foreach (Tile tile in lastMovementArea)
					{
						tile.UpdateDisplayAs(self, Tile.DisplayType.Move, Tile.EmptyTiles);
					}
					
					// hide occupied tile of other units
					if (lastTargetTile != null && !lastOccupiedTiles.Contains(lastTargetTile))
					{
						foreach (Item item in lastTargetTile.Occupants)
						{
							if (item is Unit)
							{
								Unit unit = item as Unit;
								foreach (Tile t in unit.OccupiedTiles)
								{
									if (!lastOccupiedTiles.Contains(t))
									{
										t.UpdateDisplayAs(unit, unit.Player == game.CurrentPlayer ? Tile.DisplayType.FriendPosition : Tile.DisplayType.EnemyPosition, Tile.EmptyTiles);
									}
								}
							}
						}
					}

					// hide path to target tile on board
					foreach (Tile tile in lastPath)
					{
						tile.HidePath();
					}
					
					// hdie target tile on board
					if (lastTargetTile != null)
					{
						if (lastReachable)
						{
							lastTargetTile.UpdateDisplayAs(this, Tile.DisplayType.MoveTarget, Tile.EmptyTiles);
						}
						else
						{
							lastTargetTile.HideInvalidPath();
						}
					}

					// clear flags
					lastOccupiedTiles.Clear();
					otherOccupiedTiles.Clear();
					lastMovementArea.Clear();
					lastPath.Clear();
					lastTargetTile = null;
					lastReachable = true;

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
				// show depleted effect
				if (self.ActionDepleted)
				{
					self.AddEffect(StatusType.Depleted);
				}

				// execute only if the moving unit is this unit
				if (self.IsSelected)
				{
					self.StartCoroutine(MoveSequence());
				}
			}

			/// <summary>
			/// Movement coroutine for this unit.
			/// </summary>
			protected IEnumerator MoveSequence()
			{
				List<Tile> startTiles = self.MovementStartingTiles;
				List<Tile> path = self.MovementSelectedPath;

				// show all displays on grid
				startTiles.ForEach(x => x.UpdateDisplayAs(self, Tile.DisplayType.SelfPosition, startTiles));
				path.ForEach(x => { x.UpdateDisplayAs(self, Tile.DisplayType.Move, path); x.ShowPath(path); });
				path[path.Count - 1].UpdateDisplayAs(self, Tile.DisplayType.MoveTarget, path[path.Count - 1]);

				// show unit info on ui
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
				startTiles.ForEach(x => x.UpdateDisplayAs(self, Tile.DisplayType.SelfPosition, Tile.EmptyTiles));
				path.ForEach(x => { x.UpdateDisplayAs(self, Tile.DisplayType.Move, Tile.EmptyTiles); x.HidePath(); });
				path[path.Count - 1].UpdateDisplayAs(self, Tile.DisplayType.MoveTarget, (Tile)null);

				// set flag
				self.ActionDepleted = true;
				self.AddEffect(StatusType.Depleted);

				// clear information
				self.RemoveFromSelection();
				self.MovementStartingTiles.Clear();
				self.MovementSelectedPath.Clear();
				
				// change state back to navigation
				stateMachine.ChangeState(State.Navigation);
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
				// hide depleted effect
				if (self.ActionDepleted)
				{
					self.RemoveEffect(StatusType.Depleted);
				}
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
				// show depleted effect
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
				// hide depleted effect
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

