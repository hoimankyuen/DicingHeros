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
            RegisterCallbacks(unit);
        }

        /// <summary>
        /// OnDestroy is called when an game object is destroyed.
        /// </summary>
        protected void OnDestroy()
        {
            DeregisterCallbacks(unit);
        }

        // ========================================================= UI Methods =========================================================

        /// <summary>
        /// Set the displayed information as an existing unit.
        /// </summary>
        public void SetDisplay(Unit unit)
        {
            // register and deregister callbacks
            DeregisterCallbacks(this.unit);
            RegisterCallbacks(unit);

            // set values
            this.unit = unit;
            RefreshDisplay();
        }

        /// <summary>
        /// Register all necssary callbacks to a target object.
        /// </summary>
        private void RegisterCallbacks(Unit target)
        {
            if (target == null)
                return;

            target.OnPhysicalAttackChanged += RefreshDisplay;
            target.OnMagicalAttackChanged += RefreshDisplay;
            target.OnPhysicalDefenceChanged += RefreshDisplay;
            target.OnMovementChanged += RefreshDisplay;
            target.OnCurrentAttackTypeChanged += RefreshDisplay;
        }

        /// <summary>
        /// Deregister all necssary callbacks from a target object.
        /// </summary>
        private void DeregisterCallbacks(Unit target)
        {
            if (target == null)
                return;

            target.OnPhysicalAttackChanged -= RefreshDisplay;
            target.OnMagicalAttackChanged -= RefreshDisplay;
            target.OnPhysicalDefenceChanged -= RefreshDisplay;
            target.OnMovementChanged -= RefreshDisplay;
            target.OnCurrentAttackTypeChanged -= RefreshDisplay;
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
                attackIcon.color = unit.PhysicalAttack > unit.basePhysicalAttack ? Color.green : unit.PhysicalAttack < unit.basePhysicalAttack ? Color.red : Color.white;
                attackValue.color = unit.PhysicalAttack > unit.basePhysicalAttack ? Color.green : unit.PhysicalAttack < unit.basePhysicalAttack ? Color.red : Color.white;
                attackValue.text = unit.PhysicalAttack.ToString();
                if (unit.CurrentAttackType != Unit.AttackType.Physical)
                {
                    attackIcon.color = attackIcon.color * 0.5f + Color.black * 0.5f;
                    attackValue.color = attackValue.color * 0.5f + Color.black * 0.5f;
                }

                defenceIcon.color = unit.PhysicalDefence > unit.basePhysicalDefence ? Color.green : unit.PhysicalDefence < unit.basePhysicalDefence ? Color.red : Color.white;
                defenceValue.color = unit.PhysicalDefence > unit.basePhysicalDefence ? Color.green : unit.PhysicalDefence < unit.basePhysicalDefence ? Color.red : Color.white;
                defenceValue.text =unit.PhysicalDefence.ToString();

                magicIcon.color = unit.MagicalAttack > unit.baseMagicalAttack ? Color.green : unit.MagicalAttack < unit.baseMagicalAttack ? Color.red : Color.white;
                magicValue.color = unit.MagicalAttack > unit.baseMagicalAttack ? Color.green : unit.MagicalAttack < unit.baseMagicalAttack ? Color.red : Color.white;
                magicValue.text = unit.MagicalAttack.ToString();
                if (unit.CurrentAttackType != Unit.AttackType.Magical)
                {
                    magicIcon.color = magicIcon.color * 0.5f + Color.black * 0.5f;
                    magicValue.color = magicValue.color * 0.5f + Color.black * 0.5f;
                }

                movementIcon.color = unit.Movement > unit.baseMovement ? Color.green : unit.Movement < unit.baseMovement ? Color.red : Color.white;
                movementValue.color = unit.Movement > unit.baseMovement ? Color.green : unit.Movement < unit.baseMovement ? Color.red : Color.white;
                movementValue.text = unit.Movement.ToString();
            }
           
        }
    }
}