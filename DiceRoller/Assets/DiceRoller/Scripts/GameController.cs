using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleMaskCutoff;
using UnityEngine;

namespace DiceRoller
{

	public class GameController : MonoBehaviour
	{
		// singleton
		public static GameController Instance { get; protected set; }

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected void Awake()
		{
			Instance = this;
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected void Start()
		{
			Application.targetFrameRate = 60;
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
			Instance = null;
		}
	}
}
