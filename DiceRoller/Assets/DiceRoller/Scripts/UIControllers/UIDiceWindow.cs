using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DiceRoller
{
    public class UIDiceWindow : MonoBehaviour
    {
        [Header("Main Frame Components")]
        public RectTransform mainFrame;
        public TextMeshProUGUI title;

        [Header("Dice Frame Components")]
        public RectTransform diceFrame;
        public GameObject uiDiePrefab;


		// working variables
		public List<Die> dice = new List<Die>();
		protected List<UIDie> uiDice = new List<UIDie>();

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected void Awake()
		{
			uiDiePrefab.SetActive(false);
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected void Start()
		{
			Populate();
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
		}

		// ========================================================= Behaviour Display =========================================================
		
		protected void Populate()
		{
			// clear previous ui dice
			for (int i = uiDice.Count - 1; i >= 0; i--)
			{
				Destroy(uiDice[i].gameObject);
			}
			uiDice.Clear();

			// populate a new set of dui ice
			for (int i = 0; i < dice.Count; i++)
			{
				UIDie uiDie = Instantiate(uiDiePrefab, uiDiePrefab.transform.parent).GetComponent<UIDie>();
				uiDie.gameObject.SetActive(true);
				uiDie.SetDisplay(dice[i]);
				uiDie.rectTransform.pivot = new Vector2(i % 2 == 0 ? 1 : 0, 1);
				uiDie.rectTransform.anchoredPosition = new Vector2(0, i / 2 * -uiDie.rectTransform.rect.height);	
				uiDice.Add(uiDie);
			}
		}
	}
}