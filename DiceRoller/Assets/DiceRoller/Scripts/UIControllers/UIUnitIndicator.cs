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

		// references
		public Unit Target { get; private set; }

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
			SetTarget(Target);

			if (Target != null)
			{
				Target.OnHealthChanged += RefreshDisplay;
				Target.OnPendingHealthDeltaChanged += RefreshDisplay;
			}
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		private void Update()
		{
			UpdatePosition();
			Blink();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
			DeregisterCallbacks(Target);
		}


		// ========================================================= UI Methods =========================================================

		/// <summary>
		/// Set the displayed information as an existing unit.
		/// </summary>
		public void SetTarget(Unit target)
		{
			// register and deregister callbacks
			DeregisterCallbacks(this.Target);
			RegisterCallbacks(target);

			// set values
			this.Target = target;
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
			if (Target != null)
			{
				damageText.gameObject.SetActive(Target.IsRecievingDamage);
				healthRectTransform.gameObject.SetActive(true);
				pointerImage.gameObject.SetActive(true);

				damageText.text = Target.PendingHealthDelta == 0 ? "-0" : Target.PendingHealthDelta.ToString();
				valueText.text = string.Format("{0}/{1}", Mathf.Clamp(Target.Health, 0, Target.maxHealth), Target.maxHealth);

				float solidPercentage = Mathf.Clamp(1f * (Target.PendingHealthDelta < 0 ? (Target.Health + Target.PendingHealthDelta) : Target.Health) / Target.maxHealth, 0f, 1f);
				solidRectTransform.anchorMax = new Vector2(solidPercentage, 1f);
				solidImage.color = Color.HSVToRGB(Mathf.Lerp(0f, 0.33333f, solidPercentage), 1f, 1f);

				float blinkingPercentage = Mathf.Clamp(1f * (Target.PendingHealthDelta < 0 ? Target.Health : (Target.Health + Target.PendingHealthDelta)) / Target.maxHealth, 0f, 1f);
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
			if (Target != null && Target.PendingHealthDelta != 0)
			{
				Color.RGBToHSV(blinkingImage.color, out float h, out float s, out _);
				blinkingImage.color = Color.HSVToRGB(h, s, Mathf.Lerp(0.25f, 0.5f, Mathf.InverseLerp(-1f, 1f, Mathf.Sin(Time.time * 10f))));
			}
		}

		/// <summary>
		/// Update the position of the unit indicator to make it appears on the top of the target unit.
		/// </summary>
		private void UpdatePosition()
		{
			if (Target != null)
			{
				// move the pointer to the top of the item
				Vector3 pos = Vector3.zero;
				pos.x = (Camera.main.WorldToScreenPoint(Target.transform.position).x + Camera.main.WorldToScreenPoint(Target.transform.position + Vector3.up * Target.height).x) / 2f;
				pos.y = Camera.main.WorldToScreenPoint(Target.transform.position + Vector3.up * Target.height).y;
				transform.position = pos;
			}
		}
	}
}