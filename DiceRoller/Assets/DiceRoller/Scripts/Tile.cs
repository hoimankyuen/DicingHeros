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


		// parameters
		public float tileSize = 1f;
		[HideInInspector]
		public List<Tile> connectedTiles = new List<Tile>();

		[Header("Tile Sprites")]
		public Sprite normalSprite = null;
		public Sprite positionSprite = null;
		public Sprite attackSprite = null;
		public Sprite moveSprite = null;

		[Header("Path Sprites")]
		public Sprite pathStartEnd = null;
		public Sprite pathStartRight = null;
		public Sprite pathStartFront = null;
		public Sprite pathStartLeft = null;
		public Sprite pathStartBack = null;
		public Sprite pathRightLeft = null;
		public Sprite pathFrontBack = null;
		public Sprite pathFrontRight = null;
		public Sprite pathBackRight = null;
		public Sprite pathFrontLeft = null;
		public Sprite pathBackLeft = null;
		public Sprite pathEndRight = null;
		public Sprite pathEndFront = null;
		public Sprite pathEndLeft = null;
		public Sprite pathEndBack = null;

		// reference
		protected GameController Game { get { return GameController.Instance; } }

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
				displaySpriteRenderer.sprite = positionSprite;

			else if (registeredAttackTargetDisplay.Count > 0)
				displaySpriteRenderer.sprite = attackSprite;

			else if (registeredAttackDisplay.Count > 0)
				displaySpriteRenderer.sprite = positionSprite;

			else if (registeredMoveTargetDisplay.Count > 0)
				displaySpriteRenderer.sprite = positionSprite;

			else if (registeredMoveDisplay.Count > 0)
				displaySpriteRenderer.sprite = moveSprite;

			else
				displaySpriteRenderer.sprite = normalSprite;
		}

		protected enum PathDir
		{
			Start,
			Right,
			Front,
			Left,
			Back,
			End,
		}

		/// <summary>
		/// Show a display of the path on this tile.
		/// </summary>
		public void ShowPath(List<Tile> path)
		{
			int pos = path.IndexOf(this);
			if (pos != -1)
			{
				PathDir from = PathDir.Start;
				if (pos == 0)
					from = PathDir.Start;
				else if (transform.InverseTransformPoint(path[pos - 1].transform.position).x > tileSize / 2)
					from = PathDir.Right;
				else if (transform.InverseTransformPoint(path[pos - 1].transform.position).z > tileSize / 2)
					from = PathDir.Front;
				else if (transform.InverseTransformPoint(path[pos - 1].transform.position).x < -tileSize / 2)
					from = PathDir.Left;
				else if (transform.InverseTransformPoint(path[pos - 1].transform.position).z < -tileSize / 2)
					from = PathDir.Back;

				PathDir to = PathDir.End;
				if (pos == path.Count - 1)
					to = PathDir.End;
				else if (transform.InverseTransformPoint(path[pos + 1].transform.position).x > tileSize / 2)
					to = PathDir.Right;
				else if (transform.InverseTransformPoint(path[pos + 1].transform.position).z > tileSize / 2)
					to = PathDir.Front;
				else if (transform.InverseTransformPoint(path[pos + 1].transform.position).x < -tileSize / 2)
					to = PathDir.Left;
				else if (transform.InverseTransformPoint(path[pos + 1].transform.position).z < -tileSize / 2)
					to = PathDir.Back;

				if (from == PathDir.Start && to == PathDir.End)
					pathSpriteRenderer.sprite = pathStartEnd;
				else if (from == PathDir.Start && to == PathDir.Right)
					pathSpriteRenderer.sprite = pathStartRight;
				else if (from == PathDir.Start && to == PathDir.Front)
					pathSpriteRenderer.sprite = pathStartFront;
				else if (from == PathDir.Start && to == PathDir.Left)
					pathSpriteRenderer.sprite = pathStartLeft;
				else if (from == PathDir.Start && to == PathDir.Back)
					pathSpriteRenderer.sprite = pathStartBack;
				else if (from == PathDir.Left && to == PathDir.Right || from == PathDir.Right && to == PathDir.Left)
					pathSpriteRenderer.sprite = pathRightLeft;
				else if (from == PathDir.Front && to == PathDir.Back || from == PathDir.Back && to == PathDir.Front)
					pathSpriteRenderer.sprite = pathFrontBack;
				else if (from == PathDir.Front && to == PathDir.Right || from == PathDir.Right && to == PathDir.Front)
					pathSpriteRenderer.sprite = pathFrontRight;
				else if (from == PathDir.Back && to == PathDir.Right || from == PathDir.Right && to == PathDir.Back)
					pathSpriteRenderer.sprite = pathBackRight;
				else if (from == PathDir.Front && to == PathDir.Left || from == PathDir.Left && to == PathDir.Front)
					pathSpriteRenderer.sprite = pathFrontLeft;
				else if (from == PathDir.Back && to == PathDir.Left || from == PathDir.Left && to == PathDir.Back)
					pathSpriteRenderer.sprite = pathBackLeft;
				else if (from == PathDir.Right && to == PathDir.End)
					pathSpriteRenderer.sprite = pathEndRight;
				else if (from == PathDir.Front && to == PathDir.End)
					pathSpriteRenderer.sprite = pathEndFront;
				else if (from == PathDir.Left && to == PathDir.End)
					pathSpriteRenderer.sprite = pathEndLeft;
				else if (from == PathDir.Back && to == PathDir.End)
					pathSpriteRenderer.sprite = pathEndBack;
				else
					pathSpriteRenderer.sprite = null;

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
