using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DicingHeros
{
    public class UICursor : MonoBehaviour
    {
		public enum IconType
		{
			None,
			Action,
			Inspect,
			Pick,
			Throw,
			Movement,
			PhysicalAttack,
			RangedAttack,
			MagicalAttack,
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

		// reference
		private GameController game => GameController.current;
		private StateMachine stateMachine => StateMachine.current;
		private Board board => Board.current;
		private DiceThrower diceThrower => DiceThrower.current;

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

			ShowCursor(true, null);
			SetDraggingItem(null);

			//Unit.OnItemBeingDraggedChanged += UpdateDraggingItem;
			Die.OnAnyBeingDraggedChanged += UpdateDraggingItem;
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		private void Update()
		{
			UpdatePosition();
			UpdateApparence();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
			//Unit.OnItemBeingDraggedChanged -= UpdateDraggingItem;
			Die.OnAnyBeingDraggedChanged -= UpdateDraggingItem;
		}

		// ========================================================= Icons =========================================================

		/*
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
		*/

		// ========================================================= Dragging =========================================================

		/// <summary>
		/// Change the dragging item to what is currently being dragged.
		/// </summary>
		private void UpdateDraggingItem()
		{
			/*
			if (Unit.GetFirstBeingDragged() != null)
			{
				SetDraggingItem(Unit.GetFirstBeingDragged());
			}
			*/
			if (Die.GetFirstBeingDragged() != null)
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
		private void SetDraggingItem(Item item)
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

		// ========================================================= Position =========================================================

		/// <summary>
		/// Update the position and apparence of the target indicator to make it appears on the correct position.
		/// </summary>
		private void UpdatePosition()
		{
			transform.position = InputUtils.MousePosition;
		}

		// ========================================================= Apparence =========================================================

		/// <summary>
		/// Update the position and apparence of the target indicator to make it appears on the correct position.
		/// </summary>
		private void UpdateApparence()
		{
			if (GameController.current.PersonInControl != GameController.Person.AI)
			{
				// player is in control
				if (EventSystem.current.IsPointerOverGameObject())
				{
					// always show standard cursor when pointing on UI
					ShowCursor(true, null);
				}
				else
				{
					Die inspectingDie = Die.GetFirstBeingInspected();
					Unit inspectingUnit = Unit.GetFirstBeingInspected();
					Unit selectedUnit = Unit.GetFirstSelected();

					// show selected icon otherwise
					switch (stateMachine.CurrentState)
					{
						case SMState.Navigation:
							if (inspectingDie != null)
							{
								ShowCursor(true, icons.pick);
							}
							else if (inspectingUnit != null)
							{
								if (inspectingUnit.Player == game.CurrentPlayer)
								{
									if (inspectingUnit.CurrentUnitState == Unit.UnitState.Standby || inspectingUnit.CurrentUnitState == Unit.UnitState.Moved)
									{
										ShowCursor(true, icons.action);
									}
									else
									{
										ShowCursor(true, icons.inspect);
									}
								}
								else
								{
									ShowCursor(true, icons.inspect);
								}
							}
							else
							{
								ShowCursor(true, null);
							}
							break;

						case SMState.UnitMoveSelect:
							if (board.HoveringTile != null)
							{
								ShowCursor(true, icons.movement);
							}
							else
							{
								ShowCursor(true, null);
							}
							break;

						case SMState.UnitAttackSelect:
							if (inspectingUnit != null)
							{
								if (selectedUnit.CurrentAttackType == Unit.AttackType.Physical)
								{
									if (selectedUnit.PhysicalRange <= 1)
									{
										ShowCursor(true, icons.physicalAttack);
									}
									else
									{
										ShowCursor(true, icons.rangedAttack);
									}
								}
								else if (selectedUnit.CurrentAttackType == Unit.AttackType.Magical)
								{
									ShowCursor(true, icons.magicalAttack);
								}
							}
							else
							{
								ShowCursor(true, null);
							}
							break;

						case SMState.DiceActionSelect:
							if (inspectingUnit != null && !diceThrower.ThrowDragging)
							{
								ShowCursor(true, icons.pick);
							}
							else if (diceThrower.AtThrowableSurface && !diceThrower.ThrowDragging)
							{
								ShowCursor(true, icons.throws);
							}
							else
							{
								ShowCursor(true, null);
							}
							break;

						default:
							ShowCursor(true, null);
							break;
					}
				}
			}
			else
			{
				// ai is in control
				ShowCursor(false);
			}
		}

		/// <summary>
		/// Show or hide a cursor with a information icon.
		/// </summary>
		private void ShowCursor(bool show, Sprite icon = null)
		{
			if (show == pointerImage.enabled && icon == pointerImage.sprite)
				return;

			if (show)
			{
				if (icon != null)
				{
					pointerImage.enabled = true;
					pointerImage.sprite = icons.iconCursor;
					actionImage.enabled = true;
					actionImage.sprite = icon;
				}
				else
				{
					pointerImage.enabled = true;
					pointerImage.sprite = icons.normalCursor;
					actionImage.enabled = false;
					actionImage.sprite = null;
				}
			}
			else
			{
				pointerImage.enabled = false;
				actionImage.enabled = false;
				actionImage.sprite = null;
			}
		}
	}
}