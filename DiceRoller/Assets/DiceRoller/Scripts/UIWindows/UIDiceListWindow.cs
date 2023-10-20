using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceRoller
{
    public class UIDiceListWindow : UISideWindow
	{
        [Header("Dice Frame Components")]
		public TextMeshProUGUI title;
		public RectTransform diceFrame;
        public GameObject uiDiePrefab;
		public Button allButton;

		// reference
		protected GameController game => GameController.current;
		protected StateMachine stateMachine => StateMachine.current;

		// working variables
		protected List<UIDie> uiDice = new List<UIDie>();
		protected Player inspectingPlayer = null;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			uiDiePrefab.SetActive(false);
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected override void Start()
		{
			base.Start();

			if (game != null)
			{
				game.OnPlayerChanged += Populate;
				stateMachine.OnStateChanged += RefreshButton;
				Populate();
			}
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected override void Update()
		{
			base.Update();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (game != null)
			{
				game.OnPlayerChanged -= Populate;
				stateMachine.OnStateChanged -= RefreshButton;
			}
		}

		// ========================================================= Behaviour =========================================================
		
		protected void Populate()
		{
			// only re populate if player is changed
			if (game.CurrentPlayer == inspectingPlayer)
				return;
			inspectingPlayer = game.CurrentPlayer;

			// clear previous ui dice
			for (int i = uiDice.Count - 1; i >= 0; i--)
			{
				Destroy(uiDice[i].gameObject);
			}
			uiDice.Clear();

			// check if player exist
			if (inspectingPlayer == null)
				return;

			// populate a new set of ui dice
			for (int i = 0; i < inspectingPlayer.Dice.Count; i++)
			{
				UIDie uiDie = Instantiate(uiDiePrefab, uiDiePrefab.transform.parent).GetComponent<UIDie>();
				uiDie.gameObject.SetActive(true);
				uiDie.SetInspectingTarget(inspectingPlayer.Dice[i]);
				uiDie.rectTransform.pivot = new Vector2(i % 2 == 0 ? 1 : 0, 1);
				uiDie.rectTransform.anchoredPosition = new Vector2(0, i / 2 * -uiDie.rectTransform.rect.height);	
				uiDice.Add(uiDie);
			}
		}

		protected void RefreshButton()
		{
			allButton.interactable = stateMachine.State == SMState.Navigation || stateMachine.State == SMState.DiceActionSelect;
		}

		public void SelectAll()
		{
			Die.SelectAll_Navigation();
			Die.SelectAll_DieActionSelect();
		}
	}
}