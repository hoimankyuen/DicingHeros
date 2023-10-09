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

		// working variables
		private Unit inspectingUnit = null;
		private Die inspectingDie = null;

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

			bool changed = false;
			changed |= CachedValueUtils.HasValueChanged(Unit.GetFirstBeingInspected(), ref inspectingUnit);
			changed |= CachedValueUtils.HasValueChanged(Die.GetFirstBeingInspected(), ref inspectingDie);
			if (changed)
			{
				Unit targetUnit = Unit.GetFirstBeingInspected();
				Die targetDie = Die.GetFirstBeingInspected();

				// display selectable information
				if (targetUnit != null)
				{
					// show unit information
					Unit target = Unit.GetFirstBeingInspected();

					dieDisplay.gameObject.SetActive(false);

					unitImage.gameObject.SetActive(true);
					unitImage.sprite = target.iconSprite;
					healthDisplay.gameObject.SetActive(true);
					healthDisplay.SetDisplay(target);
					statDisplay.gameObject.SetActive(true);
					statDisplay.SetDisplay(target);

				}
				else if (targetDie != null && targetDie.Value != -1)
				{
					// show dice information
					dieDisplay.gameObject.SetActive(true);
					dieDisplay.SetInspectingTarget(targetDie);

					unitImage.gameObject.SetActive(false);
					healthDisplay.gameObject.SetActive(false);
					statDisplay.gameObject.SetActive(false);
				}
				else
				{
					// show nothing selected
					dieDisplay.gameObject.SetActive(true);
					dieDisplay.SetDisplayedValue(Die.Type.Unknown, -1);

					unitImage.gameObject.SetActive(false);
					healthDisplay.gameObject.SetActive(false);
					statDisplay.gameObject.SetActive(false);
				}

			}

			

			
		}
	}
}