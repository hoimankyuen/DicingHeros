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

		// shared parameters
		public static float tileSize = 0.25f;
		public static int boardPieceSize = 8;

		[Header("Preset Board Pieces")]
		public List<BoardPiece> presetBoardPieces = new List<BoardPiece>();

		[Header("Other")]
		public GameObject tileSelection = null;

		// events
		public event Action OnBoardChanged = () => { };

		// reference
		private GameController game => GameController.current;

		// working variables
		private readonly Dictionary<Int2, BoardPiece> boardPieces = new Dictionary<Int2, BoardPiece>();
		private readonly Dictionary<Int2, Tile> tiles = new Dictionary<Int2, Tile>();

		// temp working variables
		private readonly List<Tile> tempTileList = new List<Tile>();
		private readonly HashSet<Tile> tempTileSet = new HashSet<Tile>();
		private readonly List<TileRangePair> tempTileRangePairList = new List<TileRangePair>();


		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		private void Awake()
		{
			current = this;
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		private void Start()
		{
			SetupPresetBoardPieces();
			ShowTargetTile(null);
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

		// ========================================================= Board Piece =========================================================

		public float BoardUpdatedTime { get; private set; } = 0;

		/// <summary>
		/// Setup and connect the preset board pieces.
		/// </summary>
		private void SetupPresetBoardPieces()
		{
			foreach (BoardPiece boardPiece in presetBoardPieces)
			{
				if (boardPiece.gameObject.activeInHierarchy)
				{
					AddBoardPiece(boardPiece);
				}
			}
		}

		/// <summary>
		/// Add a board piece to become part of the board.
		/// </summary>
		public void AddBoardPiece(BoardPiece boardPiece)
		{
			// finailze the positions, board should not move after here
			boardPiece.UpateAllTilesPositions();

			// add board pieces and its tiles
			boardPieces[boardPiece.boardPiecePos] = boardPiece;
			foreach (Tile tile in boardPiece.Tiles)
			{
				tiles[tile.BoardPos] = tile;
			}

			// connect tiles from existing tiles to new tiles
			Int2[] directions = new Int2[] { Int2.left, Int2.forward, Int2.right, Int2.backward };
			Int2[] startingPoints = new Int2[] { new Int2(0, 0), new Int2(0, boardPieceSize - 1), new Int2(boardPieceSize - 1, boardPieceSize - 1), new Int2(boardPieceSize - 1, 0) };
			Int2 tilePos1, tilePos2;
			for (int i = 0; i < directions.Length; i++)
			{
				for (int j = 0; j < boardPieceSize; j++)
				{
					tilePos1 = boardPiece.ToGlobalBoard(startingPoints[i] + directions[(i + 1) % 4] * j);
					tilePos2 = tilePos1 + directions[i];
					if (tiles.ContainsKey(tilePos1) && tiles.ContainsKey(tilePos2))
					{
						tiles[tilePos1].Connect(tiles[tilePos2], directions[i]);
						tiles[tilePos2].Connect(tiles[tilePos1], directions[i] * -1);
					}
				}
			}

			BoardUpdatedTime = Time.time;
			OnBoardChanged.Invoke();
		}

		/// <summary>
		/// Remove a board piece form the board.
		/// </summary>
		public void RemoveBoardPiece(BoardPiece boardPiece)
		{
			// disconnect tiles from other tiles to removeing tiles
			Int2[] directions = new Int2[] { Int2.left, Int2.forward, Int2.right, Int2.backward };
			Int2[] startingPoints = new Int2[] { new Int2(0, 0), new Int2(0, boardPieceSize - 1), new Int2(boardPieceSize - 1, boardPieceSize - 1), new Int2(boardPieceSize - 1, 0) };
			Int2 tilePos1, tilePos2;
			for (int i = 0; i < directions.Length; i++)
			{
				for (int j = 0; j < boardPieceSize; j++)
				{
					tilePos1 = boardPiece.ToGlobalBoard(startingPoints[i] + directions[(i + 1) % 4] * j);
					tilePos2 = tilePos1 + directions[i];
					if (tiles.ContainsKey(tilePos1) && tiles.ContainsKey(tilePos2))
					{
						tiles[tilePos1].Connect(null, directions[i]);
						tiles[tilePos2].Connect(null, directions[i] * -1);
					}
				}
			}

			// remove board pieces and its tiles
			boardPieces.Remove(boardPiece.boardPiecePos);
			foreach (Tile tile in boardPiece.Tiles)
			{
				tiles.Remove(tile.BoardPos);
			}

			BoardUpdatedTime = Time.time;
			OnBoardChanged.Invoke();
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

		/// <summary>
		/// Get all tiles that an object is in.
		/// </summary>
		public void GetCurrentTiles(Vector3 position, float size, List<Tile> result)
		{
			result.Clear();

			// base board piece (board piece(0, 0)) must exist if board is valid
			if (!boardPieces.ContainsKey(Int2.zero))
				return;

			Int2 minBoardPos = boardPieces[Int2.zero].WorldToLocalBoard(position) - Int2.one * Mathf.CeilToInt(size / tileSize);
			Int2 maxBoardPos = boardPieces[Int2.zero].WorldToLocalBoard(position) + Int2.one * Mathf.CeilToInt(size / tileSize);

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
			tempTileList.Clear();
			tempTileList.Add(startingTile);
			GetTilesByRule(tempTileList, AttackAreaRule.Adjacent, range, result);
			tempTileList.Clear();
		}
		/// <summary>
		/// Get all tiles that are within a certain range from the starting tiles regardless of connectivity, and return them in the supplied list.
		/// </summary>
		public void GetTilesInRange(IEnumerable<Tile> startingTiles, int range, List<Tile> result)
		{
			GetTilesByRule(startingTiles, AttackAreaRule.Adjacent, range, result);
		}

		/// <summary>
		/// Get all tiles that fulfills a given rule in relative to the starting tiles reguardless of connectivity, and return them in the supplied list.
		/// </summary>
		public void GetTilesByRule(Tile startingTile, AttackAreaRule rule, int range, List<Tile> result)
		{
			tempTileList.Clear();
			tempTileList.Add(startingTile);
			GetTilesByRule(tempTileList, rule, range, result);
			tempTileList.Clear();
		}
		/// <summary>
		/// Get all tiles that fulfills a given rule in relative to the starting tiles reguardless of connectivity, and return them in the supplied list.
		/// </summary>
		public void GetTilesByRule(IEnumerable<Tile> startingTiles, AttackAreaRule rule, int range, List<Tile> result)
		{
			// prepare containers
			result.Clear();

			// no result if no starting tiles provided
			if (startingTiles.Count() == 0)
				return;

			// calculate the bound of starting tiles
			Int2 min = Int2.MaxValue;
			Int2 max = Int2.MinValue;
			min.x = startingTiles.Select(tile => tile.BoardPos.x).Min();
			min.z = startingTiles.Select(tile => tile.BoardPos.z).Min();
			max.x = startingTiles.Select(tile => tile.BoardPos.x).Max();
			max.z = startingTiles.Select(tile => tile.BoardPos.z).Max();

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
			List<TileRangePair> open = tempTileRangePairList;
			open.Clear();
			result.Clear();

			// no result if no starting tiles provided
			if (startingTiles.Count() == 0)
				return;

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
					foreach (Tile connectedTile in current.tile.ConnectedTiles)
					{
						if (connectedTile == null)
							continue;
						if (!connectedTile.Active)
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

		// ========================================================= Shortest Path ========================================================

		private class TileCHeuristicomparer : IComparer<Tile>
		{
			public int Compare(Tile a, Tile b)
			{
				return a.pathFindingF.CompareTo(b.pathFindingF);
			}
		}
		private TileCHeuristicomparer tileHeuristicComparer = new TileCHeuristicomparer();

		private readonly List<Tile> tempOpen = new List<Tile>();
		private readonly List<Tile> tempClosed = new List<Tile>();

		/// <summary>
		/// Find the shortest path between a set of starting tile(s) to a specific target tile.
		/// </summary>
		public void GetShortestPath(Tile startingTile, Tile targetTile, int range, List<Tile> result)
		{
			tempTileList.Clear();
			tempTileList.Add(startingTile);
			GetShortestPath(tempTileList, null, targetTile, range, result);
			tempTileList.Clear();
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
			tempTileList.Clear();
			tempTileList.Add(startingTile);
			GetShortestPath(tempTileList, excludedTiles, targetTile, range, result);
			tempTileList.Clear();
		}
		/// <summary>
		/// Find the shortest path between a set of starting tile(s) to a specific target tile.
		/// </summary>
		public void GetShortestPath(IEnumerable<Tile> startingTiles, IEnumerable<Tile> excludedTiles, Tile targetTile, int range, List<Tile> result)
		{
			void ClearTempInfo(Tile tile)
			{
				tile.pathFindingPrev = null;
				tile.pathFindingG = 0;
				tile.pathFindingF = 0;
			}

			// prepare containers
			HashSet<Tile> excludedTileSet = tempTileSet;
			List<Tile> open = tempOpen;
			List<Tile> closed = tempClosed;
			excludedTileSet.Clear();
			open.Clear();
			closed.Clear();
			result.Clear();

			// no result if no starting tiles provided
			if (startingTiles.Count() == 0)
				return;

			// add initial tiles, also check if target tile is already included in the starting tiles
			foreach (Tile startingTile in startingTiles)
			{
				if (startingTile == targetTile)
				{
					result.Add(targetTile);
					return;
				}

				ClearTempInfo(startingTile);
				open.Add(startingTile);
			}

			// setup exculded tile set for faster access
			foreach (Tile tile in excludedTiles)
			{
				excludedTileSet.Add(tile);
			}

			// explore in a A* manner
			while (open.Count > 0)
			{
				// get the tile in the open list with the smallest heuristic, since the list is always sorted, the first elements should suffice
				Tile q = open[0];
				open.RemoveAt(0);

				// check if solution is already found, return path if so
				if (q == targetTile)
				{
					result.Add(q);
					do
					{
						if (q.pathFindingPrev != null)
						{
							result.Add(q.pathFindingPrev);
							q = q.pathFindingPrev;
						}
					}
					while (q.pathFindingPrev != null);
					result.Reverse();

					open.ForEach(ClearTempInfo);
					open.Clear();
					closed.ForEach(ClearTempInfo);
					closed.Clear();
					return;
				}

				// explore its connections
				if (q.pathFindingG < range)
				{
					foreach (Tile r in q.ConnectedTiles)
					{
						if (r == null)
							continue;
						if (!r.Active)
							continue;
						if (excludedTileSet.Contains(r))
							continue;

						int g = q.pathFindingG + 1;
						int h = Int2.GridDistance(r.BoardPos, targetTile.BoardPos);
						int f = g + h;

						if (open.Contains(r))
							continue;
						if (closed.Contains(r))
							continue;

						r.pathFindingPrev = q;
						r.pathFindingG = g;
						r.pathFindingF = f;
						int index = open.BinarySearch(r, tileHeuristicComparer);
						open.Insert(index < 0 ? ~index : index, r);
					}
				}

				// add current to closed
				closed.Add(q);
			}

			// completed without a path found
			open.ForEach(ClearTempInfo);
			open.Clear();
			closed.ForEach(ClearTempInfo);
			closed.Clear();
			open.Clear();
			closed.Clear();
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
						tile.UpdateAreaDisplayAs(Info.holder, Info.displayType, Tiles);
					}
					else
					{
						tile.RemoveAreaDisplay(Info.holder, Info.displayType);
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
				tempTileList.Clear();
				tempTileList.Add(area);
				ShowArea(holder, displayType, tempTileList);
				tempTileList.Clear();
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

		// ========================================================= Target Tile =========================================================

		/// <summary>
		/// Show a display of the target tile on the board.
		/// </summary>
		public void ShowTargetTile(Tile tile)
		{
			if (tile != null)
			{
				tileSelection.SetActive(true);
				tileSelection.transform.position = tile.transform.position + Vector3.up * 0.01f;
			}
			else
			{
				tileSelection.SetActive(false);
			}
		}
	}
}