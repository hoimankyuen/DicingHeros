using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DiceRoller
{
    public class UIUnit : UIItem
    {
        [Header("Data")]
        public UIUnitStateIcons unitStateIcons;

        [Header("Components")]
        public Image iconImage;
        public Image outlineImage;
        public Image overlayImage;
        public Image stateImage;
        public TextMeshProUGUI nameText;
        public UIHealthDisplay healthDisplay;

        // components
        public RectTransform rectTransform => GetComponent<RectTransform>();

        // ========================================================= Inspecting Target =========================================================

        /// <summary>
        /// The base class of the inspecting target.
        /// </summary>
        protected override Item TargetBase => target;
        private Unit target;

        // ========================================================= Monobehaviour Methods =========================================================

        /// <summary>
        /// Awake is called when the game object was created. It is always called before start and is 
        /// independent of if the game object is active or not.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        protected override void Update()
        {
            base.Update();
        }

        /// <summary>
        /// OnDestroy is called when an game object is destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            // deregister all events
            if (target != null)
            {
                target.onHealthChanged -= RefreshDisplay;
                target.onEffectSetChanged -= RefreshDisplay;
                target.onUnitStateChange -= RefreshDisplay;
                Unit.onInspectionChanged -= RefreshDisplay;
                Unit.onSelectionChanged -= RefreshDisplay;
            }
        }

        // ========================================================= UI Methods =========================================================

        /// <summary>
        /// Set the displayed information as an existing unit.
        /// </summary>
        public void SetDisplay(Unit target)
        {
            // prevent excessive calls
            if (this.target == target)
                return;

            // mandatory step for changing target
            TriggerFillerEnterExits(target);

            // register and deregister callbacks
            if (this.target != null)
            {
                this.target.onHealthChanged -= RefreshDisplay;
                this.target.onEffectSetChanged -= RefreshDisplay;
                this.target.onUnitStateChange -= RefreshDisplay;
                Unit.onInspectionChanged -= RefreshDisplay;
                Unit.onSelectionChanged -= RefreshDisplay;
            }
            if (target != null)
            {
                target.onHealthChanged += RefreshDisplay;
                target.onEffectSetChanged += RefreshDisplay;
                target.onUnitStateChange += RefreshDisplay;
                Unit.onInspectionChanged += RefreshDisplay;
                Unit.onSelectionChanged += RefreshDisplay;
            }

            // set values
            this.target = target;
            healthDisplay.SetDisplay(target);
            RefreshDisplay();
        }

        /// <summary>
		/// Change the current display of this ui element to either match the information of the inspecting object.
		/// </summary>
		private void RefreshDisplay()
        {
            if (target != null)
            {
                // change die icon
                iconImage.sprite = target.iconSprite;
                outlineImage.sprite = target.outlineSprite;
                outlineImage.enabled = target.IsSelected;
                overlayImage.sprite = target.overlaySprite;
                overlayImage.enabled = target.IsBeingInspected;

                // change name text
                nameText.text = target.name;

                // change state icon
                stateImage.enabled = target.CurrentUnitState != Unit.UnitState.Standby;
                stateImage.sprite = unitStateIcons.stateIcons[target.CurrentUnitState];
            }
        }
    }
}