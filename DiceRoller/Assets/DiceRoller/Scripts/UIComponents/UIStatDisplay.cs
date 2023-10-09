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
            if (unit == null)
            {
                // show default information      
                attackIcon.color = Color.white;
                attackValue.color = Color.white;
                attackValue.text = "==";

                defenceIcon.color = Color.white;
                defenceValue.color = Color.white;
                defenceValue.text = "==";

                magicIcon.color = Color.white;
                magicValue.color = Color.white;
                magicValue.text ="==";

                movementIcon.color = Color.white;
                movementValue.color = Color.white;
                movementValue.text = "==";
            }
            else
            {
                // show information of the inspecting unit
                attackIcon.color = unit.Melee > unit.baseMelee ? Color.green : unit.Melee < unit.baseMelee ? Color.red : Color.white;
                attackValue.color = unit.Melee > unit.baseMelee ? Color.green : unit.Melee < unit.baseMelee ? Color.red : Color.white;
                attackValue.text = unit.Melee.ToString();

                defenceIcon.color = unit.Defence > unit.baseDefence ? Color.green : unit.Defence < unit.baseDefence ? Color.red : Color.white;
                defenceValue.color = unit.Defence > unit.baseDefence ? Color.green : unit.Defence < unit.baseDefence ? Color.red : Color.white;
                defenceValue.text =unit.Defence.ToString();

                magicIcon.color = unit.Magic > unit.baseMagic ? Color.green : unit.Magic < unit.baseMagic ? Color.red : Color.white;
                magicValue.color = unit.Magic > unit.baseMagic ? Color.green : unit.Magic < unit.baseMagic ? Color.red : Color.white;
                magicValue.text = unit.Magic.ToString();

                movementIcon.color = unit.Movement > unit.baseMovement ? Color.green : unit.Movement < unit.baseMovement ? Color.red : Color.white;
                movementValue.color = unit.Movement > unit.baseMovement ? Color.green : unit.Movement < unit.baseMovement ? Color.red : Color.white;
                movementValue.text = unit.Movement.ToString();
            }
           
        }
    }
}