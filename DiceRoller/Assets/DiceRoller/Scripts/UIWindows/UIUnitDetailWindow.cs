using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceRoller
{
    public class UIUnitDetailWindow : UISideWindow
	{
		[Header("Components")]
		public Image unitImage;
		public UIHealthDisplay healthDisplay;
		public UIStatDisplay statDisplay;
		public Transform gearFrame;

		// reference
		protected GameController game => GameController.current;

		// working variables
		private Unit inspectingUnit = null;
		private List<UIEquipment> equipments = new List<UIEquipment>();
	
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

			Unit.OnItemSelectedChanged += RefreshDisplay;
			RefreshDisplay();
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
			Unit.OnItemSelectedChanged -= RefreshDisplay;
		}

		/// <summary>
		/// OnValidate is called when values where changed in the inspector.
		/// </summary>
		protected override void OnValidate()
		{
			base.OnValidate();
		}

		// ========================================================= Behaviour =========================================================

		protected void RefreshDisplay()
		{
			Unit inspectingUnit = Unit.GetFirstSelected();
			if (this.inspectingUnit != inspectingUnit)
			{
				this.inspectingUnit = inspectingUnit;

				if (this.inspectingUnit != null)
				{
					// fill in basic information
					unitImage.sprite = inspectingUnit != null ? inspectingUnit.iconSprite : null;
					healthDisplay.SetDisplay(inspectingUnit);
					statDisplay.SetDisplay(inspectingUnit);

					// removing previous equipment uis
					for (int i = gearFrame.childCount - 1; i >= 0; i--)
					{
						Destroy(gearFrame.GetChild(i).gameObject);
					}
					// add new equipment uis
					if (inspectingUnit != null)
					{
						for (int i = 0; i < inspectingUnit.Equipments.Count; i++)
						{
							// select the correct ui for the equipment
							GameObject prefab = null;
							if (inspectingUnit.Equipments[i] is SimpleKnife)
							{
								prefab = Resources.Load("UISimpleKnife") as GameObject;
							}
							else if (inspectingUnit.Equipments[i] is SimpleShoe)
							{
								prefab = Resources.Load("UISimpleShoe") as GameObject;
							}

							// spawn the equipment ui
							if (prefab != null)
							{
								GameObject uiEquipment = Instantiate(prefab, gearFrame);
								RectTransform rt = uiEquipment.GetComponent<RectTransform>();
								rt.anchorMin = new Vector2(0, 1);
								rt.anchorMax = new Vector2(1, 1);
								rt.anchoredPosition = new Vector3(0, -5 - i * (rt.rect.height + 10), 0);
								uiEquipment.GetComponent<UIEquipment>().SetInspectingTarget(inspectingUnit.Equipments[i]);
							}
						}
					}
				}
			}
		}
	}
}