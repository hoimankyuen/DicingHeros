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
    }

    public class Dice : MonoBehaviour
    {
        public float throwAngleAdjust = 10f;
        public float rollTorqueAdjust = 20f;

        public List<Face> faces = new List<Face>();

        // components
        Rigidbody rigidBody = null;
        Outline outline = null;

        bool rollInitiating = false;
        float lastMovingTime = 0;
        Quaternion lastRotation = Quaternion.identity;
        int lastValue = 0;

        bool selected = false;

        public bool IsRolling => rollInitiating || Time.time - lastMovingTime < 0.25f;
        public int Value => Quaternion.Angle(transform.rotation, lastRotation) < 1f ? lastValue : ForceGetValue();

        void Awake()
        {
            rigidBody = GetComponentInChildren<Rigidbody>();
            outline = GetComponent<Outline>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (rigidBody.velocity.sqrMagnitude > 0.01f || rigidBody.angularVelocity.sqrMagnitude > 0.01f)
            {
                lastMovingTime = Time.time;
            }
        }

        private void FixedUpdate()
        {
            rollInitiating = false;
        }

        void OnDrawGizmos()
        {
            if (Application.isEditor)
            {
                Gizmos.color = Color.red;
                foreach(Face face in faces)
                {
                    Gizmos.DrawLine(transform.position, transform.position + transform.rotation * Quaternion.Euler(face.euler) * Vector3.up * 0.25f);
                }
            }
        }

        void OnMouseEnter()
        {
            outline.Show = true;
        }

        void OnMouseExit()
        {
            outline.Show = false;
        }

        public void Roll(Vector3 position, Vector3 force, Vector3 torque)
        {
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.MovePosition(position);
            rigidBody.AddForce(force, ForceMode.VelocityChange);
            rigidBody.AddTorque(torque, ForceMode.VelocityChange);

            rollInitiating = true;
        }

        int ForceGetValue()
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