using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public interface ISelectable
    {
        bool IsHovering();
    }

    public class InputManager : MonoBehaviour
    {
        public enum ActionType
        {
            Navigate,
            UnitMovement,
            DiceAttack,
        }

        // singleton
        public static InputManager Instance { get; protected set; }

        public ActionType Type { get; protected set; } = ActionType.Navigate;

        // ========================================================= Monobehaviour Methods =========================================================

        /// <summary>
        /// Awake is called when the game object was created. It is always called before start and is 
        /// independent of if the game object is active or not.
        /// </summary>
        protected void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Start is called before the first frame update and/or the game object is first active.
        /// </summary>
        protected void Start()
        {
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        protected void Update()
        {
            DetectInput();
        }

        /// <summary>
        /// OnDestroy is called when an game object is destroyed.
        /// </summary>
        protected void OnDestroy()
        {
            Instance = null;
        }

        protected void DetectInput()
        {
            switch (Type)
            {
                case ActionType.Navigate:
                    if (Input.GetMouseButtonDown(0))
                    {

                    }
                    if (Input.GetMouseButtonUp(0))
                    {

                    }
                    break;

                case ActionType.UnitMovement:
                    if (Input.GetMouseButtonDown(0))
                    {

                    }
                    if (Input.GetMouseButtonUp(0))
                    {

                    }
                    break;

                case ActionType.DiceAttack:
                    if (Input.GetMouseButtonDown(0))
                    {

                    }
                    if (Input.GetMouseButtonUp(0))
                    {

                    }
                    break;
            }
        }
    }
}