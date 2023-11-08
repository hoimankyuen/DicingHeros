using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DicingHeros
{
    public abstract class UIItemComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        // reference
        protected GameController game => GameController.current;

        // cache
        private bool pointerEntered = false;

        // ========================================================= Inspecting Target =========================================================

        /// <summary>
        /// The base class of the inspecting target.
        /// </summary>
        protected abstract ItemComponent TargetBase { get; }

        // ========================================================= Monobehaviour Methods =========================================================

        /// <summary>
        /// Awake is called when the game object was created. It is always called before start and is 
        /// independent of if the game object is active or not.
        /// </summary>
        protected virtual void Awake()
        {
        }

        /// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
        protected virtual void Start()
        {
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        protected virtual void Update()
        {
        }

        /// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected virtual void OnDestroy()
        {
        }

        // ========================================================= Mouse Event Handler ========================================================

        /// <summary>
        /// Callback triggered by mouse enter from Event System.
        /// </summary>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            pointerEntered = true;

            if (TargetBase != null)
                TargetBase.OnUIMouseEnter();
        }

        /// <summary>
        /// Callback triggered by mouse exit from Event System.
        /// </summary>
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            pointerEntered = false;

            if (TargetBase != null)
                TargetBase.OnUIMouseExit();
        }

        /// <summary>
        /// Callback triggered by mouse button down from Event System.
        /// </summary>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (TargetBase != null)
                TargetBase.OnUIMouseDown((int)eventData.button);
        }

        /// <summary>
        /// Callback triggered by mouse button up from Event System.
        /// </summary>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (TargetBase != null)
                TargetBase.OnUIMouseUp((int)eventData.button);
        }

        /// <summary>
        /// Trigger the pointer exit and pointer enter that are missing due to pointer still within the object when changing target. Should be called when changing target by the child class.
        /// </summary>
        protected void TriggerFillerEnterExits(ItemComponent nextTargetBase)
        {
            if (pointerEntered)
            {
                if (TargetBase != null)
                    TargetBase.OnUIMouseExit();
                if (nextTargetBase != null)
                    nextTargetBase.OnUIMouseEnter();
            }
        }
    }
}