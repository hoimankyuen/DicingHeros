using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
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
		public float tileSize = 1f;
		public List<Tile> connectedTiles = new List<Tile>();
		public Int2 boardPos = Int2.zero;
		public Vector3 worldPos = Vector3.zero;
		public bool active = false;

		[Header("Resources")]
		public TileStyle style = null;

		// reference
		private GameController game { get { return GameController.current; } }

		// component
		private List<TileRangeDisplay> tileRanges = new List<TileRangeDisplay>();
		private SpriteRenderer pathRenderer = null;
		private new Collider collider = null;

		// working variables
		private Dictionary<DisplayType, List<RangeDisplayEntry>> registeredDisplay = new Dictionary<DisplayType, List<RangeDisplayEntry>>();	
		private bool hovering = false;

		public IReadOnlyCollection<Item> Occupants
		{
			get
			{
				return occupants.AsReadOnly();
			}
		}
		private List<Item> occupants = new List<Item>();

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

			worldPos = transform.position;
			active = true;
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

		/// <summary>
		/// OnDrawGizmos is called when the game object is in editor mode
		/// </summary>
		private void OnDrawGizmos()
		{
			// draw size and each face of the die
			if (Application.isEditor)
			{
				// draw connections
			}
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
			displaySpriteRenderer.transform.localScale = new Vector3(tileSize / 1.28f, tileSize / 1.28f, 1f);

			TileRangeDisplay tileRange = transform.Find("Sprites/Ranges/Range").GetComponent<TileRangeDisplay>();
			tileRange.transform.localScale = new Vector3(tileSize / 1.28f, tileSize / 1.28f, 1f);

			SpriteRenderer pathSpriteRenderer = transform.Find("Sprites/Path").GetComponent<SpriteRenderer>();
			pathSpriteRenderer.transform.localScale = new Vector3(tileSize / 1.28f, tileSize / 1.28f, 1f);

			collider = transform.Find("Collider").GetComponent<Collider>();
			collider.transform.localScale = new Vector3(tileSize, 0.1f, tileSize);
		}

		#endif

		// ========================================================= Occupation =========================================================


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
		public void UpdateDisplayAs(object holder, DisplayType displayType, Tile targetTile)
		{
			if (targetTile == this)
			{
				Show(holder, displayType, true, TileRangeDisplay.Adj.None);
			}
			else
			{
				Show(holder, displayType, false);
			}
		}

		/// <summary>
		/// Add or remove the range display of this tile based on a set of target tiles. 
		/// </summary>
		public void UpdateDisplayAs(object holder, DisplayType displayType, IEnumerable<Tile> targetTiles)
		{
			if (targetTiles.Contains(this))
			{
				// find all adjacencies of this tile
				TileRangeDisplay.Adj adjacencies = TileRangeDisplay.Adj.None;
				if (targetTiles.Any(x => x.boardPos == (boardPos + new Int2(-1, 1))))
					adjacencies |= TileRangeDisplay.Adj.TopLeft;
				if (targetTiles.Any(x => x.boardPos == (boardPos + new Int2(0, 1))))
					adjacencies |= TileRangeDisplay.Adj.Top;
				if (targetTiles.Any(x => x.boardPos == (boardPos + new Int2(1, 1))))
					adjacencies |= TileRangeDisplay.Adj.TopRight;
				if (targetTiles.Any(x => x.boardPos == (boardPos + new Int2(-1, 0))))
					adjacencies |= TileRangeDisplay.Adj.Left;
				if (targetTiles.Any(x => x.boardPos == (boardPos + new Int2(1, 0))))
					adjacencies |= TileRangeDisplay.Adj.Right;
				if (targetTiles.Any(x => x.boardPos == (boardPos + new Int2(-1, -1))))
					adjacencies |= TileRangeDisplay.Adj.BottomLeft;
				if (targetTiles.Any(x => x.boardPos == (boardPos + new Int2(0, -1))))
					adjacencies |= TileRangeDisplay.Adj.Bottom;
				if (targetTiles.Any(x => x.boardPos == (boardPos + new Int2(1, -1))))
					adjacencies |= TileRangeDisplay.Adj.BottomRight;

				Show(holder, displayType, true, adjacencies);
			}
			else
			{
				Show(holder, displayType, false);
			}
		}

		/// <summary>
		/// Inner method for add or remove the range display of this tile. 
		/// </summary>
		private void Show(object holder, DisplayType displayType, bool show,  TileRangeDisplay.Adj adjacencies = TileRangeDisplay.Adj.None)
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
				ResolveRangeOrder();
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
		}

		/// <summary>
		/// Remove the range display of this tile. 
		/// </summary>
		public void RemoveDisplay(object holder, DisplayType displayType)
		{
			UpdateDisplayAs(holder, displayType, (Tile)null);
		}

		/// <summary>
		/// Change the sprite order of the tile range to make each elements stack ontop of each other in the propper way.
		/// </summary>
		private void ResolveRangeOrder()
		{
			for (int i = 0; i < DisplayTypeCount; i++)
			{
				DisplayType displayType = (DisplayType)i;
				foreach (RangeDisplayEntry entry in registeredDisplay[displayType])
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
				else if (path[pos - 1].boardPos.x > boardPos.x)
					pathDirections.from = PathDirection.Right;
				else if (path[pos - 1].boardPos.z > boardPos.z)
					pathDirections.from = PathDirection.Top;
				else if (path[pos - 1].boardPos.x < boardPos.x)
					pathDirections.from = PathDirection.Left;
				else if (path[pos - 1].boardPos.z < boardPos.z)
					pathDirections.from = PathDirection.Bottom;

				if (pos == path.Count - 1)
					pathDirections.to = PathDirection.End;
				else if (path[pos + 1].boardPos.x > boardPos.x)
					pathDirections.to = PathDirection.Right;
				else if (path[pos + 1].boardPos.z > boardPos.z)
					pathDirections.to = PathDirection.Top;
				else if (path[pos + 1].boardPos.x < boardPos.x)
					pathDirections.to = PathDirection.Left;
				else if (path[pos + 1].boardPos.z < boardPos.z)
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
			// wrong
			Vector3 localPosition = transform.InverseTransformPoint(position);
			if (Mathf.Abs(localPosition.x) > tileSize * (0.5f - tileDetectionPadding) && Mathf.Abs(localPosition.z) > tileSize * (0.5f - tileDetectionPadding))
			{
				return Vector2.SqrMagnitude(new Vector2(Mathf.Abs(localPosition.x), Mathf.Abs(localPosition.z)) - Vector2.one * tileSize * (0.5f - tileDetectionPadding)) <= radius * radius;
			}
			else
			{
				return Mathf.Abs(localPosition.x) - tileSize * (0.5f - tileDetectionPadding) <= radius &&
				Mathf.Abs(localPosition.z) - tileSize * (0.5f - tileDetectionPadding) <= radius;
			}
		}

		/// <summary>
		/// Check if an object is within the area of this tile. Estimate the object as position and axis aligned rectangle size.
		/// </summary>
		public bool IsInTile(Vector3 position, Vector3 size)
		{
			/*
			return !(position.x - size.x * 0.5f < transform.position.x + tileSize * (0.5f - tileDetectionPadding) ||
				position.x + size.x * 0.5f > transform.position.x - tileSize * (0.5f - tileDetectionPadding) ||
				position.z - size.z * 0.5f < transform.position.z + tileSize * (0.5f - tileDetectionPadding) ||
				position.z + size.z * 0.5f > transform.position.z - tileSize * (0.5f - tileDetectionPadding));
			*/
			Vector3 localPosition = transform.InverseTransformPoint(position);
			return Mathf.Abs(localPosition.x) - tileSize * (0.5f - tileDetectionPadding) <= Mathf.Abs(size.x / 2) &&
				Mathf.Abs(localPosition.z) - tileSize * (0.5f - tileDetectionPadding) <= Mathf.Abs(size.x / 2);	
		}
	}
}
