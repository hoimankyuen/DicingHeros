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

		// working variables
		public List<Unit> units = new List<Unit>();
		protected List<UIUnit> uiUnits = new List<UIUnit>();

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
			Populate();
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
		}

		// ========================================================= Behaviour =========================================================

		protected void Populate()
		{
			// clear previous ui dice
			for (int i = uiUnits.Count - 1; i >= 0; i--)
			{
				Destroy(uiUnits[i].gameObject);
			}
			uiUnits.Clear();

			// populate a new set of dui ice
			for (int i = 0; i < units.Count; i++)
			{
				UIUnit uiUnit = Instantiate(uiUnitPrefab, uiUnitPrefab.transform.parent).GetComponent<UIUnit>();
				uiUnit.gameObject.SetActive(true);
				uiUnit.SetDisplay(units[i]);
				uiUnit.rectTransform.anchoredPosition = new Vector2(0, i * -uiUnit.rectTransform.rect.height);
				uiUnits.Add(uiUnit);
			}
		}
	}
}