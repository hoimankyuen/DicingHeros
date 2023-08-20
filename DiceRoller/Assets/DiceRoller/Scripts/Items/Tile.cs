using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public class Tile : MonoBehaviour
	{
		public enum DisplayType
		{
			Normal,
			Position,
			Attack,
			AttackTarget,
			Move,
			MoveTarget,
		}

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
		protected HashSet<object> registeredPositionDisplay = new HashSet<object>();
		protected HashSet<object> registeredMoveDisplay = new HashSet<object>();
		protected HashSet<object> registeredMoveTargetDisplay = new HashSet<object>();
		protected HashSet<object> registeredAttackDisplay = new HashSet<object>();
		protected HashSet<object> registeredAttackTargetDisplay = new HashSet<object>();

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
			switch (displayType)
			{
				case DisplayType.Position:
					registeredPositionDisplay.Add(o);
					break;
				case DisplayType.Attack:
					registeredAttackDisplay.Add(o);
					break;
				case DisplayType.AttackTarget:
					registeredAttackTargetDisplay.Add(o);
					break;
				case DisplayType.Move:
					registeredMoveDisplay.Add(o);
					break;
				case DisplayType.MoveTarget:
					registeredMoveTargetDisplay.Add(o);
					break;
			}
			ResolveDisplay();
		}

		/// <summary>
		/// Deregister a particular display from this tile by any object.
		/// </summary>
		public void RemoveDisplay(object o, DisplayType displayType)
		{
			switch (displayType)
			{
				case DisplayType.Position:
					registeredPositionDisplay.Remove(o);
					break;
				case DisplayType.Attack:
					registeredAttackDisplay.Remove(o);
					break;
				case DisplayType.AttackTarget:
					registeredAttackTargetDisplay.Remove(o);
					break;
				case DisplayType.Move:
					registeredMoveDisplay.Remove(o);
					break;
				case DisplayType.MoveTarget:
					registeredMoveTargetDisplay.Remove(o);
					break;
			}
			ResolveDisplay();
		}

		/// <summary>
		/// Change the apparence of this tile according to all display registers.
		/// </summary>
		protected void ResolveDisplay()
		{
			if (registeredPositionDisplay.Count > 0)
				displaySpriteRenderer.sprite = style.visualSprites[DisplayType.Position];

			else if (registeredAttackTargetDisplay.Count > 0)
				displaySpriteRenderer.sprite = style.visualSprites[DisplayType.AttackTarget];

			else if (registeredAttackDisplay.Count > 0)
				displaySpriteRenderer.sprite = style.visualSprites[DisplayType.Attack];

			else if (registeredMoveTargetDisplay.Count > 0)
				displaySpriteRenderer.sprite = style.visualSprites[DisplayType.MoveTarget];

			else if (registeredMoveDisplay.Count > 0)
				displaySpriteRenderer.sprite = style.visualSprites[DisplayType.Move];

			else
				displaySpriteRenderer.sprite = style.visualSprites[DisplayType.Normal];
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
