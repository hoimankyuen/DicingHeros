using QuickerEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DicingHeros
{
    public class UnitEffect : MonoBehaviour
    {
		public enum BlinkType
		{
			None,
			SoftSlow,
			SoftFast,
			SharpSlow,
			SharpFast,
		}


		[Header("Data")]
		public UnitEffectStyle effectStyle = null;

		// reference
		private GameController game => GameController.current;
		private StateMachine stateMachine => StateMachine.current;

		// component
		private Unit self = null;
		private Outline outline = null;
		private Overlay overlay = null;

		// working variables
		bool instantEffectRunning = false;
		private Color overlayColor = Color.magenta;
		private BlinkType overlayBlinkType = BlinkType.None;
		private Color outlineColor = Color.magenta;
		private BlinkType outlineBlinkType = BlinkType.None;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		private void Awake()
		{
			RetreiveReferences();
			RegisterCallbacks();
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
			BlinkEffects();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
			DeregisterCallbacks();
		}

		// ========================================================= Setup =========================================================

		/// <summary>
		/// Retrieve all required references.
		/// </summary>
		private void RetreiveReferences()
		{
			self = GetComponent<Unit>();
			outline = GetComponent<Outline>();
			overlay = GetComponent<Overlay>();
		}

		/// <summary>
		/// Register all required callbacks.
		/// </summary>
		private void RegisterCallbacks()
		{
			self.OnBlockingChanged += RefreshEffects;
			self.OnInRangeChanged += RefreshEffects;
			self.OnTargetableChanged += RefreshEffects;
			self.OnInspectionChanged += RefreshEffects;
			self.OnSelectionChanged += RefreshEffects;
			self.OnUnitStateChange += RefreshEffects;

			self.OnTakingDamage += StartDamageEffect;
		}

		/// <summary>
		/// Deregister all registered callbacks.
		/// </summary>
		private void DeregisterCallbacks()
		{
			if (self != null)
			{
				self.OnBlockingChanged -= RefreshEffects;
				self.OnInRangeChanged -= RefreshEffects;
				self.OnTargetableChanged -= RefreshEffects;
				self.OnInspectionChanged -= RefreshEffects;
				self.OnSelectionChanged -= RefreshEffects;	
				self.OnUnitStateChange -= RefreshEffects;

				self.OnTakingDamage -= StartDamageEffect;
			}
		}

		// ========================================================= Consistant Effects =========================================================

		/// <summary>
		/// refresh all effects.
		/// </summary>
		private void RefreshEffects()
		{
			// dont do if instant effect is running
			if (instantEffectRunning)
				return;

			// initialize effect
			bool overlayEnabled = false;
			Color overlayColor = Color.magenta;
			BlinkType overlayBlinkType = BlinkType.None;
			bool outlineEnabled = false;
			Color outlineColor = Color.magenta;
			BlinkType outlineBlinkType = BlinkType.None;
			
			// resolve effects
			if (self.CurrentUnitState == Unit.UnitState.Depleted)
			{
				overlayEnabled = true;
				overlayColor = effectStyle.depletedColor;
			}

			if (self.IsInRange)
			{

				if (self.Player == game.CurrentPlayer)
				{
					overlayEnabled = true;
					overlayColor = effectStyle.targetableSelfColor;
					overlayBlinkType = BlinkType.SoftSlow;
				}
				else
				{
					overlayEnabled = true;
					overlayColor = effectStyle.targetableOtherColor;
					overlayBlinkType = BlinkType.SoftSlow;
				}
			}

			if (self.IsTargetable)
			{
				if (self.Player == game.CurrentPlayer)
				{
					overlayEnabled = true;
					overlayColor = effectStyle.targetableSelfColor;
					overlayBlinkType = BlinkType.SoftFast;
				}
				else
				{
					overlayEnabled = true;
					overlayColor = effectStyle.targetableOtherColor;
					overlayBlinkType = BlinkType.SoftFast;
				}
			}

			if (self.IsBlocking)
			{
				if (self.Player == game.CurrentPlayer)
				{
					overlayEnabled = true;
					overlayColor = effectStyle.blockingFriendColor;
					overlayBlinkType = BlinkType.None;
				}
				else
				{
					overlayEnabled = true;
					overlayColor = effectStyle.blockingOtherColor;
					overlayBlinkType = BlinkType.None;
				}
			}

			if (self.IsBeingInspected)
			{
				if (self.Player == game.CurrentPlayer)
				{
					overlayEnabled = true;
					overlayColor = effectStyle.inspectedSelfColor;
					overlayBlinkType = BlinkType.None;
				}
				else
				{
					overlayEnabled = true;
					overlayColor = effectStyle.inspectedOtherColor;
					overlayBlinkType = BlinkType.None;
				}
			}

			if (self.IsSelected)
			{
				if (self.Player == game.CurrentPlayer)
				{
					outlineEnabled = true;
					outlineColor = effectStyle.selectedSelfColor;
					outlineBlinkType = BlinkType.None;
				}
				else
				{
					outlineEnabled = true;
					outlineColor = effectStyle.selectedOtherColor;
					outlineBlinkType = BlinkType.None;
				}
			}

			// finalize effects
			overlay.enabled = overlayEnabled;
			this.overlayBlinkType = overlayBlinkType;
			this.overlayColor = overlayColor;
			if (overlayEnabled && overlayBlinkType == BlinkType.None)
			{
				overlay.Color = overlayColor;
			}

			outline.enabled = outlineEnabled;
			this.outlineBlinkType = outlineBlinkType;
			this.outlineColor = outlineColor;
			if (outlineEnabled && outlineBlinkType == BlinkType.None)
			{
				outline.Color = outlineColor;
			}
		}

		/// <summary>
		/// Perform a blink animation to the effects if needed.
		/// </summary>
		private void BlinkEffects()
		{
			// dont do if instant effect is running
			if (instantEffectRunning)
				return;

			Color targetColor;

			// overlay blinking
			if (overlayBlinkType != BlinkType.None)
			{
				targetColor = overlayColor;
				switch (overlayBlinkType)
				{
					case BlinkType.SoftSlow:
						targetColor.a = Mathf.Lerp(0, overlayColor.a, Mathf.Sin(Time.time * 5f) * 0.5f + 0.5f);
						break;
					case BlinkType.SoftFast:
						targetColor.a = Mathf.Lerp(0, overlayColor.a, Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f);
						break;
					case BlinkType.SharpSlow:
						targetColor.a = Mathf.Lerp(0, overlayColor.a, ((Time.time * 5f) % 1f < 0.5f ? 0f : 1f) * 0.5f + 0.5f);
						break;
					case BlinkType.SharpFast:
						targetColor.a = Mathf.Lerp(0, overlayColor.a, ((Time.time * 50f) % 1f < 0.5f ? 0f : 1f) * 0.5f + 0.5f);
						break;
				}
				overlay.Color = targetColor;
			}

			// overlay blinking
			if (outlineBlinkType != BlinkType.None)
			{
				targetColor = outlineColor;
				switch (overlayBlinkType)
				{
					case BlinkType.SoftSlow:
						targetColor.a = Mathf.Lerp(0, outlineColor.a, Mathf.Sin(Time.time * 5f) * 0.5f + 0.5f);
						break;
					case BlinkType.SoftFast:
						targetColor.a = Mathf.Lerp(0, outlineColor.a, Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f);
						break;
					case BlinkType.SharpSlow:
						targetColor.a = Mathf.Lerp(0, outlineColor.a, ((Time.time * 5f) % 1f < 0.5f ? 0f : 1f) * 0.5f + 0.5f);
						break;
					case BlinkType.SharpFast:
						targetColor.a = Mathf.Lerp(0, outlineColor.a, ((Time.time * 50f) % 1f < 0.5f ? 0f : 1f) * 0.5f + 0.5f);
						break;
				}
				outline.Color = targetColor;
			}

		}

		// ========================================================= Instant Effects =========================================================

		/// <summary>
		/// Start an effect animation of taking damage.
		/// </summary>
		private void StartDamageEffect()
		{
			StartCoroutine(DamageEffectSequence());
		}
		private IEnumerator DamageEffectSequence()
		{
			instantEffectRunning = true;

			overlay.enabled = true;
			overlay.Color = Color.red;
			yield return new WaitForSeconds(0.05f);

			overlay.enabled = true;
			overlay.Color = new Color(0, 0, 0, 0);
			yield return new WaitForSeconds(0.05f);

			// revert back to normal effects
			instantEffectRunning = false;
			RefreshEffects();
			yield return null;
		}
	}
}