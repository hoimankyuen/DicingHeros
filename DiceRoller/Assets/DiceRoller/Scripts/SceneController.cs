using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleMaskCutoff;
using UnityEngine;

namespace DiceRoller
{
    public class SceneController : MonoBehaviour
    {
        public float throwVerticalForce = 0.1f;
        public float rollTorque = 1f;

        public List<Dice> dice = new List<Dice>();


        public float minThrowDragDistance = 0.2f;
        public float maxThrowDragDistance = 1f;
        public float minThrowForce = 5f;
        public float maxThrowForce = 10f;
        public float throwHeight = 5f;

        public GameObject throwTarget = null;
        public GameObject throwPowerIndicator = null;
        public CutoffSpriteRenderer throwPowerIndicatorCutoff = null;

        bool throwDragging = false;
        Vector3 throwDragPosition = Vector3.zero;
        Plane throwDragPlane = new Plane();
        Vector3 throwDirection = Vector3.zero;
        float throwPower = 0;
        bool thrown = false;

        void Awake()
        {
            dice.AddRange(GameObject.FindObjectsOfType<Dice>());
        }

        // Start is called before the first frame update
        void Start()
        {
            Application.targetFrameRate = 60;
        }

        // Update is called once per frame
        void Update()
        {
            DetectThrow();
            GetTotalValue();
            UpdateThrowUI();
        }

        void DetectThrow()
        {
            if (!throwDragging)
            {
                if (Input.GetMouseButton(0))
                {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Camera.main.farClipPlane))
                    {
                        if (hit.collider.CompareTag("Floor"))
                        {
                            throwDragging = true;
                            throwDragPosition = hit.point;
                            throwDragPlane = new Plane(Vector3.up, throwDragPosition);
                        }
                    }
                }
            }
            else if (throwDragging)
            {
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (throwDragPlane.Raycast(mouseRay, out float enter))
                {
                    throwDirection = (throwDragPosition -  (mouseRay.origin + mouseRay.direction * enter));
                    throwPower = throwDirection.magnitude < minThrowDragDistance ? -1f : Mathf.InverseLerp(minThrowDragDistance, maxThrowDragDistance, throwDirection.magnitude);
                    throwDirection.Normalize();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    if (throwPower != -1f)
                    {                        
                        Vector3 force = throwDirection * Mathf.Lerp(minThrowForce, maxThrowForce, throwPower);
                        Vector3 torque = Vector3.Cross(throwDirection, Vector3.down) * rollTorque;
                        Vector3 position = throwDragPosition + Vector3.up * throwHeight - force * Mathf.Sqrt(2 * throwHeight / 9.81f);
                        int castSize = (int)Mathf.Ceil(Mathf.Pow(dice.Count, 1f / 3f));

                        for (int i = 0; i < dice.Count; i++)
                        {
                            Vector3 castOffset = new Vector3(
                                i % castSize - (float)(castSize - 1) / 2f,
                                (i / castSize) % castSize - (float)(castSize - 1) / 2f,
                                i / (castSize * castSize) - (float)(castSize - 1) / 2f);
                            Quaternion randomDirection = Quaternion.Euler(new Vector3(
                               Random.Range(-5, 5),
                               Random.Range(-5, 5),
                               Random.Range(-5, 5)));
                            Vector3 randomTorque = new Vector3(
                                    Random.Range(-rollTorque * 0.5f, rollTorque * 0.5f),
                                    Random.Range(-rollTorque * 0.5f, rollTorque * 0.5f),
                                    Random.Range(-rollTorque * 0.5f, rollTorque * 0.5f));
                            dice[i].Roll(
                                position + castOffset * 0.25f,
                                randomDirection * force,
                                torque + randomTorque);
                        }
                        thrown = true;
                    }

                    throwDragging = false;
                }
            }
        }

        void UpdateThrowUI()
        {
            if (throwDragging)
            {
                throwTarget.SetActive(true);
                throwTarget.transform.position = throwDragPosition;
                if (throwPower != -1f)
                {
                    throwPowerIndicator.SetActive(true);
                    throwPowerIndicatorCutoff.CutoffTo(throwPower);
                    throwPowerIndicator.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0)) * Quaternion.FromToRotation(Vector3.forward, throwDirection) * Quaternion.Euler(new Vector3(90, 0, 0));
                }
                else
                {
                    throwPowerIndicator.SetActive(false);
                }
            }
            else
            {
                throwTarget.SetActive(false);
                throwPowerIndicator.SetActive(false);
            }
        }

        void GetTotalValue()
        {
            if (thrown)
            {
                if (dice.Aggregate(true, (result, d) => result && d.IsRolling) == false)
                {
                    int totalValue = dice.Aggregate(0, (result, d) => result + d.Value);
                    //Debug.Log(dice.Aggregate("A", (s, d) => s + " + (" + d.gameObject.name + " , " + d.GetValue + " )") + " = " + totalValue);
                    thrown = false;
                }
            }
        }
    }
}
