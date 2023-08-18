using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceRoller
{
    public class UIDieDisplay : MonoBehaviour
    {
        [Header("Data")]
        public UIDefaultDieIconEntries defaultDieIconEntries;

        [Header("Components")]
        public Image dieIcon;
        public TextMeshProUGUI dieValue;

        [Header("Displayed Values")]
        [SerializeField]
        protected Die.Type type;
        [SerializeField]
        protected int value;
        protected Die die;

        // ========================================================= Monobehaviour Methods =========================================================

        protected void OnValidate()
        {
            RefreshDisplay();
        }

        // ========================================================= UI Methods =========================================================

        public void SetDisplay(Die die)
        {
            this.die = die;
            RefreshDisplay();
        }

        public void SetDisplay(Die.Type type, int value)
        {
            die = null;
            this.type = type;
            this.value = value;
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            if (die != null)
            {
                dieIcon.sprite = die.icon;
                dieValue.text = die.Value.ToString();
            }
            else
            {
                if (defaultDieIconEntries.dieIcons.ContainsKey(type))
                    dieIcon.sprite = defaultDieIconEntries.dieIcons[type];
                dieValue.text = value == -1 ? "?" : value.ToString(); 
            }
        }
    }
}