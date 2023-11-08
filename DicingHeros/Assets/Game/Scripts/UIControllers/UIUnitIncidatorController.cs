using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DicingHeros
{
    public class UIUnitIncidatorController : MonoBehaviour
    {
		// reference
		public GameController game => GameController.current;

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
		}

		/// <summary>
		/// Start is called before the first frame update and/or the game object is first active.
		/// </summary>
		private void Start()
		{
			foreach (Player player in game.GetAllPlayers())
			{
				player.OnUnitsChanged += UpdateAllIndicators;
			}
			Unit.OnAnyBeingInspectedChanged += UpdateAllIndicators;
			UpdateAllIndicators();
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
			if (game != null)
			{ 
				foreach (Player player in game.GetAllPlayers())
				{
					if (player != null)
					{
						player.OnUnitsChanged += UpdateAllIndicators;
					}
				}
			}
			Unit.OnAnyBeingInspectedChanged -= UpdateAllIndicators;
		}

		public void UpdateAllIndicators()
		{
			// remove all not in used indicators
			foreach (UIUnitIndicator indicator in indicators)
			{
				bool notFound = true;
				foreach (Player player in game.GetAllPlayers())
				{
					if (indicator.Target != null && player.Units.Contains(indicator.Target))
					{
						notFound = false;
					}
				}
				if (notFound)
				{
					indicator.SetTarget(null);
				}
			}

			// all new indicators
			foreach (Player player in game.GetAllPlayers())
			{
				foreach (Unit unit in player.Units)
				{
					if (!indicators.Any(x => x.Target == unit))
					{
						UIUnitIndicator firstReadyIndicator = indicators.FirstOrDefault(x => x.Target == null);
						if (firstReadyIndicator == null)
						{
							firstReadyIndicator = Instantiate(indicatorPrefab, transform).GetComponent<UIUnitIndicator>();
							indicators.Add(firstReadyIndicator);
						}
						firstReadyIndicator.SetTarget(unit);
					}
				}
			}











			/*
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
			*/
		}
	}
}