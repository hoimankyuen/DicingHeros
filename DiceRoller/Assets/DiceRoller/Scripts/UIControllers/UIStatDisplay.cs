using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceRoller
{
    public class UIStatDisplay : MonoBehaviour
    {
        [Header("Components")]
        public Image attackIcon;
        public TextMeshProUGUI attackValue;
        public Image defenceIcon;
        public TextMeshProUGUI defenceValue;
        public Image magicIcon;
        public TextMeshProUGUI magicValue;
        public Image movementIcon;
        public TextMeshProUGUI movementValue;

        [Header("Displayed Values")]
        protected Unit unit;

        // ========================================================= Monobehaviour Methods =========================================================

        /// <summary>
        /// Start is called before the first frame update and/or the game object is first active.
        /// </summary>
        protected void Start()
        {
            if (unit != null)
            {
                unit.onStatChanged += RefreshDisplay;
            }
        }

        /// <summary>
        /// OnDestroy is called when an game object is destroyed.
        /// </summary>
        protected void OnDestroy()
        {
            // deregister all events
            if (unit != null)
            {
                unit.onStatChanged -= RefreshDisplay;
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
                this.unit.onStatChanged -= RefreshDisplay;
            }
            if (unit != null)
            {
                unit.onStatChanged += RefreshDisplay;
            }

            // set values
            this.unit = unit;
            RefreshDisplay();
        }


        /// <summary>
        /// Change the current display of this ui element to either match the information of the inspecting object.
        /// </summary>
        public void RefreshDisplay()
        {
            attackValue.text = unit != null ? unit.baseMelee.ToString() : "==";
            defenceValue.text = unit != null ? unit.baseDefence.ToString() : "==";
            magicValue.text = unit != null ? unit.baseMagic.ToString() : "==";
            movementValue.text = unit != null ? unit.baseMovement.ToString() : "==";
        }
    }
}