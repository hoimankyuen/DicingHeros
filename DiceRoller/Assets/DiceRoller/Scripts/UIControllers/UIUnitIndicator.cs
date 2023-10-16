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
		public TextMeshProUGUI damageText;

		public RectTransform healthRectTransform;
		public Image pointerImage;

		/*
		[Header("Testing")]
		public int maxHealth = 100;
		public int Health = 100;
		public int PendingHealthDelta = 0;
		*/

		// references
		private Unit target;


		// ========================================================= Monobehaviour Methods =========================================================


		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		private void Awake()
		{
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		private void Start()
		{
			SetDisplay(null);

			if (target != null)
			{
				target.OnHealthChanged += RefreshDisplay;
				target.OnPendingHealthDeltaChanged += RefreshDisplay;
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
			DeregisterCallbacks(target);
		}


		// ========================================================= UI Methods =========================================================

		private void RetrieveTarget()
		{
			if (Unit.GetFirstBeingInspected() != target)
			{
				SetDisplay(Unit.GetFirstBeingInspected());
			}
		}

		/// <summary>
		/// Set the displayed information as an existing unit.
		/// </summary>
		public void SetDisplay(Unit target)
		{
			// register and deregister callbacks
			DeregisterCallbacks(this.target);
			RegisterCallbacks(target);

			// set values
			this.target = target;
			RefreshDisplay();
		}

		/// <summary>
		/// Register all necssary callbacks to a target object.
		/// </summary>
		private void RegisterCallbacks(Unit target)
		{
			if (target == null)
				return;

			target.OnHealthChanged += RefreshDisplay;
			target.OnPendingHealthDeltaChanged += RefreshDisplay;
			target.OnIsRecievingDamageChanged += RefreshDisplay;
		}

		/// <summary>
		/// Deregister all necssary callbacks from a target object.
		/// </summary>
		private void DeregisterCallbacks(Unit target)
		{
			if (target == null)
				return;

			target.OnHealthChanged -= RefreshDisplay;
			target.OnPendingHealthDeltaChanged -= RefreshDisplay;
			target.OnIsRecievingDamageChanged -= RefreshDisplay;
		}


		/// <summary>
		/// Change the current display of this ui element to either match the information of the inspecting object.
		/// </summary>
		private void RefreshDisplay()
		{
			if (target != null)
			{
				damageText.gameObject.SetActive(target.IsRecievingDamage);
				healthRectTransform.gameObject.SetActive(true);
				pointerImage.gameObject.SetActive(true);

				damageText.text = target.PendingHealthDelta == 0 ? "-0" : target.PendingHealthDelta.ToString();
				valueText.text = string.Format("{0}/{1}", Mathf.Clamp(target.Health, 0, target.maxHealth), target.maxHealth);

				float solidPercentage = Mathf.Clamp(1f * (target.PendingHealthDelta < 0 ? (target.Health + target.PendingHealthDelta) : target.Health) / target.maxHealth, 0f, 1f);
				solidRectTransform.anchorMax = new Vector2(solidPercentage, 1f);
				solidImage.color = Color.HSVToRGB(Mathf.Lerp(0f, 0.33333f, solidPercentage), 1f, 1f);

				float blinkingPercentage = Mathf.Clamp(1f * (target.PendingHealthDelta < 0 ? target.Health : (target.Health + target.PendingHealthDelta)) / target.maxHealth, 0f, 1f);
				blinkingRectTransform.anchorMax = new Vector2(blinkingPercentage, 1f);
				blinkingImage.color = Color.HSVToRGB(Mathf.Lerp(0f, 0.33333f, blinkingPercentage), 1f, Mathf.Lerp(0.25f, 0.5f, Mathf.InverseLerp(-1f, 1f, Mathf.Sin(Time.time * 10f))));
			}
			else
			{
				damageText.gameObject.SetActive(false);
				healthRectTransform.gameObject.SetActive(false);
				pointerImage.gameObject.SetActive(false);

				damageText.text = "-0";
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
			if (target != null)
			{
				// move the pointer to the top of the item
				Vector3 pos = Vector3.zero;
				pos.x = (Camera.main.WorldToScreenPoint(target.transform.position).x + Camera.main.WorldToScreenPoint(target.transform.position + Vector3.up * target.height).x) / 2f;
				pos.y = Camera.main.WorldToScreenPoint(target.transform.position + Vector3.up * target.height).y;
				transform.position = pos;
			}
		}
	}
}