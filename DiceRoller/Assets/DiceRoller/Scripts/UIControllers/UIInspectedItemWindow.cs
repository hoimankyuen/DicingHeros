using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace DiceRoller
{
    public class UIInspectedItemWindow : UISideWindow
    {
		[Header("Components")]
		public UIDie dieDisplay;
		public Image unitImage;
		public UIHealthDisplay healthDisplay;
		public UIStatDisplay statDisplay;

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
			Populate();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		/// <summary>
		/// OnValidate is called when values where changed in the inspector.
		/// </summary>
		protected override void OnValidate()
		{
			base.OnValidate();
		}

		// ========================================================= Behaviour =========================================================

		protected void Populate()
		{
			// display selectable information
			if (Unit.InspectingUnits != null && Unit.InspectingUnits.Count > 0)
			{
				// show unit information
				dieDisplay.gameObject.SetActive(false);

				unitImage.gameObject.SetActive(true);
				unitImage.sprite = Unit.InspectingUnits[0].iconSprite;
				healthDisplay.gameObject.SetActive(true);
				healthDisplay.SetDisplay(Unit.InspectingUnits[0]);
				statDisplay.gameObject.SetActive(true);
				statDisplay.SetDisplay(Unit.InspectingUnits[0]);

			}
			else if (Die.InspectingDice != null && Die.InspectingDice.Count > 0 && Die.InspectingDice[0].Value != -1)
			{
				// show dice information
				dieDisplay.gameObject.SetActive(true);
				dieDisplay.SetDisplay(Die.InspectingDice[0]);

				unitImage.gameObject.SetActive(false);
				healthDisplay.gameObject.SetActive(false);
				statDisplay.gameObject.SetActive(false);
			}
			else
			{
				// show nothing selected
				dieDisplay.gameObject.SetActive(true);
				dieDisplay.SetDisplay(Die.Type.Unknown, -1);

				unitImage.gameObject.SetActive(false);
				healthDisplay.gameObject.SetActive(false);
				statDisplay.gameObject.SetActive(false);
			}
		}
	}
}