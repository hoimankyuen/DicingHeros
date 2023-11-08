using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DicingHeros
{
    public class UIHealthDisplay : MonoBehaviour
    {
        [Header("Components")]
        public Image healthIcon;
        public TextMeshProUGUI healthValue;

        // references
		private Unit unit;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		private void Start()
		{
			if (unit != null)
			{
				unit.OnHealthChanged += RefreshDisplay;
			}
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
			// deregister all events
			if (unit != null)
			{
				unit.OnHealthChanged -= RefreshDisplay;
			}
		}

		// ========================================================= UI Methods =========================================================

		/// <summary>
		/// Set the displayed information as an existing unit.
		/// </summary>
		public void SetDisplay(Unit unit)
        {
			// register and deregister callbacks
			if (this.unit != null)
			{
				this.unit.OnHealthChanged -= RefreshDisplay;
			}
			if (unit != null)
			{
				unit.OnHealthChanged += RefreshDisplay;
			}

			// set values
			this.unit = unit;
            RefreshDisplay();
        }

		/// <summary>
		/// Change the current display of this ui element to either match the information of the inspecting object.
		/// </summary>
		private void RefreshDisplay()
        {
            healthValue.text = unit != null ? string.Format("{0}/{1}", unit.Health, unit.maxHealth) : "==/==";
        }
    }
}