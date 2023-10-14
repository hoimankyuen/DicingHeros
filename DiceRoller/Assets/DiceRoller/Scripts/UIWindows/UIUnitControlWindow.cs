using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DiceRoller
{
    public class UIUnitControlWindow : UISideWindow
    {
		[Header("Components")]
		public Button skipButton;
		public Button movementButton;
		public Button attackButton;
		public Button cancelButton;

		// reference
		private StateMachine stateMachine => StateMachine.current;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected override void Start()
		{
			base.Start();
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected override void Update()
		{
			base.Update();
			UpdateApparences();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		// ========================================================= UI Methods =========================================================

		private void UpdateApparences()
		{
			Unit unit = Unit.GetFirstSelected();
			if (unit != null)
			{
				if (stateMachine.State == SMState.UnitMoveSelect)
				{
					skipButton.interactable = true;
					movementButton.interactable = false;
					attackButton.interactable = true;
				}

				if (stateMachine.State == SMState.UnitAttackSelect)
				{
					skipButton.interactable = true;
					movementButton.interactable = true;
					attackButton.interactable = false;
				}

				if (stateMachine.State == SMState.UnitDepletedSelect)
				{
					skipButton.interactable = false;
					movementButton.interactable = false;
					attackButton.interactable = false;
				}
			}
		}

		public void SkipMove()
		{
			Unit unit = Unit.GetFirstSelected();
			if (unit != null)
			{
				unit.SkipMoveSelect();
				unit.SkipAttackSelect();
			}
		}

		public void SwapToMove()
		{
			Unit unit = Unit.GetFirstSelected();
			if (unit != null)
			{
				unit.ChangeToMoveSelect();
			}
		}

		public void SwapToAttack()
		{
			Unit unit = Unit.GetFirstSelected();
			if (unit != null)
			{
				unit.ChangeToAttackSelect(); 
			}
		}

		public void Cancel()
		{
			Unit unit = Unit.GetFirstSelected();
			if (unit != null)
			{
				unit.CancelMoveSelect();
				unit.CancelAttackSelect();
				unit.CancelDepletedSelect();
			}
		}
	}
}