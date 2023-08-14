using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleMaskCutoff;
using UnityEngine;

namespace DiceRoller
{

	public class GameController : MonoBehaviour
	{
		// singleton
		public static GameController current { get; protected set; }

		// references
		protected StateMachine stateMachine { get { return StateMachine.current; } }
		protected int totalTeams = 1;
		protected int currentTeam = -1;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected void Awake()
		{
			current = this;
			Application.targetFrameRate = 60;
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected void Start()
		{
			RegisterStateBehaviours();
			stateMachine.ChangeState(State.StartTurn, 0);
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

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.RegisterStateBehaviour(this, State.StartTurn, new StartTurnStateBehaviour(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		protected void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
				stateMachine.DeregisterStateBehaviour(this);
		}

		// ========================================================= Start Turn State =========================================================

		protected class StartTurnStateBehaviour : IStateBehaviour
		{
			protected readonly GameController game = null;
			protected StateMachine stateMachine { get { return StateMachine.current; } }

			/// <summary>
			/// Constructor.
			/// </summary>
			public StartTurnStateBehaviour(GameController game)
			{
				this.game = game;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public void OnStateEnter()
			{
				stateMachine.ChangeState(State.Navigation, game.currentTeam);
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public void OnStateUpdate()
			{
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public void OnStateExit()
			{
			}
		}

		// ========================================================= Other =========================================================

		public void ProgressTurn()
		{
			if (stateMachine.CurrentState == State.Navigation)
			{
				currentTeam = (currentTeam + 1) % totalTeams;
				stateMachine.ChangeState(State.StartTurn, currentTeam);
			}
		}

	}
}
