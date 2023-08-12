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
		public static UIController Instance { get; protected set; }

		[Header("Zoom Camera")]
		public Camera zoomCamera;
		public RawImage zoomCameraDisplay;

		[Header("Dice Display")]
		public Sprite unknownDiceIcon;
		public Image diceImage;
		public TextMeshProUGUI diceValueText;

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
		private void Awake()
		{
			Instance = this;
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
			UpdateDiceDisplay();
			UpdateThrowDisplay();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
			Instance = null;
		}

		// ========================================================= Zoom Camera =========================================================

		// ========================================================= Dice Display =========================================================

		/// <summary>
		/// Change the dice display to reflect the current selected die.
		/// </summary>
		private void UpdateDiceDisplay()
		{
			if (Dice.SelectedDice != null && Dice.SelectedDice.Count > 0 && Dice.SelectedDice[0].Value != -1)
			{
				diceImage.sprite = Dice.SelectedDice[0].icon;
				diceValueText.text = Dice.SelectedDice[0].Value.ToString();
			}
			else
			{
				diceImage.sprite = unknownDiceIcon;
				diceValueText.text = "?";
			}
		}

		// ========================================================= Throw Display =========================================================

		/// <summary>
		/// Update the apparence of the throw indicator UI.
		/// </summary>
		void UpdateThrowDisplay()
		{
			if (DiceThrower.Instance.ThrowDragging)
			{
				// user throwing
				throwTarget.SetActive(true);
				throwPowerIndicator.SetActive(true);
				throwTarget.transform.position = DiceThrower.Instance.ThrowDragPosition;
				throwPowerIndicator.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0)) * Quaternion.FromToRotation(Vector3.forward, DiceThrower.Instance.ThrowDirection) * Quaternion.Euler(new Vector3(90, 0, 0));
				if (DiceThrower.Instance.ThrowPower != -1f)
				{
					// throw have enough power
					throwArrow.gameObject.SetActive(true);
					throwCross.gameObject.SetActive(false);
					throwPowerIndicatorCutoff.CutoffTo(DiceThrower.Instance.ThrowPower);
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