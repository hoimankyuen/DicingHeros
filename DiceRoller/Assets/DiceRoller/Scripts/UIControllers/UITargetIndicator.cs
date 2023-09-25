using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DiceRoller
{
    public class UITargetIndicator : MonoBehaviour
    {
		public enum IconType
		{
			None,
			SimpleMovement,
			SimpleMelee,
			SimpleRanged,
		}

		[System.Serializable]
		public enum Mode
		{
			None,
			Die,
			Unit,
			Tile,
			Mouse,
		}

		[Header("Components")]
		public Image actionImage;
		public Image pointerImage;

		[Header("Data")]
		public UICursorIcons icons;

		// working variables
		public Mode mode = Mode.None;
		public Item targetItem = null;
		public Tile targetTile = null;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		private void Awake()
		{
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
			UpdatePositionApparence();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
		}

		// ========================================================= Behaviour Methods =========================================================

		/// <summary>
		/// Setup the target indicator to act and show a certain predefined way.
		/// </summary>
		public void Setup(Mode mode, UICursor.IconType iconType)
		{
			this.mode = mode;
			actionImage.sprite = icons.icons[iconType];
		}
		
		/// <summary>
		/// Update the position and apparence of the target indicator to make it appears on the correct position.
		/// </summary>
		private void UpdatePositionApparence()
		{
			switch (mode)
			{
				case Mode.None:
					actionImage.gameObject.SetActive(false);
					pointerImage.gameObject.SetActive(false);
					break;

				case Mode.Die:
					// retrieve target
					targetItem = Die.GetFirstBeingInspected();

					// only do if an item is selected
					if (targetItem != null)
					{
						// move the pointer to the top of the item
						Vector3 pos = Vector3.zero;
						pos.x = (Camera.main.WorldToScreenPoint(targetItem.transform.position).x + Camera.main.WorldToScreenPoint(targetItem.transform.position + Vector3.up * targetItem.height).x) / 2f;
						pos.y = Camera.main.WorldToScreenPoint(targetItem.transform.position + Vector3.up * targetItem.height).y;
						transform.position = pos;
					}

					actionImage.gameObject.SetActive(targetItem != null);
					pointerImage.gameObject.SetActive(targetItem != null);
					break;

				case Mode.Unit:
					// retrieve target
					targetItem = Unit.GetFirstBeingInspected();

					// only do if an item is selected
					if (targetItem != null)
					{
						// move the pointer to the top of the item
						Vector3 pos = Vector3.zero;
						pos.x = (Camera.main.WorldToScreenPoint(targetItem.transform.position).x + Camera.main.WorldToScreenPoint(targetItem.transform.position + Vector3.up * targetItem.height).x) / 2f;
						pos.y = Camera.main.WorldToScreenPoint(targetItem.transform.position + Vector3.up * targetItem.height).y;
						transform.position = pos;
					}

					actionImage.gameObject.SetActive(targetItem != null);
					pointerImage.gameObject.SetActive(targetItem != null);
					break;

				case Mode.Tile:
					// retrieve target
					targetTile = Board.current.HoveringTile;

					// only do if a tile is selected
					if (targetTile != null)
					{
						// move the pointer to the top of the tile
						Vector3 pos = Vector3.zero;
						pos = Camera.main.WorldToScreenPoint(targetTile.transform.position);
						transform.position = pos;
					}

					actionImage.gameObject.SetActive(targetTile != null);
					pointerImage.gameObject.SetActive(targetTile != null);
					break;

				case Mode.Mouse:
					// move the pointer to mouse point
					transform.position = Input.mousePosition;

					actionImage.gameObject.SetActive(!EventSystem.current.IsPointerOverGameObject());
					pointerImage.gameObject.SetActive(!EventSystem.current.IsPointerOverGameObject());
					break;
			}
		}
	}
}