using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
	public class Board : MonoBehaviour
	{
		protected struct TileRangePair
		{
			public Tile tile;
			public int range;
		}

		protected struct TilePathRangeHeuristicPair
		{
			public Tile tile;
			public Tile previous;
			public int range;
			public float heuristic;
		}

		// singleton
		public static Board current { get; protected set; }

		public float tileSize = 1f;
		public int boardSizeX = 1;
		public int boardSizeZ = 1;
		public GameObject tilePrefab = null;

		// working variables
		protected List<Tile> tiles = new List<Tile>();

		// temporary working variables
		protected List<Tile> tempTiles = new List<Tile>();
		protected List<TileRangePair> tempTileRangePairs = new List<TileRangePair>();
		protected List<TilePathRangeHeuristicPair> tempTilePathRangeHeuristicPairs1 = new List<TilePathRangeHeuristicPair>();
		protected List<TilePathRangeHeuristicPair> tempTilePathRangeHeuristicPairs2 = new List<TilePathRangeHeuristicPair>();

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		void Awake()
		{
			current = this;
			for (int i = 0; i < transform.childCount; i++)
			{
				if (transform.GetChild(i).CompareTag("Tile"))
				{
					if (transform.GetChild(i).gameObject.activeInHierarchy)
					{
						tiles.Add(transform.GetChild(i).GetComponent<Tile>());
					}
				}
			}
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		void Start()
		{

		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		void Update()
		{

		}

		/// <summary>
		/// OnDestroy is called when the game object is destroyed.
		/// </summary>
		void OnDestroy()
		{
			current = null;
		}

		// ========================================================= Editor =========================================================

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
					GameObject go = Instantiate(tilePrefab,
						new Vector3(
							(-(float)(boardSizeX - 1) / 2 + i) * tileSize,
							0.001f,
							(-(float)(boardSizeZ - 1) / 2 + j) * tileSize),
						Quaternion.identity,
						transform);
					Tile tile = go.GetComponent<Tile>();
					tile.tileSize = tileSize;
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

		// ========================================================= Inqury =========================================================

		/// <summary>
		/// Get all tiles that an object is in.
		/// </summary>
		public void GetCurrentTiles(Vector3 position, float size, in List<Tile> result)
		{
			result.Clear();
			foreach (Tile tile in tiles)
			{
				if (tile.IsInTile(position, size))
				{
					result.Add(tile);
				}
			}
		}

		/// <summary>
		/// Find all tiles within a certain range of this tile, and return them in the supplied list.
		/// </summary>
		public void GetTilesWithinRange(List<Tile> startingTiles, int range, in List<Tile> result)
		{
			GetTilesWithinRange(startingTiles, null, range, in result);
		}
		/// <summary>
		/// Find all tiles within a certain range of this tile, and return them in the supplied list.
		/// </summary>
		public void GetTilesWithinRange(List<Tile> startingTiles, List<Tile> excludedTiles, int range, in List<Tile> result)
		{
			// prepare containers
			List<TileRangePair> open = tempTileRangePairs;
			open.Clear();
			result.Clear();

			// add initial tiles
			foreach (Tile startingTile in startingTiles)
			{
				if (excludedTiles != null && excludedTiles.Contains(startingTile))
					continue;
				open.Add(new TileRangePair() { tile = startingTile, range = range });
			}

			// explore in a depth first manner
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
						if (!connectedTile.gameObject.activeInHierarchy)
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
		public void GetShortestPath(Tile startingTile, Tile targetTile, int range, in List<Tile> result)
		{
			tempTiles.Clear();
			tempTiles.Add(startingTile);
			GetShortestPath(tempTiles, null, targetTile, range, in result);
		}
		/// <summary>
		/// Find the shortest path between a set of starting tile(s) to a specific target tile.
		/// </summary>
		public void GetShortestPath(List<Tile> startingTiles, Tile targetTile, int range, in List<Tile> result)
		{
			GetShortestPath(startingTiles, null, targetTile, range, in result);
		}
		/// <summary>
		/// Find the shortest path between a set of starting tile(s) to a specific target tile.
		/// </summary>
		public void GetShortestPath(Tile startingTile, List<Tile> excludedTiles, Tile targetTile, int range, in List<Tile> result)
		{
			tempTiles.Clear();
			tempTiles.Add(startingTile);
			GetShortestPath(tempTiles, excludedTiles, targetTile, range, in result);
		}
		/// <summary>
		/// Find the shortest path between a set of starting tile(s) to a specific target tile.
		/// </summary>
		public void GetShortestPath(List<Tile> startingTiles, List<Tile> excludedTiles, Tile targetTile, int range, in List<Tile> result)
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
				if (excludedTiles != null && excludedTiles.Contains(startingTile))
					continue;
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
					heuristic = Vector3.Distance(startingTile.transform.position, targetTile.transform.position) 
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
						if (!connectedTile.gameObject.activeInHierarchy)
							continue;
						if (excludedTiles != null && excludedTiles.Contains(connectedTile))
							continue;

						float heuristic = current.range * connectedTile.tileSize + Vector3.Distance(connectedTile.transform.position, targetTile.transform.position);

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
	}
}