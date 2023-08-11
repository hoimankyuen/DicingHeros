using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleMaskCutoff;
using UnityEngine;

namespace DiceRoller
{
    public interface IStateMachine
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
        protected List<IStateMachine> stateMachines = new List<IStateMachine>();
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
        public void RegisterStateMachine(IStateMachine stateMachine)
        {
            stateMachines.Add(stateMachine);
        }

        /// <summary>
        /// Deregister a state machine behaviour from the game controller.
        /// </summary>
        public void DeregisterStateMachine(IStateMachine stateMachine)
        {
            stateMachines.Remove(stateMachine);
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
                foreach (IStateMachine stateMachine in stateMachines)
                {
                    stateMachine.OnStateExit();
                }
                CurrentState = nexState;
                StateParams.Clear();
                StateParams.AddRange(nextStateParams);
                foreach (IStateMachine stateMachine in stateMachines)
                {
                    stateMachine.OnStateEnter();
                }
            }

            // state update
            foreach (IStateMachine stateMachine in stateMachines)
            {
                stateMachine.OnStateUpdate();
            }
        }
    }
}
