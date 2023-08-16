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

    public enum State
    {
        None,
		StartTurn,
        Navigation,
        DiceThrow,
        UnitMoveSelect,
        UnitMovement,
        DiceAttackSelect,
        DiceAttack,
    }

	public struct StateParams
	{
		public int team;
		public Unit unit;
		public List<Tile> startingTiles;
		public List<Tile> path;
		public Die dice;
		public Unit diceTarget;
	}

	public class StateMachine : MonoBehaviour
    {
		protected struct HostBehaviour
		{
			public MonoBehaviour host;
			public StateBehaviour stateBehaviour;
		}

		// singleton
		public static StateMachine current { get; protected set; }

		// working variables
		protected Dictionary<State, List<HostBehaviour>> stateBehaviours = new Dictionary<State, List<HostBehaviour>>();
        protected State nexState = State.None;
        protected StateParams nextStateParams;

        public State Current { get; protected set; } = State.None;
        public StateParams Params { get; protected set; }

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
		public void RegisterStateBehaviour(MonoBehaviour host, State state, StateBehaviour stateBehaviour)
		{
			if (!stateBehaviours.ContainsKey(state))
				stateBehaviours[state] = new List<HostBehaviour>();
			stateBehaviours[state].Add(new HostBehaviour() { host = host, stateBehaviour = stateBehaviour });
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
		public void ChangeState(State state, StateParams stateParams)
		{
			nexState = state;
			nextStateParams = stateParams;
		}

		/// <summary>
		/// Run the state machine and drive all registered state machine behaviour.
		/// </summary>
		protected void RunStateMachine()
		{
			// state transition
			if (nexState != Current)
			{
				// invoke all OnStateExit on all registered
				if (stateBehaviours.ContainsKey(Current))
				{
					stateBehaviours[Current].ForEach(x => {
						if (x.host.gameObject != null && x.host.gameObject.activeInHierarchy)
							x.stateBehaviour.OnStateExit();
					});
				}

				// change the current state and update params
				Current = nexState;
				Params = nextStateParams;

				// invoke all OnStateEnter on all registered
				if (stateBehaviours.ContainsKey(Current))
				{
					stateBehaviours[Current].ForEach(x => {
						if (x.host.gameObject != null && x.host.gameObject.activeInHierarchy)
							x.stateBehaviour.OnStateEnter();
					});
				}
			}

			// state update
			if (stateBehaviours.ContainsKey(Current))
			{
				stateBehaviours[Current].ForEach(x => {
					if (x.host.gameObject != null && x.host.gameObject.activeInHierarchy)
						x.stateBehaviour.OnStateUpdate();
				});
			}
		}
	}
}