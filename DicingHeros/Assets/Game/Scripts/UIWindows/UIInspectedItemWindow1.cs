using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace DicingHeros
{
    public class UIInspectedItemWindow1 : MonoBehaviour
    {
		[Header("Components")]
		public CanvasGroup canvasGroup;
		public RectTransform frameTransform;
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
		protected void Awake()
		{
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected void Start()
		{
			//canvasGroup.alpha = 0;

			Unit.OnAnyBeingInspectedChanged += Populate;
			Die.OnAnyBeingInspectedChanged += Populate;
			Populate();
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected void Update()
		{
			ChangePosition();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected void OnDestroy()
		{
			Unit.OnAnyBeingInspectedChanged -= Populate;
			Die.OnAnyBeingInspectedChanged -= Populate;
		}

		/// <summary>
		/// OnValidate is called when values where changed in the inspector.
		/// </summary>
		protected void OnValidate()
		{
		}

		// ========================================================= Behaviour =========================================================

		protected void ChangePosition()
		{
			Vector2 mousePosition = Input.mousePosition;
			if (mousePosition.x / Screen.currentResolution.width < 0.5f && mousePosition.y / Screen.currentResolution.height < 0.5f)
			{
				frameTransform.anchorMin = new Vector2(1, 1);
				frameTransform.anchorMax = new Vector2(1, 1);
				frameTransform.pivot = new Vector2(1, 1);
				frameTransform.anchoredPosition = Vector2.zero;
			}
			else if(mousePosition.x / Screen.currentResolution.width < 0.5f && mousePosition.y / Screen.currentResolution.height > 0.5f)
			{
				frameTransform.anchorMin = new Vector2(1, 0);
				frameTransform.anchorMax = new Vector2(1, 0);
				frameTransform.pivot = new Vector2(1, 0);
				frameTransform.anchoredPosition = Vector2.zero;

			}
			else if (mousePosition.x / Screen.currentResolution.width > 0.5f && mousePosition.y / Screen.currentResolution.height < 0.5f)
			{
				frameTransform.anchorMin = new Vector2(0, 1);
				frameTransform.anchorMax = new Vector2(0, 1);
				frameTransform.pivot = new Vector2(0, 1);
				frameTransform.anchoredPosition = Vector2.zero;
			}
			else if (mousePosition.x / Screen.currentResolution.width > 0.5f && mousePosition.y / Screen.currentResolution.height > 0.5f)
			{
				frameTransform.anchorMin = new Vector2(0, 0);
				frameTransform.anchorMax = new Vector2(0, 0);
				frameTransform.pivot = new Vector2(0, 0);
				frameTransform.anchoredPosition = Vector2.zero;
			}
		}

		protected void Populate()
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