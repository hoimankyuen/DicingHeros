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
		protected DiceThrower diceThrower { get { return DiceThrower.current; } }

		// paramaters
		[SerializeField]
		protected int totalPlayers = 2;
		[SerializeField]
		protected List<Player> players = new List<Player>();

		// properties
		public Player CurrentPlayer { get; protected set; } = null;
		public Turn CurrentTurn { get; protected set; } = null;

		// working variables
		protected List<Turn> previousTurns = new List<Turn>();
		protected Dictionary<int, Player> playersDict = new Dictionary<int, Player>();

		// events
		public Action onPlayerChanged = () => {};
		public Action onTurnChanged = () => { };

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected void Awake()
		{
			current = this;

			foreach (Player player in players)
			{
				playersDict[player.id] = player;
			}

			Application.targetFrameRate = 60;
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected void Start()
		{
			RegisterStateBehaviours();
			stateMachine.ChangeState(State.StartTurn, new StateParams() { player = players[0] });
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

		// ========================================================= Information Handling =========================================================

		/// <summary>
		/// Retrieve a player object by its id.
		/// </summary>
		public Player GetPlayerById(int id)
		{
			if (playersDict.ContainsKey(id))
			{
				return playersDict[id];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Retrieve a list of all players.
		/// </summary>
		public IReadOnlyCollection<Player> GetAllPlayers()
		{
			return players.AsReadOnly();
		}

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		protected void RegisterStateBehaviours()
		{
			stateMachine.Register(this, State.StartTurn, new StartTurnSB(this));
			stateMachine.Register(this, State.EndTurn, new EndTurnSB(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		protected void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
				stateMachine.DeregisterAll(this);
		}

		// ========================================================= Start Turn State =========================================================

		protected class StartTurnSB : StateBehaviour
		{
			protected GameController self;

			/// <summary>
			/// Constructor.
			/// </summary>
			public StartTurnSB(GameController self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				// create new turn entry and find turn information
				Turn turn = new Turn();
				int maxTurnId = self.previousTurns.Count > 0 ? self.previousTurns.Max(x => x.id) : -1;
				int maxTurnNumber = self.previousTurns.Count > 0 ? self.previousTurns.Max(x => x.turnNumber) : 1;
				IEnumerable<int> movedPlayerIds = self.previousTurns.Where(x => x.turnNumber == maxTurnNumber).Select(x => x.playerId);
				IEnumerable<int> remainingPlayerIds = self.players.Select(x => x.id).Except(movedPlayerIds);
				if (remainingPlayerIds.Count() == 0)
				{
					// all player has taken turn, increase turn number and start from the first player again
					turn.id = maxTurnId + 1;
					turn.turnNumber = maxTurnNumber + 1;
					turn.playerId = self.players[0].id;
				}
				else
				{
					// proceed to the next player
					turn.id = maxTurnId + 1;
					turn.turnNumber = maxTurnNumber;
					turn.playerId = remainingPlayerIds.First();
				}
				self.CurrentTurn = turn;
				self.onTurnChanged.Invoke();

				// change the current player
				self.CurrentPlayer = self.players.Find(x => x.id == turn.playerId);
				self.onPlayerChanged.Invoke();

				// reset remaining throw
				self.diceThrower.ResetRemainingThrow(self.CurrentPlayer.throws);

				// change to navigation state
				stateMachine.ChangeState(State.Navigation, new StateParams() { player = game.CurrentPlayer });
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
			}
		}


		// ========================================================= End Turn State =========================================================

		protected class EndTurnSB : StateBehaviour
		{
			protected GameController self;

			/// <summary>
			/// Constructor.
			/// </summary>
			public EndTurnSB(GameController self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				self.previousTurns.Add(self.CurrentTurn);
				self.CurrentTurn = null;

				stateMachine.ChangeState(State.StartTurn, new StateParams() { });
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is in the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
			}
		}

		// ========================================================= Other =========================================================

		public void ProgressTurn()
		{
			if (stateMachine.Current == State.Navigation)
			{
				stateMachine.ChangeState(State.EndTurn, new StateParams() { player = stateMachine.Params.player });;
			}
		}
	}
}
