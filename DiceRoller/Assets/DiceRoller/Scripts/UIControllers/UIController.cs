using System.Collections;
using System.Collections.Generic;
using SimpleMaskCutoff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceRoller
{
	public class UIController : MonoBehaviour
	{
		// singleton
		public static UIController current { get; protected set; }

		[Header("Selectable Display")]
		public RectTransform frame;
		public Image unitImage; 
		public UIDieDisplay dieDisplay;
		public UIStatDisplay statDisplay;
		public Sprite unknownDiceIcon;
		protected bool selectableDisplayDirty = false;

		[Header("Throw Display")]
		public GameObject throwTarget = null;
		public SpriteRenderer throwArrow = null;
		public SpriteRenderer throwCross = null;
		public GameObject throwPowerIndicator = null;
		public CutoffSpriteRenderer throwPowerIndicatorCutoff = null;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected void Awake()
		{
			current = this;
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected void Start()
		{

		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected void Update()
		{
			RefreshSelectableDisplay();
			UpdateThrowDisplay();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected void OnDestroy()
		{
			current = null;
		}

		// ========================================================= Selectable Display =========================================================

		/// <summary>
		/// Request a change in the selectable display.
		/// </summary>
		public void SetSelectableDisplayDirty()
		{
			selectableDisplayDirty = true;
		}

		/// <summary>
		/// Change the dice display to reflect the current selected die.
		/// </summary>
		protected void RefreshSelectableDisplay()
		{
			// move selectable frame to either side of the screen
			if (Input.mousePosition.x > Screen.width - frame.rect.width)
			{
				frame.anchorMin = new Vector2(0, 0);
				frame.anchorMax = new Vector2(0, 0);
				frame.pivot = new Vector2(0, 0);
			}
			else if (Input.mousePosition.x < frame.rect.width)
			{
				frame.anchorMin = new Vector2(1, 0);
				frame.anchorMax = new Vector2(1, 0);
				frame.pivot = new Vector2(1, 0);
			}

			// display selectable information
			if (Unit.InspectingUnit != null && Unit.InspectingUnit.Count > 0)
			{
				// show unit information
				unitImage.gameObject.SetActive(true);
				unitImage.sprite = Unit.InspectingUnit[0].icon;
				statDisplay.gameObject.SetActive(true);
				statDisplay.SetDisplay(Unit.InspectingUnit[0]);

				dieDisplay.gameObject.SetActive(false);

			}
			else if (Die.InspectingDice != null && Die.InspectingDice.Count > 0 && Die.InspectingDice[0].Value != -1)
			{
				// show dice information
				unitImage.gameObject.SetActive(false);
				statDisplay.gameObject.SetActive(false);

				dieDisplay.gameObject.SetActive(true);
				dieDisplay.SetDisplay(Die.InspectingDice[0]);
			}
			else
			{
				// show nothing selected
				unitImage.gameObject.SetActive(false);
				statDisplay.gameObject.SetActive(false);

				dieDisplay.gameObject.SetActive(true);
				dieDisplay.SetDisplay(Die.Type.Unknown, -1);
			}
		}

		// ========================================================= Throw Display =========================================================

		/// <summary>
		/// Update the apparence of the throw indicator UI.
		/// </summary>
		protected void UpdateThrowDisplay()
		{
			if (DiceThrower.current.ThrowDragging)
			{
				// user throwing
				throwTarget.SetActive(true);
				throwPowerIndicator.SetActive(true);
				throwTarget.transform.position = DiceThrower.current.ThrowDragPosition;
				throwPowerIndicator.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0)) * Quaternion.FromToRotation(Vector3.forward, DiceThrower.current.ThrowDirection) * Quaternion.Euler(new Vector3(90, 0, 0));
				if (DiceThrower.current.ThrowPower != -1f)
				{
					// throw have enough power
					throwArrow.gameObject.SetActive(true);
					throwCross.gameObject.SetActive(false);
					throwPowerIndicatorCutoff.CutoffTo(DiceThrower.current.ThrowPower);
				}
				else
				{
					// throw does not have enough power
					throwArrow.gameObject.SetActive(false);
					throwCross.gameObject.SetActive(true);
					throwPowerIndicatorCutoff.CutoffTo(0);
				}
			}
			else
			{
				// user not throwing, disable throw indicator
				throwTarget.SetActive(false);
				throwPowerIndicator.SetActive(false);
			}
		}
	}
}