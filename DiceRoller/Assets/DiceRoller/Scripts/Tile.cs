using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public class Tile : MonoBehaviour
    {
        public enum DisplayType
        {
            Normal,
            Position,
            Attack,
            AttackTarget,
            Move,
            MoveTarget,
        }


        // parameters
        public float tileSize = 1f;
        [HideInInspector]
        public List<Tile> connectedTiles = new List<Tile>();

        public Sprite normalSprite = null;
        public Sprite positionSprite = null;
        public Sprite attackSprite = null;
        public Sprite moveSprite = null;

        // reference
        protected GameController Game { get { return GameController.Instance; } }

        // component
        protected SpriteRenderer spriteRenderer = null;
        protected new Collider collider = null;

        // working variables
        protected HashSet<object> registeredPositionDisplay = new HashSet<object>();
        protected HashSet<object> registeredMoveDisplay = new HashSet<object>();
        protected HashSet<object> registeredMoveTargetDisplay = new HashSet<object>();
        protected HashSet<object> registeredAttackDisplay = new HashSet<object>();
        protected HashSet<object> registeredAttackTargetDisplay = new HashSet<object>();

        protected bool isHovering = false;

        // ========================================================= Monobehaviour Methods =========================================================

        /// <summary>
        /// Awake is called when the game object was created. It is always called before start and is 
        /// independent of if the game object is active or not.
        /// </summary>
        void Awake()
        {
            spriteRenderer = transform.Find("Model/Sprite").GetComponent<SpriteRenderer>();
            collider = transform.Find("Collider").GetComponent<Collider>();
        }

        /// <summary>
        /// Start is called before the first frame update and/or the game object is first active.
        /// </summary>
        void Start()
        {

        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {

        }

        /// <summary>
        /// OnDestroy is called when the game object is destroyed.
        /// </summary>
        void OnDestroy()
        {
        }

        /// <summary>
        /// OnValidate is called when any inspector value is changed.
        /// </summary>
        void OnValidate()
        {

        }

        /// <summary>
        /// OnMouseEnter is called when the mouse is start pointing to the game object.
        /// </summary>
        void OnMouseEnter()
        {
        }

        /// <summary>
        /// OnMouseExit is called when the mouse is stop pointing to the game object.
        /// </summary>
        void OnMouseExit()
        {
        }

        /// <summary>
        /// OnMouseDown is called when a mouse button is pressed when pointing to the game object.
        /// </summary>
        void OnMouseDown()
        {

        }

        // ========================================================= Editor =========================================================

        /// <summary>
        /// Regenerate all components related to this tile. Should only be called in editor.
        /// </summary>
        public void RegenerateTile()
        {
            SpriteRenderer spriteRenderer = transform.Find("Model/Sprite").GetComponent<SpriteRenderer>();
            spriteRenderer.transform.localScale = new Vector3(tileSize / 1.28f, tileSize / 1.28f, 1f);

            collider = transform.Find("Collider").GetComponent<Collider>();
            collider.transform.localScale = new Vector3(tileSize, 0.1f, tileSize);
        }

        // ========================================================= Appearance =========================================================

        /// <summary>
        /// Register a particular display to this tile by any object.
        /// </summary>
        public void AddDisplay(object o, DisplayType displayType)
        {
            switch (displayType)
            {
                case DisplayType.Position:
                    registeredPositionDisplay.Add(o);
                    break;
                case DisplayType.Attack:
                    registeredAttackDisplay.Add(o);
                    break;
                case DisplayType.AttackTarget:
                    registeredAttackTargetDisplay.Add(o);
                    break;
                case DisplayType.Move:
                    registeredMoveDisplay.Add(o);
                    break;
                case DisplayType.MoveTarget:
                    registeredMoveTargetDisplay.Add(o);
                    break;
            }
            ResolveDisplay();
        }

        /// <summary>
        /// Deregister a particular display from this tile by any object.
        /// </summary>
        public void RemoveDisplay(object o, DisplayType displayType)
        {
            switch (displayType)
            {
                case DisplayType.Position:
                    registeredPositionDisplay.Remove(o);
                    break;
                case DisplayType.Attack:
                    registeredAttackDisplay.Remove(o);
                    break;
                case DisplayType.AttackTarget:
                    registeredAttackTargetDisplay.Remove(o);
                    break;
                case DisplayType.Move:
                    registeredMoveDisplay.Remove(o);
                    break;
                case DisplayType.MoveTarget:
                    registeredMoveTargetDisplay.Remove(o);
                    break;
            }
            ResolveDisplay();
        }

        /// <summary>
        /// Change the apparence of this tile according to all display registers.
        /// </summary>
        protected void ResolveDisplay()
        {
            if (registeredPositionDisplay.Count > 0)
                spriteRenderer.sprite = positionSprite;

            else if (registeredAttackTargetDisplay.Count > 0)
                spriteRenderer.sprite = attackSprite;

            else if (registeredAttackDisplay.Count > 0)
                spriteRenderer.sprite = positionSprite;

            else if (registeredMoveTargetDisplay.Count > 0)
                spriteRenderer.sprite = positionSprite;

            else if (registeredMoveDisplay.Count > 0)
                spriteRenderer.sprite = moveSprite;

            else
                spriteRenderer.sprite = normalSprite;
        }

        // ========================================================= Inqury =========================================================

        /// <summary>
        /// Check if an object is within the area of this tile. Estimate the object as position and square size.
        /// </summary>
        public bool IsInTile(Vector3 position, float size)
        {
            Vector3 localPosition = transform.InverseTransformPoint(position);
            return Mathf.Abs(localPosition.x) - Mathf.Abs(size / 2) < tileSize / 2 && Mathf.Abs(localPosition.z) - Mathf.Abs(size / 2) < tileSize / 2;
        }
    }
}
