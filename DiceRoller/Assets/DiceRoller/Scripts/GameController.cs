using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleMaskCutoff;
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

    public class GameController : MonoBehaviour
    {
        // singleton
        public static GameController Instance { get; protected set; }

        // working variables
        protected Dictionary<State, List<Tuple<MonoBehaviour, IStateBehaviour>>> stateBehaviours = new Dictionary<State, List<Tuple<MonoBehaviour, IStateBehaviour>>>();
        protected State nexState = State.None;
        protected List<object> nextStateParams = new List<object>();
        public State CurrentState { get; protected set; } = State.None;
        public List<object> StateParams { get; protected set; } = new List<object>();

        // ========================================================= Monobehaviour Methods =========================================================

        /// <summary>
        /// Awake is called when the game object was created. It is always called before start and is 
        /// independent of if the game object is active or not.
        /// </summary>
        void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Start is called before the first frame update and/or the game object is first active.
        /// </summary>
        void Start()
        {
            Application.targetFrameRate = 60;
            ChangeState(State.Navigation);
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            RunStateMachine();
        }

        /// <summary>
        /// OnDestroy is called when an game object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Instance = null;
        }

        // ========================================================= Input Management =========================================================

        /// <summary>
        /// Register a state machine behaviour to the game controller.
        /// </summary>
        public void RegisterStateBehaviour(MonoBehaviour host, State state, IStateBehaviour stateBehaviour)
        {
            if (!stateBehaviours.ContainsKey(state))
                stateBehaviours[state] = new List<Tuple<MonoBehaviour, IStateBehaviour>>();
            stateBehaviours[state].Add(new Tuple<MonoBehaviour, IStateBehaviour>(host, stateBehaviour));
        }

        /// <summary>
        /// Deregister a state machine behaviour from the game controller.
        /// </summary>
        public void DeregisterStateBehaviour(MonoBehaviour host)
        {
            foreach (KeyValuePair<State, List<Tuple<MonoBehaviour, IStateBehaviour>>> kvp in stateBehaviours)
            {
                kvp.Value.RemoveAll(x => x.Item1 == host);
            }
        }

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
                if (stateBehaviours.ContainsKey(CurrentState))
                {
                    foreach (Tuple<MonoBehaviour, IStateBehaviour> tuple in stateBehaviours[CurrentState])
                    {
                        tuple.Item2.OnStateExit();
                    }
                }
                CurrentState = nexState;
                StateParams.Clear();
                StateParams.AddRange(nextStateParams);
                if (stateBehaviours.ContainsKey(CurrentState))
                {
                    foreach (Tuple<MonoBehaviour, IStateBehaviour> tuple in stateBehaviours[CurrentState])
                    {
                        tuple.Item2.OnStateEnter();
                    }
                }
            }

            // state update
            if (stateBehaviours.ContainsKey(CurrentState))
            {
                foreach (Tuple<MonoBehaviour, IStateBehaviour> tuple in stateBehaviours[CurrentState])
                {
                    tuple.Item2.OnStateUpdate();
                }
            }
        }
    }
}
