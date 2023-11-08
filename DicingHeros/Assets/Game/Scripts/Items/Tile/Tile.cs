using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DicingHeros
{
	public class Tile : MonoBehaviour, IEquatable<Tile>
	{
		public enum DisplayType
		{
			Normal,

			Move,
			Attack,

			MovePossible,
			AttackPossible,

			MoveTarget,
			AttackTarget,

			EnemyPosition,
			FriendPosition,
			SelfPosition,
		} // order of enum denotes display priority, where larger means higher priority
		public static readonly int DisplayTypeCount = 10;

		public enum PathDirection
		{
			Start,
			Right,
			Top,
			Left,
			Bottom,
			End,
		}

		[System.Serializable]
		public struct PathDirections
		{
			public PathDirection from;
			public PathDirection to;
			public PathDirections(PathDirection from, PathDirection to)
			{
				this.from = from;
				this.to = to;
			}
			public PathDirections Invsersed()
			{
				return new PathDirections(to, from);
			}
			public override int GetHashCode()
			{
				return from.GetHashCode() ^ (to.GetHashCode() << 8);
			}
			public override bool Equals(object obj)
			{
				return obj is PathDirections other && from == other.from && to == other.to;
			}
		}

		public class RangeDisplayEntry
		{
			public object holder;
			public TileRangeDisplay rangeDisplay;
			public RangeDisplayEntry(object holder, TileRangeDisplay range)
			{
				this.holder = holder;
				this.rangeDisplay = range;
			}
		}

		public static IReadOnlyCollection<Tile> EmptyTiles
		{ 
			get
			{
				return emptytiles.AsReadOnly();
			} 
		}
		private static readonly List<Tile> emptytiles = new List<Tile>();

		// parameters
		[Header("Tile Properties (Auto Generated)")]
		public BoardPiece boardPiece = null;
		public Int2 localBoardPos = Int2.zero;

		[Header("Resources")]
		public TileStyle style = null;

		// reference
		private GameController game => GameController.current;

		// component
		private List<TileRangeDisplay> tileRanges = new List<TileRangeDisplay>();
		private SpriteRenderer pathRenderer = null;
		private new Collider collider = null;

		// working variables
		private Dictionary<DisplayType, List<RangeDisplayEntry>> registeredDisplay = new Dictionary<DisplayType, List<RangeDisplayEntry>>();
		private bool hovering = false;

		// temp variables for faciliating path finding
		[NonSerialized]
		public Tile pathFindingPrev;
		[NonSerialized]
		public int pathFindingG;
		[NonSerialized]
		public int pathFindingF;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		private void Awake()
		{
			tileRanges.Add(transform.Find("Sprites/Ranges/Range").GetComponent<TileRangeDisplay>());
			pathRenderer = transform.Find("Sprites/Path").GetComponent<SpriteRenderer>();
			collider = transform.Find("Collider").GetComponent<Collider>();

			for (int i = 0; i < DisplayTypeCount; i++)
			{
				registeredDisplay[(DisplayType)i] = new List<RangeDisplayEntry>();
			}

			tileRanges[0].Show(false);

			Active = true;
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

		}

		/// <summary>
		/// OnDestroy is called when the game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
		}

		/// <summary>
		/// OnValidate is called when any inspector value is changed.
		/// </summary>
		private void OnValidate()
		{

		}

		/// <summary>
		/// OnMouseEnter is called when the mouse is start pointing to the game object.
		/// </summary>
		private void OnMouseEnter()
		{
			// not working, consider fixing or removing
			if (!hovering)
			{
				foreach (Item item in occupants)
				{
					item.SetHoveringFromTile(this, true);
				}
				hovering = true;
			}
		}

		/// <summary>
		/// OnMouseExit is called when the mouse is stop pointing to the game object.
		/// </summary>
		private void OnMouseExit()
		{
			// not working, consider fixing or removing
			if (hovering)
			{
				foreach (Item item in occupants)
				{
					item.SetHoveringFromTile(this, false);
				}
				hovering = false;
			}
		}

		/// <summary>
		/// OnMouseDown is called when a mouse button is pressed when pointing to the game object.
		/// </summary>
		private void OnMouseDown()
		{

		}

		// ========================================================= IEqautable Methods =========================================================

		/// <summary>
		/// Check if this object is equal to the other object.
		/// </summary>
		public bool Equals(Tile other)
		{
			return this == other;
		}

		// ========================================================= Editor =========================================================

		#if UNITY_EDITOR

		/// <summary>
		/// Regenerate all components related to this tile. Should only be called in editor.
		/// </summary>
		public void RegenerateTile()
		{
			SpriteRenderer displaySpriteRenderer = transform.Find("Sprites/Tile").GetComponent<SpriteRenderer>();
			displaySpriteRenderer.transform.localScale = new Vector3(Board.tileSize / 1.28f, Board.tileSize / 1.28f, 1f);

			TileRangeDisplay tileRange = transform.Find("Sprites/Ranges/Range").GetComponent<TileRangeDisplay>();
			tileRange.transform.localScale = new Vector3(Board.tileSize / 1.28f, Board.tileSize / 1.28f, 1f);

			SpriteRenderer pathSpriteRenderer = transform.Find("Sprites/Path").GetComponent<SpriteRenderer>();
			pathSpriteRenderer.transform.localScale = new Vector3(Board.tileSize / 1.28f, Board.tileSize / 1.28f, 1f);

			collider = transform.Find("Collider").GetComponent<Collider>();
			collider.transform.localScale = new Vector3(Board.tileSize, 0.1f, Board.tileSize);
		}

		#endif

		// ========================================================= Properties (WorldPos) =========================================================

		/// <summary>
		/// The global world position of the tile.
		/// </summary>
		public Vector3 WorldPos { get; private set; } = Vector3.negativeInfinity;

		/// <summary>
		/// Update WoldPos to current value.
		/// </summary>
		public void UpdateWordPos()
		{
			WorldPos = transform.position;
		}

		// ========================================================= Properties (BoardPos) =========================================================

		/// <summary>
		/// The global board position of the tile.
		/// </summary>
		public Int2 BoardPos { get; private set; }

		/// <summary>
		/// Update BoardPos to current value.
		/// </summary>
		public void UpdateBoardPos()
		{
			if (boardPiece != null)
			{
				BoardPos = boardPiece.ToGlobalBoard(localBoardPos);
			}
			else
			{
				BoardPos = localBoardPos;
			}
		}

		// ========================================================= Properties (Active) =========================================================

		/// <summary>
		/// Flag for this tile is on the board but not active at the moment.
		/// </summary>
		public bool Active { get; private set; } = false;

		// ========================================================= Properties (ConnectedTiles) =========================================================

		/// <summary>
		/// All connected tiles of this tile in the sequence of directions of Left, Forward, Right, Backward. Null indicated no connection in that direction.
		/// </summary>
		public IEnumerable<Tile> ConnectedTiles
		{ 
			get
			{
				return _ConnectedTiles.AsReadOnly();
			}
		}
		private List<Tile> _ConnectedTiles = new List<Tile>(); // sequence: Left, Forward, Right, Backward

		/// <summary>
		/// Connect another tile to this tile in a specific direction.
		/// </summary>
		public void Connect(Tile other, Int2 direction)
		{
			// initialize connected tiles if not done yet
			if (_ConnectedTiles.Count == 0)
			{
				for (int i = 0; i < 4; i++)
				{
					_ConnectedTiles.Add(null);
				}
			}

			// connect to other tiles
			if  (direction == Int2.left)
			{
				_ConnectedTiles[0] = other;
			}
			else if (direction == Int2.forward)
			{
				_ConnectedTiles[1] = other;
			}
			else if (direction == Int2.right)
			{
				_ConnectedTiles[2] = other;
			}
			else if (direction == Int2.backward)
			{
				_ConnectedTiles[3] = other;
			}
		}

		// ========================================================= Properties (Occupants) =========================================================

		/// <summary>
		/// A list of all item that occupied this tile.
		/// </summary>
		public IReadOnlyCollection<Item> Occupants
		{
			get
			{
				return occupants.AsReadOnly();
			}
		}
		private readonly List<Item> occupants = new List<Item>();

		/// <summary>
		/// Add an occupant to this tile, all hovering through tile.
		/// </summary>
		public void AddOccupant(Item item)
		{
			if (!occupants.Contains(item))
			{
				occupants.Add(item);
				item.SetHoveringFromTile(this, hovering);
			}
		}

		/// <summary>
		/// Remove an occupant from this tile.
		/// </summary>
		public void RemoveOccupant(Item item)
		{
			if (occupants.Contains(item))
			{
				item.SetHoveringFromTile(this, hovering);
				occupants.Remove(item);
			}
		}

		// ========================================================= Appearance =========================================================
		
		/// <summary>
		/// Add or remove the range display of this tile based on a single target tile. 
		/// </summary>
		public void UpdateAreaDisplayAs(object holder, DisplayType displayType, Tile targetTile)
		{
			if (targetTile == this)
			{
				ShowAreaDisplay(holder, displayType, true, TileRangeDisplay.Adj.None);
			}
			else
			{
				ShowAreaDisplay(holder, displayType, false);
			}
		}

		/// <summary>
		/// Add or remove the range display of this tile based on a set of target tiles. 
		/// </summary>
		public void UpdateAreaDisplayAs(object holder, DisplayType displayType, IEnumerable<Tile> targetTiles)
		{
			if (targetTiles.Contains(this))
			{
				// find all adjacencies of this tile
				TileRangeDisplay.Adj adjacencies = TileRangeDisplay.Adj.None;
				if (targetTiles.Any(x => x.BoardPos == (BoardPos + new Int2(-1, 1))))
					adjacencies |= TileRangeDisplay.Adj.TopLeft;
				if (targetTiles.Any(x => x.BoardPos == (BoardPos + new Int2(0, 1))))
					adjacencies |= TileRangeDisplay.Adj.Top;
				if (targetTiles.Any(x => x.BoardPos == (BoardPos + new Int2(1, 1))))
					adjacencies |= TileRangeDisplay.Adj.TopRight;
				if (targetTiles.Any(x => x.BoardPos == (BoardPos + new Int2(-1, 0))))
					adjacencies |= TileRangeDisplay.Adj.Left;
				if (targetTiles.Any(x => x.BoardPos == (BoardPos + new Int2(1, 0))))
					adjacencies |= TileRangeDisplay.Adj.Right;
				if (targetTiles.Any(x => x.BoardPos == (BoardPos + new Int2(-1, -1))))
					adjacencies |= TileRangeDisplay.Adj.BottomLeft;
				if (targetTiles.Any(x => x.BoardPos == (BoardPos + new Int2(0, -1))))
					adjacencies |= TileRangeDisplay.Adj.Bottom;
				if (targetTiles.Any(x => x.BoardPos == (BoardPos + new Int2(1, -1))))
					adjacencies |= TileRangeDisplay.Adj.BottomRight;

				ShowAreaDisplay(holder, displayType, true, adjacencies);
			}
			else
			{
				ShowAreaDisplay(holder, displayType, false);
			}
		}

		/// <summary>
		/// Remove the range display of this tile. 
		/// </summary>
		public void RemoveAreaDisplay(object holder, DisplayType displayType)
		{
			UpdateAreaDisplayAs(holder, displayType, (Tile)null);
		}

		/// <summary>
		/// Inner method for add or remove the range display of this tile. 
		/// </summary>
		private void ShowAreaDisplay(object holder, DisplayType displayType, bool show, TileRangeDisplay.Adj adjacencies = TileRangeDisplay.Adj.None)
		{
			if (show)
			{
				// check if there is already an entry of the same display type and holder
				RangeDisplayEntry entry = registeredDisplay[displayType].FirstOrDefault(x => x.holder == holder);
				if (entry == null)
				{
					// add new range
					TileRangeDisplay rangeDisplay = tileRanges.Find(x => !x.IsShowing());
					if (rangeDisplay == null)
					{
						rangeDisplay = Instantiate(tileRanges[0], tileRanges[0].transform.parent).GetComponent<TileRangeDisplay>();
						tileRanges.Add(rangeDisplay);
					}
					rangeDisplay.SetTileStyle(style);
					rangeDisplay.SetColor(style.frameColors[displayType], style.backgroundColors[displayType]);
					rangeDisplay.SetAdjancencies(adjacencies, style.dashed[displayType]);
					rangeDisplay.Show(true);
					registeredDisplay[displayType].Add(new RangeDisplayEntry(holder, rangeDisplay));
				}
				else
				{
					// modify existing range
					entry.rangeDisplay.SetTileStyle(style);
					entry.rangeDisplay.SetColor(style.frameColors[displayType], style.backgroundColors[displayType]);
					entry.rangeDisplay.SetAdjancencies(adjacencies, style.dashed[displayType]);
				}
			}
			else
			{
				// remove entry and return range to pool
				RangeDisplayEntry entry = registeredDisplay[displayType].FirstOrDefault(x => x.holder == holder);
				if (entry != null)
				{
					entry.rangeDisplay.Show(false);
					registeredDisplay[displayType].Remove(entry);
				}
			}

			// resolve area display ordering
			for (int i = 0; i < DisplayTypeCount; i++)
			{
				foreach (RangeDisplayEntry entry in registeredDisplay[(DisplayType)i])
				{
					entry.rangeDisplay.SetSpriteOrder(i + 1);
				}
			}
		}
	
		/// <summary>
		/// Show a display of the path on this tile.
		/// </summary>
		public void ShowPath(List<Tile> path)
		{
			int pos = path.IndexOf(this);
			if (pos != -1)
			{
				PathDirections pathDirections = new PathDirections(PathDirection.Start, PathDirection.End);
				if (pos == 0)
					pathDirections.from = PathDirection.Start;
				else if (path[pos - 1].BoardPos.x > BoardPos.x)
					pathDirections.from = PathDirection.Right;
				else if (path[pos - 1].BoardPos.z > BoardPos.z)
					pathDirections.from = PathDirection.Top;
				else if (path[pos - 1].BoardPos.x < BoardPos.x)
					pathDirections.from = PathDirection.Left;
				else if (path[pos - 1].BoardPos.z < BoardPos.z)
					pathDirections.from = PathDirection.Bottom;

				if (pos == path.Count - 1)
					pathDirections.to = PathDirection.End;
				else if (path[pos + 1].BoardPos.x > BoardPos.x)
					pathDirections.to = PathDirection.Right;
				else if (path[pos + 1].BoardPos.z > BoardPos.z)
					pathDirections.to = PathDirection.Top;
				else if (path[pos + 1].BoardPos.x < BoardPos.x)
					pathDirections.to = PathDirection.Left;
				else if (path[pos + 1].BoardPos.z < BoardPos.z)
					pathDirections.to = PathDirection.Bottom;

				pathRenderer.sprite = style.pathDirectionSprites[pathDirections];
			}
			else
			{
				pathRenderer.sprite = null;
			}
		}

		/// <summary>
		/// Hide the path display on the tile.
		/// </summary>
		public void HidePath()
		{
			pathRenderer.sprite = null;
		}

		/// <summary>
		/// Show a display of not reachable path on this tile.
		/// </summary>
		public void ShowInvalidPath()
		{
			pathRenderer.sprite = style.pathDirectionSprites[new PathDirections(PathDirection.End, PathDirection.Start)];
		}

		// ========================================================= Inqury =========================================================

		private readonly float tileDetectionPadding = 0.1f; // in percentage

		/// <summary>
		/// Check if an object is within the area of this tile. Estimate the object as position and circle size.
		/// </summary>
		public bool IsInTile(Vector3 position, float radius)
		{
			Vector3 localPosition = transform.InverseTransformPoint(position);
			if (Mathf.Abs(localPosition.x) > Board.tileSize * (0.5f - tileDetectionPadding) && Mathf.Abs(localPosition.z) > Board.tileSize * (0.5f - tileDetectionPadding))
			{
				return Vector2.SqrMagnitude(new Vector2(Mathf.Abs(localPosition.x), Mathf.Abs(localPosition.z)) - Vector2.one * Board.tileSize * (0.5f - tileDetectionPadding)) <= radius * radius;
			}
			else
			{
				return Mathf.Abs(localPosition.x) - Board.tileSize * (0.5f - tileDetectionPadding) <= radius &&
					Mathf.Abs(localPosition.z) - Board.tileSize * (0.5f - tileDetectionPadding) <= radius;
			}
		}

		/// <summary>
		/// Check if an object is within the area of this tile. Estimate the object as position and axis aligned rectangle size.
		/// </summary>
		public bool IsInTile(Vector3 position, Vector3 size)
		{
			Vector3 localPosition = transform.InverseTransformPoint(position);
			return Mathf.Abs(localPosition.x) - Board.tileSize * (0.5f - tileDetectionPadding) <= Mathf.Abs(size.x / 2) &&
				Mathf.Abs(localPosition.z) - Board.tileSize * (0.5f - tileDetectionPadding) <= Mathf.Abs(size.x / 2);	
		}
	}
}
