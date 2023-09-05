using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceRoller
{
    public class UIUnitDetailWindow : UISideWindow
	{
		[Header("Components")]
		public Image unitImage;
		public UIHealthDisplay healthDisplay;
		public UIStatDisplay statDisplay;

		// working
		public Unit unit;

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

		/// <summary>
		/// OnValidate is called when values where changed in the inspector.
		/// </summary>
		protected override void OnValidate()
		{
			base.OnValidate();
		}

		// ========================================================= Behaviour =========================================================

		protected void Populate()
		{
			unitImage.sprite = unit != null ? unit.iconSprite : null;
			healthDisplay.SetDisplay(unit);
			statDisplay.SetDisplay(unit);
		}
	}
}