using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DiceRoller
{
	public class Board : MonoBehaviour
	{
		// singleton
		public static Board current { get; protected set; }

		// reference
		private GameController game => GameController.current;

		[Header("Board Setup")]
		public float tileSize = 1f;
		public int boardSizeX = 1;
		public int boardSizeZ = 1;
		public GameObject tilePrefab = null;

		// working variables
		private readonly Dictionary<Int2, Tile> tiles = new Dictionary<Int2, Tile>();

		// temp working variables
		private readonly List<Tile> tempTiles = new List<Tile>();
		private readonly List<TileRangePair> tempTileRangePairs = new List<TileRangePair>();
		private readonly List<TilePathRangeHeuristicPair> tempTilePathRangeHeuristicPairs1 = new List<TilePathRangeHeuristicPair>();
		private readonly List<TilePathRangeHeuristicPair> tempTilePathRangeHeuristicPairs2 = new List<TilePathRangeHeuristicPair>();

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		private void Awake()
		{
			current = this;
			RetrieveAllTiles();
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		private void Start()
		{

		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		private void Update()
		{
			DetectTileHover();
		}

		/// <summary>
		/// OnDestroy is called when the game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
			current = null;
		}

		// ========================================================= Editor =========================================================

		#if UNITY_EDITOR

		/// <summary>
		/// Regenerate all components related to this board. Should only be called in editor.
		/// </summary>
		public void RegenerateBoard()
		{
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(transform.GetChild(i).gameObject);
			}

			List<List<Tile>> tileGrid = new List<List<Tile>>();
			for (int i = 0; i < boardSizeX; i++)
			{
				tileGrid.Add(new List<Tile>());
				for (int j = 0; j < boardSizeZ; j++)
				{
					GameObject go = UnityEditor.PrefabUtility.InstantiatePrefab(tilePrefab, transform) as GameObject;
					go.transform.position = BoardPosToWorldPos(new Int2(i, j));
					go.transform.rotation = Quaternion.identity;
					Tile tile = go.GetComponent<Tile>();
					tile.tileSize = tileSize;
					tile.boardPos = new Int2(i, j);
					tile.RegenerateTile();
					tileGrid[i].Add(tile);
				}
			}

			for (int i = 0; i < boardSizeX; i++)
			{
				for (int j = 0; j < boardSizeZ; j++)
				{
					if (i > 0)
						tileGrid[i][j].connectedTiles.Add(tileGrid[i - 1][j]);
					if (i < boardSizeX - 1)
						tileGrid[i][j].connectedTiles.Add(tileGrid[i + 1][j]);
					if (j > 0)
						tileGrid[i][j].connectedTiles.Add(tileGrid[i][j - 1]);
					if (j < boardSizeZ - 1)
						tileGrid[i][j].connectedTiles.Add(tileGrid[i][j + 1]);
				}
			}
		}

		#endif

		// ========================================================= Position Conversion =========================================================

		/// <summary>
		/// Convert any board position to a world position.
		/// </summary>
		private Vector3 BoardPosToWorldPos(Int2 boardPos)
		{
			return new Vector3(
				(boardPos.x - (float)(boardSizeX - 1) / 2) * tileSize,
				0.001f,
				(boardPos.z - (float)(boardSizeZ - 1) / 2) * tileSize);
		}

		/// <summary>
		/// Convert any world position to the nearest board position.
		/// </summary>
		private Int2 WorldPosToBoardPos(Vector3 worldPos)
		{
			return new Int2(
				Convert.ToInt32(worldPos.x / tileSize + (float)(boardSizeX - 1) / 2),
				Convert.ToInt32(worldPos.z / tileSize + (float)(boardSizeZ - 1) / 2));
		}

		// ========================================================= Tiles =========================================================

		/// <summary>
		/// Retrieve all available tiles of this board.
		/// </summary>
		private void RetrieveAllTiles()
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				if (transform.GetChild(i).gameObject.activeInHierarchy)
				{
					Tile tile = transform.GetChild(i).GetComponent<Tile>();
					if (tile != null)
					{
						tiles[tile.boardPos] = tile;
					}
				}
			}
		}

		// ========================================================= Properties (HoveringTile) =========================================================

		/// <summary>
		/// The tile that the user is currenly hovering on.
		/// </summary>
		public Tile HoveringTile 
		{ 
			get
			{
				return _HoveringTile;
			}
			private set
			{
				if (_HoveringTile != value)
				{
					_HoveringTile = value;
					OnHoveringTileChanged.Invoke();
				}
			}
		}
		private Tile _HoveringTile = null;

		/// <summary>
		/// Event raised whe the tile that the user is currently hovering on is changed.
		/// </summary>
		public event Action OnHoveringTileChanged = () => { };

		/// <summary>
		/// Detect if the mouse is hovering on any tile.
		/// </summary>
		private void DetectTileHover()
		{
			if (game.PersonInControl == GameController.Person.Player)
			{
				// find the target tile that the mouse is pointing to
				if (!InputUtils.IsMouseOnUI() && Die.GetFirstBeingInspected() == null && Unit.GetFirstBeingInspected() == null && !InputUtils.IsDragging)
				{
					if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Camera.main.farClipPlane, LayerMask.GetMask("Tile")))
					{
						HoveringTile = hit.collider.GetComponentInParent<Tile>();
					}
					else
					{
						HoveringTile = null;
					}
				}
				else
				{
					HoveringTile = null;
				}
			}
		}

		/// <summary>
		/// Directly set the tile that is being hovering. Used by AI actions only.
		/// </summary>
		public void SetAITileHover(Tile tile)
		{
			if (game.PersonInControl == GameController.Person.AI)
			{ 
				HoveringTile = tile;
			}
		}

		// ========================================================= Tile Inqury ========================================================
		private struct TileRangePair
		{
			public Tile tile;
			public int range;
		}

		private struct TilePathRangeHeuristicPair
		{
			public Tile tile;
			public Tile previous;
			public int range;
			public float heuristic;
		}

		private static readonly AttackAreaRule SimpleRangeRule =
		new AttackAreaRule((target, starting, range) => Int2.GridDistance(target.boardPos, starting.boardPos) <= range);

		/// <summary>
		/// Get all tiles that an object is in.
		/// </summary>
		public void GetCurrentTiles(Vector3 position, float size, List<Tile> result)
		{
			result.Clear();

			Int2 minBoardPos = WorldPosToBoardPos(position) - Int2.one * Mathf.CeilToInt(size / tileSize);
			Int2 maxBoardPos = WorldPosToBoardPos(position) + Int2.one * Mathf.CeilToInt(size / tileSize);

			for (int i = minBoardPos.x; i <= maxBoardPos.x; i++)
			{
				for (int j = minBoardPos.z; j <= maxBoardPos.z; j++)
				{
					Int2 boardPos = new Int2(i, j);
					if (tiles.ContainsKey(boardPos) && tiles[boardPos].IsInTile(position, size / 2))
					{
						result.Add(tiles[boardPos]);
					}
				}
			}
		}

		/// <summary>
		/// Get all tiles that are within a certain range from the starting tiles regardless of connectivity, and return them in the supplied list.
		/// </summary>
		public void GetTilesInRange(Tile startingTile, int range, List<Tile> result)
		{
			tempTiles.Clear();
			tempTiles.Add(startingTile);
			GetTilesByRule(tempTiles, SimpleRangeRule, range, result);
			tempTiles.Clear();
		}
		/// <summary>
		/// Get all tiles that are within a certain range from the starting tiles regardless of connectivity, and return them in the supplied list.
		/// </summary>
		public void GetTilesInRange(IEnumerable<Tile> startingTiles, int range, List<Tile> result)
		{
			GetTilesByRule(startingTiles, SimpleRangeRule, range, result);
		}

		/// <summary>
		/// Get all tiles that fulfills a given rule in relative to the starting tiles reguardless of connectivity, and return them in the supplied list.
		/// </summary>
		public void GetTilesByRule(Tile startingTile, AttackAreaRule rule, int range, List<Tile> result)
		{
			tempTiles.Clear();
			tempTiles.Add(startingTile);
			GetTilesByRule(tempTiles, rule, range, result);
			tempTiles.Clear();
		}
		/// <summary>
		/// Get all tiles that fulfills a given rule in relative to the starting tiles reguardless of connectivity, and return them in the supplied list.
		/// </summary>
		public void GetTilesByRule(IEnumerable<Tile> startingTiles, AttackAreaRule rule, int range, List<Tile> result)
		{
			// prepare containers
			result.Clear();

			// calculate the bound of starting tiles
			Int2 min = Int2.MaxValue;
			Int2 max = Int2.MinValue;
			min.x = startingTiles.Select(tile => tile.boardPos.x).Min();
			min.z = startingTiles.Select(tile => tile.boardPos.z).Min();
			max.x = startingTiles.Select(tile => tile.boardPos.x).Max();
			max.z = startingTiles.Select(tile => tile.boardPos.z).Max();

			// search for tiles within range on a subset of all tiles
			for (int x = min.x - range; x <= max.x + range; x++)
			{
				for (int z = min.z - range; z <= max.z + range; z++)
				{
					Int2 pos = new Int2(x, z);
					if (tiles.ContainsKey(pos))
					{
						Tile target = tiles[pos];
						if (startingTiles.Any(starting => rule.Evaulate(target, starting, range)))
						{
							result.Add(target);
						}
					}
				}
			}
		}

		/// <summary>
		/// Find all connected tiles within a certain range of this tile, and return them in the supplied list.
		/// </summary>
		public void GetConnectedTilesInRange(IEnumerable<Tile> startingTiles, int range, List<Tile> result)
		{
			GetConnectedTilesInRange(startingTiles, null, range, result);
		}
		/// <summary>
		/// Find all connected tiles within a certain range of this tile, and return them in the supplied list.
		/// </summary>
		public void GetConnectedTilesInRange(IEnumerable<Tile> startingTiles, IEnumerable<Tile> excludedTiles, int range, List<Tile> result)
		{
			// prepare containers
			List<TileRangePair> open = tempTileRangePairs;
			open.Clear();
			result.Clear();

			// add initial tiles
			foreach (Tile startingTile in startingTiles)
			{
				open.Add(new TileRangePair() { tile = startingTile, range = range });
			}

			// explore in a breadth first manner
			while (open.Count > 0)
			{
				// get the tile in the first of the list
				TileRangePair current = open[0];
				open.RemoveAt(0);
				result.Add(current.tile);

				// explore its connections
				if (current.range > 0)
				{
					foreach (Tile connectedTile in current.tile.connectedTiles)
					{
						if (connectedTile == null)
							continue;
						if (!connectedTile.active)
							continue;
						if (excludedTiles != null && excludedTiles.Contains(connectedTile))
							continue;
						if (result.Contains(connectedTile))
							continue;
						if (open.Exists(x => x.tile == connectedTile))
							continue;

						open.Add(new TileRangePair() { tile = connectedTile, range = current.range - 1 });
					}
				}
			}
		}

		/// <summary>
		/// Find the shortest path between a set of starting tile(s) to a specific target tile.
		/// </summary>
		public void GetShortestPath(Tile startingTile, Tile targetTile, int range, List<Tile> result)
		{
			tempTiles.Clear();
			tempTiles.Add(startingTile);
			GetShortestPath(tempTiles, null, targetTile, range, result);
			tempTiles.Clear();
		}
		/// <summary>
		/// Find the shortest path between a set of starting tile(s) to a specific target tile.
		/// </summary>
		public void GetShortestPath(IEnumerable<Tile> startingTiles, Tile targetTile, int range, List<Tile> result)
		{
			GetShortestPath(startingTiles, null, targetTile, range, result);
		}
		/// <summary>
		/// Find the shortest path between a set of starting tile(s) to a specific target tile.
		/// </summary>
		public void GetShortestPath(Tile startingTile, IEnumerable<Tile> excludedTiles, Tile targetTile, int range, List<Tile> result)
		{
			tempTiles.Clear();
			tempTiles.Add(startingTile);
			GetShortestPath(tempTiles, excludedTiles, targetTile, range, result);
			tempTiles.Clear();
		}
		/// <summary>
		/// Find the shortest path between a set of starting tile(s) to a specific target tile.
		/// </summary>
		public void GetShortestPath(IEnumerable<Tile> startingTiles, IEnumerable<Tile> excludedTiles, Tile targetTile, int range, List<Tile> result)
		{
			// prepare containers
			List<TilePathRangeHeuristicPair> open = tempTilePathRangeHeuristicPairs1;
			List<TilePathRangeHeuristicPair> closed = tempTilePathRangeHeuristicPairs2;
			open.Clear();
			closed.Clear();
			result.Clear();

			// add initial tiles, also check if target tile is already included in the starting tiles
			foreach (Tile startingTile in startingTiles)
			{
				if (startingTile == targetTile)
				{
					result.Add(targetTile);
					return;
				}

				open.Add(new TilePathRangeHeuristicPair()
				{ 
					tile = startingTile,
					previous = null,
					range = 0,
					heuristic = Vector3.Distance(startingTile.worldPos, targetTile.worldPos) 
				});
			}

			// explore in a A* manner
			while (open.Count > 0)
			{
				// get the tile in the open list with the smallest heuristic
				open.Sort((a, b) => a.heuristic.CompareTo(b.heuristic));
				TilePathRangeHeuristicPair current = open[0];
				open.RemoveAt(0);

				// check if solution is already found, return path if so
				if (current.tile == targetTile)
				{
					result.Add(current.tile);
					do
					{
						if (current.previous != null)
						{
							result.Insert(0, current.previous);
							current = closed.Find(x => x.tile == current.previous);
						}
					}
					while (current.previous != null);

					open.Clear();
					closed.Clear();
					return;
				}

				// explore its connections
				if (current.range < range)
				{
					foreach (Tile connectedTile in current.tile.connectedTiles)
					{
						if (!connectedTile.active)
							continue;
						if (excludedTiles != null && excludedTiles.Contains(connectedTile))
							continue;

						float heuristic = current.range * connectedTile.tileSize + Vector3.Distance(connectedTile.worldPos, targetTile.worldPos);

						if (open.Exists(x => x.tile == connectedTile && x.heuristic < heuristic))
							continue;

						if (closed.Exists(x => x.tile == connectedTile && x.heuristic < heuristic))
							continue;
						
						open.Add(new TilePathRangeHeuristicPair()
						{
							tile = connectedTile,
							previous = current.tile,
							range = current.range + 1,
							heuristic = heuristic
						});
					}
				}

				closed.Add(new TilePathRangeHeuristicPair() 
				{ 
					tile = current.tile,
					previous = current.previous,
					range = current.range,
					heuristic = current.heuristic
				});
			}

			// completed without a path found
			open.Clear();
			closed.Clear();
			return;
		}

		// ========================================================= Display Area =========================================================

		private struct HolderDisplayTypePair : IEquatable<HolderDisplayTypePair>
		{
			public object holder;
			public Tile.DisplayType displayType;

			public HolderDisplayTypePair(object holder, Tile.DisplayType displayType)
			{
				this.holder = holder;
				this.displayType = displayType;
			}

			public bool Equals(HolderDisplayTypePair other)
			{
				return holder == other.holder && displayType == other.displayType;
			}

			public override bool Equals(object obj)
			{
				if (obj == null || GetType() != obj.GetType())
					return false;
				return Equals((HolderDisplayTypePair)obj);
			}

			public override int GetHashCode()
			{
				return holder.GetHashCode() << 8 + (int)displayType;
			}
		}
		private class TileGroup
		{
			public HolderDisplayTypePair Info { get; private set; } = new HolderDisplayTypePair();
			public IReadOnlyList<Tile> Tiles { get { return _Tiles.AsReadOnly(); } }
			private readonly List<Tile> _Tiles = new List<Tile>();

			private static readonly Stack<TileGroup> pool = new Stack<TileGroup>();

			private TileGroup() {}

			public static TileGroup New(HolderDisplayTypePair info, IEnumerable<Tile> tiles)
			{
				TileGroup target;
				if (pool.Count == 0)
				{
					target = new TileGroup();
				}
				else
				{
					target = pool.Pop();
				}
				target.Info = info; 
				target._Tiles.Clear();
				target._Tiles.AddRange(tiles);
				return target;
			}

			public void UpdateTiles(IEnumerable<Tile> tiles)
			{
				_Tiles.Clear();
				_Tiles.AddRange(tiles);
			}

			public void ShowDisplay(bool show)
			{
				foreach (Tile tile in Tiles)
				{
					if (show)
					{
						tile.UpdateDisplayAs(Info.holder, Info.displayType, Tiles);
					}
					else
					{
						tile.RemoveDisplay(Info.holder, Info.displayType);
					}
				}
			}

			public void Recycle()
			{
				pool.Push(this);
			}
		}

		private readonly Dictionary<HolderDisplayTypePair, TileGroup> activeTileDisplayGroup = new Dictionary<HolderDisplayTypePair, TileGroup>();

		/// <summary>
		/// Display an area on the tiles.
		/// </summary>
		public void ShowArea(object holder, Tile.DisplayType displayType, Tile area)
		{
			if (area != null)
			{
				tempTiles.Clear();
				tempTiles.Add(area);
				ShowArea(holder, displayType, tempTiles);
				tempTiles.Clear();
			}
			else
			{
				HideArea(holder, displayType);
			}
		}

		/// <summary>
		/// Display an area on the tiles.
		/// </summary>
		public void ShowArea(object holder, Tile.DisplayType displayType, IEnumerable<Tile> area)
		{
			HolderDisplayTypePair targetPair = new HolderDisplayTypePair(holder, displayType);

			// remove previous display
			if (activeTileDisplayGroup.ContainsKey(targetPair))
			{
				activeTileDisplayGroup[targetPair].ShowDisplay(false);
			}

			// remove entry if needed
			if (area.Count() == 0 && activeTileDisplayGroup.ContainsKey(targetPair))
			{
				activeTileDisplayGroup[targetPair].Recycle();
				activeTileDisplayGroup.Remove(targetPair);
			}

			// add new display
			if (area.Count() != 0)
			{
				if (!activeTileDisplayGroup.ContainsKey(targetPair))
				{
					activeTileDisplayGroup[targetPair] = TileGroup.New(targetPair, area);
				}
				else
				{
					activeTileDisplayGroup[targetPair].UpdateTiles(area);
				}
				activeTileDisplayGroup[targetPair].ShowDisplay(true);
			}
		}

		/// <summary>
		/// Hide a displayed area on the tiles.
		/// </summary>
		public void HideArea(object holder, Tile.DisplayType displayType)
		{
			ShowArea(holder, displayType, Tile.EmptyTiles);
		}

		// ========================================================= Display Path =========================================================

		private readonly List<Tile> activePath = new List<Tile>();

		/// <summary>
		/// Show a display of the path on the board.
		/// </summary>
		public void ShowPath(List<Tile> path)
		{
			HidePath();
			foreach (Tile tile in path)
			{
				tile.ShowPath(path);
				activePath.Add(tile);
			}
		}

		/// <summary>
		/// Show a display of not reachable path on the board.
		/// </summary>
		public void ShowInvalidPath(Tile tile)
		{
			HidePath();
			tile.ShowInvalidPath();
			activePath.Add(tile);
		}

		/// <summary>
		/// Hide the displayed path on the board.
		/// </summary>
		public void HidePath()
		{
			foreach (Tile tile in activePath)
			{
				tile.HidePath();
			}
			activePath.Clear();
		}
	}
}