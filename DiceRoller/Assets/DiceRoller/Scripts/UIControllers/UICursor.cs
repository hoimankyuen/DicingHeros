using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DiceRoller
{
    public class UICursor : MonoBehaviour
    {
		public enum IconType
		{
			None,
			SimpleMovement,
			SimpleMelee,
			SimpleRanged,
			Custom,
		}

		[Header("Components")]
		public Image unitIcon = null;
		public UIDie dieIcon = null;
		public Image actionImage = null;
		public Image pointerImage = null;

		[Header("Data")]
		public UICursorIcons icons = null;

		[Header("Settings")]
		public bool hideCursorAtStart = false;

		// working variables
		private IconType iconType = IconType.None;
		private Sprite iconSprite = null;
		private Item draggingItem = null;

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
			dieIcon.SetAsCursor();

			if (hideCursorAtStart)
			{
				Cursor.visible = false;
			}
			
			SetIcon(IconType.None);
			SetDraggingItem(null);

			Unit.OnItemBeingDraggedChanged += UpdateDraggingItem;
			Die.OnItemBeingDraggedChanged += UpdateDraggingItem;
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
			Unit.OnItemBeingDraggedChanged -= UpdateDraggingItem;
			Die.OnItemBeingDraggedChanged -= UpdateDraggingItem;
		}

		// ========================================================= Behaviour Methods =========================================================

		/// <summary>
		/// Set the icon shown with the cursor.
		/// </summary>
		public void SetIcon(IconType type)
		{
			iconType = type;
			iconSprite = null;
		}

		/// <summary>
		/// Set the icon shown with the cursor.
		/// </summary>
		public void SetIcon(Sprite sprite)
		{
			iconType = IconType.Custom;
			iconSprite = sprite;
		}

		private void UpdateDraggingItem()
		{
			if (Unit.GetFirstBeingDragged() != null)
			{
				SetDraggingItem(Unit.GetFirstBeingDragged());
			}
			else if (Die.GetFirstBeingDragged() != null)
			{
				SetDraggingItem(Die.GetFirstBeingDragged());
			}
			else
			{
				SetDraggingItem(null);
			}
		}

		/// <summary>
		/// Set the dragging item by this cursor.
		/// </summary>
		public void SetDraggingItem(Item item)
		{
			draggingItem = item;
			if (draggingItem == null)
			{
				unitIcon.gameObject.SetActive(false);
				dieIcon.gameObject.SetActive(false);
				unitIcon.sprite = null;
				dieIcon.SetDisplayedValue(Die.Type.Unknown, 20);
			}
			else if (draggingItem is Unit)
			{
				unitIcon.gameObject.SetActive(true);
				dieIcon.gameObject.SetActive(false);
				unitIcon.sprite = draggingItem.iconSprite;
				dieIcon.SetDisplayedValue(Die.Type.Unknown, 20);
			}
			else if (draggingItem is Die)
			{
				unitIcon.gameObject.SetActive(false);
				dieIcon.gameObject.SetActive(true);
				unitIcon.sprite = null;
				dieIcon.SetInspectingTarget(draggingItem as Die);

			}
		}

		/// <summary>
		/// Update the position and apparence of the target indicator to make it appears on the correct position.
		/// </summary>
		private void UpdatePositionApparence()
		{
			// move the pointer to mouse point
			transform.position = Input.mousePosition;

			if (EventSystem.current.IsPointerOverGameObject())
			{
				// show standard cursor when pointing on UI
				pointerImage.sprite = icons.normalCursor;
				actionImage.enabled = false;
			}
			else
			{
				// show selected icon otherwise
				pointerImage.sprite = iconType == IconType.None ? icons.normalCursor : icons.iconCursor;
				actionImage.enabled = iconType != IconType.None;
				actionImage.sprite = iconType == IconType.Custom ? iconSprite : icons.icons[iconType];
			}
		}
	}
}