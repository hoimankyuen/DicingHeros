using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceRoller
{
	public class UIUnitIndicator : MonoBehaviour
	{
		[Header("Components")]
		public Image blinkingImage;
		public Image solidImage;
		public RectTransform blinkingRectTransform;
		public RectTransform solidRectTransform;
		public Image iconImage;
		public TextMeshProUGUI valueText;

		public RectTransform healthRectTransform;
		public Image pointerImage;

		/*
		[Header("Testing")]
		public int maxHealth = 100;
		public int Health = 100;
		public int PendingHealthDelta = 0;
		*/

		// references
		private RectTransform rectTransform;
		private Unit targetUnit;


		// ========================================================= Monobehaviour Methods =========================================================


		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		private void Start()
		{
			SetDisplay(null);

			if (targetUnit != null)
			{
				targetUnit.onHealthChanged += RefreshDisplay;
				targetUnit.onPendingHealthDeltaChange += RefreshDisplay;
			}
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		private void Update()
		{
			RetrieveTarget();
			UpdatePosition();
			Blink();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
			if (targetUnit != null)
			{
				targetUnit.onHealthChanged -= RefreshDisplay;
				targetUnit.onPendingHealthDeltaChange -= RefreshDisplay;
			}
		}


		// ========================================================= UI Methods =========================================================

		private void RetrieveTarget()
		{
			if (Unit.GetFirstBeingInspected() != targetUnit)
			{
				SetDisplay(Unit.GetFirstBeingInspected());
			}
		}

		/// <summary>
		/// Set the displayed information as an existing unit.
		/// </summary>
		public void SetDisplay(Unit unit)
		{
			// register and deregister callbacks
			if (targetUnit != null)
			{
				targetUnit.onHealthChanged -= RefreshDisplay;
				targetUnit.onPendingHealthDeltaChange -= RefreshDisplay;
			}
			if (unit != null)
			{
				unit.onHealthChanged += RefreshDisplay;
				unit.onPendingHealthDeltaChange += RefreshDisplay;
			}

			// set values
			targetUnit = unit;
			RefreshDisplay();
		}

		/// <summary>
		/// Change the current display of this ui element to either match the information of the inspecting object.
		/// </summary>
		private void RefreshDisplay()
		{
			if (targetUnit != null)
			{
				healthRectTransform.gameObject.SetActive(true);
				pointerImage.gameObject.SetActive(true);

				valueText.text = string.Format("{0}/{1}", Mathf.Clamp(targetUnit.Health, 0, targetUnit.maxHealth), targetUnit.maxHealth);

				float solidPercentage = Mathf.Clamp(1f * (targetUnit.PendingHealthDelta < 0 ? (targetUnit.Health + targetUnit.PendingHealthDelta) : targetUnit.Health) / targetUnit.maxHealth, 0f, 1f);
				solidRectTransform.anchorMax = new Vector2(solidPercentage, 1f);
				solidImage.color = Color.HSVToRGB(Mathf.Lerp(0f, 0.33333f, solidPercentage), 1f, 1f);

				float blinkingPercentage = Mathf.Clamp(1f * (targetUnit.PendingHealthDelta < 0 ? targetUnit.Health : (targetUnit.Health + targetUnit.PendingHealthDelta)) / targetUnit.maxHealth, 0f, 1f);
				blinkingRectTransform.anchorMax = new Vector2(blinkingPercentage, 1f);
				blinkingImage.color = Color.HSVToRGB(Mathf.Lerp(0f, 0.33333f, blinkingPercentage), 1f, Mathf.Lerp(0.25f, 0.5f, Mathf.InverseLerp(-1f, 1f, Mathf.Sin(Time.time * 10f))));
			}
			else
			{
				healthRectTransform.gameObject.SetActive(false);
				pointerImage.gameObject.SetActive(false);

				valueText.text = ("==/==");

				solidRectTransform.anchorMax = new Vector2(1f, 1f);
				solidImage.color = Color.HSVToRGB(0.33333f, 1f, 1f);

				blinkingRectTransform.anchorMax = new Vector2(1f, 1f);
				blinkingImage.color = Color.HSVToRGB(0.33333f, 1f, Mathf.Lerp(0.25f, 0.5f, Mathf.InverseLerp(-1f, 1f, Mathf.Sin(Time.time * 10f))));
			}
		}

		/// <summary>
		/// Apply an animated blink to the blinking image.
		/// </summary>
		private void Blink()
		{
			Color.RGBToHSV(blinkingImage.color, out float h, out float s, out _);

			blinkingImage.color = Color.HSVToRGB(h, s, Mathf.Lerp(0.25f, 0.5f, Mathf.InverseLerp(-1f, 1f, Mathf.Sin(Time.time * 10f))));
		}

		/// <summary>
		/// Update the position of the unit indicator to make it appears on the top of the target unit.
		/// </summary>
		private void UpdatePosition()
		{
			if (targetUnit != null)
			{
				// move the pointer to the top of the item
				Vector3 pos = Vector3.zero;
				pos.x = (Camera.main.WorldToScreenPoint(targetUnit.transform.position).x + Camera.main.WorldToScreenPoint(targetUnit.transform.position + Vector3.up * targetUnit.height).x) / 2f;
				pos.y = Camera.main.WorldToScreenPoint(targetUnit.transform.position + Vector3.up * targetUnit.height).y;
				transform.position = pos;
			}
		}
	}
}