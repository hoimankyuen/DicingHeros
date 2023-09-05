using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DiceRoller
{
	public class DiceThrower : MonoBehaviour
	{
		// singleton
		public static DiceThrower current { get; protected set; }

		// parameters
		public RangeFloat2 throwDragDistances = new RangeFloat2();
		public RangeFloat2 throwForces = new RangeFloat2(); // in percentage of height of monitor
		public float throwHeight = 5f;

		// readonly
		public readonly float rollTorque = 10000f;

		// references
		protected GameController game { get { return GameController.current; } }
		protected StateMachine stateMachine { get { return StateMachine.current; } }


		// working variables   
		protected List<Die> dice = new List<Die>();
		protected Plane throwDragPlane = new Plane();
		protected bool thrown = false;

		public bool ThrowDragging { get; protected set; } = false;
		public Vector3 ThrowDragPosition { get; protected set; } = Vector3.zero;
		public Vector3 ThrowDirection { get; protected set; } = Vector3.zero;
		public float ThrowPower { get; protected set; } = 0;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		void Awake()
		{
			current = this;
			dice.AddRange(GameObject.FindObjectsOfType<Die>());
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
			if (stateMachine.Current == State.Navigation)
			{
				DetectThrow();
			}
			//GetTotalValue();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
			current = null;
		}

		// ========================================================= Throw Dice =========================================================

		/// <summary>
		/// Detect and perform a throw action by the player.
		/// </summary>
		void DetectThrow()
		{
			if (!ThrowDragging)
			{
				if (Input.GetMouseButtonDown(0))
				{
					if (!EventSystem.current.IsPointerOverGameObject())
					{
						if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Camera.main.farClipPlane, LayerMask.GetMask("Floor", "Dice", "Unit")))
						{
							if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Floor"))
							{
								ThrowDragging = true;
								ThrowDragPosition = hit.point;
								throwDragPlane = new Plane(Vector3.up, ThrowDragPosition);
							}
						}
					}
				}
			}
			else if (ThrowDragging)
			{
				Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				
				// get the current throw parameters
				if (throwDragPlane.Raycast(mouseRay, out float enter))
				{
					ThrowDirection = (ThrowDragPosition - (mouseRay.origin + mouseRay.direction * enter));
					ThrowPower = ThrowDirection.magnitude < throwDragDistances.min ? -1f : throwDragDistances.InverseLerp(ThrowDirection.magnitude);
					ThrowDirection = ThrowDirection.normalized;
				}

				// initiate throw if mouse button is released
				if (Input.GetMouseButtonUp(0))
				{
					if (ThrowPower != -1f)
					{
						// calculate all essential variable for dice throwing
						Vector3 force = ThrowDirection * throwForces.Lerp(ThrowPower);
						Vector3 torque = Vector3.Cross(ThrowDirection, Vector3.down) * rollTorque;
						Vector3 position = ThrowDragPosition + Vector3.up * throwHeight - force * Mathf.Sqrt(2 * throwHeight / 9.81f);

						// select all do
						List<Die> throwingDice = new List<Die>();
						throwingDice.AddRange(dice.Where(x => x.team == stateMachine.Params.team));

						// shuffle the list of throwing dices
						if (throwingDice.Count > 0)
						{
							for (int i = 0; i < throwingDice.Count; i++)
							{
								int from = Random.Range(0, throwingDice.Count);
								int to = Random.Range(0, throwingDice.Count);
								Die temp = throwingDice[from];
								throwingDice[from] = throwingDice[to];
								throwingDice[to] = temp;
							}

							// place the dice in the correct 3d position and throw them
							Vector3 forward = force.normalized;
							Vector3 up = Vector3.up;
							Vector3 right = Vector3.Cross(forward, up);
							//int castSize = (int)Mathf.Ceil(Mathf.Pow(throwingDice.Count, 1f / 3f));
							int castSize = (int)Mathf.Ceil(Mathf.Sqrt(throwingDice.Count));
							for (int i = 0; i < throwingDice.Count; i++)
							{
								/*
								Vector3 castOffset =
									right * (i % castSize - (float)(castSize - 1) / 2f) +
									up * ((i / castSize) % castSize - (float)(castSize - 1) / 2f) +
									forward * (i / (castSize * castSize) - (float)(castSize - 1) / 2f);
								*/
								Vector3 castOffset =
									right * (i % castSize - (float)(castSize - 1) / 2f) +
									forward * (i / castSize - (float)(castSize - 1) / 2f);
								Quaternion randomDirection =
									Quaternion.AngleAxis(Random.Range(-5, 5) + Random.Range(-5, 5) * ThrowPower, up) *
									Quaternion.AngleAxis(Random.Range(-5, 5) + Random.Range(-5, 5) * ThrowPower, right);
								Vector3 randomTorque = new Vector3(
										Random.Range(-rollTorque * 0.5f, rollTorque * 0.5f),
										Random.Range(-rollTorque * 0.5f, rollTorque * 0.5f),
										Random.Range(-rollTorque * 0.5f, rollTorque * 0.5f));

								throwingDice[i].Throw(
										position + castOffset * 0.3f,
										randomDirection * force,
										torque + randomTorque);

								//throwingDice[i].Throw(position + castOffset * 0.25f, force, torque);
							}
						}

						thrown = true;
					}

					ThrowDragging = false;
				}
			}
		}
	}
}