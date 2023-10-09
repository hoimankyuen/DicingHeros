using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace DiceRoller
{
    public class UIEquipmentDieSlot : MonoBehaviour
    {
        [Header("Components")]
        public UIDie uiDie;

        // reference
        protected GameController game => GameController.current;

        // working variable
        protected EquipmentDieSlot equipmentDieSlot = null;

        // ========================================================= Monobehaviour Methods =========================================================

        /// <summary>
        /// Awake is called when the game object was created. It is always called before start and is 
        /// independent of if the game object is active or not.
        /// </summary>
        protected void Awake()
        {

        }

        /// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
        protected void Start()
        {
            uiDie.SetAsEquipmentSlots(this);
        }

        // Update is called once per frame
        protected void Update()
        {

        }

        /// <summary>
        /// OnDestroy is called when an game object is destroyed.
        /// </summary>
        protected void OnDestroy()
        {
            // deregister all callbacks
            if (equipmentDieSlot != null)
            {
                equipmentDieSlot.onDieChanged -= RefreshDisplay;
            }
        }

        // ========================================================= Mouse Event Handler ========================================================

        /// <summary>
        /// Callback triggered by mouse button enter from Event System.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (equipmentDieSlot != null)
                equipmentDieSlot.OnUIMouseEnter();
        }

        /// <summary>
        /// Callback triggered by mouse button exit from Event System.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (equipmentDieSlot != null)
                equipmentDieSlot.OnUIMouseExit();
        }

        /// <summary>
        /// Callback triggered by mouse button down from Event System.
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (equipmentDieSlot != null)
                equipmentDieSlot.OnUIMouseDown((int)eventData.button);
        }

        /// <summary>
        /// Callback triggered by mouse button down from Event System.
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (equipmentDieSlot != null)
                equipmentDieSlot.OnUIMouseUp((int)eventData.button);
        }

        // ========================================================= UI Methods =========================================================

        /// <summary>
        /// Set the displayed information as an existing equipment.
        /// </summary>
        public void SetInspectingTarget(EquipmentDieSlot equipmentDieSlot)
        {
            // prevent excessive calls
            if (this.equipmentDieSlot == equipmentDieSlot)
                return;

            // register and deregister callbacks
            if (this.equipmentDieSlot != null)
            {
                this.equipmentDieSlot.onDieChanged -= RefreshDisplay;
            }
            if (equipmentDieSlot != null)
            {
                equipmentDieSlot.onDieChanged += RefreshDisplay;
            }

            // set value
            this.equipmentDieSlot = equipmentDieSlot;
            RefreshDisplay();
        }

        /// <summary>
        /// Update the displaying ui to match the current information of the inspecting object.
        /// </summary>
        private void RefreshDisplay()
        {
            if (equipmentDieSlot != null)
            {
                if (equipmentDieSlot.Die != null)
                {
                    uiDie.SetInspectingTarget(equipmentDieSlot.Die);
                }
                else
                {
                    uiDie.SetInspectingTarget(equipmentDieSlot.dieType, equipmentDieSlot.parameter, equipmentDieSlot.requirement);
                }
            }
            else
            {
                uiDie.SetInspectingTarget(Die.Type.Unknown, -1);
            }
        }
    }
}