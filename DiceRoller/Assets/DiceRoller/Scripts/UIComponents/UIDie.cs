using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DiceRoller
{
	public class UIDie : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		public enum Mode
		{
			Default,
			Equipment,
			Cursor,	
		}


		[Header("Data")]
		public UIDieIcons defaultDieIcons;
		public UIItemStateIcons itemStateIcons;

		[Header("Components")]
		public Image iconImage;
		public Image outlineImage;
		public Image overlayImage;
		public Image stateImage;
		public Image rollingImage;
		public TextMeshProUGUI valueText;

		[Header("Settings")]
		[SerializeField]
		protected bool displayStatus;

		[Header("Displayed Values")]
		[SerializeField]
		protected Die.Type type;
		[SerializeField]
		protected int value;
		[SerializeField]
		protected EquipmentDieSlot.Requirement requirement;

		// working variables
		protected Mode mode;
		protected Die die;
		protected UIEquipmentDieSlot dieSlot;
		protected Coroutine rollingValueCoroutine;
		protected bool rolling = false;

		protected bool pointerEntered = false;

		public RectTransform rectTransform => GetComponent<RectTransform>();

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected void Start()
		{
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected void OnDestroy()
		{
			// deregister all events
			if (die != null)
			{
				die.onValueChanged -= RefreshDisplay;
				die.onDieStateChanged -= RefreshDisplay;
				die.onStatusChanged -= RefreshDisplay;
				Die.onInspectionChanged -= RefreshDisplay;
				Die.onSelectionChanged -= RefreshDisplay;
			}

			// stop running animations
			if (rollingValueCoroutine != null)
			{
				StopCoroutine(rollingValueCoroutine);
			}
		}

		/// <summary>
		/// OnValidate is called when values where changed in the inspector.
		/// </summary>
		protected void OnValidate()
		{
			RefreshDisplay();
		}

		// ========================================================= Mouse Event Handler ========================================================

		/// <summary>
		/// Callback triggered by mouse enter from Event System.
		/// </summary>
		public void OnPointerEnter(PointerEventData eventData)
		{
			if ((mode == Mode.Default || mode == Mode.Equipment) && die != null)
				die.OnUIMouseEnter();
			if (mode == Mode.Equipment && dieSlot != null)
				dieSlot.OnPointerEnter(eventData);

			pointerEntered = true;
		}

		/// <summary>
		/// Callback triggered by mouse exit from Event System.
		/// </summary>
		public void OnPointerExit(PointerEventData eventData)
		{
			if ((mode == Mode.Default || mode == Mode.Equipment) && die != null)
				die.OnUIMouseExit();
			if (mode == Mode.Equipment && dieSlot != null)
				dieSlot.OnPointerExit(eventData);

			pointerEntered = false;
		}

		/// <summary>
		/// Callback triggered by mouse button down from Event System.
		/// </summary>
		public void OnPointerDown(PointerEventData eventData)
		{
			if ((mode == Mode.Default || mode == Mode.Equipment) && die != null)
				die.OnUIMouseDown((int)eventData.button);
			if (mode == Mode.Equipment && dieSlot != null)
				dieSlot.OnPointerDown(eventData);
		}

		/// <summary>
		/// Callback triggered by mouse button up from Event System.
		/// </summary>
		public void OnPointerUp(PointerEventData eventData)
		{
			if ((mode == Mode.Default || mode == Mode.Equipment) && die != null)
				die.OnUIMouseUp((int)eventData.button);
			if (mode == Mode.Equipment && dieSlot != null)
				dieSlot.OnPointerUp(eventData);
		}

		// ========================================================= UI Methods =========================================================

		/// <summary>
		/// Set this ui die to be a part of the equipment ui, allowing information pass back to the equipment ui.
		/// </summary>
		public void SetAsEquipmentSlots(UIEquipmentDieSlot dieSlot)
		{
			mode = Mode.Equipment;
			this.dieSlot = dieSlot;
		}

		/// <summary>
		/// Set this ui die to be a part of the cursor ui.
		/// </summary>
		public void SetAsCursor()
		{
			mode = Mode.Cursor;
			foreach (Graphic graphic in transform.GetComponentsInChildren<Graphic>())
			{
				graphic.raycastTarget = false;
			}
		}

		/// <summary>
		/// Set the displayed information as an existing die.
		/// </summary>
		public void SetInspectingTarget(Die die)
		{
			SetTargetOrValue(die, type, value, requirement);
		}

		/// <summary>
		/// Set the displayed information as some preset value.
		/// </summary>
		public void SetDisplayedValue(Die.Type type, int value)
		{
			SetTargetOrValue(null, type, value, EquipmentDieSlot.Requirement.None);
		}

		/// <summary>
		/// Set the displayed information as some preset value.
		/// </summary>
		public void SetDisplayedValue(Die.Type type, int value, EquipmentDieSlot.Requirement requirement)
		{
			SetTargetOrValue(null, type, value, requirement);
		}

		/// <summary>
		/// Combined method of either setting the inspecting target die, or some preset value.
		/// </summary>
		protected void SetTargetOrValue(Die die, Die.Type type, int value, EquipmentDieSlot.Requirement requirement)
		{
			// prevent excessive calls
			if (die != null && this.die == die)
				return;

			// register and deregister callbacks, also trigger the missing pointer exit and enter
			if (this.die != null)
			{
				if (pointerEntered)
				{
					if ((mode == Mode.Default || mode == Mode.Equipment) && this.die != null)
						this.die.OnUIMouseExit();
					if (mode == Mode.Equipment && dieSlot != null)
						dieSlot.OnPointerExit(null);
				}

				this.die.onValueChanged -= RefreshDisplay;
				this.die.onDieStateChanged -= RefreshDisplay;
				this.die.onStatusChanged -= RefreshDisplay;
				Die.onInspectionChanged -= RefreshDisplay;
				Die.onSelectionChanged -= RefreshDisplay;
			}
			if (die != null)
			{
				if (pointerEntered)
				{
					if ((mode == Mode.Default || mode == Mode.Equipment) && die != null)
						die.OnUIMouseEnter();
					if (mode == Mode.Equipment && dieSlot != null)
						dieSlot.OnPointerEnter(null);
				}

				die.onValueChanged += RefreshDisplay;
				die.onDieStateChanged += RefreshDisplay;
				die.onStatusChanged += RefreshDisplay;
				Die.onInspectionChanged += RefreshDisplay;
				Die.onSelectionChanged += RefreshDisplay;
			}

			// stop running animations
			rolling = false;

			// set values
			this.die = die;
			this.type = type;
			this.value = value;
			this.requirement = requirement;
			RefreshDisplay();
		}


		/// <summary>
		/// Change the current display of this ui element to either match the information of the inspecting object, or match the input values.
		/// </summary>
		protected void RefreshDisplay()
		{
			if (die != null)
			{
				// change die icon
				iconImage.sprite = die.iconSprite;
				outlineImage.sprite = die.outlineSprite;
				outlineImage.enabled = displayStatus && die.IsSelected;
				overlayImage.sprite = die.overlaySprite;
				overlayImage.enabled = displayStatus && die.IsBeingInspected;

				// change value text
				valueText.text = die.Value == -1 ? "?" : die.Value.ToString();

				// start or stop rolling value animation if die value is invalid
				if (die.Value == -1)
				{
					if (!rolling)
					{
						rolling = true;
						if (gameObject.activeInHierarchy)
						{
							StartCoroutine(RollingValueAnimation(die));
						}
					}
				}
				else
				{
					rolling = false;
				}

				// change state icon
				stateImage.enabled = displayStatus && !rolling && die.CurrentDieState != Die.DieState.Normal;
				stateImage.sprite = itemStateIcons.stateIcons[die.CurrentDieState];
			}
			else
			{
				// change die icon
				if (defaultDieIcons.dieIcons.ContainsKey(type))
				{
					iconImage.sprite = defaultDieIcons.dieIcons[type];
					outlineImage.sprite = defaultDieIcons.dieOutlines[type];
					overlayImage.sprite = defaultDieIcons.dieOverlays[type];
				}
				outlineImage.enabled = false;
				overlayImage.enabled = false;

				// change value text
				switch (requirement)
				{
					case EquipmentDieSlot.Requirement.None:
						valueText.text = value == -1 ? "?" : value.ToString();
						break;
					case EquipmentDieSlot.Requirement.GreaterThan:
						valueText.text = ">" + (value == -1 ? "?" : value.ToString());
						break;
					case EquipmentDieSlot.Requirement.LesserThan:
						valueText.text = "<" + (value == -1 ? "?" : value.ToString());
						break;
					case EquipmentDieSlot.Requirement.Equals:
						valueText.text = "=" + (value == -1 ? "?" : value.ToString());
						break;
					case EquipmentDieSlot.Requirement.NotEquals:
						valueText.text = "≠" + (value == -1 ? "?" : value.ToString());
						break;
					case EquipmentDieSlot.Requirement.IsEven:
						valueText.text = "Even";
						break;
					case EquipmentDieSlot.Requirement.IsOdd:
						valueText.text = "Odd";
						break;
				}

				// change state icon
				stateImage.enabled = false;
			}
		}

		/// <summary>
		/// The animation sequence for rolling die.
		/// </summary>
		/// <returns></returns>
		protected IEnumerator RollingValueAnimation(Die die)
		{
			rollingImage.transform.rotation = Quaternion.identity;
			rollingImage.enabled = true;
			stateImage.enabled = false;
			float tickTimer = 0f;
			
			int face = 0;
			while (rolling)
			{
				tickTimer += Time.deltaTime;
				if (tickTimer > 0.1f)
				{
					valueText.text = die.faces[face].value.ToString();
					face = (face + 1) % die.faces.Count;
					tickTimer -= 0.1f;
				}
				rollingImage.transform.Rotate(Vector3.forward, -360f * Time.deltaTime);
				yield return null;
			}

			rollingImage.transform.rotation = Quaternion.identity;
			rollingImage.enabled = false;
			stateImage.enabled = displayStatus && die != null && die.CurrentDieState != Die.DieState.Normal;
		}
	}
}