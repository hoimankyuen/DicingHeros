using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DiceRoller
{
	public class UIDie : UIItem
	{
		public enum Mode
		{
			Default,
			Equipment,
			Cursor,	
		}

		[Header("Data")]
		public UIDieIcons defaultDieIcons;
		public UIDieStateIcons itemStateIcons;

		[Header("Components")]
		public Image iconImage;
		public Image outlineImage;
		public Image overlayImage;
		public Image stateImage;
		public Image rollingImage;
		public TextMeshProUGUI valueText;

		[Header("Settings")]
		[SerializeField]
		private bool displayStatus;

		[Header("Displayed Values")]
		[SerializeField]
		private Die.Type type;
		[SerializeField]
		private int value;
		[SerializeField]
		private EquipmentDieSlot.Requirement requirement;

		// working variables
		private Mode mode;

		private UIEquipmentDieSlot dieSlot;
		private Coroutine rollingValueCoroutine;
		private bool rolling = false;

		private bool pointerEntered1 = false;

		public RectTransform rectTransform => GetComponent<RectTransform>();

		// ========================================================= Inspecting Target =========================================================

		/// <summary>
		/// The base class of the inspecting target.
		/// </summary>
		protected override Item TargetBase => target;
		private Die target;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected override void Start()
		{
			base.Start();
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected override void Update()
		{
			base.Update();
		}


		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected override void OnDestroy()
		{
			base.OnDestroy();

			// deregister all events
			if (target != null)
			{
				target.onValueChanged -= RefreshDisplay;
				target.onDieStateChanged -= RefreshDisplay;
				target.OnInspectionChanged -= RefreshDisplay;
				target.OnSelectionChanged -= RefreshDisplay;
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
		public override void OnPointerEnter(PointerEventData eventData)
		{
			pointerEntered1 = true;

			if ((mode == Mode.Default || mode == Mode.Equipment) && target != null)
				target.OnUIMouseEnter();
			if (mode == Mode.Equipment && dieSlot != null)
				dieSlot.OnPointerEnter(eventData);
		}

		/// <summary>
		/// Callback triggered by mouse exit from Event System.
		/// </summary>
		public override void OnPointerExit(PointerEventData eventData)
		{
			pointerEntered1 = false;

			if ((mode == Mode.Default || mode == Mode.Equipment) && target != null)
				target.OnUIMouseExit();
			if (mode == Mode.Equipment && dieSlot != null)
				dieSlot.OnPointerExit(eventData);
		}

		/// <summary>
		/// Callback triggered by mouse button down from Event System.
		/// </summary>
		public override void OnPointerDown(PointerEventData eventData)
		{
			if ((mode == Mode.Default || mode == Mode.Equipment) && target != null)
				target.OnUIMouseDown((int)eventData.button);
			if (mode == Mode.Equipment && dieSlot != null)
				dieSlot.OnPointerDown(eventData);
		}

		/// <summary>
		/// Callback triggered by mouse button up from Event System.
		/// </summary>
		public override void OnPointerUp(PointerEventData eventData)
		{
			if ((mode == Mode.Default || mode == Mode.Equipment) && target != null)
				target.OnUIMouseUp((int)eventData.button);
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

			RefreshDisplay();
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

			RefreshDisplay();
		}

		/// <summary>
		/// Set the displayed information as an existing die.
		/// </summary>
		public void SetInspectingTarget(Die target)
		{
			SetTargetOrValue(target, type, value, requirement);
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
		private void SetTargetOrValue(Die target, Die.Type type, int value, EquipmentDieSlot.Requirement requirement)
		{
			// prevent excessive calls
			if (target != null && this.target == target)
				return;

			// register and deregister callbacks, also trigger the missing pointer exit and enter
			if (this.target != null)
			{
				if (pointerEntered1)
				{
					if ((mode == Mode.Default || mode == Mode.Equipment) && this.target != null)
						this.target.OnUIMouseExit();
					if (mode == Mode.Equipment && dieSlot != null)
						dieSlot.OnPointerExit(null);
				}

				this.target.onValueChanged -= RefreshDisplay;
				this.target.onDieStateChanged -= RefreshDisplay;
				this.target.OnInspectionChanged -= RefreshDisplay;
				this.target.OnSelectionChanged -= RefreshDisplay;
			}
			if (target != null)
			{
				if (pointerEntered1)
				{
					if ((mode == Mode.Default || mode == Mode.Equipment) && target != null)
						target.OnUIMouseEnter();
					if (mode == Mode.Equipment && dieSlot != null)
						dieSlot.OnPointerEnter(null);
				}

				target.onValueChanged += RefreshDisplay;
				target.onDieStateChanged += RefreshDisplay;
				target.OnInspectionChanged += RefreshDisplay;
				target.OnSelectionChanged += RefreshDisplay;
			}

			// stop running animations
			rolling = false;

			// set values
			this.target = target;
			this.type = type;
			this.value = value;
			this.requirement = requirement;
			RefreshDisplay();
		}

		// ========================================================= UI Methods =========================================================

		/// <summary>
		/// Change the current display of this ui element to either match the information of the inspecting object, or match the input values.
		/// </summary>
		private void RefreshDisplay()
		{
			// display the inspecting target die
			if (target != null)
			{
				// change die icon
				iconImage.sprite = target.iconSprite;
				
				outlineImage.sprite = target.outlineSprite;
				outlineImage.enabled = displayStatus && target.IsSelected;
				
				overlayImage.sprite = target.overlaySprite;
				overlayImage.enabled = displayStatus && (target.IsBeingInspected || target.CurrentDieState == Die.DieState.Holding || target.CurrentDieState == Die.DieState.Expended);
				if (displayStatus && target.IsBeingInspected)
					overlayImage.color = new Color(0, 1, 0, 0.5f);
				else if (displayStatus && (target.CurrentDieState == Die.DieState.Holding || target.CurrentDieState == Die.DieState.Expended))
					overlayImage.color = new Color(0, 0, 0, 0.5f);


				// change value text
				valueText.text = target.Value == -1 ? "?" : target.Value.ToString();

				// start or stop rolling value animation if die value is invalid
				if (target.Value == -1 && (target.CurrentDieState == Die.DieState.Casted || target.CurrentDieState == Die.DieState.Assigned))
				{
					if (!rolling)
					{
						rolling = true;
						if (gameObject.activeInHierarchy)
						{
							StartCoroutine(RollingValueAnimation(target));
						}
					}
				}
				else
				{
					rolling = false;
				}

				// change state icon
				stateImage.enabled = (mode == Mode.Default) && (displayStatus) && (!rolling) && (target.CurrentDieState != Die.DieState.Casted);
				stateImage.sprite = itemStateIcons.stateIcons[target.CurrentDieState];
			}

			// display the preset values
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
						valueText.text = "> " + (value == -1 ? "?" : value.ToString());
						break;
					case EquipmentDieSlot.Requirement.GreaterThanEqual:
						valueText.text = "≥ " + (value == -1 ? "?" : value.ToString());
						break;
					case EquipmentDieSlot.Requirement.LesserThan:
						valueText.text = "< " + (value == -1 ? "?" : value.ToString());
						break;
					case EquipmentDieSlot.Requirement.LesserThanEqual:
						valueText.text = "≤ " + (value == -1 ? "?" : value.ToString());
						break;
					case EquipmentDieSlot.Requirement.Equals:
						valueText.text = "= " + (value == -1 ? "?" : value.ToString());
						break;
					case EquipmentDieSlot.Requirement.NotEquals:
						valueText.text = "≠ " + (value == -1 ? "?" : value.ToString());
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
		private IEnumerator RollingValueAnimation(Die die)
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
			stateImage.enabled = displayStatus && die != null && die.CurrentDieState != Die.DieState.Casted;
		}
	}
}