using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DiceRoller
{
	public class UIDie : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
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

		// working variables
		protected Die die;
		protected Coroutine rollingValueCoroutine;
		protected bool rolling = false;

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
				die.onInspectionChanged -= RefreshDisplay;
				die.onSelectionChanged -= RefreshDisplay;
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
		/// Callback triggered by mouse button enter from Event System.
		/// </summary>
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (die != null)
				die.SetHoveringFromUI(true);
		}

		/// <summary>
		/// Callback triggered by mouse button exit from Event System.
		/// </summary>
		public void OnPointerExit(PointerEventData eventData)
		{
			if (die != null)
				die.SetHoveringFromUI(false);
		}

		/// <summary>
		/// Callback triggered by mouse button down from Event System.
		/// </summary>
		public void OnPointerClick(PointerEventData eventData)
		{
			if (die != null)
				die.SetPressedFromUI();
		}

		// ========================================================= UI Methods =========================================================

		/// <summary>
		/// Set the displayed information as an existing die.
		/// </summary>
		public void SetDisplay(Die die)
		{
			// prevent excessive calls
			if (this.die == die)
				return;

			// register and deregister callbacks
			if (this.die != null)
			{
				this.die.onValueChanged -= RefreshDisplay;
				this.die.onDieStateChanged -= RefreshDisplay;
				this.die.onStatusChanged -= RefreshDisplay;
				die.onInspectionChanged -= RefreshDisplay;
				die.onSelectionChanged -= RefreshDisplay;
			}
			if (die != null)
			{
				die.onValueChanged += RefreshDisplay;
				die.onDieStateChanged += RefreshDisplay;
				die.onStatusChanged += RefreshDisplay;
				die.onInspectionChanged += RefreshDisplay;
				die.onSelectionChanged += RefreshDisplay;
			}

			// stop running animations
			rolling = false;

			// set values
			this.die = die;
			RefreshDisplay();
		}

		/// <summary>
		/// Set the displayed information as some preset value.
		/// </summary>
		public void SetDisplay(Die.Type type, int value)
		{
			// deregister callbacks
			if (die != null)
			{
				die.onValueChanged -= RefreshDisplay;
				die.onDieStateChanged -= RefreshDisplay;
				die.onStatusChanged -= RefreshDisplay;
				die.onInspectionChanged -= RefreshDisplay;
				die.onSelectionChanged -= RefreshDisplay;
			}

			// stop running animations
			rolling = false;

			// set values
			die = null;
			this.type = type;
			this.value = value;
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
				stateImage.enabled = displayStatus && !rolling && die.State != Die.DieState.Normal;
				stateImage.sprite = itemStateIcons.stateIcons[die.State];
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

				valueText.text = value == -1 ? "?" : value.ToString();
				
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
			stateImage.enabled = displayStatus && die != null && die.State != Die.DieState.Normal;
		}
	}
}