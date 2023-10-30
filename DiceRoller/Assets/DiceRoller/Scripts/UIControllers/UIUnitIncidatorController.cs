using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceRoller
{
    public class UIUnitIncidatorController : MonoBehaviour
    {
		// working variables
		public GameObject indicatorPrefab = null;
        private List<UIUnitIndicator> indicators = new List<UIUnitIndicator>();

		// ========================================================= Monobehaviour Methods =========================================================

		/// <summary>
		/// Awake is called when the game object was created. It is always called before start and is 
		/// independent of if the game object is active or not.
		/// </summary>
		private void Awake()
		{
			indicators.Add(transform.GetChild(0).GetComponent<UIUnitIndicator>());
			Unit.OnAnyBeingInspectedChanged += UpdateAllIndicators;
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		private void Start()
		{

		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		private void Update()
		{

		}

		/// <summary>
		/// OnDestroy is called when an game object is destroyed.
		/// </summary>
		private void OnDestroy()
		{
			Unit.OnAnyBeingInspectedChanged -= UpdateAllIndicators;
		}

		public void UpdateAllIndicators()
		{
			IEnumerable<Unit> targets = Unit.GetAllBeingInspected();

			// remove all not in used indicators
			foreach (UIUnitIndicator indicator in indicators)
			{
				if (indicator.Target != null && !targets.Contains(indicator.Target))
				{
					indicator.SetTarget(null);
				}
			}

			// add new indicators
			foreach (Unit target in targets)
			{
				if (!indicators.Any(x => x.Target == target))
				{
					UIUnitIndicator firstReadyIndicator = indicators.FirstOrDefault(x => x.Target == null);
					if (firstReadyIndicator == null)
					{
						firstReadyIndicator = Instantiate(indicatorPrefab, transform).GetComponent<UIUnitIndicator>();
						indicators.Add(firstReadyIndicator);
					}
					firstReadyIndicator.SetTarget(target);
				}
			}
		}
	}
}