using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace DicingHeros
{
    public class UIEquipmentDieSlot : UIItemComponent
    {
        [Header("Components")]
        public UIDie uiDie;

        // ========================================================= Inspecting Target =========================================================

        /// <summary>
        /// The base class reference of the inspecting target.
        /// </summary>
        protected override ItemComponent TargetBase => target;

        /// <summary>
        /// The inspecting target.
        /// </summary>
        protected EquipmentDieSlot target = null;

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

            // setup components
            uiDie.SetAsEquipmentSlots(this);
        }

        // Update is called once per frame
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

            // deregister all callbacks
            if (target != null)
            {
                target.onDieChanged -= RefreshDisplay;
            }
        }

        // ========================================================= UI Methods =========================================================

        /// <summary>
        /// Set the displayed information as an existing equipment.
        /// </summary>
        public void SetInspectingTarget(EquipmentDieSlot target)
        {
            // prevent excessive calls
            if (this.target == target)
                return;

            // mandatory step for changing target
            TriggerFillerEnterExits(target);

            // register and deregister callbacks
            if (this.target != null)
            {
                this.target.onDieChanged -= RefreshDisplay;
            }
            if (target != null)
            {
                target.onDieChanged += RefreshDisplay;
            }

            // set value
            this.target = target;
            RefreshDisplay();
        }

        /// <summary>
        /// Update the displaying ui to match the current information of the inspecting object.
        /// </summary>
        private void RefreshDisplay()
        {
            if (target != null)
            {
                if (target.Die != null)
                {
                    uiDie.SetInspectingTarget(target.Die);
                }
                else
                {
                    uiDie.SetDisplayedValue(target.dieType, target.parameter, target.requirement);
                }
            }
            else
            {
                uiDie.SetDisplayedValue(Die.Type.Unknown, -1);
            }
        }
    }
}