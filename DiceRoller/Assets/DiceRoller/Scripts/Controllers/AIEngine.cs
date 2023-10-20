using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace DiceRoller
{
    public class AIEngine : MonoBehaviour
    {
		// singleton
		public static AIEngine current { get; protected set; }

		// reference
		public GameController game => GameController.current;
		public StateMachine stateMachine => StateMachine.current;

		// working variables
		private Coroutine runCoroutine = null;
		
		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		void Awake()
		{
			current = this;
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
		}

		/// <summary>
		/// OnDestroy is called when the game object is destroyed.
		/// </summary>
		void OnDestroy()
		{
			current = null;
		}

		// ========================================================= Properties (IsRunning) =========================================================

		/// <summary>
		/// Flag for if the ai engine is currently running.
		/// </summary>
		public bool IsRunning
		{
			get
			{
				return runCoroutine != null;
			}
		}

		// ========================================================= Functionality =========================================================

		/// <summary>
		/// Start the ai engine playing as a particular player.
		/// </summary>
		public void RunAs(Player player)
        {
			if (runCoroutine != null)
			{
				StopCoroutine(runCoroutine);
			}
			if (!player.isAI)
			{
				Debug.Log("AIEngine is attempting to run as an non AI player.");
			}
			runCoroutine = StartCoroutine(RunSequence(player));
        }

		private enum PossibleActionType
		{
			
		}

		private class PossibleAction
		{
			public Unit actionBy;
			public Unit actionOn;
			public Tile moveTo;

			public List<EquipmentUsage> equipmentUsages;
			

			public int piority;

		}

		private class EquipmentUsage
		{
			public Equipment equipment;
			public List<Die> dice;
		}


		/// <summary>
		/// The script for how the ai should behave.
		/// </summary>
        private IEnumerator RunSequence(Player player)
        {
            yield return null;

			// analysis situation
			// player.Units
			// player.Dice

			List<Unit> idleUnits = new List<Unit>();
			List<Unit> actedUnits = new List<Unit>();

			idleUnits.AddRange(player.Units.Where(x => x.CurrentUnitState != Unit.UnitState.Defeated));

			//idleUnits.Sort((a, b) => { })

			foreach (Unit unit in player.Units)
			{

			}







			// end this running sequence
			runCoroutine = null;

		}
    }
}