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

		// paramaters
		[Header("Player Information")]
		[SerializeField]
		private List<Player> players = new List<Player>();

		// references
		private UIController uiController => UIController.current;
		private StateMachine stateMachine => StateMachine.current;
		private DiceThrower diceThrower => DiceThrower.current;

		// working variables
		private Dictionary<int, Player> playersDict = new Dictionary<int, Player>();
		private List<Turn> previousTurns = new List<Turn>();

		// events
		public event Action OnPlayerChanged = () => { };
		public event Action OnTurnChanged = () => { };

		// ========================================================= Properties =========================================================
		
		/// <summary>
		/// Current player that is active within this turn.
		/// </summary>
		public Player CurrentPlayer
		{
			get
			{
				return _currentPlayer;
			}
			private set
			{
				if (_currentPlayer != value)
				{
					_currentPlayer = value;
					OnPlayerChanged.Invoke();
				}
			}
		}
		private Player _currentPlayer = null;

		/// <summary>
		/// Current turn the game is in.
		/// </summary>
		public Turn CurrentTurn
		{
			get
			{
				return _currentTurn;
			}
			private set
			{
				if (_currentTurn != value)
				{
					_currentTurn = value;
					OnTurnChanged.Invoke();
				}
			}
		}
		private Turn _currentTurn = null;

		// ========================================================= Inquiries =========================================================

		/// <summary>
		/// Retrieve a list of all players.
		/// </summary>
		public IReadOnlyCollection<Player> GetAllPlayers()
		{
			return players.AsReadOnly();
		}

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

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		private void Awake()
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
		private void Start()
		{
			RegisterStateBehaviours();
			StartGame();
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Application.Quit();
			}
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
			DeregisterStateBehaviours();
			current = null;
		}

		// ========================================================= General Behaviour =========================================================

		/// <summary>
		/// Start the game from the first player.
		/// </summary>
		private void StartGame()
		{
			stateMachine.ChangeState(SMState.StartTurn);
		}

		// ========================================================= State Machine Behaviour =========================================================

		/// <summary>
		/// Register all state behaviour to the centralized state machine.
		/// </summary>
		private void RegisterStateBehaviours()
		{
			stateMachine.Register(gameObject, this, SMState.StartTurn, new StartTurnSB(this));
			stateMachine.Register(gameObject, this, SMState.EndTurn, new EndTurnSB(this));
		}

		/// <summary>
		/// Deregister all state behaviours to the centralized state machine.
		/// </summary>
		private void DeregisterStateBehaviours()
		{
			if (stateMachine != null)
			{
				stateMachine.DeregisterAll(this);
			}
		}

		// ========================================================= Start Turn State =========================================================

		private class StartTurnSB : StateBehaviour
		{
			// host reference
			private GameController self;

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
				// create new turn entry and fill in turn information
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

				// set current turn as the newly created turn entry
				self.CurrentTurn = turn;

				// select the next player and set it as current player
				self.CurrentPlayer = self.players.Find(x => x.id == turn.playerId);

				// reset remaining throw
				self.diceThrower.ResetRemainingThrow(self.CurrentPlayer.throws);

				// wait for start turn animation and change state
				self.StartCoroutine(WaitForAnimationAndChangeState());
			}

			private IEnumerator WaitForAnimationAndChangeState()
			{
				yield return self.uiController.prompt.StartTurnAnimation(self.CurrentPlayer, self.CurrentTurn);
				stateMachine.ChangeState(SMState.Navigation);
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

		// ========================================================= Navigation State =========================================================

		/// <summary>
		/// End the current turn and start the next turn.
		/// </summary>
		public void ProgressTurn()
		{
			if (stateMachine.State == SMState.Navigation)
			{
				stateMachine.ChangeState(SMState.EndTurn);
			}
		}


		// ========================================================= End Turn State =========================================================

		private class EndTurnSB : StateBehaviour
		{
			// host reference
			private GameController self;

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
				// store the current turn
				self.previousTurns.Add(self.CurrentTurn);

				// wait for end turn animation and change state
				self.StartCoroutine(WaitForAnimationAndChangeState());
			}

			private IEnumerator WaitForAnimationAndChangeState()
			{
				yield return self.uiController.prompt.EndTurnAnimation(self.CurrentPlayer, self.CurrentTurn);
				stateMachine.ChangeState(SMState.StartTurn);
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

	}
}
