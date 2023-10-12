using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace DiceRoller
{
    public class UIEquipment : UIItemComponent
    {
        [Header("Components")]
        public Image accent;
        public TextMeshProUGUI title;
        public TextMeshProUGUI content;
        public List<UIEquipmentDieSlot> dieSlots;

        // ========================================================= Inspecting Target =========================================================

        /// <summary>
        /// The base class reference of the inspecting target.
        /// </summary>
        protected override ItemComponent TargetBase => target;

        /// <summary>
        /// The inspecting target.
        /// </summary>
        protected Equipment target = null;

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
        }

        // ========================================================= UI Methods =========================================================

        /// <summary>
        /// Set the displayed information as an existing equipment.
        /// </summary>
        public void SetInspectingTarget(Equipment target)
        {
            // prevent excessive calls
            if (this.target == target)
                return;
            
            // mandatory step for changing target
            TriggerFillerEnterExits(target);

            // set values
            this.target = target;
            RefreshDisplay();

            // setup all ui slots of this ui eqipment
            for (int i = 0; i < target.DieSlots.Count; i++)
            {
                dieSlots[i].SetInspectingTarget(target.DieSlots[i]);
            }
        }

        /// <summary>
        /// Update the displaying ui to match the current information of the inspecting object.
        /// </summary>
        private void RefreshDisplay()
        {
        }
    }
}