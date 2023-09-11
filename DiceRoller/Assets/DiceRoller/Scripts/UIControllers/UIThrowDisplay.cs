using SimpleMaskCutoff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public class UIThrowDisplay : MonoBehaviour
    {
        [Header("Throw Display")]
        public GameObject throwTarget = null;
        public SpriteRenderer throwArrow = null;
        public SpriteRenderer throwCross = null;
        public GameObject throwPowerIndicator = null;
        public CutoffSpriteRenderer throwPowerIndicatorCutoff = null;

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		protected void Awake()
		{
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		protected void Start()
		{
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		protected void Update()
		{
			UpdateThrowDisplay();
		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		protected void OnDestroy()
		{
		}

		// ========================================================= Throw Display =========================================================

		/// <summary>
		/// Update the apparence of the throw indicator UI.
		/// </summary>
		protected void UpdateThrowDisplay()
		{
			if (DiceThrower.current.ThrowDragging)
			{
				// user throwing
				gameObject.transform.position = DiceThrower.current.ThrowDragPosition;
				throwTarget.SetActive(true);
				throwPowerIndicator.SetActive(true);
				throwPowerIndicator.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0)) * Quaternion.FromToRotation(Vector3.forward, DiceThrower.current.ThrowDirection) * Quaternion.Euler(new Vector3(90, 0, 0));
				if (DiceThrower.current.ThrowPower != -1f)
				{
					// throw have enough power
					throwArrow.gameObject.SetActive(true);
					throwCross.gameObject.SetActive(false);
					throwPowerIndicatorCutoff.CutoffTo(DiceThrower.current.ThrowPower);
				}
				else
				{
					// throw does not have enough power
					throwArrow.gameObject.SetActive(false);
					throwCross.gameObject.SetActive(true);
					throwPowerIndicatorCutoff.CutoffTo(0);
				}
			}
			else
			{
				// user not throwing, disable throw indicator
				throwTarget.SetActive(false);
				throwPowerIndicator.SetActive(false);
			}
		}
	}
}