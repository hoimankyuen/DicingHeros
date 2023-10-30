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
						message += string.Format("| move to tile {0} ", Position.BoardPos);
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
						message += string.Format("| attack {0} for {1} damage to {2}% health remaining", Target.name, DamageDone, AfterHealthPercentage);
					}
					return message;
				}
			}
		}

		private struct AttackInfo
		{
			public Unit unit;
			public Tile tile;
			public AttackInfo(Unit unit, Tile tile)
			{
				this.unit = unit;
				this.tile = tile;
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

		private readonly List<int> tempIntList1 = new List<int>();
		private readonly List<Die> tempDieList1 = new List<Die>();
		private readonly List<Die> tempDieList2 = new List<Die>();
		private readonly List<Equipment> tempEquipmentList1 = new List<Equipment>();
		private readonly List<AttackInfo> tempAttackInfoList = new List<AttackInfo>();

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

			yield return new WaitUntil(() => stateMachine.CurrentState == SMState.Navigation);
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

			Debug.Log("Retrieved Die:");
			foreach (Die die in selfDice)
			{
				Debug.Log(die.name + ", value = " + die.Value);
			}

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

					for (int j = 0; j < possibleActions.Count; j++)
					{
						ShowDebugMessage("AIEngine: Possible action " + (i + 1) + " : " + possibleActions[j].ToString());
					}

					ShowDebugMessage("AIEngine: " + possibleActions.Count + " possible actions found! (Time Elasped: " + (int)((Time.time - startTime) * 1000) + "ms). Selecting best action ...");
					PossibleAction bestAction = SelectBestAction(possibleActions);

					ShowDebugMessage("AIEngine: " + (bestAction == null ? "No action selected.Skipping iteration..." : "Executing selected action = " + bestAction));
					yield return UnitActionSequence(bestAction);
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
			yield return new WaitUntil(() => stateMachine.CurrentState == SMState.DiceActionSelect);
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
			yield return new WaitUntil(() => stateMachine.CurrentState == SMState.DiceThrow);
			yield return null;
			yield return new WaitUntil(() => stateMachine.CurrentState == SMState.Navigation);
			yield return null;
		}

		/// <summary>
		/// The complete sequence for perform all necessary steps of a possible action.
		/// </summary>
		private IEnumerator UnitActionSequence(PossibleAction action)
		{
			// skip empty action
			if (action == null)
				yield break;

			// select unit
			action.Attacker.OnAIMouseEnter();
			yield return new WaitForSeconds(0.25f);
			action.Attacker.OnAIMousePress(0);
			yield return new WaitForSeconds(0.25f);
			action.Attacker.OnAIMouseExit();
			yield return null;
			yield return new WaitUntil(() => stateMachine.CurrentState == SMState.UnitMoveSelect);

			// check fulfillment again
			bool useMovementEquipmentFailed = false;
			foreach (PossibleAction.EquipmentUsage equipmentUsage in action.MovementEquipmentUsages)
			{
				for (int i = 0; i < equipmentUsage.Dice.Count(); i++)
				{
					Die die = equipmentUsage.Dice.ElementAt(i);
					EquipmentDieSlot slot = equipmentUsage.Equipment.DieSlots[i];
					if (!slot.IsFulfillBy(die))
					{
						useMovementEquipmentFailed = true;
						break;
					}
				}
			}

			// skip move if needed
			if (action.Position == null || useMovementEquipmentFailed)
			{
				if (useMovementEquipmentFailed)
					ShowDebugMessage("AIEngine: Movement Failed! Reason: Equipment failed to activate.");

				action.Attacker.SkipMoveSelect();
				yield return new WaitUntil(() => stateMachine.CurrentState == SMState.Navigation);
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

			// select target tile and initiate movement
			board.SetAITileHover(action.Position);
			yield return new WaitForSeconds(0.5f);
			InputUtils.SimulatePress(0, true);
			yield return null;
			InputUtils.SimulatePress(0, false);

			// wait for move to finish
			yield return new WaitUntil(() => stateMachine.CurrentState == SMState.UnitMove);
			yield return null;
			yield return new WaitUntil(() => stateMachine.CurrentState == SMState.UnitAttackSelect);
			yield return null;

			// check the fulfillment again
			bool useAttackEquipmentFailed = false;
			int meleeRange = action.Attacker.PhysicalRange;
			int magicRange = action.Attacker.MagicalRange;
			AttackAreaRule areaRule = action.Attacker.AttackAreaRule;
			bool isMagicAttack = false;
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
					meleeRange += equipmentUsage.Equipment.PhysicalRangeDelta;
					magicRange += equipmentUsage.Equipment.MagicalRangeDelta;
					if (equipmentUsage.Equipment.Type == Equipment.EquipmentType.MagicAttack)
						isMagicAttack = true;
				}
			}
			List<Tile> attackableArea = new List<Tile>();
			board.GetTilesByRule(action.Attacker.OccupiedTiles, areaRule, isMagicAttack ? magicRange : meleeRange, attackableArea);

			// skip attack if needed
			if (action.Target == null || useAttackEquipmentFailed || action.Target.OccupiedTiles.Intersect(attackableArea).Count() <= 0)
			{
				if (useAttackEquipmentFailed)
					ShowDebugMessage("AIEngine: Attack Failed! Reason: Equipment failed to activate.");
				else if (action.Target != null && action.Target.OccupiedTiles.Intersect(attackableArea).Count() <= 0)
					ShowDebugMessage("AIEngine: Attack Failed! Reason: Target unit cannot be reached by attack.");

				action.Attacker.SkipAttackSelect();
				yield return new WaitUntil(() => stateMachine.CurrentState == SMState.Navigation);
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
			yield return new WaitUntil(() => stateMachine.CurrentState == SMState.UnitAttack);
			yield return null;
			yield return new WaitUntil(() => stateMachine.CurrentState == SMState.Navigation);
			yield return null;
		}

		// ========================================================= Possible Action Discover ========================================================

		/// <summary>
		/// Retrieve all possible actions for all units and put them in the supplied result list. This coroutine will wait until the async calculations was complete.
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

			// async execution of cpu bounded search
			Task task = Task.Run(() =>
			{
				// search for all actions
				List<int> combinations = tempIntList1;
				List<Die> usingDice = tempDieList1;
				List<Equipment> usingEquipments = tempEquipmentList1;
				List<AttackInfo> attackInfos = tempAttackInfoList;

				results.Clear();
				foreach (Unit selfUnit in selfUnits)
				{
					// find attack options of all combinations of equipment
					GetValidCombinations(selfUnit, combinations);
					foreach (int combination in combinations)
					{
						// retrieve equipment combination information
						DecodeCombination(selfUnit, combination, usingEquipments, out int movement, out int meleeRange, out int magicRange, out AttackAreaRule areaRule, out bool isMagicAttack);

						// discover attack options
						if (DiceFulfills(usingEquipments, selfDice, usingDice))
						{
							// find attack options for all target in range
							GetTargetableEnemies(selfUnit, enemyUnits, movement, areaRule, isMagicAttack ? magicRange : meleeRange, attackInfos);
							foreach (AttackInfo attackInfo in attackInfos)
							{
								results.Add(GeneratePossibleAction(selfUnit, attackInfo, enemyCenter, usingEquipments, usingDice));
							}
						}
					}

					// find each move only options
					Tile nearestTile = GetNearestMovableTileToEnemy(selfUnit, enemyUnits);
					if (nearestTile != null)
					{
						usingEquipments.Clear();
						usingDice.Clear();
						results.Add(GeneratePossibleAction(selfUnit, new AttackInfo(null, nearestTile), enemyCenter, usingEquipments, usingDice));
					}

					// add the skip option
					results.Add(GeneratePossibleAction(selfUnit, new AttackInfo(null, null), enemyCenter, usingEquipments, usingDice));
				}
			});

			// wait for search to complete
			yield return new WaitUntil(() => task.IsCompleted || task.IsFaulted);
			if (task.IsFaulted)
			{
				ShowDebugMessage("AIEngine: Get Possible Action async has faulted and aborted.");
				ShowDebugMessage("AIEngine: Exception Message:\n" + task.Exception.Message + "\n" + task.Exception.StackTrace);
			}
			yield return null;
		}

		/// <summary>
		/// Retrieve a list of valid equipment combination and put them in the supplied list.
		/// </summary>
		private void GetValidCombinations(Unit selfUnit, List<int> results)
		{
			results.Clear();

			// try every combination of equipments for attack, using binary from right to left i.e.rightmost bit is first equipment
			int combinationCount = (int)Mathf.Pow(2, selfUnit.Equipments.Count);
			for (int combination = 0; combination < combinationCount; combination++)
			{
				// count equipment types
				int movementCount = 0;
				int meleeBuffCount = 0;
				int meleeAttackCount = 0;
				int magicBuffCount = 0;
				int magicAttackCount = 0;
				for (int i = 0; i < selfUnit.Equipments.Count; i++)
				{
					if (HasFlag(combination, i))
					{
						switch (selfUnit.Equipments[i].Type)
						{
							case Equipment.EquipmentType.MovementBuff:
								movementCount++;
								break;
							case Equipment.EquipmentType.MeleeSelfBuff:
								meleeBuffCount++;
								break;
							case Equipment.EquipmentType.MeleeAttack:
								meleeAttackCount++;
								break;
							case Equipment.EquipmentType.MagicSelfBuff:
								magicBuffCount++;
								break;
							case Equipment.EquipmentType.MagicAttack:
								magicAttackCount++;
								break;
							default:
								break;
						}
					}
				}

				// skip combinations with more than one movement
				if (movementCount > 1)
				{
					continue;
				}
				// skip combinations with more than one attack
				if (meleeAttackCount + magicAttackCount > 1)
				{
					continue;
				}
				// skip combinations with melee attack and more than one magic buff
				if (magicAttackCount == 0 && magicBuffCount > 0)
				{
					continue;
				}
				// skip combinations with magic attack and more than one melee buff
				if (magicAttackCount > 1 && meleeBuffCount > 0)
				{
					continue;
				}
				results.Add(combination);
			}
		}

		/// <summary>
		/// Decode the combination into equipment list and useful informations.
		/// </summary>
		private void DecodeCombination(Unit selfUnit, int combination, List<Equipment> usingEquipments, out int movement, out int meleeRange, out int magicRange, out AttackAreaRule areaRule, out bool isMagicAttack)
		{
			usingEquipments.Clear();
			movement = selfUnit.Movement;
			meleeRange = selfUnit.PhysicalRange;
			magicRange = selfUnit.MagicalRange;
			areaRule = AttackAreaRule.Adjacent;
			isMagicAttack = false;

			for (int i = 0; i < selfUnit.Equipments.Count; i++)
			{
				if (HasFlag(combination, i))
				{
					usingEquipments.Add(selfUnit.Equipments[i]);
					movement += selfUnit.Equipments[i].MovementDelta;
					meleeRange += selfUnit.Equipments[i].PhysicalRangeDelta;
					magicRange += selfUnit.Equipments[i].MagicalRangeDelta;
					if (selfUnit.Equipments[i].Type == Equipment.EquipmentType.MeleeAttack || selfUnit.Equipments[i].Type == Equipment.EquipmentType.MagicAttack)
						areaRule = selfUnit.Equipments[i].AreaRule;
					if (selfUnit.Equipments[i].Type == Equipment.EquipmentType.MagicAttack)
						isMagicAttack = true;
				}
			}
		}

		/// <summary>
		/// Check if a int combination has a particular flag set to 1.
		/// </summary>
		private bool HasFlag(int combination, int flag)
		{
			return (combination & 1 << flag) == (1 << flag);
		}

		/// <summary>
		/// Check if there is enough dice in the dice list to fulfill all requirment of a equipment, and put the selected dice in the supplied result list.
		/// </summary>
		private bool DiceFulfills(IEnumerable<Equipment> equipments, IEnumerable<Die> dice, List<Die> results)
		{
			results.Clear();

			// initialize container
			List<List<Die>> fulfillment = new List<List<Die>>();
			fulfillment.Clear();
			foreach (Equipment equipment in equipments)
			{
				for (int i = 0; i < equipment.DieSlots.Count; i++)
				{
					fulfillment.Add(new List<Die>());
					results.Add(null);
				}
			}

			// special case for no slots
			if (results.Count == 0)
				return true;

			// sorted dice according to value
			List<Die> sortedDice = new List<Die>();
			sortedDice.AddRange(dice);
			sortedDice.Sort((a, b) => a.Value.CompareTo(b.Value));

			// calculate individual fulfillment
			int slotOffset = 0;
			foreach (Equipment equipment in equipments)
			{
				for (int i = 0; i < equipment.DieSlots.Count; i++)
				{
					for (int j = 0; j < sortedDice.Count; j++)
					{
						if (equipment.DieSlots[i].IsFulfillBy(sortedDice[j]))
						{
							fulfillment[slotOffset + i].Add(sortedDice[j]);
						}
					}
				}
				slotOffset += equipment.DieSlots.Count;
			}

			// pick dice
			for (int i = 0; i < fulfillment.Count; i++)
			{
				// find the slot with the least fulfillment
				int leastFulfillmentSlot = -1;
				int leastFulfillmentSlotDieCount = int.MaxValue;
				for (int j = 0; j < fulfillment.Count; j++)
				{
					if (results[j] == null && fulfillment[j].Count < leastFulfillmentSlotDieCount && fulfillment[j].Count > 0)
					{
						leastFulfillmentSlot = j;
						leastFulfillmentSlotDieCount = fulfillment[j].Count;
					}
				}

				// pick the smallest die and remove die from other slot possiblilties
				if (leastFulfillmentSlot != -1)
				{
					Die selectedDie = fulfillment[leastFulfillmentSlot][0];
					results[leastFulfillmentSlot] = selectedDie;
					for (int j = 0; j < fulfillment.Count; j++)
					{
						fulfillment[j].Remove(selectedDie);
					}
				}
			}

			// determine fulfillment 
			if (results.Any(x => x == null))
			{
				results.Clear();
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Retrieve a list of all attackable enemies with the current stats, and put them in the supplied result list.
		/// </summary>
		private void GetTargetableEnemies(Unit selfUnit, IEnumerable<Unit> enemyUnits, int movement, AttackAreaRule rule, int range, List<AttackInfo> results)
		{
			List<Tile> movableTiles = new List<Tile>();
			List<Tile> attackableTiles = new List<Tile>();

			results.Clear();
			board.GetConnectedTilesInRange(selfUnit.OccupiedTiles, Unit.AllOccupiedTiles.Except(selfUnit.OccupiedTiles), movement, movableTiles);
			foreach (Tile movableTile in movableTiles)
			{
				// search each tile for attacking since GetConnectedTilesInRange is found using BFS, no sorting is required
				board.GetTilesByRule(movableTile, rule, range, attackableTiles);
				foreach (Unit enemy in enemyUnits)
				{
					if (!results.Any(x => x.unit == enemy) && attackableTiles.Intersect(enemy.OccupiedTiles).Count() > 0)
					{
						results.Add(new AttackInfo(enemy, movableTile));
					}
				}
			}
		}

		/// <summary>
		/// Retrieve the tile that is movable and nearest to any enemy.
		/// </summary>
		private Tile GetNearestMovableTileToEnemy(Unit selfUnit, IEnumerable<Unit> enemyUnits)
		{
			Unit nearestUnit = null;
			Tile nearestTile = null;
			int shortestDistance = int.MaxValue;
			List<Tile> pathToNearestTile = new List<Tile>();
			foreach (Unit enemyUnit in enemyUnits)
			{
				foreach (Tile enemyOccupliedTile in enemyUnit.OccupiedTiles)
				{
					foreach (Tile selfOccupliedTile in selfUnit.OccupiedTiles)
					{				
						int distance = Int2.GridDistance(enemyOccupliedTile.BoardPos, selfOccupliedTile.BoardPos);
						if (distance < shortestDistance)
						{
							nearestUnit = enemyUnit;
							nearestTile = enemyOccupliedTile;
							shortestDistance = distance;
						}
					}
				}
			}
			if (nearestTile != null)
			{
				board.GetShortestPath(selfUnit.OccupiedTiles, Unit.AllOccupiedTiles.Except(selfUnit.OccupiedTiles).Except(nearestUnit.OccupiedTiles), nearestUnit.OccupiedTiles.ElementAt(0), 20, pathToNearestTile);
				pathToNearestTile.RemoveAll(x => nearestUnit.OccupiedTiles.Contains(x));
				if (pathToNearestTile.Count > 0)
				{
					return pathToNearestTile[Math.Min(pathToNearestTile.Count - 1, selfUnit.Movement)];
				}
			}
			return null;
		}

		/// <summary>
		/// Generate a PossibleAction object from the supplied information.
		/// </summary>
		private PossibleAction GeneratePossibleAction(Unit selfUnit, AttackInfo attackInfo, Vector3 enemyCenter, List<Equipment> usedEquipments, List<Die> usedDice)
		{
			PossibleAction possibleAction = new PossibleAction(attacker: selfUnit);

			// add equipment info to possible action
			int diceOffset = 0;
			for (int i = 0; i < usedEquipments.Count; i++)
			{
				if (usedEquipments[i].Type == Equipment.EquipmentType.MovementBuff)
				{
					possibleAction.AddMovementEquipmentUsage(
					equipment: usedEquipments[i],
					dice: usedDice.GetRange(diceOffset, usedEquipments[i].DieSlots.Count));
				}
				else
				{
					possibleAction.AddAttackEquipmentUsage(
					equipment: usedEquipments[i],
					dice: usedDice.GetRange(diceOffset, usedEquipments[i].DieSlots.Count));
				}
				diceOffset += usedEquipments[i].DieSlots.Count;
			}

			// add movement info to possible action
			if (attackInfo.tile != null)
			{
				possibleAction.SetMovement(
					position: attackInfo.tile,
					distanceToEnemyCenter: Vector3.Distance(attackInfo.tile.WorldPos, enemyCenter));
			}

			// add attack info to possible action
			if (attackInfo.unit != null)
			{
				// calcaulate damage done
				int rawDamage = 0;
				Equipment magicAttackEquipment = usedEquipments.FirstOrDefault(x => x.Type == Equipment.EquipmentType.MagicAttack);
				Equipment meleeAttackEquipment = usedEquipments.FirstOrDefault(x => x.Type == Equipment.EquipmentType.MeleeAttack);
				if (magicAttackEquipment != null)
				{
					int rawMagicalAttackDelta = 0;
					foreach (Equipment equipment in usedEquipments)
					{
						rawMagicalAttackDelta += equipment.MagicalAttackDelta;
					}
					rawDamage = Mathf.Max(selfUnit.MagicalAttack + rawMagicalAttackDelta - attackInfo.unit.MagicalDefence, 0);

				}
				else if (meleeAttackEquipment != null)
				{
					int rawPhysicalAttackDelta = 0;
					foreach (Equipment equipment in usedEquipments)
					{
						rawPhysicalAttackDelta += equipment.PhysicalAttackDelta;
					}
					rawDamage = Mathf.Max(selfUnit.PhysicalAttack + rawPhysicalAttackDelta - attackInfo.unit.PhysicalDefence, 0);
				}
				else
				{
					rawDamage = Mathf.Max(selfUnit.PhysicalAttack - attackInfo.unit.PhysicalDefence, 0);
				}
				int damageDone = attackInfo.unit.Health - rawDamage >= 0 ? rawDamage : attackInfo.unit.Health;

				// finalize
				possibleAction.SetAttack(target: attackInfo.unit,
					damageDone: damageDone,
					beforeHealthPercentage: 1.0f * attackInfo.unit.Health / attackInfo.unit.maxHealth,
					afterHealthPercentage: 1.0f * (attackInfo.unit.Health - damageDone) / attackInfo.unit.maxHealth);
			}

			return possibleAction;
		}

		// ========================================================= Best Action Selection ========================================================

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
	}
}