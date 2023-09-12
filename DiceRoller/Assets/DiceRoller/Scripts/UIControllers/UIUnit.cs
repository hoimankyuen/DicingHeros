using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DiceRoller
{
    public class UIUnit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Data")]
        public UIItemStateIcons itemStateIcons;

        [Header("Components")]
        public Image iconImage;
        public Image outlineImage;
        public Image overlayImage;
        public Image stateImage;
        public TextMeshProUGUI nameText;
        public UIHealthDisplay healthDisplay;

        // working variables
        protected Unit unit;

        public RectTransform rectTransform => GetComponent<RectTransform>();

        // ========================================================= Monobehaviour Methods =========================================================

        /// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected void Start()
        {
        }

        /// <summary>
        /// OnDestroy is called when an game object is destroyed.
        /// </summary>
        protected void OnDestroy()
        {
            // deregister all events
            if (unit != null)
            {
                unit.onHealthChanged -= RefreshDisplay;
                unit.onStatusChanged -= RefreshDisplay;
            }
        }

        // ========================================================= Mouse Event Handler ========================================================

        /// <summary>
        /// Callback triggered by mouse button enter from Event System.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (unit != null)
                unit.SetHoveringFromUI(true);
        }

        /// <summary>
        /// Callback triggered by mouse button exit from Event System.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (unit != null)
                unit.SetHoveringFromUI(false);
        }

        /// <summary>
        /// Callback triggered by mouse button down from Event System.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (unit != null)
                unit.SetPressedFromUI();
        }

        // ========================================================= UI Methods =========================================================

        /// <summary>
        /// Set the displayed information as an existing unit.
        /// </summary>
        public void SetDisplay(Unit unit)
        {
            // prevent excessive calls
            if (this.unit == unit)
                return;

            // register and deregister callbacks
            if (this.unit != null)
            {
                this.unit.onHealthChanged -= RefreshDisplay;
                this.unit.onStatusChanged -= RefreshDisplay;
            }
            if (unit != null)
            {
                unit.onHealthChanged += RefreshDisplay;
                unit.onStatusChanged += RefreshDisplay;
            }

            // set values
            this.unit = unit;
            healthDisplay.SetDisplay(unit);
            RefreshDisplay();
        }

        /// <summary>89
		/// Change the current display of this ui element to either match the information of the inspecting object.
		/// </summary>
		protected void RefreshDisplay()
        {
            if (unit != null)
            {
                // change die icon
                iconImage.sprite = unit.iconSprite;
                outlineImage.sprite = unit.outlineSprite;
                outlineImage.enabled = unit.IsSelected;
                overlayImage.sprite = unit.overlaySprite;
                overlayImage.enabled = unit.IsInspecting;

                // change name text
                nameText.text = unit.name;

                // change state icon
                stateImage.enabled = unit.ActionDepleted;
                stateImage.sprite = itemStateIcons.stateIcons[Die.DieState.Done];
            }
        }
    }
}