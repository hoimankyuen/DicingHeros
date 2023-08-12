using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public interface IStateBehaviour
    {
        void OnStateEnter();
        void OnStateUpdate();
        void OnStateExit();
    }

    public enum State
    {
        None,
        Navigation,
        DiceThrow,
        UnitMovementSelection,
        UnitMovement,
        DiceAttackSelection,
        DiceAttack,
    }

    public class StateMachine : MonoBehaviour
    {
		protected struct HostBehaviour
		{
			public MonoBehaviour host;
			public IStateBehaviour stateBehaviour;
			public HostBehaviour(MonoBehaviour host, IStateBehaviour stateBehaviour)
			{
				this.host = host;
				this.stateBehaviour = stateBehaviour;
			}
		}


		// singleton
		public static StateMachine Instance { get; protected set; }

		// working variables
		protected Dictionary<State, List<HostBehaviour>> stateBehaviours = new Dictionary<State, List<HostBehaviour>>();
        protected State nexState = State.None;
        protected List<object> nextStateParams = new List<object>();

        public State CurrentState { get; protected set; } = State.None;
        public List<object> StateParams { get; protected set; } = new List<object>();

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
			ChangeState(State.Navigation);
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
			Instance = null;
		}

		// ========================================================= Binding =========================================================

		/// <summary>
		/// Register a state machine behaviour to the game controller.
		/// </summary>
		public void RegisterStateBehaviour(MonoBehaviour host, State state, IStateBehaviour stateBehaviour)
		{
			if (!stateBehaviours.ContainsKey(state))
				stateBehaviours[state] = new List<HostBehaviour>();
			stateBehaviours[state].Add(new HostBehaviour(host, stateBehaviour));
		}

		/// <summary>
		/// Deregister a state machine behaviour from the game controller.
		/// </summary>
		public void DeregisterStateBehaviour(MonoBehaviour host)
		{
			foreach (KeyValuePair<State, List<HostBehaviour>> kvp in stateBehaviours)
			{
				kvp.Value.RemoveAll(x => x.host == host);
			}
		}

		// ========================================================= Behaviour =========================================================
		/// <summary>
		/// Request a change on the state along with the parameters if needed.
		/// </summary>
		public void ChangeState(State state, params object[] stateParams)
		{
			nexState = state;
			nextStateParams.Clear();
			nextStateParams.AddRange(stateParams);
		}

		/// <summary>
		/// Run the state machine and drive all registered state machine behaviour.
		/// </summary>
		protected void RunStateMachine()
		{
			// state transition
			if (nexState != CurrentState)
			{
				// invoke all OnStateExit on all registered
				if (stateBehaviours.ContainsKey(CurrentState))
				{
					stateBehaviours[CurrentState].ForEach(x => {
						if (x.host.gameObject != null && x.host.gameObject.activeInHierarchy)
							x.stateBehaviour.OnStateExit();
					});
				}

				// change the current state and update params
				CurrentState = nexState;
				StateParams.Clear();
				StateParams.AddRange(nextStateParams);
				nextStateParams.Clear();

				// invoke all OnStateEnter on all registered
				if (stateBehaviours.ContainsKey(CurrentState))
				{
					stateBehaviours[CurrentState].ForEach(x => {
						if (x.host.gameObject != null && x.host.gameObject.activeInHierarchy)
							x.stateBehaviour.OnStateEnter();
					});
				}
			}

			// state update
			if (stateBehaviours.ContainsKey(CurrentState))
			{
				stateBehaviours[CurrentState].ForEach(x => {
					if (x.host.gameObject != null && x.host.gameObject.activeInHierarchy)
						x.stateBehaviour.OnStateUpdate();
				});
			}
		}
	}
}