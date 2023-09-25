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
		public Image actionImage;
		public Image pointerImage;

		[Header("Data")]
		public UICursorIcons icons;

		// working variables
		private IconType iconType;
		private Sprite iconSprite;

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
			//Cursor.visible = false;
			SetIcon(IconType.None);
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
		/// Set the icon shown with the cursor.
		/// </summary>
		public void SetIcon(IconType type)
		{
			iconType = type;
			iconSprite = null;
		}

		public void SetIcon(Sprite sprite)
		{
			iconType = IconType.Custom;
			iconSprite = sprite;
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