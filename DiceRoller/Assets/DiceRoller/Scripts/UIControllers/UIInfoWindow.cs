using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceRoller
{
	public class UIInfoWindow : UISideWindow
	{
		[Header("Info Frame Components")]
		public TextMeshProUGUI playerText;
		public TextMeshProUGUI turnText;
		public TextMeshProUGUI throwText;

		// reference
		protected GameController game => GameController.current;
		protected DiceThrower diceThrower => DiceThrower.current;

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

			if (game != null)
			{
				game.onPlayerChanged += RefreshPlayerDisplay;
				game.onTurnChanged += RefreshTurnDisplay;
				diceThrower.onRemainingThrowChanged += RefreshRemainingThrowDisplay;

				RefreshPlayerDisplay();
				RefreshTurnDisplay();
				RefreshRemainingThrowDisplay();
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
				game.onPlayerChanged -= RefreshPlayerDisplay;
				game.onTurnChanged -= RefreshRemainingThrowDisplay;
				diceThrower.onRemainingThrowChanged -= RefreshRemainingThrowDisplay;
			}
		}

		// ========================================================= Display Methods =========================================================

		protected void RefreshPlayerDisplay()
		{
			if (game.CurrentTurn != null)
			{
				playerText.text = string.Format("Player : {0}", game.GetPlayerById(game.CurrentTurn.playerId).name);
			}
			else
			{
				playerText.text = "Player : ====";
			}
		}

		protected void RefreshTurnDisplay()
		{
			if (game.CurrentTurn != null)
			{
				turnText.text = string.Format("Turn : {0}", game.CurrentTurn.turnNumber);
			}
			else
			{
				playerText.text = "Turn : ==";
			}
		}

		protected void RefreshRemainingThrowDisplay()
		{
			throwText.text = string.Format("Throws : {0}", diceThrower.RemainingThrow);
		}
	}
}