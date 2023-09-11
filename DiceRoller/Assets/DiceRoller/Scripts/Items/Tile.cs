using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
	public class Tile : MonoBehaviour
	{
		public enum DisplayType
		{
			Normal,

			Move,
			Attack,

			MoveTarget,
			AttackTarget,

			EnemyPosition,
			FriendPosition,
			SelfPosition,
		} // order of enum denotes display priority, where larger means higher priority
		public static readonly int DisplayTypeCount = 8;

		public enum PathDirection
		{
			Start,
			Right,
			Front,
			Left,
			Back,
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

		public static List<Tile> EmptyTiles { get; protected set; } = new List<Tile>();

		// parameters
		[HideInInspector]
		public float tileSize = 1f;
		[HideInInspector]
		public List<Tile> connectedTiles = new List<Tile>();
		[HideInInspector]
		public Int2 boardPos = Int2.zero;

		public TileStyle style = null;

		// reference
		protected GameController Game { get { return GameController.current; } }

		// component
		protected SpriteRenderer displaySpriteRenderer = null;
		protected SpriteRenderer pathSpriteRenderer = null;
		protected new Collider collider = null;

		// working variables
		protected Dictionary<DisplayType, HashSet<object>> registeredDisplay = new Dictionary<DisplayType, HashSet<object>>();
		protected bool isHovering = false;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		void Awake()
		{
			displaySpriteRenderer = transform.Find("Model/DisplaySprite").GetComponent<SpriteRenderer>();
			pathSpriteRenderer = transform.Find("Model/PathSprite").GetComponent<SpriteRenderer>();
			collider = transform.Find("Collider").GetComponent<Collider>();

			for (int i = 0; i < DisplayTypeCount; i++)
			{
				registeredDisplay[(DisplayType)i] = new HashSet<object>();
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
		}

		/// <summary>
		/// OnValidate is called when any inspector value is changed.
		/// </summary>
		void OnValidate()
		{

		}

		/// <summary>
		/// OnMouseEnter is called when the mouse is start pointing to the game object.
		/// </summary>
		void OnMouseEnter()
		{
		}

		/// <summary>
		/// OnMouseExit is called when the mouse is stop pointing to the game object.
		/// </summary>
		void OnMouseExit()
		{
		}

		/// <summary>
		/// OnMouseDown is called when a mouse button is pressed when pointing to the game object.
		/// </summary>
		void OnMouseDown()
		{

		}

		// ========================================================= Editor =========================================================

		/// <summary>
		/// Regenerate all components related to this tile. Should only be called in editor.
		/// </summary>
		public void RegenerateTile()
		{
			SpriteRenderer displaySpriteRenderer = transform.Find("Model/DisplaySprite").GetComponent<SpriteRenderer>();
			displaySpriteRenderer.transform.localScale = new Vector3(tileSize / 1.28f, tileSize / 1.28f, 1f);
			SpriteRenderer pathSpriteRenderer = transform.Find("Model/PathSprite").GetComponent<SpriteRenderer>();
			pathSpriteRenderer.transform.localScale = new Vector3(tileSize / 1.28f, tileSize / 1.28f, 1f);

			collider = transform.Find("Collider").GetComponent<Collider>();
			collider.transform.localScale = new Vector3(tileSize, 0.1f, tileSize);
		}

		// ========================================================= Appearance =========================================================

		/// <summary>
		/// Register a particular display to this tile by any object.
		/// </summary>
		public void AddDisplay(object o, DisplayType displayType)
		{
			registeredDisplay[displayType].Add(o);
			ResolveDisplay();
		}

		/// <summary>
		/// Deregister a particular display from this tile by any object.
		/// </summary>
		public void RemoveDisplay(object o, DisplayType displayType)
		{
			registeredDisplay[displayType].Remove(o);
			ResolveDisplay();
		}

		/// <summary>
		/// Change the apparence of this tile according to all display registers.
		/// </summary>
		protected void ResolveDisplay()
		{
			for (int i = DisplayTypeCount - 1; i >= 0; i--)
			{
				DisplayType displayType = (DisplayType)i;
				if (registeredDisplay[displayType].Count > 0 || displayType == DisplayType.Normal)
				{
					displaySpriteRenderer.sprite = style.visualSprites[displayType];
					break;
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
					pathDirections.from = PathDirection.Front;
				else if (path[pos - 1].boardPos.x < boardPos.x)
					pathDirections.from = PathDirection.Left;
				else if (path[pos - 1].boardPos.z < boardPos.z)
					pathDirections.from = PathDirection.Back;

				if (pos == path.Count - 1)
					pathDirections.to = PathDirection.End;
				else if (path[pos + 1].boardPos.x > boardPos.x)
					pathDirections.to = PathDirection.Right;
				else if (path[pos + 1].boardPos.z > boardPos.z)
					pathDirections.to = PathDirection.Front;
				else if (path[pos + 1].boardPos.x < boardPos.x)
					pathDirections.to = PathDirection.Left;
				else if (path[pos + 1].boardPos.z < boardPos.z)
					pathDirections.to = PathDirection.Back;

				pathSpriteRenderer.sprite = style.pathDirectionSprites[pathDirections];
			}
			else
			{
				pathSpriteRenderer.sprite = null;
			}
		}

		/// <summary>
		/// Hide the path display on the tile.
		/// </summary>
		public void HidePath()
		{
			pathSpriteRenderer.sprite = null;
		}

		/// <summary>
		/// Show a display of not reachable path on this tile.
		/// </summary>
		public void ShowInvalidPath()
		{
			pathSpriteRenderer.sprite = style.pathDirectionSprites[new PathDirections(PathDirection.End, PathDirection.Start)];
		}


		/// <summary>
		/// Hide the invalid path display on the tile.
		/// </summary>
		public void HideInvalidPath()
		{
			pathSpriteRenderer.sprite = null;
		}

		// ========================================================= Inqury =========================================================

		/// <summary>
		/// Check if an object is within the area of this tile. Estimate the object as position and square size.
		/// </summary>
		public bool IsInTile(Vector3 position, float size)
		{
			Vector3 localPosition = transform.InverseTransformPoint(position);
			return Mathf.Abs(localPosition.x) - Mathf.Abs(size / 2) < tileSize / 2 && Mathf.Abs(localPosition.z) - Mathf.Abs(size / 2) < tileSize / 2;
		}
	}
}
