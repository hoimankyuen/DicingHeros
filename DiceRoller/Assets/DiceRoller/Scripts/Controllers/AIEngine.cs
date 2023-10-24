using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace DiceRoller
{
    public class AIEngine : MonoBehaviour
    {
		private class PossibleAction
		{
			public class EquipmentUsage
			{
				public Equipment Equipment { get; private set; } = null;
				public IEnumerable<Die> Dice { get { return _Dice.AsReadOnly(); } }
				private readonly List<Die> _Dice = new List<Die>();

				/// <summary>
				/// Constructor.
				/// </summary>
				public EquipmentUsage(Equipment equipment, IEnumerable<Die> dice)
				{
					Equipment = equipment;
					_Dice.AddRange(dice);
				}
			}

			public Unit Attacker { get; private set; } = null;

			public IEnumerable<EquipmentUsage> MovementEquipmentUsages { get { return _MovementEquipmentUsages.AsReadOnly(); } }
			private readonly List<EquipmentUsage> _MovementEquipmentUsages = new List<EquipmentUsage>();

			public Tile Position { get; private set; } = null;
			public float DistanceToEnemyCenter { get; private set; } = float.PositiveInfinity;

			public IEnumerable<EquipmentUsage> AttackEquipmentUsages { get { return _AttackEquipmentUsages.AsReadOnly(); } }
			private readonly List<EquipmentUsage> _AttackEquipmentUsages = new List<EquipmentUsage>();

			public Unit Target { get; private set; } = null;
			public int DamageDone { get; private set; } = 0;
			public float BeforeHealthPercentage { get; private set; } = float.PositiveInfinity;
			public float AfterHealthPercentage { get; private set; } = float.PositiveInfinity;

			/// <summary>
			/// Constructor.
			public PossibleAction(Unit attacker)
			{
				Attacker = attacker;
			}

			/// <summary>
			/// Add a equipment to be used at a movement action.
			/// </summary>
			public void AddMovementEquipmentUsage(Equipment equipment, IEnumerable<Die> dice)
			{
				_MovementEquipmentUsages.Add(new EquipmentUsage(equipment, dice));
			}

			/// <summary>
			///  Set the movement action.
			/// </summary>
			public void SetMovement(Tile position, float distanceToEnemyCenter)
			{
				Position = position;
				DistanceToEnemyCenter = distanceToEnemyCenter;
			}

			/// <summary>
			/// Add a equipment to be used at an attack action.
			/// </summary>
			public void AddAttackEquipmentUsage(Equipment equipment, IEnumerable<Die> dice)
			{
				_AttackEquipmentUsages.Add(new EquipmentUsage(equipment, dice));
			}

			/// <summary>
			///  Set the attack action.
			/// </summary>
			public void SetAttack( Unit target, int damageDone, float beforeHealthPercentage, float afterHealthPercentage)
			{

				Target = target;
				DamageDone = damageDone;
				BeforeHealthPercentage = beforeHealthPercentage;
				AfterHealthPercentage = afterHealthPercentage;
			}

			/// <summary>
			/// Get a readable description of this possible action.
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				if (Attacker == null)
					return "Invalid (Without Attacker)";
				else if (Position == null && Target == null)
					return string.Format("{0} skips turn", Attacker.name);
				else
				{
					string message = string.Format("{0} ", Attacker.name);
					if (Position != null)
					{
						if (MovementEquipmentUsages.Count() > 0)
						{
							message += "using ";
							for (int i = 0; i < MovementEquipmentUsages.Count(); i++)
							{
								message += string.Format("{0} ", MovementEquipmentUsages.ElementAt(i).Equipment.DisplayableName);
								if (i < MovementEquipmentUsages.Count() - 1)
								{
									message += ", ";
								}
							}
							message += "to ";
						}
						message += string.Format("| move to tile {0} ", Position.boardPos);
					}
					if (Target != null)
					{
						if (AttackEquipmentUsages.Count() > 0)
						{
							message += "using ";
							for (int i = 0; i < AttackEquipmentUsages.Count(); i++)
							{
								message += string.Format("{0} ", AttackEquipmentUsages.ElementAt(i).Equipment.DisplayableName);
								if (i < AttackEquipmentUsages.Count() - 1)
								{
									message += ", ";
								}
							}
							message += "to ";
						}
						message += string.Format("| attack {0} for {1} damage to {2} health remaining", Target.name, DamageDone, AfterHealthPercentage);
					}
					return message;
				}
			}
		}

		// singleton
		public static AIEngine current { get; protected set; }

		// reference
		public GameController game => GameController.current;
		public StateMachine stateMachine => StateMachine.current;
		public DiceThrower diceThrower => DiceThrower.current;
		public Board board => Board.current;

		// working variables
		private Coroutine runCoroutine = null;

		// temp working variables
		private readonly List<Die> emptyDieList = new List<Die>();

		// debug settings
		private readonly bool isDebuging = true;

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
				ShowDebugMessage("AIEngine: AIEngine is attempting to run as an non AI player.");
			}
			runCoroutine = StartCoroutine(RunSequence(player));
        }

		/// <summary>
		/// Print a debug message if the isDebuging flag is set.
		/// </summary>
		private void ShowDebugMessage(string message)
		{
			if (isDebuging)
			{
				Debug.Log(message);
			}
		}

		// ========================================================= Main Action Sequence =========================================================

		/// <summary>
		/// The script for how the ai should behave.
		/// </summary>
		private IEnumerator RunSequence(Player player)
		{
			ShowDebugMessage("AIEngine: Run started!");

			// take control from the player
			InputUtils.EnablePressSimulation(true);

			yield return new WaitUntil(() => stateMachine.State == SMState.Navigation);
			yield return new WaitForSeconds(1);

			// throw dice first no matter what
			ShowDebugMessage("AIEngine: Throwing dice ...");
			yield return ThrowDiceSequence(
				player.Dice.Where(x => x.CurrentDieState == Die.DieState.Holding || x.CurrentDieState == Die.DieState.Casted),
				Vector3.zero,
				Vector3.left,
				1f);

			// retrieve player items, both self and enemy
			List<Unit> selfUnits = new List<Unit>();
			List<Die> selfDice = new List<Die>();
			List<Unit> enemyUnits = new List<Unit>();
			List<PossibleAction> possibleActions = new List<PossibleAction>();

			// do actions
			for (int i = 0; i < player.Units.Count; i++)
			{
				ShowDebugMessage("AIEngine: Running iteration " + i + " ...");
				// retrieve the current item states
				selfUnits.Clear();
				selfUnits.AddRange(player.Units.Where(x => x.CurrentUnitState == Unit.UnitState.Standby));
				selfDice.Clear();
				selfDice.AddRange(player.Dice.Where(x => x.CurrentDieState == Die.DieState.Casted));
				enemyUnits.Clear();
				foreach (Player enemyPlayer in game.GetAllPlayers().Where(x => x != player))
					enemyUnits.AddRange(enemyPlayer.Units.Where(x => x.CurrentUnitState != Unit.UnitState.Defeated && x.OccupiedTiles.Count > 0));

				// do action for each standby units
				if (selfUnits.Count > 0)
				{
					// select and do the best action available
					float startTime = Time.time;
					ShowDebugMessage("AIEngine: Finding all possible actions ...");
					yield return GetAllPossibleActionsAsync(selfUnits, enemyUnits, selfDice, possibleActions);
					ShowDebugMessage("AIEngine: " + possibleActions.Count + " possible actions found! (Time Elasped: " + (int)((Time.time - startTime) * 1000) + "ms)");
					ShowDebugMessage("AIEngine: Selecting best action...");
					PossibleAction bestAction = SelectBestAction(possibleActions);

					ShowDebugMessage("AIEngine: " + (bestAction == null ? "No action selected.Skipping iteration..." : "Running selected action = " + bestAction));
					yield return UnitActionSequence(bestAction);
					ShowDebugMessage("AIEngine: Action completed!");
				}
				else
				{
					break;
				}
			}

			// end the turn
			ShowDebugMessage("AIEngine: All unit has acted. Progressing turn ...");
			game.ProgressTurn();
			
			// resume control to the player
			InputUtils.EnablePressSimulation(false);

			// end this running sequence
			yield return null;
			runCoroutine = null;

			ShowDebugMessage("AIEngine: Run completed!");
		}

		// ========================================================= Individual Action Sequence =========================================================

		/// <summary>
		/// The complete sequence for throwing a set number of dice.
		private IEnumerator ThrowDiceSequence(IEnumerable<Die> dice, Vector3 position, Vector3 direction, float power)
		{
			// select all dice
			foreach (Die die in dice)
			{
				die.OnAIMousePress(0);
				yield return new WaitForSeconds(0.25f);
			}
			yield return new WaitForSeconds(0.25f);
			yield return new WaitUntil(() => stateMachine.State == SMState.DiceActionSelect);
			yield return null;

			// perform throw action
			diceThrower.StartAIThrow(position);
			yield return new WaitForSeconds(0.5f);

			float startTime = Time.time;
			while (Time.time - startTime < 0.5f)
			{
				diceThrower.MidAIThrow(direction, Mathf.Lerp(0, power, (Time.time - startTime) / 0.5f));
				yield return null;
			}
			yield return new WaitForSeconds(0.25f);

			diceThrower.CompleteAIThrow(dice);

			// wait for throw to finish
			yield return new WaitUntil(() => stateMachine.State == SMState.DiceThrow);
			yield return null;
			yield return new WaitUntil(() => stateMachine.State == SMState.Navigation);
			yield return null;
		}

		/// <summary>
		/// The complete sequence for perform all necessary steps of a possible action.
		/// </summary>
		private IEnumerator UnitActionSequence(PossibleAction action)
		{
			// select unit
			action.Attacker.OnAIMouseEnter();
			yield return new WaitForSeconds(0.25f);
			action.Attacker.OnAIMousePress(0);
			yield return new WaitForSeconds(0.25f);
			action.Attacker.OnAIMouseExit();
			yield return null;
			yield return new WaitUntil(() => stateMachine.State == SMState.UnitMoveSelect);

			// check fulfillment again
			bool useMovementEquipment = false;
			foreach (PossibleAction.EquipmentUsage equipmentUsage in action.MovementEquipmentUsages)
			{
				for (int i = 0; i < equipmentUsage.Dice.Count(); i++)
				{
					Die die = equipmentUsage.Dice.ElementAt(i);
					EquipmentDieSlot slot = equipmentUsage.Equipment.DieSlots[i];
					if (!slot.IsFulfillBy(die))
					{
						useMovementEquipment = true;
						break;
					}
				}
			}

			// skip move if needed
			if (action.Position == null || useMovementEquipment || !action.Attacker.MovableArea.Contains(action.Position))
			{
				action.Attacker.SkipMoveSelect();
				yield return new WaitUntil(() => stateMachine.State == SMState.Navigation);
				yield return null;
				yield break;
			}

			// assign dice to attack equipments
			foreach (PossibleAction.EquipmentUsage equipmentUsage in action.MovementEquipmentUsages)
			{
				for (int i = 0; i < equipmentUsage.Dice.Count(); i++)
				{
					Die die = equipmentUsage.Dice.ElementAt(i);
					EquipmentDieSlot slot = equipmentUsage.Equipment.DieSlots[i];
					if (!slot.IsFulfillBy(die))
					{
						useMovementEquipment = true;
						break;
					}

					die.OnAIMouseEnter();
					yield return new WaitForSeconds(0.25f);
					die.OnAIMousePress(0);
					yield return new WaitForSeconds(0.25f);
					die.OnAIMouseExit();
					yield return new WaitForSeconds(0.5f);

					slot.OnAIMouseEnter();
					yield return new WaitForSeconds(0.25f);
					slot.OnAIMouseCompletetDrag(0);
					yield return new WaitForSeconds(0.25f);
					slot.OnAIMouseExit();
					yield return new WaitForSeconds(0.5f);
				}
			}

			// select target tile and initiate movement
			board.SetAITileHover(action.Position);
			yield return new WaitForSeconds(0.5f);
			InputUtils.SimulatePress(0, true);
			yield return null;
			InputUtils.SimulatePress(0, false);

			// wait for move to finish
			yield return new WaitUntil(() => stateMachine.State == SMState.UnitMove);
			yield return null;
			yield return new WaitUntil(() => stateMachine.State == SMState.UnitAttackSelect);
			yield return null;

			// check the fulfillment again
			bool useAttackEquipmentFailed = false;
			int range = action.Attacker.AttackRange;
			AttackAreaRule areaRule = action.Attacker.AttackAreaRule;
			foreach (PossibleAction.EquipmentUsage equipmentUsage in action.AttackEquipmentUsages)
			{
				for (int i = 0; i < equipmentUsage.Dice.Count(); i++)
				{
					Die die = equipmentUsage.Dice.ElementAt(i);
					EquipmentDieSlot slot = equipmentUsage.Equipment.DieSlots[i];
					if (!slot.IsFulfillBy(die))
					{
						useAttackEquipmentFailed = true;
						break;
					}
					if (equipmentUsage.Equipment.Type == Equipment.EquipmentType.MeleeAttack || equipmentUsage.Equipment.Type == Equipment.EquipmentType.MagicAttack)
						areaRule = equipmentUsage.Equipment.AreaRule;
					range += equipmentUsage.Equipment.AttackRangeDelta;
				}
			}
			List<Tile> attackableArea = new List<Tile>();
			board.GetTilesByRule(action.Attacker.OccupiedTiles, areaRule, range, attackableArea);

			// skip attack if needed
			if (action.Target == null || useAttackEquipmentFailed || action.Target.OccupiedTiles.Intersect(attackableArea).Count() <= 0)
			{
				action.Attacker.SkipAttackSelect();
				yield return new WaitUntil(() => stateMachine.State == SMState.Navigation);
				yield return null;
				yield break;
			}

			// assign dice to attack equipments
			foreach (PossibleAction.EquipmentUsage equipmentUsage in action.AttackEquipmentUsages)
			{
				for (int i = 0; i < equipmentUsage.Dice.Count(); i++)
				{
					Die die = equipmentUsage.Dice.ElementAt(i);
					EquipmentDieSlot slot = equipmentUsage.Equipment.DieSlots[i];

					Debug.Log("Assigning " + die.name + " to " + slot.Equipment.DisplayableName);

					die.OnAIMouseEnter();
					yield return new WaitForSeconds(0.25f);
					die.OnAIMouseStartDrag(0);
					yield return new WaitForSeconds(0.25f);
					die.OnAIMouseExit();
					yield return new WaitForSeconds(0.5f);

					slot.OnAIMouseEnter();
					yield return new WaitForSeconds(0.25f);
					die.OnAIMouseCompletetDrag(0);
					yield return new WaitForSeconds(0.25f);
					slot.OnAIMouseExit();
					yield return new WaitForSeconds(0.5f);
				}
			}

			// select target unit and inititae attack
			action.Target.OnAIMouseEnter();
			yield return new WaitForSeconds(0.25f);
			action.Target.OnAIMousePress(0);
			yield return new WaitForSeconds(0.25f);
			action.Target.OnAIMouseExit();
			yield return null;

			// wait for attack to finish
			yield return new WaitUntil(() => stateMachine.State == SMState.UnitAttack);
			yield return null;
			yield return new WaitUntil(() => stateMachine.State == SMState.Navigation);
			yield return null;
		}

		// ========================================================= Calculations ========================================================

		private IEnumerator GetAllPossibleActionsAsync(IEnumerable<Unit> selfUnits, IEnumerable<Unit> enemyUnits, IEnumerable<Die> selfDice, List<PossibleAction> results)
		{
			// calculate enemy center
			Vector3 enemyCenter = Vector3.zero;
			if (enemyUnits.Count() > 0)
			{
				foreach (Unit enemyUnit in enemyUnits)
				{
					enemyCenter += enemyUnit.transform.position;
				}
				enemyCenter = enemyCenter / enemyUnits.Count();
			}

			// cache positions
			Dictionary<Unit, Vector3> positions = new Dictionary<Unit, Vector3>();
			foreach (Player player in game.GetAllPlayers())
			{
				foreach (Unit unit in player.Units)
				{
					positions[unit] = unit.transform.position;
				}
			}

			// async execution of cpu bounded search
			Task task = Task.Run(() =>
			{
				// search for all actions
				List<Die> usingDice = new List<Die>();
				List<Tuple<Unit, Tile>> attackPossibilites = new List<Tuple<Unit, Tile>>();
				results.Clear();
				foreach (Unit selfUnit in selfUnits)
				{
					// find each attack options by magic equipment attack
					foreach (Equipment equipment in selfUnit.Equipments.Where(x => x.Type == Equipment.EquipmentType.MagicAttack))
					{
						if (DiceFulfills(selfDice, equipment, usingDice))
						{
							GetTargetableEnemies(selfUnit, enemyUnits, selfUnit.Movement, equipment.AreaRule, selfUnit.AttackRange + equipment.AttackRangeDelta, attackPossibilites);
							foreach (Tuple<Unit, Tile> attackPossibility in attackPossibilites)
							{
								int rawDamge = Mathf.Max(selfUnit.Magic + equipment.MagicDelta - attackPossibility.Item1.Defence, 0);
								int damageDone = attackPossibility.Item1.Health - rawDamge >= 0 ? rawDamge : attackPossibility.Item1.Health;

								PossibleAction possibleAction = new PossibleAction(attacker: selfUnit);

								possibleAction.SetMovement(
									position: attackPossibility.Item2,
									distanceToEnemyCenter: Vector3.Distance(attackPossibility.Item2.worldPos, enemyCenter));

								possibleAction.AddAttackEquipmentUsage(
									equipment: equipment,
									dice: usingDice);

								possibleAction.SetAttack(target: attackPossibility.Item1,
									damageDone: damageDone,
									beforeHealthPercentage: 1.0f * attackPossibility.Item1.Health / attackPossibility.Item1.maxHealth,
									afterHealthPercentage: 1.0f * (attackPossibility.Item1.Health - damageDone) / attackPossibility.Item1.maxHealth);

								results.Add(possibleAction);
							}
						}
					}

					// find each attack options by melee equipment attack
					foreach (Equipment equipment in selfUnit.Equipments.Where(x => x.Type == Equipment.EquipmentType.MeleeAttack))
					{
						if (DiceFulfills(selfDice, equipment, usingDice))
						{
							GetTargetableEnemies(selfUnit, enemyUnits, selfUnit.Movement, equipment.AreaRule, selfUnit.AttackRange + equipment.AttackRangeDelta, attackPossibilites);
							foreach (Tuple<Unit, Tile> attackPossibility in attackPossibilites)
							{
								int rawDamge = Mathf.Max(selfUnit.Melee + equipment.MeleeDelta - attackPossibility.Item1.Defence, 0);
								int damageDone = attackPossibility.Item1.Health - rawDamge >= 0 ? rawDamge : attackPossibility.Item1.Health;

								PossibleAction possibleAction = new PossibleAction(attacker: selfUnit);

								possibleAction.SetMovement(
									position: attackPossibility.Item2,
									distanceToEnemyCenter: Vector3.Distance(attackPossibility.Item2.worldPos, enemyCenter));

								possibleAction.AddAttackEquipmentUsage(
									equipment: equipment,
									dice: usingDice);

								possibleAction.SetAttack(target: attackPossibility.Item1,
									damageDone: damageDone,
									beforeHealthPercentage: 1.0f * attackPossibility.Item1.Health / attackPossibility.Item1.maxHealth,
									afterHealthPercentage: 1.0f * (attackPossibility.Item1.Health - damageDone) / attackPossibility.Item1.maxHealth);

								results.Add(possibleAction);
							}
						}
					}

					// find each attack options by melee attack without equipments
					GetTargetableEnemies(selfUnit, enemyUnits, selfUnit.Movement, selfUnit.AttackAreaRule, selfUnit.AttackRange, attackPossibilites);
					foreach (Tuple<Unit, Tile> attackPossibility in attackPossibilites)
					{

						int rawDamge = Mathf.Max(selfUnit.Melee - attackPossibility.Item1.Defence, 0);
						int damageDone = attackPossibility.Item1.Health - rawDamge >= 0 ? rawDamge : attackPossibility.Item1.Health;

						PossibleAction possibleAction = new PossibleAction(attacker: selfUnit);

						possibleAction.SetMovement(
							position: attackPossibility.Item2,
							distanceToEnemyCenter: Vector3.Distance(attackPossibility.Item2.worldPos, enemyCenter));

						possibleAction.SetAttack(target: attackPossibility.Item1,
							damageDone: damageDone,
							beforeHealthPercentage: 1.0f * attackPossibility.Item1.Health / attackPossibility.Item1.maxHealth,
							afterHealthPercentage: 1.0f * (attackPossibility.Item1.Health - damageDone) / attackPossibility.Item1.maxHealth);

						results.Add(possibleAction);
					}

					// find each move only options
					Unit nearestUnit = null;
					int shortestDistance = int.MaxValue;
					List<Tile> pathToNearestUnit = new List<Tile>();
					Tile targetPosition = null;
					foreach (Unit enemyUnit in enemyUnits)
					{
						int distance = Int2.GridDistance(selfUnit.OccupiedTiles.ElementAt(0).boardPos, enemyUnit.OccupiedTiles.ElementAt(0).boardPos);
						if (distance < shortestDistance)
						{
							nearestUnit = enemyUnit;
						}
					}
					if (nearestUnit != null)
					{
						board.GetShortestPath(selfUnit.OccupiedTiles, Unit.AllOccupiedTiles.Except(selfUnit.OccupiedTiles).Except(nearestUnit.OccupiedTiles), nearestUnit.OccupiedTiles.ElementAt(0), 20, pathToNearestUnit);
						pathToNearestUnit.RemoveAll(x => nearestUnit.OccupiedTiles.Contains(x));
						if (pathToNearestUnit.Count > 0)
						{
							targetPosition = pathToNearestUnit[Math.Min(pathToNearestUnit.Count - 1, selfUnit.Movement)];
						}
					}
					if (targetPosition != null)
					{
						PossibleAction possibleAction = new PossibleAction(attacker: selfUnit);

						possibleAction.SetMovement(
							position: targetPosition,
							distanceToEnemyCenter: Vector3.Distance(targetPosition.worldPos, enemyCenter));

						results.Add(possibleAction);
					}

					// add the skip option
					results.Add(new PossibleAction(attacker: selfUnit));
				}
			});

			// wait for search to complete
			task.Wait();
			yield return new WaitUntil(() => task.IsCompleted);
		}

		/// <summary>
		/// Select the best action by a certain criteria.
		/// </summary>
		private PossibleAction SelectBestAction(IEnumerable<PossibleAction> possibleActions)
		{
			// criteria: afterHealthPercentage > usingDice > distanceToEnemyCenter
			PossibleAction bestAction = null;
			if (possibleActions.Count() > 0)
			{
				bestAction = possibleActions.ElementAt(0);
				foreach (PossibleAction possibleAction in possibleActions)
				{
					// afterHealthPercentage
					if (possibleAction.AfterHealthPercentage < bestAction.AfterHealthPercentage)
					{
						bestAction = possibleAction;
						continue;
					}
					if (possibleAction.AfterHealthPercentage > bestAction.AfterHealthPercentage)
					{
						continue;
					}

					// die count
					int possibleActionDieCount = 0;
					foreach (PossibleAction.EquipmentUsage equipmentUsage in possibleAction.AttackEquipmentUsages)
						possibleActionDieCount += equipmentUsage.Dice.Count();
					int bestActionDieCount = 0;
					foreach (PossibleAction.EquipmentUsage equipmentUsage in bestAction.AttackEquipmentUsages)
						bestActionDieCount += equipmentUsage.Dice.Count();
					if (possibleActionDieCount < bestActionDieCount)
					{
						bestAction = possibleAction;
						continue;
					}
					if (possibleActionDieCount > bestActionDieCount)
					{
						continue;
					}

					// distanceToEnemyCenter
					if (possibleAction.DistanceToEnemyCenter < bestAction.DistanceToEnemyCenter)
					{
						bestAction = possibleAction;
						continue;
					}
					if (possibleAction.DistanceToEnemyCenter > bestAction.DistanceToEnemyCenter)
					{
						continue;
					}
				}
			}
			return bestAction;
		}

		/// <summary>
		/// Check if there is enough dice in the dice list to fulfill all requirment of a equipment.
		/// </summary>
		private bool DiceFulfills(IEnumerable<Die> dice, Equipment equipment, List<Die> selectedDice)
		{
			// initialize container
			List<List<Die>> fulfillment = new List<List<Die>>();
			selectedDice.Clear();
			for (int i = 0; i < equipment.DieSlots.Count; i++)
			{
				fulfillment.Add(new List<Die>());
				selectedDice.Add(null);
			}

			// special case for no slots
			if (equipment.DieSlots.Count == 0)
				return true;

			// sorted dice according to value
			List<Die> sortedDice = new List<Die>();
			sortedDice.AddRange(dice);
			sortedDice.Sort((a, b) => a.Value.CompareTo(b.Value));

			// calculate individual fulfillment
			for (int i = 0; i < equipment.DieSlots.Count; i++)
			{
				for (int j = 0; j < sortedDice.Count; j++)
				{ 
					if (equipment.DieSlots[i].IsFulfillBy(sortedDice[j]))
					{
						fulfillment[i].Add(sortedDice[j]);
					}
				}
			}

			// pick dice
			for (int i = 0; i < fulfillment.Count; i++)
			{
				// find the slot with the least fulfillment
				int leastFulfillmentSlot = -1;
				int leastFulfillmentSlotDieCount = int.MaxValue;
				for (int j = 0; j < fulfillment.Count; j++)
				{
					if (selectedDice[j] == null && fulfillment[j].Count < leastFulfillmentSlotDieCount && fulfillment[j].Count > 0)
					{
						leastFulfillmentSlot = j;
						leastFulfillmentSlotDieCount = fulfillment[j].Count;
					}
				}

				// pick the smallest die and remove die from other slot possiblilties
				if (leastFulfillmentSlot != -1)
				{
					Die selectedDie = fulfillment[leastFulfillmentSlot][0];
					selectedDice[leastFulfillmentSlot] = selectedDie;
					for (int j = 0; j < fulfillment.Count; j++)
					{
						fulfillment[j].Remove(selectedDie);
					}
				}
			}

			// determine fulfillment 
			if (selectedDice.Any(x => x == null))
			{
				selectedDice.Clear();
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Retrieve a list of all attackable enemies with the current stats.
		/// </summary>
		private void GetTargetableEnemies(Unit attacker, IEnumerable<Unit> enemies, int movement, AttackAreaRule rule, int range, List<Tuple<Unit, Tile>> results)
		{
			List<Tile> movableTiles = new List<Tile>();
			List<Tile> attackableTiles = new List<Tile>();
			results.Clear();

			board.GetConnectedTilesInRange(attacker.OccupiedTiles, Unit.AllOccupiedTiles.Except(attacker.OccupiedTiles), movement, movableTiles);
			foreach (Tile movableTile in movableTiles)
			{
				// search each tile for attacking since GetConnectedTilesInRange is found using BFS, no sorting is required
				board.GetTilesByRule(movableTile, rule, range, attackableTiles);
				foreach (Unit enemy in enemies)
				{
					if (!results.Any(x => x.Item1 == enemy) && attackableTiles.Intersect(enemy.OccupiedTiles).Count() > 0)
					{
						results.Add(new Tuple<Unit, Tile>(enemy, movableTile));
					}
				}
			}
		}
	}
}