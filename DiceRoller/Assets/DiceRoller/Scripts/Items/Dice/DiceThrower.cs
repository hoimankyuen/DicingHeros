using System;
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

		// properties
		public int RemainingThrow { get; protected set; } = 0;
		public bool ThrowDragging { get; protected set; } = false;
		public Vector3 ThrowDragPosition { get; protected set; } = Vector3.zero;
		public Vector3 ThrowDirection { get; protected set; } = Vector3.zero;
		public float ThrowPower { get; protected set; } = 0;

		// events
		public Action onRemainingThrowChanged = () => { };

		// working variables   
		protected Plane throwDragPlane = new Plane();

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected void Awake()
		{
			current = this;
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected void Start()
		{
			RegisterStateBehaviours();
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected void Update()
		{
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected void OnDestroy()
		{
			DeregisterStateBehaviours();
			current = null;
		}

		// ========================================================= Throw Count Behaviours =========================================================

		/// <summary>
		/// Reset the remaining throw to a specific number.
		/// </summary>
		public void ResetRemainingThrow(int count)
		{
			RemainingThrow = (count > 0) ? count : 0;
			onRemainingThrowChanged.Invoke();
		}

		/// <summary>
		/// Add or remove throws to the remaining throw.
		/// </summary>
		public void ModifyRemainingThrow(int delta)
		{
			RemainingThrow = (RemainingThrow + delta > 0) ? (RemainingThrow + delta) : 0;
			onRemainingThrowChanged.Invoke();
		}

		// ========================================================= Throw Dice Behaviours =========================================================

		protected enum DetectThrowResult
		{ 
			None,
			Thrown,
			Aborted,
		}

		/// <summary>
		/// Detect and perform a throw action by the player. Return true if thrown.
		/// </summary>
		protected DetectThrowResult DetectThrow(IReadOnlyCollection<Die> dice)
		{
			if (!ThrowDragging)
			{
				// detect start dragging by checking mouse button is pressed on a floor
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

				// drag completed, determine out come
				if (Input.GetMouseButtonUp(0))
				{
					ThrowDragging = false;

					// do not throw if no throw remaining
					if (RemainingThrow <= 0)
					{
						ThrowDragging = false;
						return DetectThrowResult.None;
					}

					// do not throw if no die is selected
					if (dice.Count == 0)
					{
						ThrowDragging = false;
						return DetectThrowResult.None;
					}

					//initiate throw
					if (ThrowPower != -1f)
					{
						// calculate all essential variable for dice throwing
						Vector3 force = ThrowDirection * throwForces.Lerp(ThrowPower);
						Vector3 torque = Vector3.Cross(ThrowDirection, Vector3.down) * rollTorque;
						Vector3 position = ThrowDragPosition + Vector3.up * throwHeight - force * Mathf.Sqrt(2 * throwHeight / 9.81f);

						// select all dice
						List<Die> throwingDice = new List<Die>();
						throwingDice.AddRange(dice);

						// turn all holding dice to casted
						foreach (Die die in throwingDice)
						{
							die.RetieveFromHold();
						}

						// shuffle the list of throwing dices
						for (int i = 0; i < throwingDice.Count; i++)
						{
							int from = UnityEngine.Random.Range(0, throwingDice.Count);
							int to = UnityEngine.Random.Range(0, throwingDice.Count);
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
								Quaternion.AngleAxis(UnityEngine.Random.Range(-5, 5) + UnityEngine.Random.Range(-5, 5) * ThrowPower, up) *
								Quaternion.AngleAxis(UnityEngine.Random.Range(-5, 5) + UnityEngine.Random.Range(-5, 5) * ThrowPower, right);
							Vector3 randomTorque = new Vector3(
									UnityEngine.Random.Range(-rollTorque * 0.5f, rollTorque * 0.5f),
									UnityEngine.Random.Range(-rollTorque * 0.5f, rollTorque * 0.5f),
									UnityEngine.Random.Range(-rollTorque * 0.5f, rollTorque * 0.5f));

							throwingDice[i].Throw(
									position + castOffset * 0.3f,
									randomDirection * force,
									torque + randomTorque);

							//throwingDice[i].Throw(position + castOffset * 0.25f, force, torque);
						}

						// decrease remaining throws
						RemainingThrow--;
						onRemainingThrowChanged.Invoke();

						return DetectThrowResult.Thrown;
					}
				}
			}
			return DetectThrowResult.None;
		}

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.Register(gameObject, this, SMState.DiceActionSelect, new DiceActionSelectSB(this));
			stateMachine.Register(gameObject, this, SMState.DiceThrow, new DiceThrowSB(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		protected void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
				stateMachine.DeregisterAll(this);
		}

		// ========================================================= Dice Action State =========================================================
		private class DiceActionSelectSB : StateBehaviour
		{
			// host reference
			private readonly DiceThrower self = null;

			// caches
			private Vector2 pressedPosition1 = Vector2.negativeInfinity;

			/// <summary>
			/// Constructor.
			/// </summary>
			public DiceActionSelectSB(DiceThrower self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				// detect abort bu checking left mouse button click
				if (InputUtils.GetMousePress(1, ref pressedPosition1))
				{
					Die.ClearSelected();
					stateMachine.ChangeState(SMState.Navigation);
				}

				// change to dice throw state if a throw is detected
				DetectThrowResult result = self.DetectThrow(Die.GetAllSelected());
				if (result == DetectThrowResult.Thrown)
				{
					stateMachine.ChangeState(SMState.DiceThrow);
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				InputUtils.ResetPressCache(ref pressedPosition1);
			}
		}

		// ========================================================= Dice Action State =========================================================
		protected class DiceThrowSB : StateBehaviour
		{
			protected readonly DiceThrower self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public DiceThrowSB(DiceThrower self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
				// change to navigation state if dice throw is completed
				if (Die.GetAllSelected().All(x => !x.IsMoving || x.IsFallen))
				{
					Die.ClearSelected();
					stateMachine.ChangeState(SMState.Navigation);
				}
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{

			}
		}
	}
}