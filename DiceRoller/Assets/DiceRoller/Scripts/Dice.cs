using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickerEffects;

namespace DiceRoller
{
    [System.Serializable]
    public class Face
    {
        public Vector3 euler;
        public int value;

        public string name()
        {
            return string.Format("Value: {0} at ({1}, {2}, {3})", value, euler.x, euler.y, euler.z);
        }
    }

    public class Dice : MonoBehaviour
    {
        public static List<Dice> SelectedDice { get; protected set; } = new List<Dice>();

        // parameters
        public Sprite icon = null;
        public float size = 1f;
        public List<Face> faces = new List<Face>();

        // reference
        protected GameController Game { get { return GameController.Instance; } }

        // components
        protected Rigidbody rigidBody = null;
        protected Outline outline = null;
        protected Transform effectTransform = null;
        protected LineRenderer lineRenderer = null;

        // working variables
        protected bool selected = false;

        protected bool rollInitiating = false;
        protected float lastMovingTime = 0;
        protected Quaternion lastRotation = Quaternion.identity;
        protected int lastValue = 0;

        protected Vector3 lastPosition = Vector3.zero;
        protected List<Tile> lastInTiles = new List<Tile>();


        public Unit connectedUnit = null;


        // ========================================================= Derived Properties =========================================================

        public bool IsRolling => rollInitiating || Time.time - lastMovingTime < 0.25f;

        public int Value => IsRolling ? -1 : Quaternion.Angle(transform.rotation, lastRotation) < 1f ? lastValue : ForceGetValue();
       
        public List<Tile> InTiles => Vector3.Distance(transform.position, lastPosition) < 0.0001f ? lastInTiles : ForceGetInTiles();

        // ========================================================= Monobehaviour Methods =========================================================

        /// <summary>
        /// Awake is called when the game object was created. It is always called before start and is 
        /// independent of if the game object is active or not.
        /// </summary>
        void Awake()
        {
            // retrieve components
            rigidBody = GetComponentInChildren<Rigidbody>();
            outline = GetComponentInChildren<Outline>();
            effectTransform = transform.Find("Effect");
            lineRenderer = transform.Find("Effect/Line").GetComponent<LineRenderer>();
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
            if (rigidBody.velocity.sqrMagnitude > 0.01f || rigidBody.angularVelocity.sqrMagnitude > 0.01f)
            {
                lastMovingTime = Time.time;
            }

            effectTransform.rotation = Quaternion.identity;

            if (connectedUnit != null)
            {
                lineRenderer.gameObject.SetActive(true);
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, connectedUnit.transform.position + 0.1f * Vector3.up);
            }
            else
            {
                lineRenderer.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// FixedUpdate is called at a regular interval, along side with physics simulation.
        /// </summary>
        private void FixedUpdate()
        {
            rollInitiating = false;
        }

        /// <summary>
        /// OnDrawGizmos is called when the game object is in editor mode
        /// </summary>
        void OnDrawGizmos()
        {
            if (Application.isEditor)
            {
                for (int i = 0; i < faces.Count; i++)
                {
                    Gizmos.color = Color.HSVToRGB((float)i / faces.Count, 1, 1);
                    Gizmos.DrawLine(transform.position, transform.position + transform.rotation * Quaternion.Euler(faces[i].euler) * Vector3.up * 0.25f);
                }
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, size / 2);
            }
        }

        /// <summary>
        /// OnMouseEnter is called when the mouse is start pointing to the game object.
        /// </summary>
        void OnMouseEnter()
        {
            if (!IsRolling && !selected)
            {
                outline.Show = true;
                SelectedDice.Add(this);
                lastInTiles.AddRange(Board.Instance.GetCurrentTiles(transform.position, size));
                foreach (Tile tile in lastInTiles)
                {
                    tile.AddDisplay(this, Tile.DisplayType.Position);
                }

                selected = true;
            }
        }

        /// <summary>
        /// OnMouseExit is called when the mouse is stop pointing to the game object.
        /// </summary>
        void OnMouseExit()
        {
            if (selected)
            {
                outline.Show = false;
                SelectedDice.Remove(this);
                foreach (Tile tile in lastInTiles)
                {
                    tile.RemoveDisplay(this, Tile.DisplayType.Position);
                }
                lastInTiles.Clear();

                selected = false;
            }
        }

        /// <summary>
        /// OnMouseDown is called when a mouse button is pressed when pointing to the game object.
        /// </summary>
        void OnMouseDown()
        {
            
        }

        // ========================================================= Behaviour =========================================================

        /// <summary>
        /// Find which tiles this game object is in.
        /// </summary>
        protected List<Tile> ForceGetInTiles()
        {
            lastInTiles.Clear();
            lastInTiles.AddRange(Board.Instance.GetCurrentTiles(transform.position, size));
            return lastInTiles;
        }

        /// <summary>
        /// Throw this die from a specific position with specific force and torque.
        /// </summary>
        public void Throw(Vector3 position, Vector3 force, Vector3 torque)
        {
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.MovePosition(position);
            rigidBody.AddForce(force, ForceMode.VelocityChange);
            rigidBody.AddTorque(torque, ForceMode.VelocityChange);

            rollInitiating = true;
        }

        /// <summary>
        /// Find the value got by this die.
        /// </summary>
        protected int ForceGetValue()
        {   
            float nearestAngle = float.MaxValue;
            int value = 0;
            foreach (Face face in faces)
            {
                float angle = Vector3.Angle(transform.rotation * Quaternion.Euler(face.euler) * Vector3.up, Vector3.up);
                if (angle < nearestAngle)
                {
                    nearestAngle = angle;
                    value = face.value;
                }
            }
            lastRotation = transform.rotation;
            lastValue = value;
            return value;
        }
    }
}