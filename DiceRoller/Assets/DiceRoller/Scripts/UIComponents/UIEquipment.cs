using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace DiceRoller
{
    public class UIEquipment : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Components")]
        public Image accent;
        public TextMeshProUGUI title;
        public TextMeshProUGUI content;
        public List<UIEquipmentDieSlot> dieSlots;

        // reference
        protected GameController game => GameController.current;

        // working variable
        protected Equipment equipment = null;

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
        }

        // Update is called once per frame
        protected void Update()
        {
        }

		// ========================================================= Mouse Event Handler ========================================================

		/// <summary>
		/// Callback triggered by mouse enter from Event System.
		/// </summary>
		public void OnPointerEnter(PointerEventData eventData)
		{
            if (equipment != null)
                equipment.OnUIMouseEnter();
        }

		/// <summary>
		/// Callback triggered by mouse exit from Event System.
		/// </summary>
		public void OnPointerExit(PointerEventData eventData)
		{
            if (equipment != null)
                equipment.OnUIMouseExit();
		}

		/// <summary>
		/// Callback triggered by mouse button down from Event System.
		/// </summary>
		public void OnPointerDown(PointerEventData eventData)
		{
            if (equipment != null)
                equipment.OnUIMouseDown((int)eventData.button);
        }

		/// <summary>
		/// Callback triggered by mouse button up from Event System.
		/// </summary>
		public void OnPointerUp(PointerEventData eventData)
		{
            if (equipment != null)
                equipment.OnUIMouseUp((int)eventData.button);
        }

        // ========================================================= UI Methods =========================================================

        /// <summary>
        /// Set the displayed information as an existing equipment.
        /// </summary>
        public void SetInspectingTarget(Equipment equipment)
        {
            this.equipment = equipment;
            
            for (int i = 0; i < equipment.DieSlots.Count; i++)
            {
                dieSlots[i].SetInspectingTarget(equipment.DieSlots[i]);
            }
           
        }
    }
}