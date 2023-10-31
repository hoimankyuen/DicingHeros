using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace DiceRoller
{
	public class UIEquipment : UIItemComponent
	{
		[Header("Components")]
		public Image outlineImage;
		public Image overlayImage;
		public TextMeshProUGUI titleText;
		public TextMeshProUGUI contentText;
		public List<UIEquipmentDieSlot> dieSlots;

		// ========================================================= Inspecting Target =========================================================

		/// <summary>
		/// The base class reference of the inspecting target.
		/// </summary>
		protected override ItemComponent TargetBase => target;

		/// <summary>
		/// The inspecting target.
		/// </summary>
		protected Equipment target = null;

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

			DeregisterCallbacks(target);
		}

		// ========================================================= UI Methods =========================================================

		/// <summary>
		/// Set the displayed information as an existing equipment.
		/// </summary>
		public void SetInspectingTarget(Equipment target)
		{
			// prevent excessive calls
			if (this.target == target)
				return;

			// mandatory step for changing target
			TriggerFillerEnterExits(target);

			// register and deregister callbacks
			DeregisterCallbacks(this.target);
			RegisterCallbacks(target);

			// set information
			this.target = target;
			RefreshDisplay();

			// setup all ui slots of this ui eqipment
			for (int i = 0; i < dieSlots.Count; i++)
			{
				if (i < target.DieSlots.Count)
				{
					dieSlots[i].SetInspectingTarget(target.DieSlots[i]);
				}
				else
				{
					dieSlots[i].gameObject.SetActive(false);
				}
			}
		}

		/// <summary>
		/// Register all necssary callbacks to a target object.
		/// </summary>
		private void RegisterCallbacks(Equipment target)
		{
			if (target == null)
				return;

			target.OnInspectionChanged += RefreshDisplay;
			target.OnFulfillmentChanged += RefreshDisplay;
			target.OnIsActivatedChanged += RefreshDisplay;
		}

		/// <summary>
		/// Deregister all necssary callbacks from a target object.
		/// </summary>
		private void DeregisterCallbacks(Equipment target)
		{
			if (target == null)
				return;

			this.target.OnInspectionChanged -= RefreshDisplay;
			this.target.OnFulfillmentChanged -= RefreshDisplay;
			this.target.OnIsActivatedChanged -= RefreshDisplay;
		}

		/// <summary>
		/// Update the displaying ui to match the current information of the inspecting object.
		/// </summary>
		private void RefreshDisplay()
		{
			if (target != null)
			{
				// change text
				titleText.text = target.DisplayableName;
				contentText.text = target.DisplayableEffectDiscription;

				// change effects
				outlineImage.enabled = target.IsActivated;

				overlayImage.enabled = target.IsBeingInspected || !target.IsRequirementFulfilled;
				if (target.IsBeingInspected)
					overlayImage.color = new Color(0, 1, 0, 0.5f);
				else if (!target.IsRequirementFulfilled)
					overlayImage.color = new Color(0, 0, 0, 0.5f);
			}
		}
	}
}