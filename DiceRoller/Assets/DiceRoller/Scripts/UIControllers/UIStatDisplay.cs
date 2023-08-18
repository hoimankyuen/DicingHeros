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
        public Image healthIcon;
        public TextMeshProUGUI healthValue;
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

        // ========================================================= UI Methods =========================================================

        public void SetDisplay(Unit unit)
        {
            this.unit = unit;
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            healthValue.text = string.Format("{0} / {1}", unit.CurrentHealth, unit.maxHealth);
            attackValue.text = unit.melee.ToString();
            defenceValue.text = unit.defence.ToString();
            magicValue.text = unit.magic.ToString();
            movementValue.text = unit.movement.ToString();
        }
    }
}