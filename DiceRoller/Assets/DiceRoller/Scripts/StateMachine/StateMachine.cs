using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
	public abstract class StateBehaviour
	{
		// references
		protected GameController game { get { return GameController.current; } }
		protected StateMachine stateMachine { get { return StateMachine.current; } }
		protected Board board { get { return Board.current; } }

		// events
		public abstract void OnStateEnter();
		public abstract void OnStateUpdate();
		public abstract void OnStateExit();
	}

	public class StateMachine : MonoBehaviour
	{
		protected struct HostedBehaviour
		{
			public GameObject gameObject;
			public object host;
			public StateBehaviour stateBehaviour;
		}

		// singleton
		public static StateMachine current { get; protected set; }

		// working variables
		protected Dictionary<SMState, List<HostedBehaviour>> stateBehaviours = new Dictionary<SMState, List<HostedBehaviour>>();
		protected SMState nextState = SMState.None;
		//protected StateParams nextStateParams;

		public SMState CurrentState { get; protected set; } = SMState.None;
		public event Action OnStateChanged = () => { };
		//public StateParams Params { get; protected set; }

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

		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected void Update()
		{
			RunStateMachine();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected void OnDestroy()
		{
			current = null;
		}

		// ========================================================= Binding =========================================================

		/// <summary>
		/// Register a state machine behaviour to the game controller.
		/// </summary>
		public void Register(GameObject gameObject, object host, SMState state, StateBehaviour stateBehaviour)
		{
			if (!stateBehaviours.ContainsKey(state))
				stateBehaviours[state] = new List<HostedBehaviour>();
			stateBehaviours[state].Add(new HostedBehaviour() { gameObject = gameObject, host = host, stateBehaviour = stateBehaviour });
		}

		/// <summary>
		/// Deregister a state machine behaviour from the game controller.
		/// </summary>
		public void DeregisterAll(object host)
		{
			foreach (KeyValuePair<SMState, List<HostedBehaviour>> kvp in stateBehaviours)
			{
				kvp.Value.RemoveAll(x => x.host == host);
			}
		}

		// ========================================================= Behaviour =========================================================
		/// <summary>
		/// Request a change on the state along with the parameters if needed.
		/// </summary>
		public void ChangeState(SMState state)
		{
			nextState = state;
		}
		/*
		public void ChangeState(State state, StateParams stateParams)
		{
			nextState = state;
			nextStateParams = stateParams;
		}
		*/

		/// <summary>
		/// Run the state machine and drive all registered state machine behaviour.
		/// </summary>
		protected void RunStateMachine()
		{
			// state transition
			if (nextState != SMState.None)
			{
				// invoke all OnStateExit on all registered
				if (stateBehaviours.ContainsKey(CurrentState))
				{
					stateBehaviours[CurrentState].ForEach(x =>
					{
						if (x.gameObject != null && x.gameObject.activeInHierarchy)
							x.stateBehaviour.OnStateExit();
					});
				}

				// change the current state and update params, reset flag
				CurrentState = nextState;
				//Params = nextStateParams;
				nextState = SMState.None;

				// invoke all OnStateEnter on all registered
				if (stateBehaviours.ContainsKey(CurrentState))
				{
					stateBehaviours[CurrentState].ForEach(x =>
					{
						if (x.gameObject != null && x.gameObject.activeInHierarchy)
							x.stateBehaviour.OnStateEnter();
					});
				}

				OnStateChanged.Invoke();
			}

			// state update
			if (stateBehaviours.ContainsKey(CurrentState))
			{
				stateBehaviours[CurrentState].ForEach(x =>
				{
					if (x.gameObject != null && x.gameObject.activeInHierarchy)
						x.stateBehaviour.OnStateUpdate();
				});
			}
		}
	}
}