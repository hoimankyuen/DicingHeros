using QuickerEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public class DieEffect : MonoBehaviour
	{
		[Header("Data")]
		public DieEffectStyle effectStyle = null;

		// reference
		private GameController game => GameController.current;

		// component
		private Die self = null;
		private Outline outline = null;
		private Overlay overlay = null;

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
			self = GetComponent<Die>();
			outline = GetComponent<Outline>();
			overlay = GetComponent<Overlay>();
		}

		/// <summary>
		/// Register all required callbacks.
		/// </summary>
		private void RegisterCallbacks()
		{
			self.OnInspectionChanged += RefreshEffects;
			self.OnSelectionChanged += RefreshEffects;
		}

		/// <summary>
		/// Deregister all registered callbacks.
		/// </summary>
		private void DeregisterCallbacks()
		{
			if (self != null)
			{
				self.OnInspectionChanged -= RefreshEffects;
				self.OnSelectionChanged -= RefreshEffects;
			}
		}


		// ========================================================= Functionalities =========================================================

		/// <summary>
		/// refresh all effects.
		/// </summary>
		private void RefreshEffects()
		{
			// initialize effect
			bool overlayEnabled = false;
			Color overlayColor = Color.magenta;
			bool outlineEnabled = false;
			Color outlineColor = Color.magenta;

			// resolve effects
			if (self.IsBeingInspected && self.Player == game.CurrentPlayer)
			{
				overlayEnabled = true;
				overlayColor = effectStyle.inspectedSelfColor;
			}

			if (self.IsBeingInspected && self.Player != game.CurrentPlayer)
			{
				overlayEnabled = true;
				overlayColor = effectStyle.inspectedOtherColor;
			}

			if (self.IsSelected)
			{
				if (self.Player == game.CurrentPlayer)
				{
					outlineEnabled = true;
					outlineColor = effectStyle.selectedSelfColor;
				}
				else
				{
					outlineEnabled = true;
					outlineColor = effectStyle.selectedOtherColor;
				}
			}

			// finalize effects
			overlay.enabled = overlayEnabled;
			if (overlayEnabled)
			{
				overlay.Color = overlayColor;
			}

			outline.enabled = outlineEnabled;
			if (outlineEnabled)
			{
				outline.Color = outlineColor;
			}
		}
	}
}