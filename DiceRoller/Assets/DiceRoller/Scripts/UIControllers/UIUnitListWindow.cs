using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceRoller
{
    public class UIUnitListWindow : UISideWindow
	{
		[Header("Unit Frame Components")]
		public TextMeshProUGUI title;
		public RectTransform unitFrame;
		public GameObject uiUnitPrefab;

		// reference
		protected GameController game => GameController.current;

		// working variables
		protected List<UIUnit> uiUnits = new List<UIUnit>();
		protected Player inspectingPlayer = null;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			uiUnitPrefab.SetActive(false);
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected override void Start()
		{
			base.Start();

			if (game != null)
			{
				game.onPlayerChanged += Populate;
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
				game.onPlayerChanged -= Populate;
			}
		}

		// ========================================================= Behaviour =========================================================

		protected void Populate()
		{
			// only re populate if player is changed
			if (game.CurrentPlayer == inspectingPlayer)
				return;
			inspectingPlayer = game.CurrentPlayer;

			// clear previous ui unit
			for (int i = uiUnits.Count - 1; i >= 0; i--)
			{
				Destroy(uiUnits[i].gameObject);
			}
			uiUnits.Clear();

			// check if player exist
			if (inspectingPlayer == null)
				return;

			// populate a new set of ui dice
			for (int i = 0; i < inspectingPlayer.units.Count; i++)
			{
				UIUnit uiUnit = Instantiate(uiUnitPrefab, uiUnitPrefab.transform.parent).GetComponent<UIUnit>();
				uiUnit.gameObject.SetActive(true);
				uiUnit.SetDisplay(inspectingPlayer.units[i]);
				uiUnit.rectTransform.anchoredPosition = new Vector2(0, i * -uiUnit.rectTransform.rect.height);
				uiUnits.Add(uiUnit);
			}
		}
	}
}